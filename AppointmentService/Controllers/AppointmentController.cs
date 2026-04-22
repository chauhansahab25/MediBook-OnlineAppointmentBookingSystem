using AppointmentService.DTOs;
using AppointmentService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentService.Controllers;

[ApiController]
[Route("api/v1/appointments")]
[Produces("application/json")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService service)
    {
        _service = service;
    }

    /// <summary>Book a new appointment</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Book([FromBody] BookAppointmentDto dto)
    {
        try
        {
            var result = await _service.BookAppointment(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get appointment by ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _service.GetById(id);

        if (appointment == null)
        {
            return NotFound(new { message = "Appointment not found." });
        }

        return Ok(appointment);
    }

    /// <summary>Get all appointments for a patient</summary>
    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        var appointments = await _service.GetByPatient(patientId);
        return Ok(appointments);
    }

    /// <summary>Get upcoming appointments for a patient</summary>
    [HttpGet("patient/{patientId}/upcoming")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUpcoming(int patientId)
    {
        var appointments = await _service.GetUpcomingByPatient(patientId);
        return Ok(appointments);
    }

    /// <summary>Get all appointments for a provider</summary>
    [HttpGet("provider/{providerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProvider(int providerId)
    {
        var appointments = await _service.GetByProvider(providerId);
        return Ok(appointments);
    }

    /// <summary>Get appointments for a provider on a specific date</summary>
    [HttpGet("provider/{providerId}/date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProviderAndDate(
        int providerId, [FromQuery] DateTime date)
    {
        var appointments = await _service.GetByProviderAndDate(providerId, date);
        return Ok(appointments);
    }

    /// <summary>Get total appointment count for a provider</summary>
    [HttpGet("provider/{providerId}/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount(int providerId)
    {
        var count = await _service.GetAppointmentCount(providerId);
        return Ok(new { providerId, count });
    }

    /// <summary>Cancel an appointment</summary>
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var result = await _service.CancelAppointment(id);

            if (!result)
            {
                return NotFound(new { message = "Appointment not found." });
            }

            return Ok(new { message = "Appointment cancelled successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Reschedule an appointment</summary>
    [HttpPut("{id}/reschedule")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reschedule(
        int id, [FromBody] RescheduleAppointmentDto dto)
    {
        try
        {
            var result = await _service.RescheduleAppointment(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Mark an appointment as complete</summary>
    [HttpPut("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            var result = await _service.CompleteAppointment(id);

            if (!result)
            {
                return NotFound(new { message = "Appointment not found." });
            }

            return Ok(new { message = "Appointment marked as completed." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Update appointment status manually</summary>
    [HttpPut("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id, [FromBody] UpdateStatusDto dto)
    {
        var result = await _service.UpdateStatus(id, dto.Status);

        if (!result)
        {
            return NotFound(new { message = "Appointment not found." });
        }

        return Ok(new { message = $"Status updated to {dto.Status}." });
    }
} 
