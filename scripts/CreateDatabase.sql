-- Script para crear la base de datos Hospital Appointment System
-- Ejecutar en SQL Server Management Studio

-- Crear la base de datos
CREATE DATABASE HospitalAppointmentSystemDb;
GO

-- Usar la base de datos
USE HospitalAppointmentSystemDb;
GO

-- Crear tabla Users
CREATE TABLE Users (
    Id int IDENTITY(1,1) PRIMARY KEY,
    Email nvarchar(256) NOT NULL UNIQUE,
    PasswordHash nvarchar(MAX) NOT NULL,
    Role nvarchar(50) NOT NULL CHECK (Role IN ('Patient', 'Doctor', 'Admin')),
    Name nvarchar(100) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
);

-- Crear tabla Doctors
CREATE TABLE Doctors (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId int NOT NULL FOREIGN KEY REFERENCES Users(Id),
    Specialty nvarchar(100) NOT NULL,
    Phone nvarchar(20) NOT NULL
);

-- Crear tabla Appointments
CREATE TABLE Appointments (
    Id int IDENTITY(1,1) PRIMARY KEY,
    PatientId int NOT NULL FOREIGN KEY REFERENCES Users(Id),
    DoctorId int NOT NULL FOREIGN KEY REFERENCES Doctors(Id),
    Date date NOT NULL,
    Time time NOT NULL,
    Status nvarchar(50) NOT NULL CHECK (Status IN ('Scheduled', 'Completed', 'Cancelled')),
    Reason nvarchar(500) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
);

-- Crear tabla Prescriptions
CREATE TABLE Prescriptions (
    Id int IDENTITY(1,1) PRIMARY KEY,
    PatientId int NOT NULL FOREIGN KEY REFERENCES Users(Id),
    DoctorId int NOT NULL FOREIGN KEY REFERENCES Doctors(Id),
    Medication nvarchar(200) NOT NULL,
    Dosage nvarchar(100) NOT NULL,
    Instructions nvarchar(1000) NOT NULL,
    IssueDate datetime2 NOT NULL,
    ExpiryDate datetime2 NOT NULL,
    RenewalCount int NOT NULL DEFAULT 0
);

-- Crear tabla Schedules
CREATE TABLE Schedules (
    Id int IDENTITY(1,1) PRIMARY KEY,
    DoctorId int NOT NULL FOREIGN KEY REFERENCES Doctors(Id),
    DayOfWeek nvarchar(20) NOT NULL,
    StartTime time NOT NULL,
    EndTime time NOT NULL,
    IsAvailable bit NOT NULL DEFAULT 1
);

-- Crear tabla Notifications
CREATE TABLE Notifications (
    Id int IDENTITY(1,1) PRIMARY KEY,
    UserId int NOT NULL FOREIGN KEY REFERENCES Users(Id),
    Type nvarchar(50) NOT NULL CHECK (Type IN ('Email', 'SMS')),
    Message nvarchar(1000) NOT NULL,
    SentAt datetime2 NOT NULL DEFAULT GETUTCDATE()
);

-- Crear tabla Feedbacks
CREATE TABLE Feedbacks (
    Id int IDENTITY(1,1) PRIMARY KEY,
    AppointmentId int NOT NULL FOREIGN KEY REFERENCES Appointments(Id),
    Rating int NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment nvarchar(1000),
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
);

-- Insertar usuario administrador de prueba
INSERT INTO Users (Email, PasswordHash, Role, Name) 
VALUES ('admin@hospital.com', 'TempPassword123', 'Admin', 'Administrador Sistema');

PRINT 'Base de datos creada exitosamente!';
