using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;
using HospitalAppointmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointmentSystem.Infrastructure.Repositories;

public class PrescriptionRepository : IPrescriptionRepository
{
    private readonly ApplicationDbContext _context;

    public PrescriptionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Prescription> CreateAsync(Prescription prescription)
    {
        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();
        return prescription;
    }

    public async Task<Prescription?> GetByIdAsync(int id)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .ThenInclude(d => d.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Prescription>> GetByPatientIdAsync(int patientId)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .ThenInclude(d => d.User)
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.IssueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Prescription>> GetByDoctorIdAsync(int doctorId)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .ThenInclude(d => d.User)
            .Where(p => p.DoctorId == doctorId)
            .OrderByDescending(p => p.IssueDate)
            .ToListAsync();
    }

    public async Task<Prescription> UpdateAsync(Prescription prescription)
    {
        _context.Prescriptions.Update(prescription);
        await _context.SaveChangesAsync();
        return prescription;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription == null)
            return false;

        _context.Prescriptions.Remove(prescription);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Prescription>> GetExpiredAsync()
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .ThenInclude(d => d.User)
            .Where(p => p.ExpiryDate < DateTime.UtcNow)
            .OrderBy(p => p.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Prescription>> GetExpiringInDaysAsync(int days)
    {
        var targetDate = DateTime.UtcNow.AddDays(days);
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .ThenInclude(d => d.User)
            .Where(p => p.ExpiryDate <= targetDate && p.ExpiryDate > DateTime.UtcNow)
            .OrderBy(p => p.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Prescription>> GetAllAsync()
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .ThenInclude(d => d.User)
            .OrderByDescending(p => p.IssueDate)
            .ToListAsync();
    }
}
