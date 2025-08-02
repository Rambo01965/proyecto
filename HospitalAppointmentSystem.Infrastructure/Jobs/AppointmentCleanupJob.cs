using Microsoft.Extensions.Logging;
using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Enums;

namespace HospitalAppointmentSystem.Infrastructure.Jobs;

public class AppointmentCleanupJob
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ILogger<AppointmentCleanupJob> _logger;

    public AppointmentCleanupJob(IAppointmentRepository appointmentRepository, ILogger<AppointmentCleanupJob> logger)
    {
        _appointmentRepository = appointmentRepository;
        _logger = logger;
    }

    public async Task CleanupExpiredAppointmentsAsync()
    {
        try
        {
            _logger.LogInformation("Starting appointment cleanup job");

            // Get appointments older than 30 days that are completed or cancelled
            var cutoffDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
            var appointments = await _appointmentRepository.GetAllAsync();
            
            var expiredAppointments = appointments.Where(a => 
                a.Date < cutoffDate && 
                (a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.Cancelled))
                .ToList();

            _logger.LogInformation("Found {Count} expired appointments to clean up", expiredAppointments.Count);

            foreach (var appointment in expiredAppointments)
            {
                await _appointmentRepository.DeleteAsync(appointment.Id);
                _logger.LogDebug("Deleted expired appointment {AppointmentId}", appointment.Id);
            }

            _logger.LogInformation("Appointment cleanup job completed successfully. Cleaned up {Count} appointments", expiredAppointments.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during appointment cleanup job");
            throw;
        }
    }
}
