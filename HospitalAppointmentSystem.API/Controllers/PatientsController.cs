using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HospitalAppointmentSystem.API.Models;
using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Domain.Enums;

namespace HospitalAppointmentSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAppointmentService _appointmentService;
    private readonly IPrescriptionService _prescriptionService;
    private readonly INotificationService _notificationService;

    public PatientsController(
        IUserService userService,
        IAppointmentService appointmentService,
        IPrescriptionService prescriptionService,
        INotificationService notificationService)
    {
        _userService = userService;
        _appointmentService = appointmentService;
        _prescriptionService = prescriptionService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPatients()
    {
        var users = await _userService.GetAllAsync();
        var patients = users.Where(u => u.Role == UserRole.Patient)
                           .Select(u => new UserDto
                           {
                               Id = u.Id,
                               Email = u.Email,
                               Name = u.Name,
                               Role = u.Role,
                               CreatedAt = u.CreatedAt
                           });

        return Ok(patients);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatient(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Patient)
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

    [HttpGet("{id}/appointments")]
    public async Task<IActionResult> GetPatientAppointments(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Patient)
            return NotFound("Patient not found");

        var appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(id);
        var appointmentDtos = appointments.Select(a => new AppointmentDto
        {
            Id = a.Id,
            PatientId = a.PatientId,
            PatientName = a.Patient?.Name ?? "",
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor?.User?.Name ?? "",
            Date = a.Date,
            Time = a.Time,
            Status = a.Status,
            Reason = a.Reason,
            CreatedAt = a.CreatedAt
        });

        return Ok(appointmentDtos);
    }

    [HttpGet("{id}/prescriptions")]
    public async Task<IActionResult> GetPatientPrescriptions(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Patient)
            return NotFound("Patient not found");

        var prescriptions = await _prescriptionService.GetPrescriptionsByPatientIdAsync(id);
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

    [HttpGet("{id}/notifications")]
    public async Task<IActionResult> GetPatientNotifications(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Patient)
            return NotFound("Patient not found");

        var notifications = await _notificationService.GetUserNotificationsAsync(id);
        return Ok(notifications);
    }

    [HttpPost("{id}/appointments")]
    public async Task<IActionResult> CreateAppointment(int id, [FromBody] CreateAppointmentDto createAppointmentDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Patient)
            return NotFound("Patient not found");

        var appointment = new Appointment
        {
            PatientId = id,
            DoctorId = createAppointmentDto.DoctorId,
            Date = createAppointmentDto.Date,
            Time = createAppointmentDto.Time,
            Reason = createAppointmentDto.Reason,
            Status = AppointmentStatus.Scheduled
        };

        var createdAppointment = await _appointmentService.CreateAppointmentAsync(appointment);
        
        // Send confirmation notification
        await _notificationService.SendAppointmentConfirmationAsync(createdAppointment.Id);

        var appointmentDto = new AppointmentDto
        {
            Id = createdAppointment.Id,
            PatientId = createdAppointment.PatientId,
            PatientName = createdAppointment.Patient?.Name ?? "",
            DoctorId = createdAppointment.DoctorId,
            DoctorName = createdAppointment.Doctor?.User?.Name ?? "",
            Date = createdAppointment.Date,
            Time = createdAppointment.Time,
            Status = createdAppointment.Status,
            Reason = createdAppointment.Reason,
            CreatedAt = createdAppointment.CreatedAt
        };

        return CreatedAtAction(nameof(GetPatient), new { id = createdAppointment.PatientId }, appointmentDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.GetByIdAsync(id);
        if (user == null || user.Role != UserRole.Patient)
            return NotFound("Patient not found");

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
}
