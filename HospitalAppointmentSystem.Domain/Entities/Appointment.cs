using HospitalAppointmentSystem.Domain.Enums;

namespace HospitalAppointmentSystem.Domain.Entities;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public AppointmentStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Feedback? Feedback { get; set; }
}
