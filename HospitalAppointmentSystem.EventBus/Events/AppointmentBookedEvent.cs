namespace HospitalAppointmentSystem.EventBus.Events;

public class AppointmentBookedEvent : IEvent
{
    public Guid Id { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public int AppointmentId { get; private set; }
    public int PatientId { get; private set; }
    public int DoctorId { get; private set; }
    public DateOnly Date { get; private set; }
    public TimeOnly Time { get; private set; }
    public string Reason { get; private set; }
    public string PatientName { get; private set; }
    public string PatientEmail { get; private set; }
    public string DoctorName { get; private set; }

    public AppointmentBookedEvent(
        int appointmentId,
        int patientId,
        int doctorId,
        DateOnly date,
        TimeOnly time,
        string reason,
        string patientName,
        string patientEmail,
        string doctorName)
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        AppointmentId = appointmentId;
        PatientId = patientId;
        DoctorId = doctorId;
        Date = date;
        Time = time;
        Reason = reason;
        PatientName = patientName;
        PatientEmail = patientEmail;
        DoctorName = doctorName;
    }
}
