using HospitalAppointmentSystem.Domain.Entities;

namespace HospitalAppointmentSystem.Application.Interfaces;

public interface IPrescriptionService
{
    Task<Prescription> CreatePrescriptionAsync(Prescription prescription);
    Task<Prescription?> GetPrescriptionByIdAsync(int id);
    Task<IEnumerable<Prescription>> GetPrescriptionsByPatientIdAsync(int patientId);
    Task<IEnumerable<Prescription>> GetPrescriptionsByDoctorIdAsync(int doctorId);
    Task<Prescription> UpdatePrescriptionAsync(Prescription prescription);
    Task<bool> DeletePrescriptionAsync(int id);
    Task<IEnumerable<Prescription>> GetExpiredPrescriptionsAsync();
    Task<IEnumerable<Prescription>> GetPrescriptionsExpiringInDaysAsync(int days);
    Task<Prescription> RenewPrescriptionAsync(int prescriptionId);
    Task<bool> ValidatePrescriptionAsync(int prescriptionId);
}
