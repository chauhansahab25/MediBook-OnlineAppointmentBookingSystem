using MedicalRecordService.Entities;

namespace MedicalRecordService.Repositories;

public interface IRecordRepository
{
    Task<MedicalRecord?> FindById(int recordId);

    Task<MedicalRecord?> FindByAppointmentId(int appointmentId);

    Task<List<MedicalRecord>> FindByPatientId(int patientId);

    Task<List<MedicalRecord>> FindByProviderId(int providerId);

    Task<List<MedicalRecord>> FindByPatientIdOrderByCreatedAtDesc(int patientId);

    Task<List<MedicalRecord>> FindByFollowUpDate(DateTime date);

    Task<int> CountByPatientId(int patientId);

    Task<bool> DeleteByRecordId(int recordId);

    Task<MedicalRecord> Add(MedicalRecord record);

    Task<MedicalRecord> Update(MedicalRecord record);
} 
