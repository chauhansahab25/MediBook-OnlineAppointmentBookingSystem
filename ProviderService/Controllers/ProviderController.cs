using Microsoft.AspNetCore.Mvc;
using ProviderService.Entities;
using ProviderService.Services;

namespace ProviderService.Controllers;

[ApiController]
[Route("api/v1/providers")]
[Produces("application/json")]
public class ProviderController : ControllerBase
{
    private readonly IProviderService _service;

    public ProviderController(IProviderService service)
    {
        _service = service;
    }

    /// <summary>Register a new healthcare provider</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] Provider provider)
    {
        var result = await _service.RegisterProvider(provider);
        return Ok(result);
    }

    /// <summary>Get all providers</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var providers = await _service.GetAllProviders();
        return Ok(providers);
    }

    /// <summary>Get a provider by ID</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var provider = await _service.GetProviderById(id);

        if (provider == null)
        {
            return NotFound(new { message = "Provider not found." });
        }

        return Ok(provider);
    }

    /// <summary>Get providers by specialization</summary>
    [HttpGet("specialization/{specialization}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySpecialization(string specialization)
    {
        var providers = await _service.GetBySpecialization(specialization);
        return Ok(providers);
    }

    /// <summary>Search providers by name or specialization</summary>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string text)
    {
        var providers = await _service.SearchProviders(text);
        return Ok(providers);
    }

    /// <summary>Update a provider profile</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] Provider provider)
    {
        try
        {
            var updated = await _service.UpdateProvider(id, provider);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Verify a provider — Admin only</summary>
    [HttpPut("{id}/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify(int id)
    {
        var result = await _service.VerifyProvider(id);

        if (!result)
        {
            return NotFound(new { message = "Provider not found." });
        }

        return Ok(new { message = "Provider verified successfully." });
    }

    /// <summary>Set provider availability status</summary>
    [HttpPut("{id}/availability")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAvailability(int id, [FromQuery] bool available)
    {
        var result = await _service.SetAvailability(id, available);

        if (!result)
        {
            return NotFound(new { message = "Provider not found." });
        }

        return Ok(new { message = $"Availability set to {available}." });
    }

    /// <summary>Update provider average rating</summary>
    [HttpPut("{id}/rating")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRating(int id, [FromQuery] double rating)
    {
        var result = await _service.UpdateRating(id, rating);

        if (!result)
        {
            return NotFound(new { message = "Provider not found." });
        }

        return Ok(new { message = $"Rating updated to {rating}." });
    }

    /// <summary>Delete a provider</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteProvider(id);

        if (!result)
        {
            return NotFound(new { message = "Provider not found." });
        }

        return Ok(new { message = "Provider deleted successfully." });
    }
}