namespace HospitalAppointmentSystem.Domain.Entities;

public class Doctor
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Specialty { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
