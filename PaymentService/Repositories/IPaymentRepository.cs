using PaymentService.Entities;

namespace PaymentService.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> FindById(int paymentId);

    Task<Payment?> FindByAppointmentId(int appointmentId);

    Task<List<Payment>> FindByPatientId(int patientId);

    Task<List<Payment>> FindByStatus(string status);

    Task<Payment?> FindByTransactionId(string transactionId);

    Task<List<Payment>> FindByProviderId(int providerId);

    Task<decimal> SumAmountByPatientId(int patientId);

    Task<List<Payment>> FindByPaidAtBetween(DateTime startDate, DateTime endDate);

    Task<Payment> Add(Payment payment);

    Task<Payment> Update(Payment payment);
}
