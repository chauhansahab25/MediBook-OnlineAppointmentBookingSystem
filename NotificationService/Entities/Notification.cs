using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotificationService.Entities;

public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int NotificationId { get; set; }

    [Required]
    public int RecipientId { get; set; }

    // BOOKING, REMINDER, CANCELLATION, PAYMENT, FOLLOWUP
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    // APP, EMAIL, SMS
    [Required]
    [MaxLength(20)]
    public string Channel { get; set; } = "APP";

    // e.g., AppointmentId, PaymentId
    public int? RelatedId { get; set; }

    // e.g., "Appointment", "Payment"
    [MaxLength(100)]
    public string? RelatedType { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
