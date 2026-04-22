using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalRecordService.Entities;

public class MedicalRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RecordId { get; set; }

    // One record per appointment — enforced by unique index
    [Required]
    public int AppointmentId { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int ProviderId { get; set; }

    // Stored encrypted (AES-256)
    [Required]
    public string Diagnosis { get; set; } = string.Empty;

    // Stored encrypted (AES-256)
    public string? Prescription { get; set; }

    // Stored encrypted (AES-256)
    public string? Notes { get; set; }

    // AWS S3 document URL
    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
