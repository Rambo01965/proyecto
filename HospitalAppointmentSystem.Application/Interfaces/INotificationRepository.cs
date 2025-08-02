using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Application.Interfaces;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<Notification?> GetByIdAsync(int id);
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Notification>> GetUnsentAsync();
    Task<Notification> UpdateAsync(Notification notification);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Notification>> GetAllAsync();
}
