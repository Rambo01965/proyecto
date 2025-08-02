using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
}
