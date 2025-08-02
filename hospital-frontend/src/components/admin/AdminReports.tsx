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
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Divider
} from '@mui/material';
import { 
  AssessmentOutlined,
  PeopleOutlined,
  CalendarTodayOutlined,
  LocalHospitalOutlined,
  TrendingUpOutlined,
  DownloadOutlined
} from '@mui/icons-material';
import { format, subDays, startOfMonth, endOfMonth } from 'date-fns';
import apiService from '../../services/apiService.ts';
import { useAuth } from '../../context/AuthContext.tsx';

interface SystemStats {
  totalUsers: number;
  totalPatients: number;
  totalDoctors: number;
  totalAppointments: number;
  scheduledAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
  expiredPrescriptions: number;
}

interface AppointmentReport {
  totalAppointments: number;
  byStatus: { status: string; count: number }[];
  byMonth: { month: string; count: number }[];
}

interface AdminReportsProps {
  onBack?: () => void;
}

const AdminReports: React.FC<AdminReportsProps> = ({ onBack }) => {
  const { user } = useAuth();
  const [stats, setStats] = useState<SystemStats | null>(null);
  const [appointmentReport, setAppointmentReport] = useState<AppointmentReport | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    fetchReportData();
  }, []);

  const fetchReportData = async () => {
    try {
      setLoading(true);
      
      // Fetch system statistics from dashboard endpoint
      const statsResponse = await apiService.get('/admin/dashboard');
      setStats(statsResponse.data);

      // Fetch appointment report data
      const reportResponse = await apiService.get('/admin/reports/appointments');
      setAppointmentReport(reportResponse.data);
      
    } catch (error: any) {
      console.error('Error fetching report data:', error);
      setError('Error al cargar los datos del reporte. Por favor intenta de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  const calculateGrowthRate = () => {
    if (!appointmentReport || !appointmentReport.byMonth || appointmentReport.byMonth.length < 2) return 0;
    const currentMonth = appointmentReport.byMonth[appointmentReport.byMonth.length - 1]?.count || 0;
    const previousMonth = appointmentReport.byMonth[appointmentReport.byMonth.length - 2]?.count || 0;
    if (previousMonth === 0) return 0;
    return ((currentMonth - previousMonth) / previousMonth * 100);
  };

  const handleExportReport = () => {
    if (!stats || !appointmentReport) {
      alert('No hay datos disponibles para exportar.');
      return;
    }

    // Generar datos CSV
    const csvData = [
      ['REPORTE DEL SISTEMA HOSPITALARIO'],
      ['Fecha de Generación:', new Date().toLocaleDateString('es-ES')],
      [''],
      ['ESTADÍSTICAS GENERALES'],
      ['Total de Usuarios', stats.totalUsers],
      ['Pacientes', stats.totalPatients],
      ['Doctores', stats.totalDoctors],
      ['Total de Citas', stats.totalAppointments],
      ['Citas Programadas', stats.scheduledAppointments],
      ['Citas Completadas', stats.completedAppointments],
      ['Citas Canceladas', stats.cancelledAppointments],
      ['Prescripciones Expiradas', stats.expiredPrescriptions],
      [''],
      ['CITAS POR ESTADO']
    ];

    // Agregar datos de citas por estado
    if (appointmentReport.byStatus) {
      appointmentReport.byStatus.forEach(status => {
        csvData.push([status.status, status.count]);
      });
    }

    // Agregar datos mensuales
    if (appointmentReport.byMonth && appointmentReport.byMonth.length > 0) {
      csvData.push(['']);
      csvData.push(['TENDENCIAS MENSUALES']);
      csvData.push(['Mes', 'Cantidad de Citas']);
      appointmentReport.byMonth.forEach(month => {
        csvData.push([month.month, month.count]);
      });
    }

    // Convertir a CSV
    const csvContent = csvData.map(row => 
      row.map(field => 
        typeof field === 'string' && field.includes(',') 
          ? `"${field}"` 
          : field
      ).join(',')
    ).join('\n');

    // Crear y descargar archivo
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', `reporte-hospitalario-${new Date().toISOString().split('T')[0]}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
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

  const growthRate = calculateGrowthRate();

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Box display="flex" alignItems="center" justifyContent="space-between" mb={3}>
        <Box display="flex" alignItems="center">
          <AssessmentOutlined color="primary" sx={{ mr: 2, fontSize: 32 }} />
          <Typography variant="h4" component="h1">
            Reportes y Análisis del Sistema
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<DownloadOutlined />}
          onClick={handleExportReport}
        >
          Exportar Reporte
        </Button>
      </Box>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {stats && (
        <>
          {/* Overview Statistics */}
          <Typography variant="h5" gutterBottom sx={{ mb: 3 }}>
            Resumen del Sistema
          </Typography>
          
          <Grid container spacing={3} sx={{ mb: 4 }}>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent sx={{ textAlign: 'center' }}>
                  <PeopleOutlined color="primary" sx={{ fontSize: 40, mb: 1 }} />
                  <Typography variant="h4" color="primary">{stats.totalUsers}</Typography>
                  <Typography variant="body2" color="text.secondary">Total de Usuarios</Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent sx={{ textAlign: 'center' }}>
                  <PeopleOutlined color="info" sx={{ fontSize: 40, mb: 1 }} />
                  <Typography variant="h4" color="info.main">{stats.totalPatients}</Typography>
                  <Typography variant="body2" color="text.secondary">Pacientes</Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent sx={{ textAlign: 'center' }}>
                  <LocalHospitalOutlined color="success" sx={{ fontSize: 40, mb: 1 }} />
                  <Typography variant="h4" color="success.main">{stats.totalDoctors}</Typography>
                  <Typography variant="body2" color="text.secondary">Doctores</Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent sx={{ textAlign: 'center' }}>
                  <CalendarTodayOutlined color="warning" sx={{ fontSize: 40, mb: 1 }} />
                  <Typography variant="h4" color="warning.main">{stats.totalAppointments}</Typography>
                  <Typography variant="body2" color="text.secondary">Total de Citas</Typography>
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          {/* Appointment Analytics */}
          <Typography variant="h5" gutterBottom sx={{ mb: 3 }}>
            Análisis de Citas
          </Typography>
          
          <Grid container spacing={3} sx={{ mb: 4 }}>
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>Citas Programadas</Typography>
                  <Box display="flex" alignItems="center" mb={2}>
                    <Box>
                      <Typography variant="h4" color="primary">
                        {stats.scheduledAppointments}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Programadas
                      </Typography>
                    </Box>
                    <Box ml={3}>
                      <Box display="flex" alignItems="center">
                        <TrendingUpOutlined 
                          color={growthRate >= 0 ? "success" : "error"} 
                          sx={{ mr: 1 }} 
                        />
                        <Typography 
                          variant="h6" 
                          color={growthRate >= 0 ? "success.main" : "error.main"}
                        >
                          {growthRate >= 0 ? '+' : ''}{growthRate.toFixed(1)}%
                        </Typography>
                      </Box>
                      <Typography variant="body2" color="text.secondary">
                        Tendencia de Crecimiento
                      </Typography>
                    </Box>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
            
            <Grid item xs={12} md={6}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom>Estado de Citas</Typography>
                  <Grid container spacing={2}>
                    <Grid item xs={6}>
                      <Box textAlign="center">
                        <Typography variant="h4" color="success.main">
                          {stats.completedAppointments}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Completadas
                        </Typography>
                      </Box>
                    </Grid>
                    <Grid item xs={6}>
                      <Box textAlign="center">
                        <Typography variant="h4" color="error.main">
                          {stats.cancelledAppointments}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Canceladas
                        </Typography>
                      </Box>
                    </Grid>
                  </Grid>
                  <Divider sx={{ my: 2 }} />
                  <Typography variant="body2" color="text.secondary" textAlign="center">
                    Tasa de Completación: {stats.totalAppointments > 0 
                      ? ((stats.completedAppointments / stats.totalAppointments) * 100).toFixed(1)
                      : 0}%
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          {/* Monthly Trends */}
          {appointmentReport && appointmentReport.byMonth && appointmentReport.byMonth.length > 0 && (
            <>
              <Typography variant="h5" gutterBottom sx={{ mb: 3 }}>
                Tendencias Mensuales
              </Typography>
              
              <Card sx={{ mb: 4 }}>
                <CardContent>
                  <TableContainer>
                    <Table>
                      <TableHead>
                        <TableRow>
                          <TableCell><strong>Mes</strong></TableCell>
                          <TableCell align="center"><strong>Citas</strong></TableCell>
                          <TableCell align="center"><strong>Crecimiento</strong></TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {appointmentReport.byMonth.map((data, index) => {
                          const prevMonthAppointments = index > 0 ? appointmentReport.byMonth[index - 1].count : 0;
                          const monthGrowth = prevMonthAppointments > 0 
                            ? ((data.count - prevMonthAppointments) / prevMonthAppointments * 100)
                            : 0;
                          
                          return (
                            <TableRow key={data.month}>
                              <TableCell>{data.month}</TableCell>
                              <TableCell align="center">{data.count}</TableCell>
                              <TableCell align="center">
                                <Typography 
                                  color={monthGrowth >= 0 ? "success.main" : "error.main"}
                                >
                                  {monthGrowth >= 0 ? '+' : ''}{monthGrowth.toFixed(1)}%
                                </Typography>
                              </TableCell>
                            </TableRow>
                          );
                        })}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </CardContent>
              </Card>
            </>
          )}

          {/* Quick Insights */}
          <Typography variant="h5" gutterBottom sx={{ mb: 3 }}>
            Perspectivas Rápidas
          </Typography>
          
          <Grid container spacing={3} sx={{ mb: 4 }}>
            <Grid item xs={12} md={4}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom color="primary">
                    Estado del Sistema
                  </Typography>
                  <Typography variant="body2" paragraph>
                    El sistema opera normalmente con {stats.totalUsers} usuarios activos 
                    y {stats.totalAppointments} citas totales procesadas.
                  </Typography>
                  <Typography variant="body2" color="success.main">
                    ✓ Todos los sistemas operativos
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
            
            <Grid item xs={12} md={4}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom color="success">
                    Rendimiento
                  </Typography>
                  <Typography variant="body2" paragraph>
                    La tasa de completación de citas está en{' '}
                    {stats.totalAppointments > 0 
                      ? ((stats.completedAppointments / stats.totalAppointments) * 100).toFixed(1)
                      : 0}%, 
                    indicando buena satisfacción del paciente y disponibilidad del doctor.
                  </Typography>
                  <Typography variant="body2" color="success.main">
                    ✓ Rendimiento por encima del promedio
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
            
            <Grid item xs={12} md={4}>
              <Card>
                <CardContent>
                  <Typography variant="h6" gutterBottom color="info">
                    Tendencia de Crecimiento
                  </Typography>
                  <Typography variant="body2" paragraph>
                    {growthRate >= 0 
                      ? `Las citas han aumentado un ${growthRate.toFixed(1)}% este mes, mostrando crecimiento positivo.`
                      : `Las citas han disminuido un ${Math.abs(growthRate).toFixed(1)}% este mes. Considera revisar los procesos de programación.`
                    }
                  </Typography>
                  <Typography variant="body2" color={growthRate >= 0 ? "success.main" : "warning.main"}>
                    {growthRate >= 0 ? '✓ Crecimiento positivo' : '⚠ Necesita atención'}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </>
      )}

      <Box mt={4} display="flex" justifyContent="flex-start">
        <Button variant="outlined" onClick={handleBack}>
          Volver al Dashboard
        </Button>
      </Box>
    </Container>
  );
};

export default AdminReports;
