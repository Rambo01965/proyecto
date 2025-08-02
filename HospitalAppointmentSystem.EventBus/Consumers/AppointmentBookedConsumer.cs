using HospitalAppointmentSystem.EventBus.Events;
using HospitalAppointmentSystem.EventBus.Interfaces;
using Microsoft.Extensions.Logging;

namespace HospitalAppointmentSystem.EventBus.Consumers;

public class AppointmentBookedConsumer : IEventHandler<AppointmentBookedEvent>
{
    private readonly ILogger<AppointmentBookedConsumer> _logger;

    public AppointmentBookedConsumer(ILogger<AppointmentBookedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(AppointmentBookedEvent @event)
    {
        _logger.LogInformation("Processing AppointmentBookedEvent for Appointment ID: {AppointmentId}", @event.AppointmentId);

        try
        {
            // Send confirmation email to patient
            await SendConfirmationEmail(@event);

            // Send notification to doctor
            await SendDoctorNotification(@event);

            // Log appointment booking
            await LogAppointmentBooking(@event);

            _logger.LogInformation("Successfully processed AppointmentBookedEvent for Appointment ID: {AppointmentId}", @event.AppointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AppointmentBookedEvent for Appointment ID: {AppointmentId}", @event.AppointmentId);
            throw;
        }
    }

    private async Task SendConfirmationEmail(AppointmentBookedEvent @event)
    {
        // TODO: Integrate with actual email service in Phase 3
        _logger.LogInformation("Sending confirmation email to {PatientEmail} for appointment on {Date} at {Time}",
            @event.PatientEmail, @event.Date, @event.Time);

        // Simulate email sending
        await Task.Delay(100);
        
        _logger.LogInformation("Confirmation email sent successfully to {PatientEmail}", @event.PatientEmail);
    }

    private async Task SendDoctorNotification(AppointmentBookedEvent @event)
    {
        // TODO: Integrate with notification service
        _logger.LogInformation("Sending notification to Dr. {DoctorName} about new appointment with {PatientName}",
            @event.DoctorName, @event.PatientName);

        // Simulate notification sending
        await Task.Delay(50);
        
        _logger.LogInformation("Doctor notification sent successfully to Dr. {DoctorName}", @event.DoctorName);
    }

    private async Task LogAppointmentBooking(AppointmentBookedEvent @event)
    {
        _logger.LogInformation("Appointment booked - ID: {AppointmentId}, Patient: {PatientName}, Doctor: {DoctorName}, Date: {Date}, Time: {Time}, Reason: {Reason}",
            @event.AppointmentId, @event.PatientName, @event.DoctorName, @event.Date, @event.Time, @event.Reason);

        await Task.CompletedTask;
    }
}
