# Hospital Appointment System - API Documentation

## Overview
This document provides comprehensive documentation for the Hospital Appointment System REST API. The API is built using ASP.NET Core 9 and follows RESTful principles.

## Base URL
```
https://localhost:5001/api
http://localhost:5000/api
```

## Authentication
The API uses JWT Bearer token authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

## Response Format
All API responses follow a consistent JSON format:

### Success Response
```json
{
  "data": {...},
  "message": "Success message",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Error Response
```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Error description",
    "details": {...}
  },
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## Endpoints

### Authentication Endpoints

#### POST /api/auth/login
Authenticate a user and receive a JWT token.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "name": "John Doe",
    "role": "Patient",
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

#### POST /api/auth/register
Register a new user account.

**Request Body:**
```json
{
  "email": "newuser@example.com",
  "password": "password123",
  "name": "Jane Doe",
  "role": "Patient"
}
```

**Response:** 201 Created with user details

### Patient Endpoints

#### GET /api/patients
Get all patients (Admin only).

**Response:**
```json
[
  {
    "id": 1,
    "email": "patient@example.com",
    "name": "John Patient",
    "role": "Patient",
    "createdAt": "2024-01-01T00:00:00Z"
  }
]
```

#### GET /api/patients/{id}
Get a specific patient by ID.

#### GET /api/patients/{id}/appointments
Get all appointments for a specific patient.

#### GET /api/patients/{id}/prescriptions
Get all prescriptions for a specific patient.

#### POST /api/patients/{id}/appointments
Create a new appointment for a patient.

**Request Body:**
```json
{
  "doctorId": 1,
  "date": "2024-01-15",
  "time": "14:30:00",
  "reason": "Regular checkup"
}
```

### Doctor Endpoints

#### GET /api/doctors
Get all doctors.

**Response:**
```json
[
  {
    "id": 1,
    "userId": 2,
    "name": "Dr. Smith",
    "specialty": "Cardiology",
    "phone": "+1234567890",
    "email": "dr.smith@hospital.com"
  }
]
```

#### GET /api/doctors/{id}
Get a specific doctor by ID.

#### GET /api/doctors/{id}/appointments
Get all appointments for a specific doctor.

#### GET /api/doctors/{id}/schedule
Get the schedule for a specific doctor.

**Response:**
```json
[
  {
    "id": 1,
    "doctorId": 1,
    "dayOfWeek": "Monday",
    "startTime": "09:00:00",
    "endTime": "17:00:00",
    "isAvailable": true
  }
]
```

### Appointment Endpoints

#### GET /api/appointments
Get all appointments (with pagination).

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10)
- `status` (optional): Filter by status (Scheduled, Completed, Cancelled)

#### GET /api/appointments/{id}
Get a specific appointment by ID.

#### POST /api/appointments
Create a new appointment.

#### PUT /api/appointments/{id}
Update an existing appointment.

#### DELETE /api/appointments/{id}
Cancel an appointment.

### Admin Endpoints

#### GET /api/admin/dashboard
Get dashboard statistics (Admin only).

**Response:**
```json
{
  "totalPatients": 150,
  "totalDoctors": 25,
  "totalAppointments": 500,
  "appointmentsToday": 12,
  "appointmentsThisWeek": 85,
  "appointmentsThisMonth": 320
}
```

#### GET /api/admin/users
Get all users with pagination.

#### POST /api/admin/users
Create a new user.

#### PUT /api/admin/users/{id}
Update user information.

#### DELETE /api/admin/users/{id}
Delete a user account.

#### GET /api/admin/reports/appointments
Generate appointment reports.

**Query Parameters:**
- `startDate`: Start date for the report
- `endDate`: End date for the report
- `doctorId` (optional): Filter by doctor
- `status` (optional): Filter by appointment status

### Notification Test Endpoints

#### POST /api/notifications/test-email
Send a test email to verify SendGrid integration.

**Request Body:**
```json
{
  "toEmail": "test@example.com",
  "toName": "Test User"
}
```

#### POST /api/notifications/test-sms
Send a test SMS to verify Twilio integration.

**Request Body:**
```json
{
  "phoneNumber": "+1234567890",
  "name": "Test User"
}
```

#### POST /api/notifications/schedule-test-job
Schedule a test background job to verify Hangfire integration.

#### GET /api/notifications/health
Get the health status of notification services.

## Data Models

### User
```json
{
  "id": 1,
  "email": "user@example.com",
  "name": "User Name",
  "role": "Patient|Doctor|Admin",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Doctor
```json
{
  "id": 1,
  "userId": 2,
  "name": "Dr. Name",
  "specialty": "Specialty",
  "phone": "+1234567890",
  "email": "doctor@example.com"
}
```

### Appointment
```json
{
  "id": 1,
  "patientId": 1,
  "doctorId": 1,
  "patientName": "Patient Name",
  "doctorName": "Dr. Name",
  "date": "2024-01-15",
  "time": "14:30:00",
  "status": "Scheduled|Completed|Cancelled",
  "reason": "Appointment reason",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Prescription
```json
{
  "id": 1,
  "patientId": 1,
  "doctorId": 1,
  "patientName": "Patient Name",
  "doctorName": "Dr. Name",
  "medication": "Medication Name",
  "dosage": "10mg",
  "instructions": "Take twice daily",
  "issueDate": "2024-01-01T00:00:00Z",
  "expiryDate": "2024-01-31T00:00:00Z",
  "renewalCount": 0
}
```

## Error Codes

| Code | Description |
|------|-------------|
| 400 | Bad Request - Invalid input data |
| 401 | Unauthorized - Authentication required |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Resource already exists |
| 422 | Unprocessable Entity - Validation failed |
| 500 | Internal Server Error - Server error |

## Rate Limiting
The API implements rate limiting to prevent abuse:
- 100 requests per minute per IP address
- 1000 requests per hour per authenticated user

## Pagination
List endpoints support pagination with the following parameters:
- `page`: Page number (1-based, default: 1)
- `pageSize`: Items per page (max: 100, default: 10)

Response includes pagination metadata:
```json
{
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "totalPages": 10,
    "totalItems": 100,
    "pageSize": 10,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

## Webhook Events
The system can send webhook notifications for the following events:
- Appointment created
- Appointment cancelled
- Appointment completed
- Prescription issued
- Prescription expired

## SDK and Libraries
Official SDKs are available for:
- JavaScript/TypeScript
- C# .NET
- Python
- Java

## Support
For API support, please contact:
- Email: api-support@hospital.com
- Documentation: https://api-docs.hospital.com
- Status Page: https://status.hospital.com
