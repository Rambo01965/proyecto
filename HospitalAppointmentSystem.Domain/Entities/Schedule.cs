namespace HospitalAppointmentSystem.Domain.Entities;

public class Schedule
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation properties
    public Doctor Doctor { get; set; } = null!;
}
