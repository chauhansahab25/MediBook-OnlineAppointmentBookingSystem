using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Entities;

namespace PaymentService.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> FindById(int paymentId)
    {
        return await _context.Payments.FindAsync(paymentId);
    }

    public async Task<Payment?> FindByAppointmentId(int appointmentId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
    }

    public async Task<List<Payment>> FindByPatientId(int patientId)
    {
        return await _context.Payments
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Payment>> FindByStatus(string status)
    {
        return await _context.Payments
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Payment?> FindByTransactionId(string transactionId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
    }

    public async Task<List<Payment>> FindByProviderId(int providerId)
    {
        return await _context.Payments
            .Where(p => p.ProviderId == providerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> SumAmountByPatientId(int patientId)
    {
        return await _context.Payments
            .Where(p => p.PatientId == patientId && p.Status == "Paid")
            .SumAsync(p => p.Amount);
    }

    public async Task<List<Payment>> FindByPaidAtBetween(
        DateTime startDate, DateTime endDate)
    {
        return await _context.Payments
            .Where(p => p.PaidAt.HasValue
                     && p.PaidAt.Value >= startDate
                     && p.PaidAt.Value <= endDate)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();
    }

    public async Task<Payment> Add(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment> Update(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
        return payment;
    }
} 
