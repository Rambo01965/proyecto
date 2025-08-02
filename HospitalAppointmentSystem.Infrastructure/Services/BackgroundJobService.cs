using Hangfire;
using Microsoft.Extensions.Logging;
using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Infrastructure.Jobs;

namespace HospitalAppointmentSystem.Infrastructure.Services;

public class BackgroundJobService : IBackgroundJobService
{
    private readonly ILogger<BackgroundJobService> _logger;

    public BackgroundJobService(ILogger<BackgroundJobService> logger)
    {
        _logger = logger;
    }

    public string ScheduleAppointmentReminder(int appointmentId, DateTime appointmentDateTime)
    {
        var jobId = BackgroundJob.Schedule<AppointmentReminderJob>(
            job => job.SendAppointmentReminderAsync(appointmentId),
            appointmentDateTime);

        _logger.LogInformation("Scheduled appointment reminder job {JobId} for appointment {AppointmentId} at {ReminderTime}", 
            jobId, appointmentId, appointmentDateTime);

        return jobId;
    }

    public string ScheduleRecurringCleanupJob()
    {
        var jobId = "appointment-cleanup";
        RecurringJob.AddOrUpdate<AppointmentCleanupJob>(
            jobId,
            job => job.CleanupExpiredAppointmentsAsync(),
            Cron.Daily(2)); // Run daily at 2 AM

        _logger.LogInformation("Scheduled recurring appointment cleanup job with ID: {JobId}", jobId);
        return jobId;
    }

    public void DeleteJob(string jobId)
    {
        BackgroundJob.Delete(jobId);
        _logger.LogInformation("Cancelled background job {JobId}", jobId);
    }

    public string EnqueueEmailJob(string toEmail, string toName, string subject, string htmlContent, string textContent = "")
    {
        var jobId = BackgroundJob.Enqueue<EmailJob>(
            job => job.SendEmailAsync(toEmail, toName, subject, htmlContent));

        _logger.LogInformation("Enqueued email job {JobId} for {Email}", jobId, toEmail);
        return jobId;
    }

    public string EnqueueSmsJob(string phoneNumber, string message)
    {
        var jobId = BackgroundJob.Enqueue<SmsJob>(
            job => job.SendSmsAsync(phoneNumber, message));

        _logger.LogInformation("Enqueued SMS job {JobId} for {PhoneNumber}", jobId, phoneNumber);
        return jobId;
    }
}

// Background job classes
public class AppointmentReminderJob
{
    private readonly IAppointmentService _appointmentService;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<AppointmentReminderJob> _logger;

    public AppointmentReminderJob(
        IAppointmentService appointmentService,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<AppointmentReminderJob> logger)
    {
        _appointmentService = appointmentService;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task SendAppointmentReminderAsync(int appointmentId)
    {
        try
        {
            var appointment = await _appointmentService.GetByIdAsync(appointmentId);
            if (appointment == null)
            {
                _logger.LogWarning("Appointment {AppointmentId} not found for reminder", appointmentId);
                return;
            }

            var appointmentDateTime = appointment.Date.ToDateTime(appointment.Time);
            var patientName = appointment.Patient?.Name ?? "Patient";
            var patientEmail = appointment.Patient?.Email ?? "";
            var doctorName = appointment.Doctor?.User?.Name ?? "Doctor";

            // Send email reminder
            if (!string.IsNullOrEmpty(patientEmail))
            {
                await _emailService.SendAppointmentReminderAsync(
                    patientEmail, patientName, doctorName, appointmentDateTime, appointment.Time);
            }

            // Send SMS reminder if phone number is available
            var phoneNumber = appointment.Doctor?.Phone; // Assuming we'll add phone to patient later
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                await _smsService.SendAppointmentReminderSmsAsync(
                    phoneNumber, patientName, doctorName, appointmentDateTime, appointment.Time);
            }

            _logger.LogInformation("Sent appointment reminder for appointment {AppointmentId}", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending appointment reminder for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }
}

public class AppointmentCleanupJob
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentCleanupJob> _logger;

    public AppointmentCleanupJob(IAppointmentService appointmentService, ILogger<AppointmentCleanupJob> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    public async Task CleanupExpiredAppointmentsAsync()
    {
        try
        {
            // Get appointments older than 30 days that are completed or cancelled
            var cutoffDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
            var appointments = await _appointmentService.GetByDateRangeAsync(DateOnly.MinValue, cutoffDate);
            
            var expiredAppointments = appointments.Where(a => 
                a.Status == HospitalAppointmentSystem.Domain.Enums.AppointmentStatus.Completed ||
                a.Status == HospitalAppointmentSystem.Domain.Enums.AppointmentStatus.Cancelled).ToList();

            foreach (var appointment in expiredAppointments)
            {
                // In a real scenario, you might archive instead of delete
                await _appointmentService.DeleteAsync(appointment.Id);
            }

            _logger.LogInformation("Cleaned up {Count} expired appointments", expiredAppointments.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during appointment cleanup");
            throw;
        }
    }
}

public class EmailJob
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailJob> _logger;

    public EmailJob(IEmailService emailService, ILogger<EmailJob> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlContent)
    {
        try
        {
            await _emailService.SendEmailAsync(toEmail, toName, subject, htmlContent);
            _logger.LogInformation("Background email job completed for {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in background email job for {Email}", toEmail);
            throw;
        }
    }
}

public class SmsJob
{
    private readonly ISmsService _smsService;
    private readonly ILogger<SmsJob> _logger;

    public SmsJob(ISmsService smsService, ILogger<SmsJob> logger)
    {
        _smsService = smsService;
        _logger = logger;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            await _smsService.SendSmsAsync(phoneNumber, message);
            _logger.LogInformation("Background SMS job completed for {PhoneNumber}", phoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in background SMS job for {PhoneNumber}", phoneNumber);
            throw;
        }
    }
}
