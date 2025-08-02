using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Application.Interfaces;

public interface IUserService
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<string> AuthenticateAsync(string email, string password);
    Task<bool> ValidatePasswordAsync(User user, string password);
    string HashPassword(string password);
}
