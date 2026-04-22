using System.Security.Cryptography;
using System.Text;
using MedicalRecordService.DTOs;
using MedicalRecordService.Entities;
using MedicalRecordService.Repositories;

namespace MedicalRecordService.Services;

public class RecordService : IRecordService
{
    private readonly IRecordRepository _repo;
    private readonly IConfiguration _configuration;

    public RecordService(IRecordRepository repo, IConfiguration configuration)
    {
        _repo = repo;
        _configuration = configuration;
    }

    // ── Create Record ─────────────────────────────────────────────────────────

    public async Task<RecordResponseDto> CreateRecord(CreateRecordDto dto)
    {
        // Check if record already exists for this appointment
        var existing = await _repo.FindByAppointmentId(dto.AppointmentId);

        if (existing != null)
        {
            throw new InvalidOperationException(
                "A medical record already exists for this appointment.");
        }

        var record = new MedicalRecord
        {
            AppointmentId = dto.AppointmentId,
            PatientId = dto.PatientId,
            ProviderId = dto.ProviderId,

            // Encrypt sensitive fields before saving
            Diagnosis = Encrypt(dto.Diagnosis),
            Prescription = dto.Prescription != null ? Encrypt(dto.Prescription) : null,
            Notes = dto.Notes != null ? Encrypt(dto.Notes) : null,

            AttachmentUrl = dto.AttachmentUrl,
            FollowUpDate = dto.FollowUpDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var saved = await _repo.Add(record);
        return MapToResponse(saved);
    }

    // ── Get Record By Appointment ─────────────────────────────────────────────

    public async Task<RecordResponseDto?> GetRecordByAppointment(int appointmentId)
    {
        var record = await _repo.FindByAppointmentId(appointmentId);

        if (record == null)
        {
            return null;
        }

        return MapToResponse(record);
    }

    // ── Get Records By Patient ────────────────────────────────────────────────

    public async Task<List<RecordResponseDto>> GetRecordsByPatient(int patientId)
    {
        var records = await _repo.FindByPatientIdOrderByCreatedAtDesc(patientId);
        return records.Select(MapToResponse).ToList();
    }

    // ── Get Records By Provider ───────────────────────────────────────────────

    public async Task<List<RecordResponseDto>> GetRecordsByProvider(int providerId)
    {
        var records = await _repo.FindByProviderId(providerId);
        return records.Select(MapToResponse).ToList();
    }

    // ── Get Record By ID ──────────────────────────────────────────────────────

    public async Task<RecordResponseDto?> GetRecordById(int recordId)
    {
        var record = await _repo.FindById(recordId);

        if (record == null)
        {
            return null;
        }

        return MapToResponse(record);
    }

    // ── Update Record ─────────────────────────────────────────────────────────

    public async Task<RecordResponseDto> UpdateRecord(int recordId, UpdateRecordDto dto)
    {
        var record = await _repo.FindById(recordId);

        if (record == null)
        {
            throw new KeyNotFoundException("Medical record not found.");
        }

        // Re-encrypt updated fields
        record.Diagnosis = Encrypt(dto.Diagnosis);
        record.Prescription = dto.Prescription != null ? Encrypt(dto.Prescription) : null;
        record.Notes = dto.Notes != null ? Encrypt(dto.Notes) : null;
        record.FollowUpDate = dto.FollowUpDate;
        record.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.Update(record);
        return MapToResponse(updated);
    }

    // ── Delete Record ─────────────────────────────────────────────────────────

    public async Task<bool> DeleteRecord(int recordId)
    {
        return await _repo.DeleteByRecordId(recordId);
    }

    // ── Get Follow-Up Records for Today ──────────────────────────────────────

    public async Task<List<RecordResponseDto>> GetFollowUpRecords(DateTime date)
    {
        var records = await _repo.FindByFollowUpDate(date);
        return records.Select(MapToResponse).ToList();
    }

    // ── Get Record Count For Patient ──────────────────────────────────────────

    public async Task<int> GetRecordCount(int patientId)
    {
        return await _repo.CountByPatientId(patientId);
    }

    // ── Attach Document (AWS S3 URL) ──────────────────────────────────────────

    public async Task<RecordResponseDto> AttachDocument(int recordId, AttachDocumentDto dto)
    {
        var record = await _repo.FindById(recordId);

        if (record == null)
        {
            throw new KeyNotFoundException("Medical record not found.");
        }

        record.AttachmentUrl = dto.AttachmentUrl;
        record.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.Update(record);
        return MapToResponse(updated);
    }

    // ── AES-256 Encryption ────────────────────────────────────────────────────

    private string Encrypt(string plainText)
    {
        try
        {
            string key = _configuration["Encryption:Key"]
                ?? "DefaultEncryptionKey12345678901234";

            // Key must be exactly 32 bytes for AES-256
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();

            // Prepend IV to the encrypted data
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }
        catch
        {
            // Fallback — return plain text if encryption fails in dev
            return plainText;
        }
    }

    // ── AES-256 Decryption ────────────────────────────────────────────────────

    private string Decrypt(string cipherText)
    {
        try
        {
            string key = _configuration["Encryption:Key"]
                ?? "DefaultEncryptionKey12345678901234";

            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            byte[] fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = keyBytes;

            // Extract IV from the first 16 bytes
            byte[] iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aes.IV = iv;

            byte[] cipherBytes = new byte[fullCipher.Length - iv.Length];
            Array.Copy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
        catch
        {
            // Fallback — return as-is if not encrypted (dev mode)
            return cipherText;
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private RecordResponseDto MapToResponse(MedicalRecord r)
    {
        return new RecordResponseDto
        {
            RecordId = r.RecordId,
            AppointmentId = r.AppointmentId,
            PatientId = r.PatientId,
            ProviderId = r.ProviderId,

            // Decrypt before returning to client
            Diagnosis = Decrypt(r.Diagnosis),
            Prescription = r.Prescription != null ? Decrypt(r.Prescription) : null,
            Notes = r.Notes != null ? Decrypt(r.Notes) : null,

            AttachmentUrl = r.AttachmentUrl,
            FollowUpDate = r.FollowUpDate,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
} 
