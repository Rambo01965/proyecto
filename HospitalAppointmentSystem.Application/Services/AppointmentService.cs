using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Domain.Enums;
// using HospitalAppointmentSystem.EventBus.Publishers; // Temporarily disabled

namespace HospitalAppointmentSystem.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    // private readonly AppointmentBookedPublisher _appointmentBookedPublisher; // Temporarily disabled
    // private readonly AppointmentCancelledPublisher _appointmentCancelledPublisher; // Temporarily disabled

    public AppointmentService(
        IAppointmentRepository appointmentRepository)
        // AppointmentBookedPublisher appointmentBookedPublisher, // Temporarily disabled
        // AppointmentCancelledPublisher appointmentCancelledPublisher) // Temporarily disabled
    {
        _appointmentRepository = appointmentRepository;
        // _appointmentBookedPublisher = appointmentBookedPublisher; // Temporarily disabled
        // _appointmentCancelledPublisher = appointmentCancelledPublisher; // Temporarily disabled
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _appointmentRepository.GetByIdAsync(id);
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        // Validate that the time slot is available
        var isAvailable = await _appointmentRepository.IsTimeSlotAvailableAsync(
            appointment.DoctorId, appointment.Date, appointment.Time);

        if (!isAvailable)
        {
            throw new InvalidOperationException("The selected time slot is not available.");
        }

        appointment.Status = AppointmentStatus.Scheduled;
        var createdAppointment = await _appointmentRepository.AddAsync(appointment);

        // Publish AppointmentBooked event (temporarily disabled)
        // try
        // {
        //     await _appointmentBookedPublisher.PublishAppointmentBookedAsync(
        //         createdAppointment.Id,
        //         createdAppointment.PatientId,
        //         createdAppointment.DoctorId,
        //         createdAppointment.Date,
        //         createdAppointment.Time,
        //         createdAppointment.Reason,
        //         createdAppointment.Patient?.Name ?? "Unknown Patient",
        //         createdAppointment.Patient?.Email ?? "unknown@email.com",
        //         createdAppointment.Doctor?.User?.Name ?? "Unknown Doctor");
        // }
        // catch (Exception ex)
        // {
        //     // Log the error but don't fail the appointment creation
        //     // In a real scenario, you might want to use a proper logger
        //     Console.WriteLine($"Failed to publish AppointmentBooked event: {ex.Message}");
        // }

        return createdAppointment;
    }

    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        var existingAppointment = await _appointmentRepository.GetByIdAsync(appointment.Id);
        if (existingAppointment == null)
        {
            throw new ArgumentException("Appointment not found.");
        }

        // If date or time is being changed, validate availability
        if (existingAppointment.Date != appointment.Date || existingAppointment.Time != appointment.Time)
        {
            var isAvailable = await _appointmentRepository.IsTimeSlotAvailableAsync(
                appointment.DoctorId, appointment.Date, appointment.Time);

            if (!isAvailable)
            {
                throw new InvalidOperationException("The selected time slot is not available.");
            }
        }

        return await _appointmentRepository.UpdateAsync(appointment);
    }

    public async Task DeleteAsync(int id)
    {
        await _appointmentRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _appointmentRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
    {
        return await _appointmentRepository.GetByPatientIdAsync(patientId);
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId)
    {
        return await _appointmentRepository.GetByDoctorIdAsync(doctorId);
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _appointmentRepository.GetByDateRangeAsync(startDate, endDate);
    }

    public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateOnly date, TimeOnly time)
    {
        return await _appointmentRepository.IsTimeSlotAvailableAsync(doctorId, date, time);
    }

    public async Task<Appointment> UpdateStatusAsync(int id, AppointmentStatus status)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            throw new ArgumentException("Appointment not found.");
        }

        var previousStatus = appointment.Status;
        appointment.Status = status;
        var updatedAppointment = await _appointmentRepository.UpdateAsync(appointment);

        // Publish AppointmentCancelled event if status changed to Cancelled (temporarily disabled)
        // if (status == AppointmentStatus.Cancelled && previousStatus != AppointmentStatus.Cancelled)
        // {
        //     try
        //     {
        //         await _appointmentCancelledPublisher.PublishAppointmentCancelledAsync(
        //             updatedAppointment.Id,
        //             updatedAppointment.PatientId,
        //             updatedAppointment.DoctorId,
        //             updatedAppointment.Date,
        //             updatedAppointment.Time,
        //             updatedAppointment.Reason,
        //             updatedAppointment.Patient?.Name ?? "Unknown Patient",
        //             updatedAppointment.Patient?.Email ?? "unknown@email.com",
        //             updatedAppointment.Doctor?.User?.Name ?? "Unknown Doctor",
        //             "Appointment cancelled by user");
        //     }
        //     catch (Exception ex)
        //     {
        //         // Log the error but don't fail the status update
        //         Console.WriteLine($"Failed to publish AppointmentCancelled event: {ex.Message}");
        //     }
        // }

        return updatedAppointment;
    }

    public async Task<Appointment> CancelAppointmentAsync(int id, string cancellationReason)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        if (appointment == null)
        {
            throw new ArgumentException("Appointment not found.");
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException("Appointment is already cancelled.");
        }

        var previousStatus = appointment.Status;
        appointment.Status = AppointmentStatus.Cancelled;
        var updatedAppointment = await _appointmentRepository.UpdateAsync(appointment);

        // Publish AppointmentCancelled event (temporarily disabled)
        // try
        // {
        //     await _appointmentCancelledPublisher.PublishAppointmentCancelledAsync(
        //         updatedAppointment.Id,
        //         updatedAppointment.PatientId,
        //         updatedAppointment.DoctorId,
        //         updatedAppointment.Date,
        //         updatedAppointment.Time,
        //         updatedAppointment.Reason,
        //         updatedAppointment.Patient?.Name ?? "Unknown Patient",
        //         updatedAppointment.Patient?.Email ?? "unknown@email.com",
        //         updatedAppointment.Doctor?.User?.Name ?? "Unknown Doctor",
        //         cancellationReason);
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine($"Failed to publish AppointmentCancelled event: {ex.Message}");
        // }

        return updatedAppointment;
    }

    // Alias methods for consistency with controllers
    public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
    {
        return await CreateAsync(appointment);
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await GetAllAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(int patientId)
    {
        return await GetByPatientIdAsync(patientId);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(int doctorId)
    {
        return await GetByDoctorIdAsync(doctorId);
    }
}
