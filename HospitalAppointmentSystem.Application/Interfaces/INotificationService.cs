using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Application.Interfaces;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(int userId, string type, string message);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
    Task<IEnumerable<Notification>> GetUnsentNotificationsAsync();
    Task MarkAsSentAsync(int notificationId);
    Task SendEmailNotificationAsync(string email, string subject, string message);
    Task SendSmsNotificationAsync(string phoneNumber, string message);
    Task SendAppointmentReminderAsync(int appointmentId);
    Task SendAppointmentConfirmationAsync(int appointmentId);
    Task SendAppointmentCancellationAsync(int appointmentId);
}
