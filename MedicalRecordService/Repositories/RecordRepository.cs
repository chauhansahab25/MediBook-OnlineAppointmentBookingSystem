using MedicalRecordService.Data;
using MedicalRecordService.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.Repositories;

public class RecordRepository : IRecordRepository
{
    private readonly AppDbContext _context;

    public RecordRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MedicalRecord?> FindById(int recordId)
    {
        return await _context.MedicalRecords.FindAsync(recordId);
    }

    public async Task<MedicalRecord?> FindByAppointmentId(int appointmentId)
    {
        return await _context.MedicalRecords
            .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
    }

    public async Task<List<MedicalRecord>> FindByPatientId(int patientId)
    {
        return await _context.MedicalRecords
            .Where(r => r.PatientId == patientId)
            .ToListAsync();
    }

    public async Task<List<MedicalRecord>> FindByProviderId(int providerId)
    {
        return await _context.MedicalRecords
            .Where(r => r.ProviderId == providerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MedicalRecord>> FindByPatientIdOrderByCreatedAtDesc(int patientId)
    {
        return await _context.MedicalRecords
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<MedicalRecord>> FindByFollowUpDate(DateTime date)
    {
        return await _context.MedicalRecords
            .Where(r => r.FollowUpDate.HasValue
                     && r.FollowUpDate.Value.Date == date.Date)
            .ToListAsync();
    }

    public async Task<int> CountByPatientId(int patientId)
    {
        return await _context.MedicalRecords
            .CountAsync(r => r.PatientId == patientId);
    }

    public async Task<bool> DeleteByRecordId(int recordId)
    {
        var record = await _context.MedicalRecords.FindAsync(recordId);

        if (record == null)
        {
            return false;
        }

        _context.MedicalRecords.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MedicalRecord> Add(MedicalRecord record)
    {
        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();
        return record;
    }

    public async Task<MedicalRecord> Update(MedicalRecord record)
    {
        _context.MedicalRecords.Update(record);
        await _context.SaveChangesAsync();
        return record;
    }
}
