using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using HospitalAppointmentSystem.Infrastructure.Data;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Domain.Enums;
using HospitalAppointmentSystem.API.Models;
using System.Net;

namespace HospitalAppointmentSystem.Tests.IntegrationTests;

public class AppointmentControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AppointmentControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database context
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using var scope = sp.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                // Ensure the database is created
                db.Database.EnsureCreated();

                // Seed the database with test data
                SeedTestData(db);
            });
        });

        _client = _factory.CreateClient();
    }

    private static void SeedTestData(ApplicationDbContext context)
    {
        // Clear existing data
        context.Appointments.RemoveRange(context.Appointments);
        context.Doctors.RemoveRange(context.Doctors);
        context.Users.RemoveRange(context.Users);
        context.SaveChanges();

        // Add test users
        var patient = new User
        {
            Id = 1,
            Email = "patient@test.com",
            Name = "Test Patient",
            Role = UserRole.Patient,
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };

        var doctorUser = new User
        {
            Id = 2,
            Email = "doctor@test.com",
            Name = "Test Doctor",
            Role = UserRole.Doctor,
            PasswordHash = "hashedpassword",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(patient, doctorUser);
        context.SaveChanges();

        // Add test doctor
        var doctor = new Doctor
        {
            Id = 1,
            UserId = 2,
            Specialty = "General Medicine",
            Phone = "123-456-7890"
        };

        context.Doctors.Add(doctor);
        context.SaveChanges();

        // Add test appointments
        var appointment = new Appointment
        {
            Id = 1,
            PatientId = 1,
            DoctorId = 1,
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
            Time = TimeOnly.FromDateTime(DateTime.Now.AddHours(2)),
            Status = AppointmentStatus.Scheduled,
            Reason = "Regular checkup",
            CreatedAt = DateTime.UtcNow
        };

        context.Appointments.Add(appointment);
        context.SaveChanges();
    }

    [Fact]
    public async Task GetAppointments_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task GetAppointmentById_ExistingId_ReturnsAppointment()
    {
        // Arrange
        var appointmentId = 1;

        // Act
        var response = await _client.GetAsync($"/api/appointments/{appointmentId}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var appointment = JsonSerializer.Deserialize<AppointmentDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(appointment);
        Assert.Equal(appointmentId, appointment.Id);
    }

    [Fact]
    public async Task GetAppointmentById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var appointmentId = 999;

        // Act
        var response = await _client.GetAsync($"/api/appointments/{appointmentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAppointment_ValidData_ReturnsCreated()
    {
        // Arrange
        var createAppointmentDto = new CreateAppointmentDto
        {
            PatientId = 1,
            DoctorId = 1,
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
            Time = TimeOnly.FromDateTime(DateTime.Now.AddHours(3)),
            Reason = "Follow-up appointment"
        };

        var json = JsonSerializer.Serialize(createAppointmentDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/appointments", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var appointment = JsonSerializer.Deserialize<AppointmentDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(appointment);
        Assert.Equal(createAppointmentDto.PatientId, appointment.PatientId);
        Assert.Equal(createAppointmentDto.DoctorId, appointment.DoctorId);
        Assert.Equal(createAppointmentDto.Reason, appointment.Reason);
    }

    [Fact]
    public async Task CreateAppointment_InvalidData_ReturnsBadRequest()
    {
        // Arrange
        var createAppointmentDto = new CreateAppointmentDto
        {
            PatientId = 0, // Invalid patient ID
            DoctorId = 1,
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
            Time = TimeOnly.FromDateTime(DateTime.Now.AddHours(3)),
            Reason = "Follow-up appointment"
        };

        var json = JsonSerializer.Serialize(createAppointmentDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/appointments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAppointment_ValidData_ReturnsOk()
    {
        // Arrange
        var appointmentId = 1;
        var updateAppointmentDto = new UpdateAppointmentDto
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(3)),
            Time = TimeOnly.FromDateTime(DateTime.Now.AddHours(4)),
            Reason = "Updated reason"
        };

        var json = JsonSerializer.Serialize(updateAppointmentDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/api/appointments/{appointmentId}", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var appointment = JsonSerializer.Deserialize<AppointmentDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(appointment);
        Assert.Equal(updateAppointmentDto.Reason, appointment.Reason);
    }

    [Fact]
    public async Task DeleteAppointment_ExistingId_ReturnsNoContent()
    {
        // Arrange
        var appointmentId = 1;

        // Act
        var response = await _client.DeleteAsync($"/api/appointments/{appointmentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteAppointment_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var appointmentId = 999;

        // Act
        var response = await _client.DeleteAsync($"/api/appointments/{appointmentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public class CreateAppointmentDto
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class UpdateAppointmentDto
{
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public string Reason { get; set; } = string.Empty;
}
