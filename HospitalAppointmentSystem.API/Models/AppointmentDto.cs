using HospitalAppointmentSystem.Domain.Enums;

namespace HospitalAppointmentSystem.API.Models;

public class AppointmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public UserDto? Patient { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public DoctorDto? Doctor { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
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
    public DateOnly? Date { get; set; }
    public TimeOnly? Time { get; set; }
    public AppointmentStatus? Status { get; set; }
    public string? Reason { get; set; }
}
