namespace HospitalAppointmentSystem.Application.Interfaces;

public interface IBackgroundJobService
{
    string EnqueueEmailJob(string toEmail, string toName, string subject, string htmlContent, string textContent = "");
    string EnqueueSmsJob(string phoneNumber, string message);
    string ScheduleAppointmentReminder(int appointmentId, DateTime appointmentDateTime);
    string ScheduleRecurringCleanupJob();
    void DeleteJob(string jobId);
}
