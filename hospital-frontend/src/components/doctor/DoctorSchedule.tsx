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
  Chip,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Avatar,
  Divider
} from '@mui/material';
import { 
  ScheduleOutlined, 
  PersonOutlined,
  AccessTimeOutlined,
  CalendarTodayOutlined,
  CheckCircleOutlined,
  CancelOutlined,
  EditOutlined,
  LocalHospitalOutlined
} from '@mui/icons-material';
import { format, parseISO, isToday, isTomorrow, isYesterday } from 'date-fns';
import apiService from '../../services/apiService.ts';
import { useAuth } from '../../context/AuthContext.tsx';

interface Appointment {
  id: number;
  patientName: string;
  patientId: number;
  appointmentDate: string;
  appointmentTime: string;
  reason: string;
  status: 'Scheduled' | 'Completed' | 'Cancelled' | 'No Show';
  notes?: string;
}

interface DoctorScheduleProps {
  onBack?: () => void;
}

const DoctorSchedule: React.FC<DoctorScheduleProps> = ({ onBack }) => {
  const { user } = useAuth();
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [selectedDate, setSelectedDate] = useState<string>(new Date().toISOString().split('T')[0]);
  const [editDialogOpen, setEditDialogOpen] = useState<boolean>(false);
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(null);
  const [notes, setNotes] = useState<string>('');
  const [status, setStatus] = useState<string>('');

  useEffect(() => {
    fetchAppointments();
  }, [selectedDate]);

  const fetchAppointments = async () => {
    try {
      setLoading(true);
      // Fetch appointments for the doctor on the selected date
      const response = await apiService.get(`/appointments/doctor/${user?.id}?date=${selectedDate}`);
      setAppointments(response.data);
    } catch (error: any) {
      console.error('Error fetching appointments:', error);
      setError('Error loading appointments. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleEditAppointment = (appointment: Appointment) => {
    setSelectedAppointment(appointment);
    setNotes(appointment.notes || '');
    setStatus(appointment.status);
    setEditDialogOpen(true);
  };

  const handleSaveAppointment = async () => {
    if (!selectedAppointment) return;

    try {
      await apiService.put(`/appointments/${selectedAppointment.id}`, {
        ...selectedAppointment,
        notes,
        status
      });
      
      // Update local state
      setAppointments(prev => 
        prev.map(apt => 
          apt.id === selectedAppointment.id 
            ? { ...apt, notes, status: status as any }
            : apt
        )
      );
      
      setEditDialogOpen(false);
      setSelectedAppointment(null);
    } catch (error: any) {
      console.error('Error updating appointment:', error);
      setError('Error updating appointment. Please try again.');
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

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Scheduled': return <ScheduleOutlined />;
      case 'Completed': return <CheckCircleOutlined />;
      case 'Cancelled': return <CancelOutlined />;
      case 'No Show': return <AccessTimeOutlined />;
      default: return <ScheduleOutlined />;
    }
  };

  const formatDateDisplay = (dateString: string) => {
    const date = parseISO(dateString);
    if (isToday(date)) return 'Today';
    if (isTomorrow(date)) return 'Tomorrow';
    if (isYesterday(date)) return 'Yesterday';
    return format(date, 'MMMM d, yyyy');
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
        <LocalHospitalOutlined color="primary" sx={{ mr: 2, fontSize: 32 }} />
        <Typography variant="h4" component="h1">
          Mi Horario
        </Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Date Selection */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={3} alignItems="center">
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                type="date"
                label="Seleccionar Fecha"
                value={selectedDate}
                onChange={(e) => setSelectedDate(e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="h6">
                Citas para {formatDateDisplay(selectedDate)}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {appointments.length} cita{appointments.length !== 1 ? 's' : ''} programada{appointments.length !== 1 ? 's' : ''}
              </Typography>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Appointments List */}
      {appointments.length === 0 ? (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <CalendarTodayOutlined sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              No hay citas programadas
            </Typography>
            <Typography variant="body2" color="text.secondary">
              No tienes citas para {formatDateDisplay(selectedDate)}.
            </Typography>
          </CardContent>
        </Card>
      ) : (
        <Grid container spacing={3}>
          {appointments.map((appointment) => (
            <Grid item xs={12} key={appointment.id}>
              <Card>
                <CardContent>
                  <Grid container spacing={3} alignItems="center">
                    <Grid item xs={12} md={2}>
                      <Box display="flex" alignItems="center">
                        <Avatar sx={{ bgcolor: 'primary.main', mr: 2 }}>
                          <PersonOutlined />
                        </Avatar>
                        <Box>
                          <Typography variant="h6">
                            {appointment.appointmentTime}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {format(parseISO(`${appointment.appointmentDate}T${appointment.appointmentTime}`), 'h:mm a')}
                          </Typography>
                        </Box>
                      </Box>
                    </Grid>

                    <Grid item xs={12} md={3}>
                      <Typography variant="h6" gutterBottom>
                        {appointment.patientName}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        ID Paciente: {appointment.patientId}
                      </Typography>
                    </Grid>

                    <Grid item xs={12} md={3}>
                      <Typography variant="body1" gutterBottom>
                        <strong>Motivo:</strong> {appointment.reason}
                      </Typography>
                      {appointment.notes && (
                        <Typography variant="body2" color="text.secondary">
                          <strong>Notas:</strong> {appointment.notes}
                        </Typography>
                      )}
                    </Grid>

                    <Grid item xs={12} md={2}>
                      <Chip
                        icon={getStatusIcon(appointment.status)}
                        label={appointment.status}
                        color={getStatusColor(appointment.status) as any}
                        variant="outlined"
                      />
                    </Grid>

                    <Grid item xs={12} md={2}>
                      <Button
                        variant="outlined"
                        startIcon={<EditOutlined />}
                        onClick={() => handleEditAppointment(appointment)}
                        fullWidth
                      >
                        Actualizar
                      </Button>
                    </Grid>
                  </Grid>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      {/* Edit Appointment Dialog */}
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Actualizar Cita</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Typography variant="h6" gutterBottom>
              {selectedAppointment?.patientName}
            </Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              {selectedAppointment && format(
                parseISO(`${selectedAppointment.appointmentDate}T${selectedAppointment.appointmentTime}`), 
                'MMMM d, yyyy at h:mm a'
              )}
            </Typography>
            
            <Divider sx={{ my: 2 }} />

            <FormControl fullWidth sx={{ mb: 3 }}>
              <InputLabel>Estado</InputLabel>
              <Select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                label="Estado"
              >
                <MenuItem value="Scheduled">Programada</MenuItem>
                <MenuItem value="Completed">Completada</MenuItem>
                <MenuItem value="Cancelled">Cancelada</MenuItem>
                <MenuItem value="No Show">No Asistió</MenuItem>
              </Select>
            </FormControl>

            <TextField
              fullWidth
              multiline
              rows={4}
              label="Notas"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Agregar notas sobre la cita..."
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>Cancelar</Button>
          <Button onClick={handleSaveAppointment} variant="contained">
            Guardar Cambios
          </Button>
        </DialogActions>
      </Dialog>

      <Box mt={4} display="flex" justifyContent="flex-start">
        <Button variant="outlined" onClick={handleBack}>
          Volver al Dashboard
        </Button>
      </Box>
    </Container>
  );
};

export default DoctorSchedule;
