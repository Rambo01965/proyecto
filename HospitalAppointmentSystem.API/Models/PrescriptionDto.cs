namespace HospitalAppointmentSystem.API.Models;

public class PrescriptionDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Medication { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int RenewalCount { get; set; }
}

public class CreatePrescriptionDto
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string Medication { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
}

public class UpdatePrescriptionDto
{
    public string Medication { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public int RenewalCount { get; set; }
}
