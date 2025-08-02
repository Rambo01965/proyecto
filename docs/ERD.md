# Hospital Appointment System - Entity Relationship Diagram

## Database Schema Overview

This document describes the database schema for the Hospital Appointment System. The system uses SQL Server with Entity Framework Core for data access.

## Entity Relationship Diagram (Text Representation)

```
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│      Users      │       │     Doctors     │       │  Appointments   │
├─────────────────┤       ├─────────────────┤       ├─────────────────┤
│ Id (PK)         │◄──────┤ UserId (FK)     │       │ Id (PK)         │
│ Email (Unique)  │       │ Id (PK)         │◄──────┤ DoctorId (FK)   │
│ PasswordHash    │       │ Specialty       │       │ PatientId (FK)  │──┐
│ Role            │       │ Phone           │       │ Date            │  │
│ Name            │       └─────────────────┘       │ Time            │  │
│ CreatedAt       │                                 │ Status          │  │
└─────────────────┘                                 │ Reason          │  │
        │                                           │ CreatedAt       │  │
        │                                           └─────────────────┘  │
        │                                                   │            │
        │                                                   │            │
        │                                           ┌─────────────────┐  │
        │                                           │   Feedbacks     │  │
        │                                           ├─────────────────┤  │
        │                                           │ Id (PK)         │  │
        │                                           │ AppointmentId   │──┘
        │                                           │ Rating          │
        │                                           │ Comment         │
        │                                           │ CreatedAt       │
        │                                           └─────────────────┘
        │
        │                   ┌─────────────────┐
        │                   │ Prescriptions   │
        │                   ├─────────────────┤
        │                   │ Id (PK)         │
        │                   │ PatientId (FK)  │──┘
        │                   │ DoctorId (FK)   │──┐
        │                   │ Medication      │  │
        │                   │ Dosage          │  │
        │                   │ Instructions    │  │
        │                   │ IssueDate       │  │
        │                   │ ExpiryDate      │  │
        │                   │ RenewalCount    │  │
        │                   └─────────────────┘  │
        │                                        │
        │                   ┌─────────────────┐  │
        │                   │   Schedules     │  │
        │                   ├─────────────────┤  │
        │                   │ Id (PK)         │  │
        │                   │ DoctorId (FK)   │──┘
        │                   │ DayOfWeek       │
        │                   │ StartTime       │
        │                   │ EndTime         │
        │                   │ IsAvailable     │
        │                   └─────────────────┘
        │
        │                   ┌─────────────────┐
        │                   │ Notifications   │
        │                   ├─────────────────┤
        │                   │ Id (PK)         │
        │                   │ UserId (FK)     │──┘
        │                   │ Type            │
        │                   │ Message         │
        │                   │ SentAt          │
        │                   └─────────────────┘
```

## Table Definitions

### Users Table
- **Primary Key**: Id (int, identity)
- **Unique Constraints**: Email
- **Relationships**: 
  - One-to-One with Doctors (if Role = Doctor)
  - One-to-Many with Appointments (as Patient)
  - One-to-Many with Prescriptions (as Patient)
  - One-to-Many with Notifications

### Doctors Table
- **Primary Key**: Id (int, identity)
- **Foreign Keys**: UserId → Users.Id
- **Relationships**:
  - One-to-One with Users
  - One-to-Many with Appointments
  - One-to-Many with Prescriptions
  - One-to-Many with Schedules

### Appointments Table
- **Primary Key**: Id (int, identity)
- **Foreign Keys**: 
  - PatientId → Users.Id
  - DoctorId → Doctors.Id
- **Unique Constraints**: (DoctorId, Date, Time)
- **Relationships**:
  - Many-to-One with Users (Patient)
  - Many-to-One with Doctors
  - One-to-One with Feedbacks

### Prescriptions Table
- **Primary Key**: Id (int, identity)
- **Foreign Keys**:
  - PatientId → Users.Id
  - DoctorId → Doctors.Id
- **Relationships**:
  - Many-to-One with Users (Patient)
  - Many-to-One with Doctors

### Schedules Table
- **Primary Key**: Id (int, identity)
- **Foreign Keys**: DoctorId → Doctors.Id
- **Relationships**:
  - Many-to-One with Doctors

### Notifications Table
- **Primary Key**: Id (int, identity)
- **Foreign Keys**: UserId → Users.Id
- **Relationships**:
  - Many-to-One with Users

### Feedbacks Table
- **Primary Key**: Id (int, identity)
- **Foreign Keys**: AppointmentId → Appointments.Id
- **Relationships**:
  - One-to-One with Appointments

## Indexes

### Performance Indexes
- `IX_Users_Email` - Unique index on Users.Email
- `IX_Appointments_DoctorId_Date_Time` - Unique composite index
- `IX_Appointments_PatientId` - Index for patient queries
- `IX_Appointments_Date` - Index for date-based queries
- `IX_Prescriptions_PatientId` - Index for patient prescriptions
- `IX_Prescriptions_ExpiryDate` - Index for expiry tracking
- `IX_Schedules_DoctorId` - Index for doctor schedules
- `IX_Notifications_UserId` - Index for user notifications

## Data Types and Constraints

### Users
```sql
Id: int IDENTITY(1,1) PRIMARY KEY
Email: nvarchar(256) NOT NULL UNIQUE
PasswordHash: nvarchar(MAX) NOT NULL
Role: nvarchar(50) NOT NULL CHECK (Role IN ('Patient', 'Doctor', 'Admin'))
Name: nvarchar(100) NOT NULL
CreatedAt: datetime2 NOT NULL DEFAULT GETUTCDATE()
```

### Doctors
```sql
Id: int IDENTITY(1,1) PRIMARY KEY
UserId: int NOT NULL FOREIGN KEY REFERENCES Users(Id)
Specialty: nvarchar(100) NOT NULL
Phone: nvarchar(20) NOT NULL
```

### Appointments
```sql
Id: int IDENTITY(1,1) PRIMARY KEY
PatientId: int NOT NULL FOREIGN KEY REFERENCES Users(Id)
DoctorId: int NOT NULL FOREIGN KEY REFERENCES Doctors(Id)
Date: date NOT NULL
Time: time NOT NULL
Status: nvarchar(50) NOT NULL CHECK (Status IN ('Scheduled', 'Completed', 'Cancelled'))
Reason: nvarchar(500) NOT NULL
CreatedAt: datetime2 NOT NULL DEFAULT GETUTCDATE()
```

### Prescriptions
```sql
Id: int IDENTITY(1,1) PRIMARY KEY
PatientId: int NOT NULL FOREIGN KEY REFERENCES Users(Id)
DoctorId: int NOT NULL FOREIGN KEY REFERENCES Doctors(Id)
Medication: nvarchar(200) NOT NULL
Dosage: nvarchar(100) NOT NULL
Instructions: nvarchar(1000) NOT NULL
IssueDate: datetime2 NOT NULL
ExpiryDate: datetime2 NOT NULL
RenewalCount: int NOT NULL DEFAULT 0
```

### Schedules
```sql
Id: int IDENTITY(1,1) PRIMARY KEY
DoctorId: int NOT NULL FOREIGN KEY REFERENCES Doctors(Id)
DayOfWeek: nvarchar(20) NOT NULL CHECK (DayOfWeek IN ('Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'))
StartTime: time NOT NULL
EndTime: time NOT NULL
IsAvailable: bit NOT NULL DEFAULT 1
```

### Notifications
```sql
Id: int IDENTITY(1,1) PRIMARY KEY
UserId: int NOT NULL FOREIGN KEY REFERENCES Users(Id)
Type: nvarchar(50) NOT NULL CHECK (Type IN ('Email', 'SMS'))
Message: nvarchar(1000) NOT NULL
SentAt: datetime2 NOT NULL DEFAULT GETUTCDATE()
```

### Feedbacks
```sql
Id: int IDENTITY(1,1) PRIMARY KEY
AppointmentId: int NOT NULL FOREIGN KEY REFERENCES Appointments(Id)
Rating: int NOT NULL CHECK (Rating >= 1 AND Rating <= 5)
Comment: nvarchar(1000)
CreatedAt: datetime2 NOT NULL DEFAULT GETUTCDATE()
```

## Business Rules

1. **User Roles**: Users can be Patient, Doctor, or Admin
2. **Doctor Constraint**: Only users with Role = 'Doctor' can have entries in Doctors table
3. **Appointment Scheduling**: No two appointments can be scheduled for the same doctor at the same date and time
4. **Prescription Expiry**: Prescriptions have expiry dates and renewal counts
5. **Feedback**: Each appointment can have at most one feedback entry
6. **Schedule Availability**: Doctors can have multiple schedule entries for different days
7. **Notification Types**: Notifications can be either Email or SMS

## Migration Strategy

The database schema is managed through Entity Framework Core migrations:
- Initial migration creates all tables and relationships
- Subsequent migrations handle schema updates
- Seed data can be added through migration scripts

## Security Considerations

- Password hashes are stored, never plain text passwords
- Email addresses are unique and used for authentication
- Foreign key constraints ensure data integrity
- Check constraints validate enum values
- Indexes optimize query performance while maintaining data consistency
