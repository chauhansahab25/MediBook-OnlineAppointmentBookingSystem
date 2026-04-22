namespace PaymentService.DTOs;

public class ProcessPaymentDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int ProviderId { get; set; }
    public decimal Amount { get; set; }
    public string Mode { get; set; } = "Card";
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
}

public class RefundPaymentDto
{
    public int PaymentId { get; set; }
    public string? Reason { get; set; }
}

public class UpdatePaymentStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
}

public class PaymentResponseDto
{
    public int PaymentId { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int ProviderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RevenueResponseDto
{
    public int ProviderId { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalPayments { get; set; }
} 
