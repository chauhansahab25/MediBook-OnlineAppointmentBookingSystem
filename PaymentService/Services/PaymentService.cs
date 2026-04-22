using PaymentService.DTOs;
using PaymentService.Entities;
using PaymentService.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Stripe;

namespace PaymentService.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repo;
    private readonly IConfiguration _configuration;

    public PaymentService(IPaymentRepository repo, IConfiguration configuration)
    {
        _repo = repo;
        _configuration = configuration;

        // Set Stripe API key
        StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];

        // Set QuestPDF license
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ── Process Payment ───────────────────────────────────────────────────────

    public async Task<PaymentResponseDto> ProcessPayment(ProcessPaymentDto dto)
    {
        string transactionId = string.Empty;
        string status = "Paid";

        // Only call Stripe for Card payments
        if (dto.Mode == "Card")
        {
            try
            {
                var options = new ChargeCreateOptions
                {
                    Amount = (long)(dto.Amount * 100),
                    Currency = dto.Currency.ToLower(),
                    Description = $"MediBook Appointment #{dto.AppointmentId}",
                    Source = "tok_visa" // test token
                };

                var service = new ChargeService();
                var charge = await service.CreateAsync(options);

                transactionId = charge.Id;
                status = charge.Paid ? "Paid" : "Failed";
            }
            catch
            {
                // If Stripe is not configured fallback to manual
                transactionId = Guid.NewGuid().ToString();
                status = "Paid";
            }
        }
        else
        {
            // For UPI, Wallet, Cash — just record as paid
            transactionId = Guid.NewGuid().ToString();
            status = "Paid";
        }

        var payment = new Payment
        {
            AppointmentId = dto.AppointmentId,
            PatientId = dto.PatientId,
            ProviderId = dto.ProviderId,
            Amount = dto.Amount,
            Status = status,
            Mode = dto.Mode,
            TransactionId = transactionId,
            Currency = dto.Currency,
            Notes = dto.Notes,
            PaidAt = status == "Paid" ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _repo.Add(payment);
        return MapToResponse(saved);
    }

    // ── Get Payment By Appointment ────────────────────────────────────────────

    public async Task<PaymentResponseDto?> GetPaymentByAppointment(int appointmentId)
    {
        var payment = await _repo.FindByAppointmentId(appointmentId);

        if (payment == null)
        {
            return null;
        }

        return MapToResponse(payment);
    }

    // ── Get Payments By Patient ───────────────────────────────────────────────

    public async Task<List<PaymentResponseDto>> GetPaymentsByPatient(int patientId)
    {
        var payments = await _repo.FindByPatientId(patientId);
        return payments.Select(MapToResponse).ToList();
    }

    // ── Get Payment History By Date Range ─────────────────────────────────────

    public async Task<List<PaymentResponseDto>> GetPaymentHistory(
        DateTime startDate, DateTime endDate)
    {
        var payments = await _repo.FindByPaidAtBetween(startDate, endDate);
        return payments.Select(MapToResponse).ToList();
    }

    // ── Refund Payment ────────────────────────────────────────────────────────

    public async Task<bool> RefundPayment(RefundPaymentDto dto)
    {
        var payment = await _repo.FindById(dto.PaymentId);

        if (payment == null)
        {
            return false;
        }

        if (payment.Status != "Paid")
        {
            throw new InvalidOperationException("Only paid payments can be refunded.");
        }

        // Try Stripe refund for card payments
        if (payment.Mode == "Card" && !string.IsNullOrEmpty(payment.TransactionId))
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    Charge = payment.TransactionId,
                    Reason = RefundReasons.RequestedByCustomer
                };

                var service = new RefundService();
                await service.CreateAsync(options);
            }
            catch
            {
                // Continue even if Stripe refund fails in dev
            }
        }

        payment.Status = "Refunded";
        payment.RefundedAt = DateTime.UtcNow;
        payment.Notes = dto.Reason ?? payment.Notes;

        await _repo.Update(payment);
        return true;
    }

    // ── Get Payment Status ────────────────────────────────────────────────────

    public async Task<string> GetPaymentStatus(int paymentId)
    {
        var payment = await _repo.FindById(paymentId);

        if (payment == null)
        {
            throw new KeyNotFoundException("Payment not found.");
        }

        return payment.Status;
    }

    // ── Update Payment Status ─────────────────────────────────────────────────

    public async Task<bool> UpdatePaymentStatus(int paymentId, UpdatePaymentStatusDto dto)
    {
        var payment = await _repo.FindById(paymentId);

        if (payment == null)
        {
            return false;
        }

        payment.Status = dto.Status;

        if (!string.IsNullOrEmpty(dto.TransactionId))
        {
            payment.TransactionId = dto.TransactionId;
        }

        if (dto.Status == "Paid" && payment.PaidAt == null)
        {
            payment.PaidAt = DateTime.UtcNow;
        }

        await _repo.Update(payment);
        return true;
    }

    // ── Generate Invoice (QuestPDF) ───────────────────────────────────────────

    public async Task<byte[]> GenerateInvoice(int paymentId)
    {
        var payment = await _repo.FindById(paymentId);

        if (payment == null)
        {
            throw new KeyNotFoundException("Payment not found.");
        }

        // Build PDF invoice using QuestPDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(12));

                // ── Header ────────────────────────────────────────────────
                page.Header().Column(col =>
                {
                    col.Item().Text("MediBook — Payment Invoice")
                        .FontSize(22)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    col.Item().Text($"Invoice Date: {DateTime.UtcNow:dd MMM yyyy}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);
                });

                // ── Content ───────────────────────────────────────────────
                page.Content().PaddingTop(20).Column(col =>
                {
                    col.Item().BorderBottom(1).PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Text("Payment ID").Bold();
                        row.RelativeItem().Text($"#{payment.PaymentId}");
                    });

                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Appointment ID").Bold();
                        row.RelativeItem().Text($"#{payment.AppointmentId}");
                    });

                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Patient ID").Bold();
                        row.RelativeItem().Text($"#{payment.PatientId}");
                    });

                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Amount").Bold();
                        row.RelativeItem()
                            .Text($"{payment.Currency} {payment.Amount:F2}");
                    });

                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Payment Mode").Bold();
                        row.RelativeItem().Text(payment.Mode);
                    });

                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Transaction ID").Bold();
                        row.RelativeItem()
                            .Text(payment.TransactionId ?? "N/A");
                    });

                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Status").Bold();
                        row.RelativeItem().Text(payment.Status)
                            .FontColor(Colors.Green.Darken2);
                    });

                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text("Paid At").Bold();
                        row.RelativeItem()
                            .Text(payment.PaidAt.HasValue
                                ? payment.PaidAt.Value.ToString("dd MMM yyyy HH:mm")
                                : "N/A");
                    });
                });

                // ── Footer ────────────────────────────────────────────────
                page.Footer()
                    .AlignCenter()
                    .Text("Thank you for using MediBook. This is a system-generated invoice.")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
            });
        });

        return document.GeneratePdf();
    }

    // ── Get Total Revenue For Provider ────────────────────────────────────────

    public async Task<RevenueResponseDto> GetTotalRevenue(int providerId)
    {
        var payments = await _repo.FindByProviderId(providerId);

        var paidPayments = payments.Where(p => p.Status == "Paid").ToList();

        return new RevenueResponseDto
        {
            ProviderId = providerId,
            TotalRevenue = paidPayments.Sum(p => p.Amount),
            TotalPayments = paidPayments.Count
        };
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private PaymentResponseDto MapToResponse(Payment p)
    {
        return new PaymentResponseDto
        {
            PaymentId = p.PaymentId,
            AppointmentId = p.AppointmentId,
            PatientId = p.PatientId,
            ProviderId = p.ProviderId,
            Amount = p.Amount,
            Status = p.Status,
            Mode = p.Mode,
            TransactionId = p.TransactionId,
            Currency = p.Currency,
            PaidAt = p.PaidAt,
            RefundedAt = p.RefundedAt,
            Notes = p.Notes,
            CreatedAt = p.CreatedAt
        };
    }
} 
