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
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Tabs,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Tooltip
} from '@mui/material';
import { 
  PeopleOutlined, 
  SearchOutlined,
  PersonOutlined,
  LocalHospitalOutlined,
  AdminPanelSettingsOutlined,
  AddOutlined,
  EditOutlined,
  DeleteOutlined,
  EmailOutlined,
  PhoneOutlined
} from '@mui/icons-material';
import { format, parseISO } from 'date-fns';
import apiService from '../../services/apiService.ts';
import { useAuth } from '../../context/AuthContext.tsx';

interface User {
  id: number;
  name: string;
  email: string;
  role: 'Patient' | 'Doctor' | 'Admin'; // After transformation, always string
  phone?: string;
  createdAt: string;
  isActive?: boolean; // Optional since backend might not send it
}

interface AdminUserManagementProps {
  onBack?: () => void;
}

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel(props: TabPanelProps) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`user-tabpanel-${index}`}
      aria-labelledby={`user-tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

// Role conversion functions
const convertRoleNumberToString = (role: number | string): 'Patient' | 'Doctor' | 'Admin' => {
  if (typeof role === 'string') return role as 'Patient' | 'Doctor' | 'Admin';
  switch (role) {
    case 1: return 'Patient';
    case 2: return 'Doctor';
    case 3: return 'Admin';
    default: return 'Patient';
  }
};

const convertRoleStringToNumber = (role: string): number => {
  switch (role) {
    case 'Patient': return 1;
    case 'Doctor': return 2;
    case 'Admin': return 3;
    default: return 1;
  }
};

const AdminUserManagement: React.FC<AdminUserManagementProps> = ({ onBack }) => {
  const { user } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [filteredUsers, setFilteredUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [selectedRole, setSelectedRole] = useState<string>('');
  const [tabValue, setTabValue] = useState<number>(0);
  const [editDialogOpen, setEditDialogOpen] = useState<boolean>(false);
  const [addDialogOpen, setAddDialogOpen] = useState<boolean>(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    role: 'Patient',
    phone: ''
  });

  useEffect(() => {
    fetchUsers();
  }, []);

  useEffect(() => {
    filterUsers();
  }, [users, searchTerm, selectedRole, tabValue]);

  const fetchUsers = async () => {
    try {
      setLoading(true);
      const response = await apiService.get('/admin/users');
      
      // Transform backend data to match frontend expectations
      const transformedUsers = response.data.map((user: any) => ({
        ...user,
        role: convertRoleNumberToString(user.role),
        isActive: user.isActive ?? true, // Default to true if not provided
        phone: user.phone || '' // Default to empty string if not provided
      }));
      
      setUsers(transformedUsers);
    } catch (error: any) {
      console.error('Error fetching users:', error);
      setError('Error al cargar usuarios. Por favor, inténtelo de nuevo.');
    } finally {
      setLoading(false);
    }
  };

  const filterUsers = () => {
    let filtered = users;

    // Filter by tab (role)
    const roleFilter = tabValue === 0 ? 'Patient' : tabValue === 1 ? 'Doctor' : 'Admin';
    filtered = filtered.filter(user => user.role === roleFilter);

    // Filter by search term
    if (searchTerm.trim()) {
      filtered = filtered.filter(user =>
        user.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        user.email.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    setFilteredUsers(filtered);
  };

  const handleEditUser = (user: User) => {
    setSelectedUser(user);
    setFormData({
      name: user.name,
      email: user.email,
      role: user.role,
      phone: user.phone || ''
    });
    setEditDialogOpen(true);
  };

  const handleAddUser = () => {
    setFormData({
      name: '',
      email: '',
      role: 'Patient',
      phone: ''
    });
    setAddDialogOpen(true);
  };

  const handleSaveUser = async () => {
    try {
      // Convert role to number for backend
      const dataToSend = {
        ...formData,
        role: convertRoleStringToNumber(formData.role)
      };
      
      if (selectedUser) {
        // Update existing user
        await apiService.put(`/admin/users/${selectedUser.id}`, dataToSend);
      } else {
        // Create new user - add password field for new users
        const createData = {
          ...dataToSend,
          password: 'TempPassword123!' // Default password - should be changed on first login
        };
        await apiService.post('/admin/users', createData);
      }
      
      fetchUsers(); // Refresh the list
      setEditDialogOpen(false);
      setAddDialogOpen(false);
      setSelectedUser(null);
    } catch (error: any) {
      console.error('Error saving user:', error);
      setError('Error al guardar usuario. Por favor, inténtelo de nuevo.');
    }
  };

  const handleDeleteUser = async (userId: number) => {
    if (window.confirm('Are you sure you want to delete this user?')) {
      try {
        await apiService.delete(`/admin/users/${userId}`);
        fetchUsers(); // Refresh the list
      } catch (error: any) {
        console.error('Error deleting user:', error);
        setError('Error al eliminar usuario. Por favor, inténtelo de nuevo.');
      }
    }
  };

  const getRoleIcon = (role: string) => {
    switch (role) {
      case 'Patient': return <PersonOutlined />;
      case 'Doctor': return <LocalHospitalOutlined />;
      case 'Admin': return <AdminPanelSettingsOutlined />;
      default: return <PersonOutlined />;
    }
  };

  const getRoleColor = (role: string) => {
    switch (role) {
      case 'Patient': return 'primary';
      case 'Doctor': return 'success';
      case 'Admin': return 'error';
      default: return 'default';
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
      <Box display="flex" alignItems="center" justifyContent="space-between" mb={3}>
        <Box display="flex" alignItems="center">
          <PeopleOutlined color="primary" sx={{ mr: 2, fontSize: 32 }} />
          <Typography variant="h4" component="h1">
            Gestión de Usuarios
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddOutlined />}
          onClick={handleAddUser}
        >
          Agregar Nuevo Usuario
        </Button>
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
            label="Buscar Usuarios"
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

      {/* Tabs for different user types */}
      <Card>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={tabValue} onChange={(e, newValue) => setTabValue(newValue)}>
            <Tab label={`Pacientes (${users.filter(u => u.role === 'Patient').length})`} />
            <Tab label={`Doctores (${users.filter(u => u.role === 'Doctor').length})`} />
            <Tab label={`Administradores (${users.filter(u => u.role === 'Admin').length})`} />
          </Tabs>
        </Box>

        {/* Users Table */}
        <TabPanel value={tabValue} index={tabValue}>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Usuario</TableCell>
                  <TableCell>Contacto</TableCell>
                  <TableCell>Rol</TableCell>
                  <TableCell>Creado</TableCell>
                  <TableCell>Estado</TableCell>
                  <TableCell align="center">Acciones</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredUsers.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell>
                      <Box display="flex" alignItems="center">
                        <Avatar sx={{ bgcolor: getRoleColor(user.role) + '.main', mr: 2 }}>
                          {getRoleIcon(user.role)}
                        </Avatar>
                        <Box>
                          <Typography variant="subtitle2">{user.name}</Typography>
                          <Typography variant="body2" color="text.secondary">
                            ID: {user.id}
                          </Typography>
                        </Box>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Box>
                        <Box display="flex" alignItems="center" mb={0.5}>
                          <EmailOutlined sx={{ mr: 1, fontSize: 16, color: 'text.secondary' }} />
                          <Typography variant="body2">{user.email}</Typography>
                        </Box>
                        {user.phone && (
                          <Box display="flex" alignItems="center">
                            <PhoneOutlined sx={{ mr: 1, fontSize: 16, color: 'text.secondary' }} />
                            <Typography variant="body2">{user.phone}</Typography>
                          </Box>
                        )}
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Chip
                        icon={getRoleIcon(user.role)}
                        label={user.role}
                        color={getRoleColor(user.role) as any}
                        variant="outlined"
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {format(parseISO(user.createdAt), 'MMM d, yyyy')}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={user.isActive ? 'Activo' : 'Inactivo'}
                        color={user.isActive ? 'success' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="center">
                      <Tooltip title="Editar Usuario">
                        <IconButton
                          size="small"
                          onClick={() => handleEditUser(user)}
                        >
                          <EditOutlined />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Eliminar Usuario">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleDeleteUser(user.id)}
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

          {filteredUsers.length === 0 && (
            <Box sx={{ textAlign: 'center', py: 6 }}>
              <PeopleOutlined sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No se encontraron usuarios
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {searchTerm ? 'Intente ajustar sus criterios de búsqueda.' : 'No hay usuarios en esta categoría aún.'}
              </Typography>
            </Box>
          )}
        </TabPanel>
      </Card>

      {/* Edit User Dialog */}
      <Dialog open={editDialogOpen} onClose={() => setEditDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Editar Usuario</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              fullWidth
              label="Nombre"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              sx={{ mb: 2 }}
            />
            <TextField
              fullWidth
              label="Email"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              sx={{ mb: 2 }}
            />
            <TextField
              fullWidth
              label="Teléfono"
              value={formData.phone}
              onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
              sx={{ mb: 2 }}
            />
            <FormControl fullWidth>
              <InputLabel>Rol</InputLabel>
              <Select
                value={formData.role}
                onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                label="Rol"
              >
                <MenuItem value="Patient">Paciente</MenuItem>
                <MenuItem value="Doctor">Doctor</MenuItem>
                <MenuItem value="Admin">Administrador</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setEditDialogOpen(false)}>Cancelar</Button>
          <Button onClick={handleSaveUser} variant="contained">
            Guardar Cambios
          </Button>
        </DialogActions>
      </Dialog>

      {/* Add User Dialog */}
      <Dialog open={addDialogOpen} onClose={() => setAddDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Agregar Nuevo Usuario</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              fullWidth
              label="Nombre"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              sx={{ mb: 2 }}
            />
            <TextField
              fullWidth
              label="Email"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              sx={{ mb: 2 }}
            />
            <TextField
              fullWidth
              label="Teléfono"
              value={formData.phone}
              onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
              sx={{ mb: 2 }}
            />
            <FormControl fullWidth>
              <InputLabel>Rol</InputLabel>
              <Select
                value={formData.role}
                onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                label="Rol"
              >
                <MenuItem value="Patient">Paciente</MenuItem>
                <MenuItem value="Doctor">Doctor</MenuItem>
                <MenuItem value="Admin">Administrador</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAddDialogOpen(false)}>Cancelar</Button>
          <Button onClick={handleSaveUser} variant="contained">
            Agregar Usuario
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

export default AdminUserManagement;
