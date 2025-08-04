import React, { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  Grid,
  Alert,
  CircularProgress,
  TextField,
  Button,
  Avatar,
  Chip,
  InputAdornment,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemText,
  Divider
} from '@mui/material';
import { 
  PeopleOutlined, 
  SearchOutlined,
  PersonOutlined,
  CalendarTodayOutlined,
  HistoryOutlined,
  LocalHospitalOutlined,
  EmailOutlined,
  PhoneOutlined
} from '@mui/icons-material';
import { format, parseISO } from 'date-fns';
import apiService from '../../services/apiService.ts';
import { useAuth } from '../../context/AuthContext.tsx';

interface Patient {
  id: number;
  name: string;
  email: string;
  phone?: string;
  dateOfBirth?: string;
  lastVisit?: string;
  totalAppointments: number;
}

interface Appointment {
  id: number;
  appointmentDate: string;
  appointmentTime: string;
  reason: string;
  status: string;
  notes?: string;
}

interface DoctorPatientsProps {
  onBack?: () => void;
}

const DoctorPatients: React.FC<DoctorPatientsProps> = ({ onBack }) => {
  const { user } = useAuth();
  const [patients, setPatients] = useState<Patient[]>([]);
  const [filteredPatients, setFilteredPatients] = useState<Patient[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);
  const [patientHistory, setPatientHistory] = useState<Appointment[]>([]);
  const [historyDialogOpen, setHistoryDialogOpen] = useState<boolean>(false);
  const [historyLoading, setHistoryLoading] = useState<boolean>(false);

  useEffect(() => {
    fetchPatients();
  }, []);

  useEffect(() => {
    filterPatients();
  }, [patients, searchTerm]);

  const fetchPatients = async () => {
    try {
      setLoading(true);
      // Verificar usuarios existentes para depuración
      try {
        const usersResponse = await apiService.get('/admin/users');
        console.log('Todos los usuarios:', usersResponse.data);
        console.log('Usuario actual:', user);
      } catch (error) {
        console.log('No se pudo obtener lista de usuarios (puede ser por permisos)');
      }
      
      // Primero obtener el doctor ID basado en el user ID
      const doctorsResponse = await apiService.get('/doctors');
      console.log('Doctores disponibles:', doctorsResponse.data);
      console.log('Usuario actual ID:', user?.id, 'Tipo:', typeof user?.id);
      
      // Buscar doctor con conversión de tipos para asegurar comparación correcta
      const currentDoctor = doctorsResponse.data.find((doc: any) => {
        console.log('Comparando doctor userId:', doc.userId, 'con user.id:', user?.id);
        return doc.userId === user?.id || doc.userId === Number(user?.id) || String(doc.userId) === String(user?.id);
      });
      
      if (!currentDoctor) {
        console.error('No se encontró doctor para el usuario:', user?.id);
        console.error('Doctores disponibles:', doctorsResponse.data.map((d: any) => ({ id: d.id, userId: d.userId, name: d.user?.name })));
        setError('No se pudo encontrar el perfil de doctor. Verifica que tu cuenta esté configurada como doctor.');
        return;
      }
      
      console.log('Doctor encontrado:', currentDoctor);

      // Obtener las citas del doctor
      const appointmentsResponse = await apiService.get(`/appointments/doctor/${currentDoctor.id}`);
      const appointments = appointmentsResponse.data;
      
      // Extraer pacientes únicos de las citas
      const uniquePatients = appointments.reduce((acc: any[], appointment: any) => {
        const existingPatient = acc.find(p => p.id === appointment.patient.id);
        if (!existingPatient) {
          acc.push({
            id: appointment.patient.id,
            name: appointment.patient.name,
            email: appointment.patient.email,
            phone: appointment.patient.phone || 'No disponible',
            lastAppointment: appointment.date,
            totalAppointments: 1
          });
        } else {
          existingPatient.totalAppointments += 1;
          // Actualizar última cita si es más reciente
          if (new Date(appointment.date) > new Date(existingPatient.lastAppointment)) {
            existingPatient.lastAppointment = appointment.date;
          }
        }
        return acc;
      }, []);
      
      setPatients(uniquePatients);
    } catch (error: any) {
      console.error('Error fetching patients:', error);
      setError('Error al cargar los pacientes. Por favor intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  const filterPatients = () => {
    let filtered = patients;

    if (searchTerm.trim()) {
      filtered = filtered.filter(patient =>
        patient.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        patient.email.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    setFilteredPatients(filtered);
  };

  const handleViewHistory = async (patient: Patient) => {
    setSelectedPatient(patient);
    setHistoryDialogOpen(true);
    setHistoryLoading(true);

    try {
      // Obtener historial de citas del paciente
      const response = await apiService.get(`/appointments/patient/${patient.id}`);
      const allPatientAppointments = response.data;
      
      // Obtener el doctor ID actual
      const doctorsResponse = await apiService.get('/doctors');
      const currentDoctor = doctorsResponse.data.find((doc: any) => {
        return doc.userId === user?.id || doc.userId === Number(user?.id) || String(doc.userId) === String(user?.id);
      });
      
      // Filtrar solo las citas con este doctor
      const doctorAppointments = allPatientAppointments.filter(
        (appointment: any) => appointment.doctorId === currentDoctor?.id
      );
      
      // Transform and validate appointment data
    const transformedAppointments = doctorAppointments.map((apt: any) => ({
      id: apt.id,
      appointmentDate: apt.date, // Backend uses 'date' not 'appointmentDate'
      appointmentTime: apt.time, // Backend uses 'time' not 'appointmentTime'
      reason: apt.reason,
      status: apt.status,
      notes: apt.notes || ''
    }));
    
    console.log('Patient history appointments:', transformedAppointments);
    setPatientHistory(transformedAppointments);
    } catch (error: any) {
      console.error('Error fetching patient history:', error);
      setError('Error al cargar el historial del paciente. Por favor intenta de nuevo.');
    } finally {
      setHistoryLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Scheduled': return 'primary';
      case 'Completed': return 'success';
      case 'Cancelled': return 'error';
      case 'No Show': return 'warning';
      default: return 'default';
    }
  };

  const calculateAge = (dateOfBirth?: string): string => {
    if (!dateOfBirth) return 'N/A';
    
    const birthDate = new Date(dateOfBirth);
    const today = new Date();
    const age = today.getFullYear() - birthDate.getFullYear();
    const monthDiff = today.getMonth() - birthDate.getMonth();
    
    if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
      return `${age - 1} años`;
    }
    return `${age} años`;
  };

  const formatAppointmentDateTime = (appointmentDate?: string, appointmentTime?: string): string => {
    try {
      if (!appointmentDate || !appointmentTime) {
        return 'Fecha no disponible';
      }
      
      // Try to parse the date and time
      const dateTimeString = `${appointmentDate}T${appointmentTime}`;
      const parsedDate = parseISO(dateTimeString);
      
      // Check if the parsed date is valid
      if (isNaN(parsedDate.getTime())) {
        console.warn('Invalid date/time:', { appointmentDate, appointmentTime });
        return `${appointmentDate} ${appointmentTime}`;
      }
      
      return format(parsedDate, 'MMM d, yyyy at h:mm a');
    } catch (error) {
      console.error('Error formatting date:', error, { appointmentDate, appointmentTime });
      return `${appointmentDate || 'N/A'} ${appointmentTime || 'N/A'}`;
    }
  };

  const handleBack = () => {
    if (onBack) {
      onBack();
    } else {
      window.history.back();
    }
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4, display: 'flex', justifyContent: 'center' }}>
        <CircularProgress />
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Box display="flex" alignItems="center" mb={3}>
        <PeopleOutlined color="primary" sx={{ mr: 2, fontSize: 32 }} />
        <Typography variant="h4" component="h1">
          Mis Pacientes
        </Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Search Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <TextField
            fullWidth
            label="Buscar Pacientes"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Buscar por nombre o email..."
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchOutlined />
                </InputAdornment>
              ),
            }}
          />
        </CardContent>
      </Card>

      {/* Results Section */}
      <Typography variant="h6" gutterBottom>
        {filteredPatients.length} Paciente{filteredPatients.length !== 1 ? 's' : ''} Encontrado{filteredPatients.length !== 1 ? 's' : ''}
      </Typography>

      {filteredPatients.length === 0 ? (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <PeopleOutlined sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              No se encontraron pacientes
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {searchTerm ? 'Intenta ajustar tus criterios de búsqueda.' : 'Aún no tienes pacientes.'}
            </Typography>
          </CardContent>
        </Card>
      ) : (
        <Grid container spacing={3}>
          {filteredPatients.map((patient) => (
            <Grid item xs={12} md={6} lg={4} key={patient.id}>
              <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                <CardContent sx={{ flexGrow: 1 }}>
                  <Box display="flex" alignItems="center" mb={2}>
                    <Avatar sx={{ bgcolor: 'primary.main', mr: 2 }}>
                      <PersonOutlined />
                    </Avatar>
                    <Box>
                      <Typography variant="h6" component="h2">
                        {patient.name}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        ID Paciente: {patient.id}
                      </Typography>
                    </Box>
                  </Box>

                  <Box display="flex" alignItems="center" mb={1}>
                    <EmailOutlined sx={{ mr: 1, color: 'text.secondary', fontSize: 20 }} />
                    <Typography variant="body2" color="text.secondary">
                      {patient.email}
                    </Typography>
                  </Box>

                  {patient.phone && (
                    <Box display="flex" alignItems="center" mb={1}>
                      <PhoneOutlined sx={{ mr: 1, color: 'text.secondary', fontSize: 20 }} />
                      <Typography variant="body2" color="text.secondary">
                        {patient.phone}
                      </Typography>
                    </Box>
                  )}

                  <Box display="flex" alignItems="center" mb={2}>
                    <CalendarTodayOutlined sx={{ mr: 1, color: 'text.secondary', fontSize: 20 }} />
                    <Typography variant="body2" color="text.secondary">
                      Age: {calculateAge(patient.dateOfBirth)}
                    </Typography>
                  </Box>

                  <Box display="flex" justifyContent="space-between" alignItems="center" mb={2}>
                    <Typography variant="body2">
                      <strong>Total Appointments:</strong> {patient.totalAppointments}
                    </Typography>
                    {patient.lastVisit && (
                      <Typography variant="body2" color="text.secondary">
                        Last visit: {format(parseISO(patient.lastVisit), 'MMM d, yyyy')}
                      </Typography>
                    )}
                  </Box>
                </CardContent>

                <CardContent sx={{ pt: 0 }}>
                  <Button
                    variant="outlined"
                    fullWidth
                    startIcon={<HistoryOutlined />}
                    onClick={() => handleViewHistory(patient)}
                  >
                    View History
                  </Button>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {/* Patient History Dialog */}
      <Dialog 
        open={historyDialogOpen} 
        onClose={() => setHistoryDialogOpen(false)} 
        maxWidth="md" 
        fullWidth
      >
        <DialogTitle>
          <Box display="flex" alignItems="center">
            <LocalHospitalOutlined sx={{ mr: 2 }} />
            {selectedPatient?.name} - Appointment History
          </Box>
        </DialogTitle>
        <DialogContent>
          {historyLoading ? (
            <Box display="flex" justifyContent="center" py={4}>
              <CircularProgress />
            </Box>
          ) : patientHistory.length === 0 ? (
            <Typography variant="body1" color="text.secondary" sx={{ py: 4, textAlign: 'center' }}>
              No appointment history found.
            </Typography>
          ) : (
            <List>
              {patientHistory.map((appointment, index) => (
                <React.Fragment key={appointment.id}>
                  <ListItem alignItems="flex-start">
                    <ListItemText
                      primary={
                        <Box display="flex" justifyContent="space-between" alignItems="center">
                          <Typography variant="h6">
                            {formatAppointmentDateTime(appointment.appointmentDate, appointment.appointmentTime)}
                          </Typography>
                          <Chip
                            label={appointment.status}
                            color={getStatusColor(appointment.status) as any}
                            size="small"
                            variant="outlined"
                          />
                        </Box>
                      }
                      secondary={
                        <Box sx={{ mt: 1 }}>
                          <Typography variant="body2" color="text.primary">
                            <strong>Reason:</strong> {appointment.reason}
                          </Typography>
                          {appointment.notes && (
                            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                              <strong>Notes:</strong> {appointment.notes}
                            </Typography>
                          )}
                        </Box>
                      }
                    />
                  </ListItem>
                  {index < patientHistory.length - 1 && <Divider />}
                </React.Fragment>
              ))}
            </List>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setHistoryDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>

      <Box mt={4} display="flex" justifyContent="flex-start">
        <Button variant="outlined" onClick={handleBack}>
          Back to Dashboard
        </Button>
      </Box>
    </Container>
  );
};

export default DoctorPatients;
