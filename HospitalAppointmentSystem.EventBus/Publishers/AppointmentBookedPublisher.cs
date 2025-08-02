using HospitalAppointmentSystem.EventBus.Events;
using HospitalAppointmentSystem.EventBus.Interfaces;
using Microsoft.Extensions.Logging;

namespace HospitalAppointmentSystem.EventBus.Publishers;

public class AppointmentBookedPublisher
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<AppointmentBookedPublisher> _logger;

    public AppointmentBookedPublisher(IEventBus eventBus, ILogger<AppointmentBookedPublisher> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task PublishAppointmentBookedAsync(
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
        try
        {
            var appointmentBookedEvent = new AppointmentBookedEvent(
                appointmentId,
                patientId,
                doctorId,
                date,
                time,
                reason,
                patientName,
                patientEmail,
                doctorName);

            await _eventBus.PublishAsync(appointmentBookedEvent);

            _logger.LogInformation("Published AppointmentBookedEvent for Appointment ID: {AppointmentId}", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing AppointmentBookedEvent for Appointment ID: {AppointmentId}", appointmentId);
            throw;
        }
    }
}

public class AppointmentCancelledPublisher
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<AppointmentCancelledPublisher> _logger;

    public AppointmentCancelledPublisher(IEventBus eventBus, ILogger<AppointmentCancelledPublisher> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task PublishAppointmentCancelledAsync(
        int appointmentId,
        int patientId,
        int doctorId,
        DateOnly date,
        TimeOnly time,
        string reason,
        string patientName,
        string patientEmail,
        string doctorName,
        string cancellationReason)
    {
        try
        {
            var appointmentCancelledEvent = new AppointmentCancelledEvent(
                appointmentId,
                patientId,
                doctorId,
                date,
                time,
                reason,
                patientName,
                patientEmail,
                doctorName,
                cancellationReason);

            await _eventBus.PublishAsync(appointmentCancelledEvent);

            _logger.LogInformation("Published AppointmentCancelledEvent for Appointment ID: {AppointmentId}", appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing AppointmentCancelledEvent for Appointment ID: {AppointmentId}", appointmentId);
            throw;
        }
    }
}
