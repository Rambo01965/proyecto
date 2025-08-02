using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointmentSystem.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _context.Notifications
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
    {
        return await _context.Notifications
            .Include(n => n.User)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetUnsentAsync()
    {
        return await _context.Notifications
            .Include(n => n.User)
            .Where(n => n.SentAt == null)
            .OrderBy(n => n.Id)
            .ToListAsync();
    }

    public async Task<Notification> UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Notification>> GetAllAsync()
    {
        return await _context.Notifications
            .Include(n => n.User)
            .OrderByDescending(n => n.Id)
            .ToListAsync();
    }
}
