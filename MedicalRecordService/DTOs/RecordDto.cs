namespace MedicalRecordService.DTOs;

public class CreateRecordDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int ProviderId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Prescription { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime? FollowUpDate { get; set; }
}

public class UpdateRecordDto
{
    public string Diagnosis { get; set; } = string.Empty;
    public string? Prescription { get; set; }
    public string? Notes { get; set; }
    public DateTime? FollowUpDate { get; set; }
}

public class AttachDocumentDto
{
    public string AttachmentUrl { get; set; } = string.Empty;
}

public class RecordResponseDto
{
    public int RecordId { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int ProviderId { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string? Prescription { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 
