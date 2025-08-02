using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using HospitalAppointmentSystem.Application.Services;
using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Domain.Enums;
using HospitalAppointmentSystem.EventBus.Interfaces;

namespace HospitalAppointmentSystem.Tests.UnitTests;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
    private readonly Mock<IDoctorRepository> _mockDoctorRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<ILogger<AppointmentService>> _mockLogger;
    private readonly AppointmentService _appointmentService;

    public AppointmentServiceTests()
    {
        _mockAppointmentRepository = new Mock<IAppointmentRepository>();
        _mockDoctorRepository = new Mock<IDoctorRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEventBus = new Mock<IEventBus>();
        _mockLogger = new Mock<ILogger<AppointmentService>>();

        _appointmentService = new AppointmentService(
            _mockAppointmentRepository.Object,
            _mockDoctorRepository.Object,
            _mockUserRepository.Object,
            _mockEventBus.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAppointmentAsync_ValidAppointment_ReturnsAppointment()
    {
        // Arrange
        var appointment = new Appointment
        {
            Id = 1,
            PatientId = 1,
            DoctorId = 1,
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Time = TimeOnly.FromDateTime(DateTime.Now.AddHours(2)),
            Status = AppointmentStatus.Scheduled,
            Reason = "Regular checkup"
        };

        var patient = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@test.com",
            Role = UserRole.Patient
        };

        var doctor = new Doctor
        {
            Id = 1,
            UserId = 2,
            Specialty = "General Medicine",
            Phone = "123-456-7890"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(patient);
        
        _mockDoctorRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(doctor);

        _mockAppointmentRepository.Setup(x => x.CreateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(appointment);

        // Act
        var result = await _appointmentService.CreateAppointmentAsync(appointment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(appointment.Id, result.Id);
        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
        
        _mockAppointmentRepository.Verify(x => x.CreateAsync(It.IsAny<Appointment>()), Times.Once);
        _mockEventBus.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_ExistingId_ReturnsAppointment()
    {
        // Arrange
        var appointmentId = 1;
        var expectedAppointment = new Appointment
        {
            Id = appointmentId,
            PatientId = 1,
            DoctorId = 1,
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Time = TimeOnly.FromDateTime(DateTime.Now.AddHours(2)),
            Status = AppointmentStatus.Scheduled,
            Reason = "Regular checkup"
        };

        _mockAppointmentRepository.Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(expectedAppointment);

        // Act
        var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAppointment.Id, result.Id);
        Assert.Equal(expectedAppointment.PatientId, result.PatientId);
        Assert.Equal(expectedAppointment.DoctorId, result.DoctorId);
    }

    [Fact]
    public async Task GetAppointmentByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var appointmentId = 999;
        _mockAppointmentRepository.Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act
        var result = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CancelAppointmentAsync_ExistingAppointment_UpdatesStatusAndPublishesEvent()
    {
        // Arrange
        var appointmentId = 1;
        var cancellationReason = "Patient request";
        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = 1,
            DoctorId = 1,
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Time = TimeOnly.FromDateTime(DateTime.Now.AddHours(2)),
            Status = AppointmentStatus.Scheduled,
            Reason = "Regular checkup"
        };

        var patient = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@test.com",
            Role = UserRole.Patient
        };

        var doctor = new Doctor
        {
            Id = 1,
            UserId = 2,
            Specialty = "General Medicine",
            Phone = "123-456-7890"
        };

        _mockAppointmentRepository.Setup(x => x.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);
        
        _mockUserRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(patient);
        
        _mockDoctorRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(doctor);

        _mockAppointmentRepository.Setup(x => x.UpdateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(appointment);

        // Act
        var result = await _appointmentService.CancelAppointmentAsync(appointmentId, cancellationReason);

        // Assert
        Assert.True(result);
        _mockAppointmentRepository.Verify(x => x.UpdateAsync(It.Is<Appointment>(a => 
            a.Status == AppointmentStatus.Cancelled)), Times.Once);
        _mockEventBus.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task GetAppointmentsByPatientIdAsync_ValidPatientId_ReturnsAppointments()
    {
        // Arrange
        var patientId = 1;
        var expectedAppointments = new List<Appointment>
        {
            new Appointment
            {
                Id = 1,
                PatientId = patientId,
                DoctorId = 1,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                Time = TimeOnly.FromDateTime(DateTime.Now.AddHours(2)),
                Status = AppointmentStatus.Scheduled,
                Reason = "Regular checkup"
            },
            new Appointment
            {
                Id = 2,
                PatientId = patientId,
                DoctorId = 2,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                Time = TimeOnly.FromDateTime(DateTime.Now.AddHours(3)),
                Status = AppointmentStatus.Completed,
                Reason = "Follow-up"
            }
        };

        _mockAppointmentRepository.Setup(x => x.GetByPatientIdAsync(patientId))
            .ReturnsAsync(expectedAppointments);

        // Act
        var result = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.Equal(patientId, a.PatientId));
    }

    [Fact]
    public async Task GetAppointmentsByDoctorIdAsync_ValidDoctorId_ReturnsAppointments()
    {
        // Arrange
        var doctorId = 1;
        var expectedAppointments = new List<Appointment>
        {
            new Appointment
            {
                Id = 1,
                PatientId = 1,
                DoctorId = doctorId,
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                Time = TimeOnly.FromDateTime(DateTime.Now.AddHours(2)),
                Status = AppointmentStatus.Scheduled,
                Reason = "Regular checkup"
            }
        };

        _mockAppointmentRepository.Setup(x => x.GetByDoctorIdAsync(doctorId))
            .ReturnsAsync(expectedAppointments);

        // Act
        var result = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, a => Assert.Equal(doctorId, a.DoctorId));
    }
}
