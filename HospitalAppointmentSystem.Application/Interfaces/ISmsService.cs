namespace HospitalAppointmentSystem.Application.Interfaces;

public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
    Task<bool> SendAppointmentConfirmationSmsAsync(string phoneNumber, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime);
    Task<bool> SendAppointmentCancellationSmsAsync(string phoneNumber, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime, string reason);
    Task<bool> SendAppointmentReminderSmsAsync(string phoneNumber, string patientName, string doctorName, DateTime appointmentDate, TimeOnly appointmentTime);
}
