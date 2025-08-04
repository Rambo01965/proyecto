import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Grid,
  Alert,
  CircularProgress,
  Avatar,
  Chip,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem
} from '@mui/material';
import { 
  PersonOutlined, 
  SearchOutlined, 
  PhoneOutlined,
  LocalHospitalOutlined,
  CalendarTodayOutlined
} from '@mui/icons-material';
import apiService from '../../services/apiService.ts';

interface Doctor {
  id: number;
  name: string;
  specialty: string;
  phone: string;
  userId: number;
}

interface DoctorSearchProps {
  onBack?: () => void;
}

const DoctorSearch: React.FC<DoctorSearchProps> = ({ onBack }) => {
  const navigate = useNavigate();
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [filteredDoctors, setFilteredDoctors] = useState<Doctor[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [selectedSpecialty, setSelectedSpecialty] = useState<string>('');
  const [specialties, setSpecialties] = useState<string[]>([]);

  useEffect(() => {
    fetchDoctors();
  }, []);

  useEffect(() => {
    filterDoctors();
  }, [doctors, searchTerm, selectedSpecialty]);

  const fetchDoctors = async () => {
    try {
      setLoading(true);
      const response = await apiService.get('/doctors');
      setDoctors(response.data);
      
      // Extract unique specialties
      const uniqueSpecialties = [...new Set(response.data.map((doctor: Doctor) => doctor.specialty))] as string[];
      setSpecialties(uniqueSpecialties);
    } catch (error: any) {
      console.error('Error fetching doctors:', error);
      setError('Error al cargar doctores. Por favor intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  const filterDoctors = () => {
    let filtered = doctors;

    // Filter by search term (name)
    if (searchTerm.trim()) {
      filtered = filtered.filter(doctor =>
        doctor.name.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Filter by specialty
    if (selectedSpecialty) {
      filtered = filtered.filter(doctor => doctor.specialty === selectedSpecialty);
    }

    setFilteredDoctors(filtered);
  };

  const handleBookAppointment = (doctorId: number) => {
    // Navigate to appointment booking with pre-selected doctor
    navigate('/book-appointment', { state: { selectedDoctorId: doctorId } });
  };

  const handleBack = () => {
    if (onBack) {
      onBack();
    } else {
      window.history.back();
    }
  };

  const clearFilters = () => {
    setSearchTerm('');
    setSelectedSpecialty('');
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
        <PersonOutlined color="primary" sx={{ mr: 2, fontSize: 32 }} />
        <Typography variant="h4" component="h1">
          Buscar Doctores
        </Typography>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Search and Filter Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={3} alignItems="center">
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Buscar por Nombre del Doctor"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchOutlined />
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid item xs={12} md={4}>
              <FormControl fullWidth>
                <InputLabel>Filtrar por Especialidad</InputLabel>
                <Select
                  value={selectedSpecialty}
                  onChange={(e) => setSelectedSpecialty(e.target.value)}
                  label="Filtrar por Especialidad"
                >
                  <MenuItem value="">Todas las Especialidades</MenuItem>
                  {specialties.map((specialty) => (
                    <MenuItem key={specialty} value={specialty}>
                      {specialty}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={4}>
              <Button
                variant="outlined"
                onClick={clearFilters}
                fullWidth
              >
                Limpiar Filtros
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Results Section */}
      <Typography variant="h6" gutterBottom>
        {filteredDoctors.length} Doctor{filteredDoctors.length !== 1 ? 'es' : ''} Encontrado{filteredDoctors.length !== 1 ? 's' : ''}
      </Typography>

      {filteredDoctors.length === 0 ? (
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <PersonOutlined sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              No se encontraron doctores
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Intenta ajustar tus criterios de búsqueda o limpiar los filtros.
            </Typography>
            <Button variant="outlined" onClick={clearFilters}>
              Limpiar Todos los Filtros
            </Button>
          </CardContent>
        </Card>
      ) : (
        <Grid container spacing={3}>
          {filteredDoctors.map((doctor) => (
            <Grid item xs={12} md={6} lg={4} key={doctor.id}>
              <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                <CardContent sx={{ flexGrow: 1 }}>
                  <Box display="flex" alignItems="center" mb={2}>
                    <Avatar sx={{ bgcolor: 'primary.main', mr: 2 }}>
                      <LocalHospitalOutlined />
                    </Avatar>
                    <Box>
                      <Typography variant="h6" component="h2">
                        Dr. {doctor.name}
                      </Typography>
                      <Chip
                        label={doctor.specialty}
                        size="small"
                        color="primary"
                        variant="outlined"
                      />
                    </Box>
                  </Box>

                  <Box display="flex" alignItems="center" mb={2}>
                    <PhoneOutlined sx={{ mr: 1, color: 'text.secondary', fontSize: 20 }} />
                    <Typography variant="body2" color="text.secondary">
                      {doctor.phone || 'Teléfono no disponible'}
                    </Typography>
                  </Box>

                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    Especializado en {doctor.specialty.toLowerCase()} con años de experiencia 
                    brindando servicios de atención médica de calidad.
                  </Typography>
                </CardContent>

                <CardContent sx={{ pt: 0 }}>
                  <Button
                    variant="contained"
                    fullWidth
                    startIcon={<CalendarTodayOutlined />}
                    onClick={() => handleBookAppointment(doctor.id)}
                  >
                    Reservar Cita
                  </Button>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}

      <Box mt={4} display="flex" justifyContent="flex-start">
        <Button variant="outlined" onClick={handleBack}>
          Volver al Dashboard
        </Button>
      </Box>
    </Container>
  );
};

export default DoctorSearch;
