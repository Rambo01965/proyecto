using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HospitalAppointmentSystem.API.Models;
using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Domain.Enums;

namespace HospitalAppointmentSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IDoctorService _doctorService;
    private readonly IAppointmentService _appointmentService;
    private readonly IPrescriptionService _prescriptionService;
    private readonly INotificationService _notificationService;

    public AdminController(
        IUserService userService,
        IDoctorService doctorService,
        IAppointmentService appointmentService,
        IPrescriptionService prescriptionService,
        INotificationService notificationService)
    {
        _userService = userService;
        _doctorService = doctorService;
        _appointmentService = appointmentService;
        _prescriptionService = prescriptionService;
        _notificationService = notificationService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var users = await _userService.GetAllAsync();
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        var prescriptions = await _prescriptionService.GetExpiredPrescriptionsAsync();

        var dashboard = new
        {
            TotalUsers = users.Count(),
            TotalPatients = users.Count(u => u.Role == UserRole.Patient),
            TotalDoctors = users.Count(u => u.Role == UserRole.Doctor),
            TotalAppointments = appointments.Count(),
            ScheduledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Scheduled),
            CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
            CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
            ExpiredPrescriptions = prescriptions.Count()
        };

        return Ok(dashboard);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllAsync();
        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            Name = u.Name,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        });

        return Ok(userDtos);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return Ok(userDto);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Check if user already exists
        var existingUser = await _userService.GetByEmailAsync(createUserDto.Email);
        if (existingUser != null)
            return BadRequest(new { message = "User with this email already exists" });

        var user = new User
        {
            Email = createUserDto.Email,
            PasswordHash = createUserDto.Password, // Will be hashed in the service
            Name = createUserDto.Name,
            Role = createUserDto.Role
        };

        var createdUser = await _userService.CreateAsync(user);

        // Si el usuario es un doctor, crear automáticamente el perfil de doctor
        if (createdUser.Role == UserRole.Doctor)
        {
            try
            {
                var doctor = new Doctor
                {
                    UserId = createdUser.Id,
                    Specialty = "General", // Especialidad por defecto
                    Phone = "", // Se puede actualizar después
                    User = createdUser
                };
                
                await _doctorService.CreateAsync(doctor);
            }
            catch (Exception ex)
            {
                // Log el error pero no fallar la creación del usuario
                // El perfil de doctor se puede crear manualmente después
                Console.WriteLine($"Error creando perfil de doctor: {ex.Message}");
            }
        }

        var userDto = new UserDto
        {
            Id = createdUser.Id,
            Email = createdUser.Email,
            Name = createdUser.Name,
            Role = createdUser.Role,
            CreatedAt = createdUser.CreatedAt
        };

        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, userDto);
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        user.Name = updateUserDto.Name;
        user.Email = updateUserDto.Email;

        var updatedUser = await _userService.UpdateAsync(user);

        var userDto = new UserDto
        {
            Id = updatedUser.Id,
            Email = updatedUser.Email,
            Name = updatedUser.Name,
            Role = updatedUser.Role,
            CreatedAt = updatedUser.CreatedAt
        };

        return Ok(userDto);
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        // Check if user exists first
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        await _userService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAllAppointments()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        var appointmentDtos = appointments.Select(a => new AppointmentDto
        {
            Id = a.Id,
            PatientId = a.PatientId,
            PatientName = a.Patient?.Name ?? "",
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor?.User?.Name ?? "",
            Date = a.Date,
            Time = a.Time,
            Status = a.Status.ToString(),
            Reason = a.Reason,
            CreatedAt = a.CreatedAt
        });

        return Ok(appointmentDtos);
    }

    [HttpPut("appointments/{id}")]
    public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDto updateAppointmentDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingAppointment = await _appointmentService.GetByIdAsync(id);
        if (existingAppointment == null)
            return NotFound();

        try
        {
            // Update only provided fields
            bool hasChanges = false;
            
            if (updateAppointmentDto.Date.HasValue && existingAppointment.Date != updateAppointmentDto.Date.Value)
            {
                existingAppointment.Date = updateAppointmentDto.Date.Value;
                hasChanges = true;
            }
            
            if (updateAppointmentDto.Time.HasValue && existingAppointment.Time != updateAppointmentDto.Time.Value)
            {
                existingAppointment.Time = updateAppointmentDto.Time.Value;
                hasChanges = true;
            }
            
            if (updateAppointmentDto.Status.HasValue && existingAppointment.Status != updateAppointmentDto.Status.Value)
            {
                existingAppointment.Status = updateAppointmentDto.Status.Value;
                hasChanges = true;
            }
            
            if (!string.IsNullOrEmpty(updateAppointmentDto.Reason) && existingAppointment.Reason != updateAppointmentDto.Reason)
            {
                existingAppointment.Reason = updateAppointmentDto.Reason;
                hasChanges = true;
            }

            if (!hasChanges)
            {
                // No changes to save, return existing appointment
                return Ok(MapAppointmentToDto(existingAppointment));
            }

            // Only call UpdateAsync if there are actual changes
            var updatedAppointment = await _appointmentService.UpdateAsync(existingAppointment);
            return Ok(MapAppointmentToDto(updatedAppointment));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the full exception for debugging
            Console.WriteLine($"Error updating appointment: {ex.Message}");
            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            return StatusCode(500, new { message = "An error occurred while updating the appointment.", details = ex.Message });
        }
    }

    private AppointmentDto MapAppointmentToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient?.Name ?? "",
            DoctorId = appointment.DoctorId,
            DoctorName = appointment.Doctor?.User?.Name ?? "",
            Date = appointment.Date,
            Time = appointment.Time,
            Status = appointment.Status.ToString(),
            Reason = appointment.Reason,
            CreatedAt = appointment.CreatedAt
        };
    }

    [HttpDelete("appointments/{id}")]
    public async Task<IActionResult> DeleteAppointment(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null)
            return NotFound();

        await _appointmentService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("doctors")]
    public async Task<IActionResult> GetAllDoctors()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();
        var doctorDtos = doctors.Select(d => new DoctorDto
        {
            Id = d.Id,
            UserId = d.UserId,
            Name = d.User?.Name ?? "",
            Email = d.User?.Email ?? "",
            Specialty = d.Specialty,
            Phone = d.Phone
        });

        return Ok(doctorDtos);
    }

    [HttpPost("doctors")]
    public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorDto createDoctorDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // First create the user
        var user = new User
        {
            Email = createDoctorDto.Email,
            PasswordHash = createDoctorDto.Password, // Will be hashed in the service
            Name = createDoctorDto.Name,
            Role = UserRole.Doctor
        };

        var createdUser = await _userService.CreateAsync(user);

        // Then create the doctor profile
        var doctor = new Doctor
        {
            UserId = createdUser.Id,
            Specialty = createDoctorDto.Specialty,
            Phone = createDoctorDto.Phone
        };

        var createdDoctor = await _doctorService.CreateDoctorAsync(doctor);

        var doctorDto = new DoctorDto
        {
            Id = createdDoctor.Id,
            UserId = createdDoctor.UserId,
            Name = createdUser.Name,
            Email = createdUser.Email,
            Specialty = createdDoctor.Specialty,
            Phone = createdDoctor.Phone
        };

        return CreatedAtAction(nameof(GetAllDoctors), doctorDto);
    }

    [HttpGet("prescriptions")]
    public async Task<IActionResult> GetAllPrescriptions()
    {
        var prescriptions = await _prescriptionService.GetExpiredPrescriptionsAsync();
        var prescriptionDtos = prescriptions.Select(p => new PrescriptionDto
        {
            Id = p.Id,
            PatientId = p.PatientId,
            PatientName = p.Patient?.Name ?? "",
            DoctorId = p.DoctorId,
            DoctorName = p.Doctor?.User?.Name ?? "",
            Medication = p.Medication,
            Dosage = p.Dosage,
            Instructions = p.Instructions,
            IssueDate = p.IssueDate,
            ExpiryDate = p.ExpiryDate,
            RenewalCount = p.RenewalCount
        });

        return Ok(prescriptionDtos);
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetUnsentNotifications()
    {
        var notifications = await _notificationService.GetUnsentNotificationsAsync();
        return Ok(notifications);
    }

    [HttpPost("notifications/{id}/send")]
    public async Task<IActionResult> SendNotification(int id)
    {
        await _notificationService.MarkAsSentAsync(id);
        return Ok(new { message = "Notification marked as sent" });
    }

    [HttpGet("reports/appointments")]
    public async Task<IActionResult> GetAppointmentReport()
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync();
        
        var report = new
        {
            TotalAppointments = appointments.Count(),
            ByStatus = appointments.GroupBy(a => a.Status)
                                 .Select(g => new { Status = g.Key.ToString(), Count = g.Count() }),
            ByMonth = appointments.GroupBy(a => new { a.Date.Year, a.Date.Month })
                                .Select(g => new { 
                                    Month = $"{g.Key.Year}-{g.Key.Month:00}", 
                                    Count = g.Count() 
                                })
                                .OrderBy(x => x.Month)
        };

        return Ok(report);
    }
}
