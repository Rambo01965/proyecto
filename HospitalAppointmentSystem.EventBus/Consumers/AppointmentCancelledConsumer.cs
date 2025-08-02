using HospitalAppointmentSystem.EventBus.Events;
using HospitalAppointmentSystem.EventBus.Interfaces;
using Microsoft.Extensions.Logging;

namespace HospitalAppointmentSystem.EventBus.Consumers;

public class AppointmentCancelledConsumer : IEventHandler<AppointmentCancelledEvent>
{
    private readonly ILogger<AppointmentCancelledConsumer> _logger;

    public AppointmentCancelledConsumer(ILogger<AppointmentCancelledConsumer> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(AppointmentCancelledEvent @event)
    {
        _logger.LogInformation("Processing AppointmentCancelledEvent for Appointment ID: {AppointmentId}", @event.AppointmentId);

        try
        {
            // Send cancellation email to patient
            await SendCancellationEmail(@event);

            // Send notification to doctor
            await SendDoctorNotification(@event);

            // Log appointment cancellation
            await LogAppointmentCancellation(@event);

            // Update availability (if needed)
            await UpdateDoctorAvailability(@event);

            _logger.LogInformation("Successfully processed AppointmentCancelledEvent for Appointment ID: {AppointmentId}", @event.AppointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AppointmentCancelledEvent for Appointment ID: {AppointmentId}", @event.AppointmentId);
            throw;
        }
    }

    private async Task SendCancellationEmail(AppointmentCancelledEvent @event)
    {
        // TODO: Integrate with actual email service in Phase 3
        _logger.LogInformation("Sending cancellation email to {PatientEmail} for appointment on {Date} at {Time}. Reason: {CancellationReason}",
            @event.PatientEmail, @event.Date, @event.Time, @event.CancellationReason);

        // Simulate email sending
        await Task.Delay(100);
        
        _logger.LogInformation("Cancellation email sent successfully to {PatientEmail}", @event.PatientEmail);
    }

    private async Task SendDoctorNotification(AppointmentCancelledEvent @event)
    {
        // TODO: Integrate with notification service
        _logger.LogInformation("Sending cancellation notification to Dr. {DoctorName} about cancelled appointment with {PatientName}",
            @event.DoctorName, @event.PatientName);

        // Simulate notification sending
        await Task.Delay(50);
        
        _logger.LogInformation("Doctor cancellation notification sent successfully to Dr. {DoctorName}", @event.DoctorName);
    }

    private async Task LogAppointmentCancellation(AppointmentCancelledEvent @event)
    {
        _logger.LogInformation("Appointment cancelled - ID: {AppointmentId}, Patient: {PatientName}, Doctor: {DoctorName}, Date: {Date}, Time: {Time}, Reason: {CancellationReason}",
            @event.AppointmentId, @event.PatientName, @event.DoctorName, @event.Date, @event.Time, @event.CancellationReason);

        await Task.CompletedTask;
    }

    private async Task UpdateDoctorAvailability(AppointmentCancelledEvent @event)
    {
        _logger.LogInformation("Updating doctor availability for Dr. {DoctorName} on {Date} at {Time}",
            @event.DoctorName, @event.Date, @event.Time);

        // TODO: Implement logic to mark the time slot as available again
        await Task.Delay(25);
        
        _logger.LogInformation("Doctor availability updated successfully for Dr. {DoctorName}", @event.DoctorName);
    }
}
