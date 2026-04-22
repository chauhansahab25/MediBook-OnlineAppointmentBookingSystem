using MedicalRecordService.DTOs;
using MedicalRecordService.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedicalRecordService.Controllers;

[ApiController]
[Route("api/v1/records")]
[Produces("application/json")]
public class RecordController : ControllerBase
{
    private readonly IRecordService _service;

    public RecordController(IRecordService service)
    {
        _service = service;
    }

    /// <summary>Create a new medical record for a completed appointment</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateRecordDto dto)
    {
        try
        {
            var result = await _service.CreateRecord(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Get medical record by appointment ID</summary>
    [HttpGet("appointment/{appointmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByAppointment(int appointmentId)
    {
        var record = await _service.GetRecordByAppointment(appointmentId);

        if (record == null)
        {
            return NotFound(new { message = "Medical record not found." });
        }

        return Ok(record);
    }

    /// <summary>Get medical record by record ID</summary>
    [HttpGet("{recordId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int recordId)
    {
        var record = await _service.GetRecordById(recordId);

        if (record == null)
        {
            return NotFound(new { message = "Medical record not found." });
        }

        return Ok(record);
    }

    /// <summary>Get all medical records for a patient</summary>
    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        var records = await _service.GetRecordsByPatient(patientId);
        return Ok(records);
    }

    /// <summary>Get total record count for a patient</summary>
    [HttpGet("patient/{patientId}/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount(int patientId)
    {
        var count = await _service.GetRecordCount(patientId);
        return Ok(new { patientId, count });
    }

    /// <summary>Get all medical records created by a provider</summary>
    [HttpGet("provider/{providerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProvider(int providerId)
    {
        var records = await _service.GetRecordsByProvider(providerId);
        return Ok(records);
    }

    /// <summary>Get all records with follow-up date on a specific date</summary>
    [HttpGet("followups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFollowUps([FromQuery] DateTime date)
    {
        var records = await _service.GetFollowUpRecords(date);
        return Ok(records);
    }

    /// <summary>Update a medical record</summary>
    [HttpPut("{recordId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int recordId, [FromBody] UpdateRecordDto dto)
    {
        try
        {
            var result = await _service.UpdateRecord(recordId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Attach a document URL (AWS S3) to a medical record</summary>
    [HttpPut("{recordId}/attach")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AttachDocument(
        int recordId,
        [FromBody] AttachDocumentDto dto)
    {
        try
        {
            var result = await _service.AttachDocument(recordId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Delete a medical record</summary>
    [HttpDelete("{recordId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int recordId)
    {
        var result = await _service.DeleteRecord(recordId);

        if (!result)
        {
            return NotFound(new { message = "Medical record not found." });
        }

        return Ok(new { message = "Medical record deleted successfully." });
    }
}
