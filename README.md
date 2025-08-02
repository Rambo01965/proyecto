# Hospital Appointment System

A comprehensive hospital appointment management system built with .NET 9, featuring real-time notifications, background job processing, and event-driven architecture.

## 🚀 Features

### Core Functionality
- **User Management** - Patients, Doctors, and Administrators
- **Appointment Scheduling** - Create, update, cancel appointments
- **Prescription Management** - Digital prescriptions with expiry tracking
- **Real-time Notifications** - Email and SMS notifications
- **Background Job Processing** - Automated reminders and cleanup tasks
- **Event-Driven Architecture** - RabbitMQ for reliable message processing

### Technical Features
- **JWT Authentication** - Secure API access
- **Clean Architecture** - Separation of concerns with multiple layers
- **Entity Framework Core** - Database management with SQL Server
- **Swagger Documentation** - Interactive API documentation
- **Structured Logging** - Comprehensive logging with Serilog
- **Docker Support** - Easy deployment with Docker Compose

## 🏗️ Architecture

```
├── HospitalAppointmentSystem.API          # Web API Layer
├── HospitalAppointmentSystem.Application  # Business Logic Layer
├── HospitalAppointmentSystem.Domain       # Domain Entities & Enums
├── HospitalAppointmentSystem.Infrastructure # Data Access & External Services
├── HospitalAppointmentSystem.EventBus     # Event-Driven Messaging
├── HospitalAppointmentSystem.Tests        # Unit & Integration Tests
└── hospital-frontend                       # React Frontend (Optional)
```

## 🛠️ Technology Stack

### Backend (.NET 9)
- **Framework**: ASP.NET Core Web API
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Message Queue**: RabbitMQ
- **Background Jobs**: Hangfire
- **Logging**: Serilog
- **Documentation**: Swagger/OpenAPI

### External Services
- **Email**: SendGrid
- **SMS**: Twilio
- **Data Generation**: Bogus (for testing)

### Frontend (React)
- **Framework**: React 18 with TypeScript
- **UI Library**: Material-UI (MUI)
- **HTTP Client**: Axios
- **Routing**: React Router

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- SQL Server (LocalDB or full instance)
- RabbitMQ (via Docker recommended)
- Node.js 18+ (for frontend)

### 1. Clone and Setup
```bash
git clone <repository-url>
cd hospital-appointment-system
```

### 2. Start Infrastructure Services
```bash
# Start RabbitMQ and SQL Server
docker-compose up -d
```

### 3. Configure API Keys
Update `appsettings.json` with your API keys:
```json
{
  "SendGrid": {
    "ApiKey": "YOUR_SENDGRID_API_KEY",
    "FromEmail": "your-email@domain.com",
    "FromName": "Your Hospital Name"
  },
  "Twilio": {
    "AccountSid": "YOUR_TWILIO_ACCOUNT_SID",
    "AuthToken": "YOUR_TWILIO_AUTH_TOKEN",
    "FromPhoneNumber": "+1234567890"
  }
}
```

### 4. Run the API
```bash
cd HospitalAppointmentSystem.API
dotnet run
```

### 5. Access the Application
- **API Documentation**: http://localhost:5000 (Swagger UI)
- **Hangfire Dashboard**: http://localhost:5000/hangfire
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## 📚 API Documentation

### Authentication Endpoints
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration

### Patient Endpoints
- `GET /api/patients` - Get all patients
- `GET /api/patients/{id}` - Get patient by ID
- `GET /api/patients/{id}/appointments` - Get patient appointments
- `GET /api/patients/{id}/prescriptions` - Get patient prescriptions
- `POST /api/patients/{id}/appointments` - Create appointment

### Doctor Endpoints
- `GET /api/doctors` - Get all doctors
- `GET /api/doctors/{id}` - Get doctor by ID
- `GET /api/doctors/{id}/appointments` - Get doctor appointments
- `GET /api/doctors/{id}/schedule` - Get doctor schedule

### Admin Endpoints
- `GET /api/admin/dashboard` - Admin dashboard data
- `GET /api/admin/users` - Manage users
- `GET /api/admin/appointments` - View all appointments
- `GET /api/admin/reports/appointments` - Appointment reports

### Notification Test Endpoints
- `POST /api/notifications/test-email` - Send test email
- `POST /api/notifications/test-sms` - Send test SMS
- `POST /api/notifications/schedule-test-job` - Schedule test background job

## 🔧 Configuration

### Database Connection
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HospitalAppointmentSystemDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "HospitalAppointmentSystem",
    "Audience": "HospitalAppointmentSystemUsers",
    "ExpirationInMinutes": 60
  }
}
```

### Serilog Configuration
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/hospital-appointment-system-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## 🎯 Event-Driven Architecture

The system uses RabbitMQ for event-driven communication:

### Events
- **AppointmentBookedEvent** - Triggered when appointment is created
- **AppointmentCancelledEvent** - Triggered when appointment is cancelled

### Event Handlers
- **AppointmentBookedConsumer** - Sends confirmation emails/SMS
- **AppointmentCancelledConsumer** - Sends cancellation notifications

### Background Jobs
- **Appointment Reminders** - Scheduled 24 hours before appointment
- **Data Cleanup** - Daily cleanup of old records
- **Email Queue Processing** - Retry failed email deliveries

## 🧪 Testing

### Run Unit Tests
```bash
cd HospitalAppointmentSystem.Tests
dotnet test
```

### Test Notifications
1. Use the `/api/notifications/test-email` endpoint
2. Use the `/api/notifications/test-sms` endpoint
3. Check Hangfire dashboard for background jobs

### Test Event Flow
1. Create an appointment via API
2. Check RabbitMQ management for published events
3. Verify email/SMS notifications are sent
4. Check logs for event processing

## 📊 Monitoring & Observability

### Logging
- **Console Logging** - Development environment
- **File Logging** - Production logs in `/logs` directory
- **Structured Logging** - JSON format with Serilog

### Dashboards
- **Hangfire Dashboard** - Background job monitoring
- **RabbitMQ Management** - Message queue monitoring
- **Swagger UI** - API testing and documentation

### Health Checks
- `GET /api/notifications/health` - Service health status

## 🚀 Deployment

### Docker Deployment
```bash
# Build and run with Docker Compose
docker-compose up --build
```

### Production Considerations
1. **Security**: Update JWT secret keys
2. **Database**: Use production SQL Server instance
3. **SSL**: Configure HTTPS certificates
4. **Monitoring**: Set up application monitoring
5. **Backup**: Configure database backups

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For support and questions:
- Check the API documentation at `/swagger`
- Review the logs in `/logs` directory
- Check Hangfire dashboard for background job issues
- Verify RabbitMQ connection and queues

## 🔄 Version History

- **v1.0.0** - Initial release with core functionality
- **v1.1.0** - Added event-driven architecture
- **v1.2.0** - Added notification services (Email/SMS)
- **v1.3.0** - Added background job processing
- **v1.4.0** - Added comprehensive logging and monitoring

---

Built with ❤️ using .NET 9 and modern development practices.
