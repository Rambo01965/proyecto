namespace HospitalAppointmentSystem.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlContent, string textContent = "");
    Task<bool> SendAppointmentConfirmationAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime, string reason);
    Task<bool> SendAppointmentCancellationAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime, string reason);
    Task<bool> SendAppointmentReminderAsync(string patientEmail, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime);
}
