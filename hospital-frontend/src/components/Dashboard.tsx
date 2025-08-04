import React, { useState } from 'react';
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  Grid,
  Button,
  AppBar,
  Toolbar,
  IconButton
} from '@mui/material';
import { LogoutOutlined, PersonOutlined, LocalHospitalOutlined, CalendarTodayOutlined } from '@mui/icons-material';
import { useAuth } from '../context/AuthContext.tsx';
import { UserRole } from '../types/index.ts';
import AppointmentBooking from './patient/AppointmentBooking.tsx';
import AppointmentList from './patient/AppointmentList.tsx';
import DoctorSearch from './patient/DoctorSearch.tsx';
import DoctorSchedule from './doctor/DoctorSchedule.tsx';
import DoctorPatients from './doctor/DoctorPatients.tsx';
import AdminUserManagement from './admin/AdminUserManagement.tsx';
import AdminAppointmentManagement from './admin/AdminAppointmentManagement.tsx';
import AdminReports from './admin/AdminReports.tsx';

type ViewType = 'dashboard' | 'book-appointment' | 'view-appointments' | 'search-doctors' | 'doctor-schedule' | 'doctor-patients' | 'admin-users' | 'admin-appointments' | 'admin-reports';

const Dashboard: React.FC = () => {
  const { user, logout } = useAuth();
  const [currentView, setCurrentView] = useState<ViewType>('dashboard');

  const handleLogout = () => {
    logout();
    window.location.href = '/login';
  };

  const handleBookAppointment = () => {
    setCurrentView('book-appointment');
  };

  const handleViewAppointments = () => {
    setCurrentView('view-appointments');
  };

  const handleSearchDoctors = () => {
    setCurrentView('search-doctors');
  };

  const handleBackToDashboard = () => {
    setCurrentView('dashboard');
  };

  const handleViewSchedule = () => {
    setCurrentView('doctor-schedule');
  };

  const handleManagePatients = () => {
    setCurrentView('doctor-patients');
  };

  const handleManageUsers = () => {
    setCurrentView('admin-users');
  };

  const handleViewAllAppointments = () => {
    setCurrentView('admin-appointments');
  };

  const handleGenerateReports = () => {
    setCurrentView('admin-reports');
  };

  const getRoleDisplayName = (role: UserRole) => {
    switch (role) {
      case UserRole.Patient:
        return 'Paciente';
      case UserRole.Doctor:
        return 'Doctor';
      case UserRole.Admin:
        return 'Administrador';
      default:
        return 'Usuario';
    }
  };

  const getPatientActions = () => (
    <Grid container spacing={3}>
      <Grid item xs={12} md={6}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" mb={2}>
              <CalendarTodayOutlined color="primary" sx={{ mr: 1 }} />
              <Typography variant="h6">Reservar Cita</Typography>
            </Box>
            <Typography variant="body2" color="text.secondary" mb={2}>
              Programa una nueva cita con doctores disponibles
            </Typography>
            <Button variant="contained" fullWidth onClick={handleBookAppointment}>
              Reservar Ahora
            </Button>
          </CardContent>
        </Card>
      </Grid>
      <Grid item xs={12} md={6}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" mb={2}>
              <LocalHospitalOutlined color="primary" sx={{ mr: 1 }} />
              <Typography variant="h6">Mis Citas</Typography>
            </Box>
            <Typography variant="body2" color="text.secondary" mb={2}>
              Ver y gestionar tus citas programadas
            </Typography>
            <Button variant="outlined" fullWidth onClick={handleViewAppointments}>
              Ver Citas
            </Button>
          </CardContent>
        </Card>
      </Grid>
      <Grid item xs={12} md={6}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" mb={2}>
              <PersonOutlined color="primary" sx={{ mr: 1 }} />
              <Typography variant="h6">Buscar Doctores</Typography>
            </Box>
            <Typography variant="body2" color="text.secondary" mb={2}>
              Buscar doctores por especialidad
            </Typography>
            <Button variant="outlined" fullWidth onClick={handleSearchDoctors}>
              Buscar Doctores
            </Button>
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );

  const getDoctorActions = () => (
    <Grid container spacing={3}>
      <Grid item xs={12} md={6}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" mb={2}>
              <CalendarTodayOutlined color="primary" sx={{ mr: 1 }} />
              <Typography variant="h6">Mi Horario</Typography>
            </Box>
            <Typography variant="body2" color="text.secondary" mb={2}>
              Ver y gestionar tu horario de citas
            </Typography>
            <Button variant="contained" fullWidth onClick={handleViewSchedule}>
              Ver Horario
            </Button>
          </CardContent>
        </Card>
      </Grid>
      <Grid item xs={12} md={6}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" mb={2}>
              <LocalHospitalOutlined color="primary" sx={{ mr: 1 }} />
              <Typography variant="h6">Citas de Pacientes</Typography>
            </Box>
            <Typography variant="body2" color="text.secondary" mb={2}>
              Gestionar citas y consultas de pacientes
            </Typography>
            <Button variant="outlined" fullWidth onClick={handleManagePatients}>
              Ver Pacientes
            </Button>
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );

  const getAdminActions = () => (
    <Grid container spacing={3}>
      <Grid item xs={12} md={4}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" mb={2}>
              <PersonOutlined color="primary" sx={{ mr: 1 }} />
              <Typography variant="h6">Gestión de Usuarios</Typography>
            </Box>
            <Typography variant="body2" color="text.secondary" mb={2}>
              Gestionar usuarios, doctores y pacientes
            </Typography>
            <Button variant="contained" fullWidth onClick={handleManageUsers}>
              Gestionar Usuarios
            </Button>
          </CardContent>
        </Card>
      </Grid>
      <Grid item xs={12} md={4}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" mb={2}>
              <CalendarTodayOutlined color="primary" sx={{ mr: 1 }} />
              <Typography variant="h6">Citas Médicas</Typography>
            </Box>
            <Typography variant="body2" color="text.secondary" mb={2}>
              Ver y gestionar todas las citas
            </Typography>
            <Button variant="outlined" fullWidth onClick={handleViewAllAppointments}>
              Ver Todas
            </Button>
          </CardContent>
        </Card>
      </Grid>
      <Grid item xs={12} md={4}>
        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" mb={2}>
              <LocalHospitalOutlined color="primary" sx={{ mr: 1 }} />
              <Typography variant="h6">Reportes</Typography>
            </Box>
            <Typography variant="body2" color="text.secondary" mb={2}>
              Generar reportes y análisis del sistema
            </Typography>
            <Button variant="outlined" fullWidth onClick={handleGenerateReports}>
              Generar Reportes
            </Button>
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );

  const renderActions = () => {
    // Handle both string and number role values
    const userRole = user?.role;
    if (userRole === UserRole.Patient || userRole === 'Patient' || userRole === 1) {
      return getPatientActions();
    } else if (userRole === UserRole.Doctor || userRole === 'Doctor' || userRole === 2) {
      return getDoctorActions();
    } else if (userRole === UserRole.Admin || userRole === 'Admin' || userRole === 3) {
      return getAdminActions();
    } else {
      return null;
    }
  };

  // Render different views based on currentView state
  const renderCurrentView = () => {
    switch (currentView) {
      case 'book-appointment':
        return <AppointmentBooking onBack={() => setCurrentView('dashboard')} />;
      case 'view-appointments':
        return <AppointmentList onBack={() => setCurrentView('dashboard')} />;
      case 'search-doctors':
        return <DoctorSearch onBack={() => setCurrentView('dashboard')} />;
      case 'doctor-schedule':
        return <DoctorSchedule onBack={() => setCurrentView('dashboard')} />;
      case 'doctor-patients':
        return <DoctorPatients onBack={() => setCurrentView('dashboard')} />;
      case 'admin-users':
        return <AdminUserManagement onBack={() => setCurrentView('dashboard')} />;
      case 'admin-appointments':
        return <AdminAppointmentManagement onBack={() => setCurrentView('dashboard')} />;
      case 'admin-reports':
        return <AdminReports onBack={() => setCurrentView('dashboard')} />;
      case 'dashboard':
      default:
        return (
          <>
            <AppBar position="static">
              <Toolbar>
                <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
                  Sistema de Gestión Hospitalaria
                </Typography>
                <Typography variant="body1" sx={{ mr: 2 }}>
                  Bienvenido, {user?.name}
                </Typography>
                <IconButton color="inherit" onClick={handleLogout}>
                  <LogoutOutlined />
                </IconButton>
              </Toolbar>
            </AppBar>

            <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
              <Box mb={4}>
                <Typography variant="h4" gutterBottom>
                  Panel de Control
                </Typography>
                <Typography variant="h6" color="text.secondary">
                  Portal de {getRoleDisplayName(user?.role || UserRole.Patient)}
                </Typography>
              </Box>

              {renderActions()}
            </Container>
          </>
        );
    }
  };

  return renderCurrentView();
};

export default Dashboard;
