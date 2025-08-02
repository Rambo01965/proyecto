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
  Chip,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Avatar,
  Divider
} from '@mui/material';
import { 
  CalendarTodayOutlined, 
  SearchOutlined,
  PersonOutlined,
  LocalHospitalOutlined,
  VisibilityOutlined,
  EditOutlined,
  DeleteOutlined,
  FilterListOutlined,
  ScheduleOutlined,
  CheckCircleOutlined,
  CancelOutlined,
  AccessTimeOutlined
} from '@mui/icons-material';
import { format, parseISO, startOfDay, endOfDay, isWithinInterval } from 'date-fns';
import apiService from '../../services/apiService.ts';
import { useAuth } from '../../context/AuthContext.tsx';

interface Appointment {
  id: number;
  patientName: string;
  doctorName: string;
  patientId: number;
  doctorId: number;
  appointmentDate: string;
  appointmentTime: string;
  reason: string;
  status: 'Scheduled' | 'Completed' | 'Cancelled' | 'No Show';
  notes?: string;
  createdAt: string;
}

interface AdminAppointmentManagementProps {
  onBack?: () => void;
}

const AdminAppointmentManagement: React.FC<AdminAppointmentManagementProps> = ({ onBack }) => {
  const { user } = useAuth();
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [filteredAppointments, setFilteredAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string>('');
  const [dateFilter, setDateFilter] = useState<string>('');
  const [selectedAppointment, setSelectedAppointment] = useState<Appointment | null>(null);
  const [viewDialogOpen, setViewDialogOpen] = useState<boolean>(false);
  const [editDialogOpen, setEditDialogOpen] = useState<boolean>(false);
  const [editStatus, setEditStatus] = useState<string>('');
  const [editNotes, setEditNotes] = useState<string>('');

  useEffect(() => {
    fetchAppointments();
  }, []);

  useEffect(() => {
    filterAppointments();
  }, [appointments, searchTerm, statusFilter, dateFilter]);

  const fetchAppointments = async () => {
    try {
      setLoading(true);
      const response = await apiService.get('/admin/appointments');
      setAppointments(response.data);
    } catch (error: any) {
      console.error('Error fetching appointments:', error);
      setError('Error loading appointments. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const filterAppointments = () => {
    let filtered = appointments;

    // Filter by search term (patient or doctor name)
    if (searchTerm.trim()) {
      filtered = filtered.filter(appointment =>
        appointment.patientName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        appointment.doctorName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        appointment.reason.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Filter by status
    if (statusFilter) {
      filtered = filtered.filter(appointment => appointment.status === statusFilter);
    }

    // Filter by date
    if (dateFilter) {
      const filterDate = parseISO(dateFilter);
      filtered = filtered.filter(appointment => {
        const appointmentDate = parseISO(appointment.appointmentDate);
        return isWithinInterval(appointmentDate, {
          start: startOfDay(filterDate),
          end: endOfDay(filterDate)
        });
      });
    }

    setFilteredAppointments(filtered);
  };

  const handleViewAppointment = (appointment: Appointment) => {
    setSelectedAppointment(appointment);
    setViewDialogOpen(true);
  };

  const handleEditAppointment = (appointment: Appointment) => {
    setSelectedAppointment(appointment);
    setEditStatus(appointment.status);
    setEditNotes(appointment.notes || '');
    setEditDialogOpen(true);
  };

  const handleSaveAppointment = async () => {
    if (!selectedAppointment) return;

    try {
      await apiService.put(`/admin/appointments/${selectedAppointment.id}`, {
        status: editStatus,
        notes: editNotes
      });
      
      // Update local state
      setAppointments(prev => 
        prev.map(apt => 
          apt.id === selectedAppointment.id 
            ? { ...apt, status: editStatus as any, notes: editNotes }
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

  const handleDeleteAppointment = async (appointmentId: number) => {
    if (window.confirm('Are you sure you want to delete this appointment?')) {
      try {
        await apiService.delete(`/admin/appointments/${appointmentId}`);
        setAppointments(prev => prev.filter(apt => apt.id !== appointmentId));
      } catch (error: any) {
        console.error('Error deleting appointment:', error);
        setError('Error deleting appointment. Please try again.');
      }
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

  const getStatusStats = () => {
    const stats = {
      total: appointments.length,
      scheduled: appointments.filter(a => a.status === 'Scheduled').length,
      completed: appointments.filter(a => a.status === 'Completed').length,
      cancelled: appointments.filter(a => a.status === 'Cancelled').length,
      noShow: appointments.filter(a => a.status === 'No Show').length
    };
    return stats;
  };

  const clearFilters = () => {
    setSearchTerm('');
    setStatusFilter('');
    setDateFilter('');
  };

  const handleBack = () => {
    if (onBack) {
      onBack();
    } else {
      // Fallback: go back in browser history
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

  const stats = getStatusStats();

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Box display="flex" alignItems="center" mb={3}>
        <CalendarTodayOutlined color="primary" sx={{ mr: 2, fontSize: 32 }} />
        <Typography variant="h4" component="h1">
          Gestión de Citas
        </Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Statistics Cards */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={2.4}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="primary">{stats.total}</Typography>
              <Typography variant="body2" color="text.secondary">Total</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2.4}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="primary">{stats.scheduled}</Typography>
              <Typography variant="body2" color="text.secondary">Programadas</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2.4}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="success.main">{stats.completed}</Typography>
              <Typography variant="body2" color="text.secondary">Completadas</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2.4}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="error.main">{stats.cancelled}</Typography>
              <Typography variant="body2" color="text.secondary">Canceladas</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2.4}>
          <Card>
            <CardContent sx={{ textAlign: 'center' }}>
              <Typography variant="h4" color="warning.main">{stats.noShow}</Typography>
              <Typography variant="body2" color="text.secondary">No Asistieron</Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Filters Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={3} alignItems="center">
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Buscar Citas"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Buscar por paciente, doctor o motivo..."
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchOutlined />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth>
                <InputLabel>Filtrar por Estado</InputLabel>
                <Select
                  value={statusFilter}
                  onChange={(e) => setStatusFilter(e.target.value)}
                  label="Filtrar por Estado"
                >
                  <MenuItem value="">Todos los Estados</MenuItem>
                  <MenuItem value="Scheduled">Programadas</MenuItem>
                  <MenuItem value="Completed">Completadas</MenuItem>
                  <MenuItem value="Cancelled">Canceladas</MenuItem>
                  <MenuItem value="No Show">No Asistieron</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                type="date"
                label="Filtrar por Fecha"
                value={dateFilter}
                onChange={(e) => setDateFilter(e.target.value)}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12} md={2}>
              <Button
                variant="outlined"
                onClick={clearFilters}
                startIcon={<FilterListOutlined />}
                fullWidth
              >
                Limpiar Filtros
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Appointments Table */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            {filteredAppointments.length} Cita{filteredAppointments.length !== 1 ? 's' : ''} Encontrada{filteredAppointments.length !== 1 ? 's' : ''}
          </Typography>
          
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Fecha y Hora</TableCell>
                  <TableCell>Paciente</TableCell>
                  <TableCell>Doctor</TableCell>
                  <TableCell>Motivo</TableCell>
                  <TableCell>Estado</TableCell>
                  <TableCell align="center">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredAppointments.map((appointment) => (
                  <TableRow key={appointment.id}>
                    <TableCell>
                      <Box>
                        <Typography variant="subtitle2">
                          {format(parseISO(appointment.appointmentDate), 'MMM d, yyyy')}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {format(parseISO(`${appointment.appointmentDate}T${appointment.appointmentTime}`), 'h:mm a')}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Box display="flex" alignItems="center">
                        <Avatar sx={{ bgcolor: 'primary.main', mr: 2, width: 32, height: 32 }}>
                          <PersonOutlined fontSize="small" />
                        </Avatar>
                        <Box>
                          <Typography variant="subtitle2">{appointment.patientName}</Typography>
                          <Typography variant="body2" color="text.secondary">
                            ID: {appointment.patientId}
                          </Typography>
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Box display="flex" alignItems="center">
                        <Avatar sx={{ bgcolor: 'success.main', mr: 2, width: 32, height: 32 }}>
                          <LocalHospitalOutlined fontSize="small" />
                        </Avatar>
                        <Box>
                          <Typography variant="subtitle2">Dr. {appointment.doctorName}</Typography>
                          <Typography variant="body2" color="text.secondary">
                            ID: {appointment.doctorId}
                          </Typography>
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{appointment.reason}</Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        icon={getStatusIcon(appointment.status)}
                        label={appointment.status}
                        color={getStatusColor(appointment.status) as any}
                        variant="outlined"
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="center">
                      <Tooltip title="Ver Detalles">
                        <IconButton
                          size="small"
                          onClick={() => handleViewAppointment(appointment)}
                        >
                          <VisibilityOutlined />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Editar Cita">
                        <IconButton
                          size="small"
                          onClick={() => handleEditAppointment(appointment)}
                        >
                          <EditOutlined />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Eliminar Cita">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleDeleteAppointment(appointment.id)}
                        >
                          <DeleteOutlined />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          {filteredAppointments.length === 0 && (
            <Box sx={{ textAlign: 'center', py: 6 }}>
              <CalendarTodayOutlined sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No se encontraron citas
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {searchTerm || statusFilter || dateFilter 
                  ? 'Intenta ajustar tus criterios de búsqueda.' 
                  : 'Aún no hay citas en el sistema.'}
              </Typography>
            </Box>
          )}
        </CardContent>
      </Card>

      {/* View Appointment Dialog */}
      <Dialog open={viewDialogOpen} onClose={() => setViewDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>Detalles de la Cita</DialogTitle>
        <DialogContent>
          {selectedAppointment && (
            <Box sx={{ pt: 2 }}>
              <Grid container spacing={3}>
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" gutterBottom>Información del Paciente</Typography>
                  <Typography><strong>Nombre:</strong> {selectedAppointment.patientName}</Typography>
                  <Typography><strong>ID:</strong> {selectedAppointment.patientId}</Typography>
                </Grid>
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" gutterBottom>Información del Doctor</Typography>
                  <Typography><strong>Nombre:</strong> Dr. {selectedAppointment.doctorName}</Typography>
                  <Typography><strong>ID:</strong> {selectedAppointment.doctorId}</Typography>
                </Grid>
                <Grid item xs={12}>
                  <Divider sx={{ my: 2 }} />
                </Grid>
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" gutterBottom>Detalles de la Cita</Typography>
                  <Typography><strong>Fecha:</strong> {format(parseISO(selectedAppointment.appointmentDate), 'MMMM d, yyyy')}</Typography>
                  <Typography><strong>Hora:</strong> {format(parseISO(`${selectedAppointment.appointmentDate}T${selectedAppointment.appointmentTime}`), 'h:mm a')}</Typography>
                  <Typography><strong>Motivo:</strong> {selectedAppointment.reason}</Typography>
                </Grid>
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" gutterBottom>Estado y Notas</Typography>
                  <Box mb={1}>
                    <Chip
                      icon={getStatusIcon(selectedAppointment.status)}
                      label={selectedAppointment.status}
                      color={getStatusColor(selectedAppointment.status) as any}
                      variant="outlined"
                    />
                  </Box>
                  {selectedAppointment.notes && (
                    <Typography><strong>Notas:</strong> {selectedAppointment.notes}</Typography>
                  )}
                  <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
                    <strong>Creado:</strong> {format(parseISO(selectedAppointment.createdAt), 'MMM d, yyyy at h:mm a')}
                  </Typography>
                </Grid>
              </Grid>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViewDialogOpen(false)}>Cerrar</Button>
        </DialogActions>
      </Dialog>

      {/* Edit Appointment Dialog */}
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Editar Cita</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            {selectedAppointment && (
              <>
                <Typography variant="h6" gutterBottom>
                  {selectedAppointment.patientName} - Dr. {selectedAppointment.doctorName}
                </Typography>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  {format(parseISO(`${selectedAppointment.appointmentDate}T${selectedAppointment.appointmentTime}`), 'MMMM d, yyyy at h:mm a')}
                </Typography>
                
                <Divider sx={{ my: 2 }} />

                <FormControl fullWidth sx={{ mb: 3 }}>
                  <InputLabel>Estado</InputLabel>
                  <Select
                    value={editStatus}
                    onChange={(e) => setEditStatus(e.target.value)}
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
                  value={editNotes}
                  onChange={(e) => setEditNotes(e.target.value)}
                  placeholder="Agregar notas sobre la cita..."
                />
              </>
            )}
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

export default AdminAppointmentManagement;
