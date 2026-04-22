using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentService.Entities;

public class Payment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PaymentId { get; set; }

    [Required]
    public int AppointmentId { get; set; }

    [Required]
    public int PatientId { get; set; }

    public int ProviderId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    // Pending, Paid, Refunded, Failed
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Pending";

    // Card, UPI, Wallet, Cash
    [Required]
    [MaxLength(50)]
    public string Mode { get; set; } = "Card";

    [MaxLength(200)]
    public string? TransactionId { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = "USD";

    public DateTime? PaidAt { get; set; }

    public DateTime? RefundedAt { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 
