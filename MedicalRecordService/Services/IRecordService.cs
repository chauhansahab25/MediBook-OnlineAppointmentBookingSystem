using MedicalRecordService.DTOs;

namespace MedicalRecordService.Services;

public interface IRecordService
{
    Task<RecordResponseDto> CreateRecord(CreateRecordDto dto);

    Task<RecordResponseDto?> GetRecordByAppointment(int appointmentId);

    Task<List<RecordResponseDto>> GetRecordsByPatient(int patientId);

    Task<List<RecordResponseDto>> GetRecordsByProvider(int providerId);

    Task<RecordResponseDto?> GetRecordById(int recordId);

    Task<RecordResponseDto> UpdateRecord(int recordId, UpdateRecordDto dto);

    Task<bool> DeleteRecord(int recordId);

    Task<List<RecordResponseDto>> GetFollowUpRecords(DateTime date);

    Task<int> GetRecordCount(int patientId);

    Task<RecordResponseDto> AttachDocument(int recordId, AttachDocumentDto dto);
} 
