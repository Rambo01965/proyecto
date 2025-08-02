using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Domain.Enums;

namespace HospitalAppointmentSystem.Application.Interfaces;

public interface IAppointmentService
{
    Task<Appointment?> GetByIdAsync(int id);
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<Appointment> UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateOnly date, TimeOnly time);
    Task<Appointment> UpdateStatusAsync(int id, AppointmentStatus status);
    Task<Appointment> CancelAppointmentAsync(int id, string cancellationReason);
    Task<Appointment> CreateAppointmentAsync(Appointment appointment);
    Task<IEnumerable<Appointment>> GetAllAppointmentsAsync();
    Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(int patientId);
    Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(int doctorId);
}
