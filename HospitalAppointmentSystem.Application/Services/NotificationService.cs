using Microsoft.Extensions.Logging;
using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Domain.Enums;

namespace HospitalAppointmentSystem.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IBackgroundJobService _backgroundJobService;

    public NotificationService(
        INotificationRepository notificationRepository,
        IAppointmentRepository appointmentRepository,
        IUserRepository userRepository,
        IDoctorRepository doctorRepository,
        IEmailService emailService,
        ISmsService smsService,
        IBackgroundJobService backgroundJobService)
    {
        _notificationRepository = notificationRepository;
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
        _doctorRepository = doctorRepository;
        _emailService = emailService;
        _smsService = smsService;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<Notification> CreateNotificationAsync(int userId, string type, string message)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            SentAt = DateTime.UtcNow // Set current time
        };

        return await _notificationRepository.CreateAsync(notification);
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId)
    {
        return await _notificationRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Notification>> GetUnsentNotificationsAsync()
    {
        return await _notificationRepository.GetUnsentAsync();
    }

    public async Task MarkAsSentAsync(int notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification != null)
        {
            notification.SentAt = DateTime.UtcNow;
            await _notificationRepository.UpdateAsync(notification);
        }
    }

    public async Task SendEmailNotificationAsync(string email, string subject, string message)
    {
        try
        {
            await _emailService.SendEmailAsync(email, "Patient", subject, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email to {email}: {ex.Message}");
            // Fallback to background job
            _backgroundJobService.EnqueueEmailJob(email, "Patient", subject, message);
        }
    }

    public async Task SendSmsNotificationAsync(string phoneNumber, string message)
    {
        try
        {
            await _smsService.SendSmsAsync(phoneNumber, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send SMS to {phoneNumber}: {ex.Message}");
            // Fallback to background job
            _backgroundJobService.EnqueueSmsJob(phoneNumber, message);
        }
    }

    public async Task SendAppointmentReminderAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null) return;

        var patient = await _userRepository.GetByIdAsync(appointment.PatientId);
        var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId);

        if (patient != null && doctor != null)
        {
            var appointmentDateTime = appointment.Date.ToDateTime(appointment.Time);
            
            // Create notification record
            await CreateNotificationAsync(patient.Id, "Email", "Appointment reminder sent");
            
            // Send email reminder
            await _emailService.SendAppointmentReminderAsync(
                patient.Email, patient.Name, doctor.User?.Name ?? "Doctor", 
                appointmentDateTime, appointment.Time);
                
            // Schedule reminder 24 hours before appointment
            var reminderTime = appointmentDateTime.AddHours(-24);
            if (reminderTime > DateTime.Now)
            {
                _backgroundJobService.ScheduleAppointmentReminder(appointmentId, reminderTime);
            }
        }
    }

    public async Task SendAppointmentConfirmationAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null) return;

        var patient = await _userRepository.GetByIdAsync(appointment.PatientId);
        var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId);

        if (patient != null && doctor != null)
        {
            var appointmentDateTime = appointment.Date.ToDateTime(appointment.Time);
            
            // Create notification record
            await CreateNotificationAsync(patient.Id, "Email", "Appointment confirmation sent");
            
            // Send email confirmation
            await _emailService.SendAppointmentConfirmationAsync(
                patient.Email, patient.Name, doctor.User?.Name ?? "Doctor", 
                appointmentDateTime, appointment.Time, appointment.Reason);
        }
    }

    public async Task SendAppointmentCancellationAsync(int appointmentId)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null) return;

        var patient = await _userRepository.GetByIdAsync(appointment.PatientId);
        var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId);

        if (patient != null && doctor != null)
        {
            var appointmentDateTime = appointment.Date.ToDateTime(appointment.Time);
            
            // Create notification record
            await CreateNotificationAsync(patient.Id, "Email", "Appointment cancellation sent");
            
            // Send email cancellation
            await _emailService.SendAppointmentCancellationAsync(
                patient.Email, patient.Name, doctor.User?.Name ?? "Doctor", 
                appointmentDateTime, appointment.Time, "Appointment cancelled by system");
        }
    }
}
