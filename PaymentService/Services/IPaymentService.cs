using PaymentService.DTOs;

namespace PaymentService.Services;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPayment(ProcessPaymentDto dto);

    Task<PaymentResponseDto?> GetPaymentByAppointment(int appointmentId);

    Task<List<PaymentResponseDto>> GetPaymentsByPatient(int patientId);

    Task<List<PaymentResponseDto>> GetPaymentHistory(DateTime startDate, DateTime endDate);

    Task<bool> RefundPayment(RefundPaymentDto dto);

    Task<string> GetPaymentStatus(int paymentId);

    Task<bool> UpdatePaymentStatus(int paymentId, UpdatePaymentStatusDto dto);

    Task<byte[]> GenerateInvoice(int paymentId);

    Task<RevenueResponseDto> GetTotalRevenue(int providerId);
} 
