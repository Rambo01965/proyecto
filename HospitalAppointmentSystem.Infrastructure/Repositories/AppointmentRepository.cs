using Microsoft.EntityFrameworkCore;
using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Infrastructure.Data;

namespace HospitalAppointmentSystem.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.Feedback)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Appointment> AddAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        // Get the existing appointment from the database with navigation properties
        var existingAppointment = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(a => a.Id == appointment.Id);

        if (existingAppointment == null)
        {
            throw new ArgumentException("Appointment not found.");
        }

        // Update only the scalar properties, not navigation properties
        existingAppointment.Date = appointment.Date;
        existingAppointment.Time = appointment.Time;
        existingAppointment.Status = appointment.Status;
        existingAppointment.Reason = appointment.Reason;
        // Don't update PatientId and DoctorId as they shouldn't change
        // Don't update CreatedAt as it's immutable

        await _context.SaveChangesAsync();
        return existingAppointment;
    }

    public async Task DeleteAsync(int id)
    {
        var appointment = await GetByIdAsync(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Time)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Where(a => a.PatientId == patientId)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Time)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Time)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Where(a => a.Date >= startDate && a.Date <= endDate)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Time)
            .ToListAsync();
    }

    public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateOnly date, TimeOnly time)
    {
        return !await _context.Appointments
            .AnyAsync(a => a.DoctorId == doctorId && 
                          a.Date == date && 
                          a.Time == time &&
                          a.Status != Domain.Enums.AppointmentStatus.Cancelled);
    }
}
