import React, { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  Button,
  Grid,
  Alert,
  CircularProgress,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle
} from '@mui/material';
import { 
  CalendarTodayOutlined, 
  PersonOutlined, 
  AccessTimeOutlined,
  CancelOutlined,
  CheckCircleOutlined 
} from '@mui/icons-material';
import apiService from '../../services/apiService.ts';
import { useAuth } from '../../context/AuthContext.tsx';

interface Appointment {
  id: number;
  doctorName: string;
  doctorSpecialty: string;
  date: string;
  time: string;
  status: string;
  reason: string;
  createdAt: string;
}

interface AppointmentListProps {
  onBack?: () => void;
}

const AppointmentList: React.FC<AppointmentListProps> = ({ onBack }) => {
  const { user } = useAuth();
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [cancelDialog, setCancelDialog] = useState<{ open: boolean; appointmentId: number | null }>({
    open: false,
    appointmentId: null
  });
  const [cancelling, setCancelling] = useState<boolean>(false);

  useEffect(() => {
    fetchAppointments();
  }, []);

  const fetchAppointments = async () => {
    try {
      setLoading(true);
      
      if (!user?.id) {
        setError('Usuario no autenticado');
        return;
      }
      
      const response = await apiService.get(`/appointments/patient/${user.id}`);
      setAppointments(response.data);
    } catch (error: any) {
      console.error('Error fetching appointments:', error);
      setError('Error al cargar las citas. Por favor intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'scheduled':
        return 'primary';
      case 'completed':
        return 'success';
      case 'cancelled':
        return 'error';
      default:
        return 'default';
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'scheduled':
        return <AccessTimeOutlined fontSize="small" />;
      case 'completed':
        return <CheckCircleOutlined fontSize="small" />;
      case 'cancelled':
        return <CancelOutlined fontSize="small" />;
      default:
        return null;
    }
  };

  const handleCancelAppointment = async () => {
    if (!cancelDialog.appointmentId) return;

    setCancelling(true);
    try {
      await apiService.put(`/appointments/${cancelDialog.appointmentId}/cancel`);
      await fetchAppointments(); // Refresh the list
      setCancelDialog({ open: false, appointmentId: null });
    } catch (error: any) {
      console.error('Error cancelling appointment:', error);
      setError('Error al cancelar la cita. Por favor intenta de nuevo.');
    } finally {
      setCancelling(false);
    }
  };

  const openCancelDialog = (appointmentId: number) => {
    setCancelDialog({ open: true, appointmentId });
  };

  const closeCancelDialog = () => {
    setCancelDialog({ open: false, appointmentId: null });
  };

  const handleBack = () => {
    if (onBack) {
      onBack();
    } else {
      window.history.back();
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
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
        <CalendarTodayOutlined color="primary" sx={{ mr: 2, fontSize: 32 }} />
        <Typography variant="h4" component="h1">
          Mis Citas
        </Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {appointments.length === 0 ? (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <CalendarTodayOutlined sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              No se encontraron citas
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Aún no has programado ninguna cita. ¡Reserva tu primera cita para comenzar!
            </Typography>
            <Button variant="contained" onClick={handleBack}>
              Reserva tu primera cita
            </Button>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardContent>
            <TableContainer component={Paper} elevation={0}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Doctor</TableCell>
                    <TableCell>Fecha y Hora</TableCell>
                    <TableCell>Motivo</TableCell>
                    <TableCell>Estado</TableCell>
                    <TableCell>Acciones</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {appointments.map((appointment) => (
                    <TableRow key={appointment.id} hover>
                      <TableCell>
                        <Box display="flex" alignItems="center">
                          <PersonOutlined sx={{ mr: 1, color: 'text.secondary' }} />
                          <Box>
                            <Typography variant="subtitle2">
                              Dr. {appointment.doctorName}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                              {appointment.doctorSpecialty}
                            </Typography>
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Box>
                          <Typography variant="body2">
                            {formatDate(appointment.date)}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">
                            {appointment.time}
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" sx={{ maxWidth: 200 }}>
                          {appointment.reason}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          icon={getStatusIcon(appointment.status)}
                          label={appointment.status}
                          color={getStatusColor(appointment.status) as any}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        {appointment.status.toLowerCase() === 'scheduled' && (
                          <Button
                            size="small"
                            color="error"
                            onClick={() => openCancelDialog(appointment.id)}
                          >
                            Cancelar
                          </Button>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      )}

      <Box mt={3} display="flex" justifyContent="flex-start">
        <Button variant="outlined" onClick={handleBack}>
          Volver al Dashboard
        </Button>
      </Box>

      {/* Cancel Confirmation Dialog */}
      <Dialog
        open={cancelDialog.open}
        onClose={closeCancelDialog}
        aria-labelledby="cancel-dialog-title"
      >
        <DialogTitle id="cancel-dialog-title">
          Cancelar Cita
        </DialogTitle>
        <DialogContent>
          <DialogContentText>
            ¿Estás seguro de que quieres cancelar esta cita? Esta acción no se puede deshacer.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeCancelDialog} disabled={cancelling}>
            Mantener Cita
          </Button>
          <Button
            onClick={handleCancelAppointment}
            color="error"
            disabled={cancelling}
            startIcon={cancelling ? <CircularProgress size={16} /> : null}
          >
            {cancelling ? 'Cancelando...' : 'Cancelar Cita'}
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default AppointmentList;
