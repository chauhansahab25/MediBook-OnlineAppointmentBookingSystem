using Microsoft.AspNetCore.Mvc;
using ScheduleService.DTOs;
using ScheduleService.Services;

namespace ScheduleService.Controllers;

[ApiController]
[Route("api/v1/slots")]
[Produces("application/json")]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _service;

    public ScheduleController(IScheduleService service)
    {
        _service = service;
    }

    /// <summary>Add a single availability slot</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddSlot([FromBody] AddSlotDto dto)
    {
        var slot = await _service.AddSlot(dto);
        return Ok(slot);
    }

    /// <summary>Add multiple slots at once</summary>
    [HttpPost("bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddBulkSlots([FromBody] List<BulkSlotDto> dtos)
    {
        var slots = await _service.AddBulkSlots(dtos);
        return Ok(slots);
    }

    /// <summary>Generate recurring slots (Daily or Weekly)</summary>
    [HttpPost("generateRecurring")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateRecurring([FromBody] RecurringSlotDto dto)
    {
        try
        {
            var slots = await _service.GenerateRecurringSlots(dto);
            return Ok(slots);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get all slots for a provider</summary>
    [HttpGet("provider/{providerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProvider(int providerId)
    {
        var slots = await _service.GetSlotsByProvider(providerId);
        return Ok(slots);
    }

    /// <summary>Get available slots for a provider on a specific date</summary>
    [HttpGet("provider/{providerId}/available")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailable(int providerId, [FromQuery] DateTime date)
    {
        var slots = await _service.GetAvailableSlots(providerId, date);
        return Ok(slots);
    }

    /// <summary>Get a slot by ID</summary>
    [HttpGet("{slotId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int slotId)
    {
        var slot = await _service.GetSlotById(slotId);

        if (slot == null)
        {
            return NotFound(new { message = "Slot not found." });
        }

        return Ok(slot);
    }

    /// <summary>Update a slot</summary>
    [HttpPut("{slotId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int slotId, [FromBody] UpdateSlotDto dto)
    {
        try
        {
            var updated = await _service.UpdateSlot(slotId, dto);
            return Ok(updated);
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

    /// <summary>Block a slot (for leave or unavailability)</summary>
    [HttpPut("{slotId}/block")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Block(int slotId)
    {
        try
        {
            var result = await _service.BlockSlot(slotId);

            if (!result)
            {
                return NotFound(new { message = "Slot not found." });
            }

            return Ok(new { message = "Slot blocked successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Unblock a slot</summary>
    [HttpPut("{slotId}/unblock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unblock(int slotId)
    {
        var result = await _service.UnblockSlot(slotId);

        if (!result)
        {
            return NotFound(new { message = "Slot not found." });
        }

        return Ok(new { message = "Slot unblocked successfully." });
    }

    /// <summary>Book a slot — called by Appointment Service</summary>
    [HttpPut("{slotId}/book")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Book(int slotId)
    {
        try
        {
            var result = await _service.BookSlot(slotId);

            if (!result)
            {
                return NotFound(new { message = "Slot not found." });
            }

            return Ok(new { message = "Slot booked successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Unbook a slot — release back to available</summary>
    [HttpPut("{slotId}/unbook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unbook(int slotId)
    {
        var result = await _service.UnbookSlot(slotId);

        if (!result)
        {
            return NotFound(new { message = "Slot not found." });
        }

        return Ok(new { message = "Slot released back to available." });
    }

    /// <summary>Delete a slot</summary>
    [HttpDelete("{slotId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int slotId)
    {
        var result = await _service.DeleteSlot(slotId);

        if (!result)
        {
            return NotFound(new { message = "Slot not found." });
        }

        return Ok(new { message = "Slot deleted successfully." });
    }
} 
