using HospitalAppointmentSystem.Domain.Enums;

namespace HospitalAppointmentSystem.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Doctor? Doctor { get; set; }
    public ICollection<Appointment> PatientAppointments { get; set; } = new List<Appointment>();
    public ICollection<Prescription> PatientPrescriptions { get; set; } = new List<Prescription>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
