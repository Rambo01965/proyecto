using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public DoctorService(IDoctorRepository doctorRepository, IAppointmentRepository appointmentRepository)
    {
        _doctorRepository = doctorRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Doctor?> GetByIdAsync(int id)
    {
        return await _doctorRepository.GetByIdAsync(id);
    }

    public async Task<Doctor?> GetByUserIdAsync(int userId)
    {
        return await _doctorRepository.GetByUserIdAsync(userId);
    }

    public async Task<Doctor> CreateAsync(Doctor doctor)
    {
        return await _doctorRepository.AddAsync(doctor);
    }

    public async Task<Doctor> UpdateAsync(Doctor doctor)
    {
        return await _doctorRepository.UpdateAsync(doctor);
    }

    public async Task DeleteAsync(int id)
    {
        await _doctorRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Doctor>> GetAllAsync()
    {
        return await _doctorRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
    {
        return await GetAllAsync();
    }

    public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
    {
        return await CreateAsync(doctor);
    }

    public async Task<IEnumerable<Doctor>> GetBySpecialtyAsync(string specialty)
    {
        return await _doctorRepository.GetBySpecialtyAsync(specialty);
    }

    public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync(DateOnly date, TimeOnly time)
    {
        var allDoctors = await _doctorRepository.GetAllAsync();
        var availableDoctors = new List<Doctor>();

        foreach (var doctor in allDoctors)
        {
            var isAvailable = await _appointmentRepository.IsTimeSlotAvailableAsync(doctor.Id, date, time);
            if (isAvailable && IsDoctorScheduledForTime(doctor, date, time))
            {
                availableDoctors.Add(doctor);
            }
        }

        return availableDoctors;
    }

    private bool IsDoctorScheduledForTime(Doctor doctor, DateOnly date, TimeOnly time)
    {
        var dayOfWeek = date.DayOfWeek.ToString();
        var schedule = doctor.Schedules.FirstOrDefault(s => 
            s.DayOfWeek.Equals(dayOfWeek, StringComparison.OrdinalIgnoreCase) && 
            s.IsAvailable);

        if (schedule == null) return false;

        return time >= schedule.StartTime && time <= schedule.EndTime;
    }
}
