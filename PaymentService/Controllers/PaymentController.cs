using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/v1/payments")]
[Produces("application/json")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentController(IPaymentService service)
    {
        _service = service;
    }

    /// <summary>Process a new payment for an appointment</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
    {
        try
        {
            var result = await _service.ProcessPayment(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get payment by appointment ID</summary>
    [HttpGet("appointment/{appointmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByAppointment(int appointmentId)
    {
        var payment = await _service.GetPaymentByAppointment(appointmentId);

        if (payment == null)
        {
            return NotFound(new { message = "Payment not found." });
        }

        return Ok(payment);
    }

    /// <summary>Get all payments for a patient</summary>
    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        var payments = await _service.GetPaymentsByPatient(patientId);
        return Ok(payments);
    }

    /// <summary>Get payment history between two dates</summary>
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var payments = await _service.GetPaymentHistory(startDate, endDate);
        return Ok(payments);
    }

    /// <summary>Get payment status by payment ID</summary>
    [HttpGet("{paymentId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(int paymentId)
    {
        try
        {
            var status = await _service.GetPaymentStatus(paymentId);
            return Ok(new { paymentId, status });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Get total revenue for a provider</summary>
    [HttpGet("provider/{providerId}/revenue")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotalRevenue(int providerId)
    {
        var revenue = await _service.GetTotalRevenue(providerId);
        return Ok(revenue);
    }

    /// <summary>Download invoice PDF for a payment</summary>
    [HttpGet("{paymentId}/invoice")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInvoice(int paymentId)
    {
        try
        {
            var pdfBytes = await _service.GenerateInvoice(paymentId);

            return File(
                pdfBytes,
                "application/pdf",
                $"Invoice_Payment_{paymentId}.pdf"
            );
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Refund a payment</summary>
    [HttpPost("refund")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Refund([FromBody] RefundPaymentDto dto)
    {
        try
        {
            var result = await _service.RefundPayment(dto);

            if (!result)
            {
                return NotFound(new { message = "Payment not found." });
            }

            return Ok(new { message = "Payment refunded successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Update payment status manually</summary>
    [HttpPut("{paymentId}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int paymentId,
        [FromBody] UpdatePaymentStatusDto dto)
    {
        var result = await _service.UpdatePaymentStatus(paymentId, dto);

        if (!result)
        {
            return NotFound(new { message = "Payment not found." });
        }

        return Ok(new { message = $"Payment status updated to {dto.Status}." });
    }
}
