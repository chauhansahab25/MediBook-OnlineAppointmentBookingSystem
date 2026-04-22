using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppointmentService.Entities;

public class Appointment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AppointmentId { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public int ProviderId { get; set; }

    [Required]
    public int SlotId { get; set; }

    [Required]
    [MaxLength(150)]
    public string ServiceType { get; set; } = string.Empty;

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    // Scheduled, Completed, Cancelled, No-Show
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Scheduled";

    public string? Notes { get; set; }

    // InPerson, Teleconsultation
    [Required]
    [MaxLength(50)]
    public string ModeOfConsultation { get; set; } = "InPerson";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
} 
