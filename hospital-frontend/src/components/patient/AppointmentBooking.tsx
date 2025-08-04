import React, { useState, useEffect } from 'react';
import { useLocation } from 'react-router-dom';
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Grid,
  MenuItem,
  Alert,
  CircularProgress,
  FormControl,
  InputLabel,
  Select
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { CalendarTodayOutlined } from '@mui/icons-material';
import apiService from '../../services/apiService.ts';
import { useAuth } from '../../context/AuthContext.tsx';

interface Doctor {
  id: number;
  name: string;
  specialty: string;
  phone: string;
}

interface AppointmentData {
  doctorId: number;
  date: string;
  time: string;
  reason: string;
}

interface AppointmentBookingProps {
  onBack?: () => void;
}

const AppointmentBooking: React.FC<AppointmentBookingProps> = ({ onBack }) => {
  const { user } = useAuth();
  const location = useLocation();
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [selectedDoctor, setSelectedDoctor] = useState<number | ''>('');
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const [selectedTime, setSelectedTime] = useState<string>('');
  const [reason, setReason] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [success, setSuccess] = useState<string>('');
  const [error, setError] = useState<string>('');

  // Available time slots
  const timeSlots = [
    '09:00', '09:30', '10:00', '10:30', '11:00', '11:30',
    '14:00', '14:30', '15:00', '15:30', '16:00', '16:30', '17:00'
  ];

  useEffect(() => {
    fetchDoctors();
  }, []);

  useEffect(() => {
    // Check if a doctor was preselected from navigation
    const state = location.state as { selectedDoctorId?: number };
    if (state?.selectedDoctorId) {
      setSelectedDoctor(state.selectedDoctorId);
    }
  }, [location.state, doctors]);

  const fetchDoctors = async () => {
    try {
      const response = await apiService.get('/doctors');
      setDoctors(response.data);
    } catch (error) {
      console.error('Error fetching doctors:', error);
      setError('Error al cargar doctores. Por favor intenta de nuevo.');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!selectedDoctor || !selectedDate || !selectedTime || !reason.trim()) {
      setError('Por favor completa todos los campos');
      return;
    }

    setLoading(true);
    setError('');
    setSuccess('');

    try {
      const appointmentData: AppointmentData = {
        doctorId: selectedDoctor as number,
        date: selectedDate.toISOString().split('T')[0],
        time: selectedTime,
        reason: reason.trim()
      };

      await apiService.post('/appointments/patient', appointmentData);
      
      setSuccess('¡Cita reservada exitosamente!');
      // Reset form
      setSelectedDoctor('');
      setSelectedDate(null);
      setSelectedTime('');
      setReason('');
    } catch (error: any) {
      console.error('Error booking appointment:', error);
      setError(error.response?.data?.message || 'Error al reservar la cita. Por favor intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  const handleBack = () => {
    if (onBack) {
      onBack();
    } else {
      window.history.back();
    }
  };

  return (
    <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
      <Box display="flex" alignItems="center" mb={3}>
        <CalendarTodayOutlined color="primary" sx={{ mr: 2, fontSize: 32 }} />
        <Typography variant="h4" component="h1">
          Reservar Nueva Cita
        </Typography>
      </Box>

      <Card>
        <CardContent>
          <form onSubmit={handleSubmit}>
            <Grid container spacing={3}>
              {/* Doctor Selection */}
              <Grid item xs={12}>
                <FormControl fullWidth>
                  <InputLabel>Seleccionar Doctor</InputLabel>
                  <Select
                    value={selectedDoctor}
                    onChange={(e) => setSelectedDoctor(e.target.value as number)}
                    label="Seleccionar Doctor"
                  >
                    {doctors.map((doctor) => (
                      <MenuItem key={doctor.id} value={doctor.id}>
                        Dr. {doctor.name} - {doctor.specialty}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              {/* Date Selection */}
              <Grid item xs={12} md={6}>
                <LocalizationProvider dateAdapter={AdapterDateFns}>
                  <DatePicker
                    label="Fecha de la Cita"
                    value={selectedDate}
                    onChange={(newValue) => setSelectedDate(newValue)}
                    minDate={new Date()}
                    slotProps={{ textField: { fullWidth: true } }}
                  />
                </LocalizationProvider>
              </Grid>

              {/* Time Selection */}
              <Grid item xs={12} md={6}>
                <FormControl fullWidth>
                  <InputLabel>Seleccionar Hora</InputLabel>
                  <Select
                    value={selectedTime}
                    onChange={(e) => setSelectedTime(e.target.value)}
                    label="Seleccionar Hora"
                  >
                    {timeSlots.map((time) => (
                      <MenuItem key={time} value={time}>
                        {time}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              {/* Reason */}
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Motivo de la Visita"
                  multiline
                  rows={3}
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  placeholder="Por favor describe el motivo de tu cita..."
                />
              </Grid>

              {/* Error/Success Messages */}
              {error && (
                <Grid item xs={12}>
                  <Alert severity="error">{error}</Alert>
                </Grid>
              )}

              {success && (
                <Grid item xs={12}>
                  <Alert severity="success">{success}</Alert>
                </Grid>
              )}

              {/* Action Buttons */}
              <Grid item xs={12}>
                <Box display="flex" gap={2} justifyContent="flex-end">
                  <Button
                    variant="outlined"
                    onClick={handleBack}
                    disabled={loading}
                  >
                    Volver
                  </Button>
                  <Button
                    type="submit"
                    variant="contained"
                    disabled={loading}
                    startIcon={loading ? <CircularProgress size={20} /> : null}
                  >
                    {loading ? 'Reservando...' : 'Reservar Cita'}
                  </Button>
                </Box>
              </Grid>
            </Grid>
          </form>
        </CardContent>
      </Card>
    </Container>
  );
};

export default AppointmentBooking;
