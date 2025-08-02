using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Application.Interfaces;

public interface IPrescriptionRepository
{
    Task<Prescription> CreateAsync(Prescription prescription);
    Task<Prescription?> GetByIdAsync(int id);
    Task<IEnumerable<Prescription>> GetByPatientIdAsync(int patientId);
    Task<IEnumerable<Prescription>> GetByDoctorIdAsync(int doctorId);
    Task<Prescription> UpdateAsync(Prescription prescription);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Prescription>> GetExpiredAsync();
    Task<IEnumerable<Prescription>> GetExpiringInDaysAsync(int days);
    Task<IEnumerable<Prescription>> GetAllAsync();
}
