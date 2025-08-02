using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Application.Interfaces;

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(int id);
    Task<Doctor?> GetByUserIdAsync(int userId);
    Task<Doctor> AddAsync(Doctor doctor);
    Task<Doctor> UpdateAsync(Doctor doctor);
    Task DeleteAsync(int id);
    Task<IEnumerable<Doctor>> GetAllAsync();
    Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty);
}
