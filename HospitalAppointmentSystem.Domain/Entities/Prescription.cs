namespace HospitalAppointmentSystem.Domain.Entities;

public class Prescription
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string Medication { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; }
    public int RenewalCount { get; set; } = 0;

    // Navigation properties
    public User Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
}
