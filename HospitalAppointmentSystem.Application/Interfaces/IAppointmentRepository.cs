using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Application.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(int id);
    Task<Appointment> AddAsync(Appointment appointment);
    Task<Appointment> UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);
    Task<IEnumerable<Appointment>> GetAllAsync();
    Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate);
    Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateOnly date, TimeOnly time);
}
