namespace HospitalAppointmentSystem.Domain.Entities;

public class Feedback
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int Rating { get; set; } // 1-5 stars
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Appointment Appointment { get; set; } = null!;
}
