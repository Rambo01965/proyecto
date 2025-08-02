using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HospitalAppointmentSystem.API.Models;
using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Domain.Enums;

namespace HospitalAppointmentSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var appointments = await _appointmentService.GetAllAsync();
        var appointmentDtos = appointments.Select(MapToDto);

        return Ok(appointmentDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null)
            return NotFound();

        return Ok(MapToDto(appointment));
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatientId(int patientId)
    {
        // Check if current user is the patient or an admin/doctor
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (currentUserId != patientId && userRole != "Admin" && userRole != "Doctor")
            return Forbid();

        var appointments = await _appointmentService.GetByPatientIdAsync(patientId);
        var appointmentDtos = appointments.Select(MapToDto);

        return Ok(appointmentDtos);
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctorId(int doctorId)
    {
        var appointments = await _appointmentService.GetByDoctorIdAsync(doctorId);
        var appointmentDtos = appointments.Select(MapToDto);

        return Ok(appointmentDtos);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentDto createAppointmentDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var appointment = new Appointment
            {
                PatientId = createAppointmentDto.PatientId,
                DoctorId = createAppointmentDto.DoctorId,
                Date = createAppointmentDto.Date,
                Time = createAppointmentDto.Time,
                Reason = createAppointmentDto.Reason
            };

            var createdAppointment = await _appointmentService.CreateAsync(appointment);
            var completeAppointment = await _appointmentService.GetByIdAsync(createdAppointment.Id);

            return CreatedAtAction(nameof(GetById), new { id = createdAppointment.Id }, MapToDto(completeAppointment!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDto updateAppointmentDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingAppointment = await _appointmentService.GetByIdAsync(id);
        if (existingAppointment == null)
            return NotFound();

        try
        {
            // Update only provided fields
            if (updateAppointmentDto.Date.HasValue)
                existingAppointment.Date = updateAppointmentDto.Date.Value;
            
            if (updateAppointmentDto.Time.HasValue)
                existingAppointment.Time = updateAppointmentDto.Time.Value;
            
            if (updateAppointmentDto.Status.HasValue)
                existingAppointment.Status = updateAppointmentDto.Status.Value;
            
            if (!string.IsNullOrEmpty(updateAppointmentDto.Reason))
                existingAppointment.Reason = updateAppointmentDto.Reason;

            var updatedAppointment = await _appointmentService.UpdateAsync(existingAppointment);
            return Ok(MapToDto(updatedAppointment));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AppointmentStatus status)
    {
        try
        {
            var updatedAppointment = await _appointmentService.UpdateStatusAsync(id, status);
            return Ok(MapToDto(updatedAppointment));
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null)
            return NotFound();

        await _appointmentService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("availability")]
    public async Task<IActionResult> CheckAvailability([FromQuery] int doctorId, [FromQuery] DateOnly date, [FromQuery] TimeOnly time)
    {
        var isAvailable = await _appointmentService.IsTimeSlotAvailableAsync(doctorId, date, time);
        return Ok(new { isAvailable });
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            Date = appointment.Date,
            Time = appointment.Time,
            Status = appointment.Status,
            Reason = appointment.Reason,
            CreatedAt = appointment.CreatedAt,
            Patient = new UserDto
            {
                Id = appointment.Patient.Id,
                Email = appointment.Patient.Email,
                Name = appointment.Patient.Name,
                Role = appointment.Patient.Role,
                CreatedAt = appointment.Patient.CreatedAt
            },
            Doctor = new DoctorDto
            {
                Id = appointment.Doctor.Id,
                UserId = appointment.Doctor.UserId,
                Specialty = appointment.Doctor.Specialty,
                Phone = appointment.Doctor.Phone,
                User = new UserDto
                {
                    Id = appointment.Doctor.User.Id,
                    Email = appointment.Doctor.User.Email,
                    Name = appointment.Doctor.User.Name,
                    Role = appointment.Doctor.User.Role,
                    CreatedAt = appointment.Doctor.User.CreatedAt
                }
            }
        };
    }
}
