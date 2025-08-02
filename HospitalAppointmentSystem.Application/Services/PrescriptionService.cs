using HospitalAppointmentSystem.Application.Interfaces;
using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Application.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDoctorRepository _doctorRepository;

    public PrescriptionService(
        IPrescriptionRepository prescriptionRepository,
        IUserRepository userRepository,
        IDoctorRepository doctorRepository)
    {
        _prescriptionRepository = prescriptionRepository;
        _userRepository = userRepository;
        _doctorRepository = doctorRepository;
    }

    public async Task<Prescription> CreatePrescriptionAsync(Prescription prescription)
    {
        // Validate patient exists
        var patient = await _userRepository.GetByIdAsync(prescription.PatientId);
        if (patient == null)
            throw new ArgumentException("Patient not found");

        // Validate doctor exists
        var doctor = await _doctorRepository.GetByIdAsync(prescription.DoctorId);
        if (doctor == null)
            throw new ArgumentException("Doctor not found");

        prescription.IssueDate = DateTime.UtcNow;
        prescription.RenewalCount = 0;

        return await _prescriptionRepository.CreateAsync(prescription);
    }

    public async Task<Prescription?> GetPrescriptionByIdAsync(int id)
    {
        return await _prescriptionRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Prescription>> GetPrescriptionsByPatientIdAsync(int patientId)
    {
        return await _prescriptionRepository.GetByPatientIdAsync(patientId);
    }

    public async Task<IEnumerable<Prescription>> GetPrescriptionsByDoctorIdAsync(int doctorId)
    {
        return await _prescriptionRepository.GetByDoctorIdAsync(doctorId);
    }

    public async Task<Prescription> UpdatePrescriptionAsync(Prescription prescription)
    {
        var existingPrescription = await _prescriptionRepository.GetByIdAsync(prescription.Id);
        if (existingPrescription == null)
            throw new ArgumentException("Prescription not found");

        return await _prescriptionRepository.UpdateAsync(prescription);
    }

    public async Task<bool> DeletePrescriptionAsync(int id)
    {
        return await _prescriptionRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Prescription>> GetExpiredPrescriptionsAsync()
    {
        return await _prescriptionRepository.GetExpiredAsync();
    }

    public async Task<IEnumerable<Prescription>> GetPrescriptionsExpiringInDaysAsync(int days)
    {
        return await _prescriptionRepository.GetExpiringInDaysAsync(days);
    }

    public async Task<Prescription> RenewPrescriptionAsync(int prescriptionId)
    {
        var prescription = await _prescriptionRepository.GetByIdAsync(prescriptionId);
        if (prescription == null)
            throw new ArgumentException("Prescription not found");

        // Create new prescription based on existing one
        var renewedPrescription = new Prescription
        {
            PatientId = prescription.PatientId,
            DoctorId = prescription.DoctorId,
            Medication = prescription.Medication,
            Dosage = prescription.Dosage,
            Instructions = prescription.Instructions,
            IssueDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMonths(1), // Default 1 month renewal
            RenewalCount = prescription.RenewalCount + 1
        };

        return await _prescriptionRepository.CreateAsync(renewedPrescription);
    }

    public async Task<bool> ValidatePrescriptionAsync(int prescriptionId)
    {
        var prescription = await _prescriptionRepository.GetByIdAsync(prescriptionId);
        if (prescription == null)
            return false;

        return prescription.ExpiryDate > DateTime.UtcNow;
    }
}
