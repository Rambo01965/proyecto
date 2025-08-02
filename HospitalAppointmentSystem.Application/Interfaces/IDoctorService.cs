using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Application.Interfaces;

public interface IDoctorService
{
    Task<Doctor?> GetByIdAsync(int id);
    Task<Doctor?> GetByUserIdAsync(int userId);
    Task<Doctor> CreateAsync(Doctor doctor);
    Task<Doctor> UpdateAsync(Doctor doctor);
    Task DeleteAsync(int id);
    Task<IEnumerable<Doctor>> GetAllAsync();
    Task<IEnumerable<Doctor>> GetAllDoctorsAsync(); // Alias for GetAllAsync
    Task<Doctor> CreateDoctorAsync(Doctor doctor); // Alias for CreateAsync
    Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty);
    Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync(DateOnly date, TimeOnly time);
}
