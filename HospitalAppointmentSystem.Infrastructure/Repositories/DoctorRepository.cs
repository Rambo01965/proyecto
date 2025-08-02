using Microsoft.EntityFrameworkCore;
using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Infrastructure.Data;

namespace HospitalAppointmentSystem.Infrastructure.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly ApplicationDbContext _context;

    public DoctorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Doctor?> GetByIdAsync(int id)
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Schedules)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Doctor?> GetByUserIdAsync(int userId)
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Schedules)
            .FirstOrDefaultAsync(d => d.UserId == userId);
    }

    public async Task<Doctor> AddAsync(Doctor doctor)
    {
        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();
        return doctor;
    }

    public async Task<Doctor> UpdateAsync(Doctor doctor)
    {
        _context.Doctors.Update(doctor);
        await _context.SaveChangesAsync();
        return doctor;
    }

    public async Task DeleteAsync(int id)
    {
        var doctor = await GetByIdAsync(id);
        if (doctor != null)
        {
            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync()
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Schedules)
            .ToListAsync();
    }

    public async Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty)
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Schedules)
            .Where(d => d.Specialty.ToLower().Contains(specialty.ToLower()))
            .ToListAsync();
    }
}
