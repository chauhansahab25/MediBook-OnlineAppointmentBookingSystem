using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProviderService.DTOs;
using ProviderService.Entities;
using ProviderService.Services;
using System.Text.Json;

namespace ProviderService.Controllers;

[ApiController]
[Route("api/v1/providers")]
public class ProviderController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProviderController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProviders([FromQuery] bool? verifiedOnly)
    {
        List<Provider> providers;
        if (verifiedOnly.HasValue)
        {
            providers = await _providerService.GetAllProviders(verifiedOnly.Value);
        }
        else
        {
            providers = await _providerService.GetAllProviders(false);
        }
        
        // Enrich with user data
        var enrichedProviders = new List<object>();
        using var httpClient = new HttpClient();
        var authServiceUrl = "http://localhost:5219/api/v1/auth";
        
        foreach (var provider in providers)
        {
            string fullName = $"User #{provider.UserId}";
            string email = "N/A";
            string role = "Unknown";
            
            try
            {
                var response = await httpClient.GetAsync($"{authServiceUrl}/users/{provider.UserId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("fullName", out var nameElement))
                        fullName = nameElement.GetString() ?? fullName;
                    if (root.TryGetProperty("email", out var emailElement))
                        email = emailElement.GetString() ?? email;
                    if (root.TryGetProperty("role", out var roleElement))
                        role = roleElement.GetString() ?? role;
                    
                    // Skip if user is not a Provider
                    if (role != "Provider")
                        continue;
                }
            }
            catch { /* Use defaults */ }
            
            // Fetch Rating from ReviewService
            double avgRating = 0;
            try
            {
                var reviewServiceUrl = "http://localhost:5211/api/v1/reviews"; 
                var ratingResponse = await httpClient.GetAsync($"{reviewServiceUrl}/provider/{provider.ProviderId}/avgrating");
                if (ratingResponse.IsSuccessStatusCode)
                {
                    var ratingJson = await ratingResponse.Content.ReadAsStringAsync();
                    using var ratingDoc = JsonDocument.Parse(ratingJson);
                    if (ratingDoc.RootElement.TryGetProperty("averageRating", out var avgElement))
                        avgRating = avgElement.GetDouble();
                }
            }
            catch { avgRating = provider.AvgRating; }

            enrichedProviders.Add(new
            {
                provider.ProviderId,
                provider.UserId,
                FullName = fullName,
                Email = email,
                provider.Specialization,
                provider.Qualification,
                provider.ExperienceYears,
                provider.Bio,
                provider.ClinicName,
                provider.ClinicAddress,
                AvgRating = avgRating,
                provider.IsVerified,
                provider.IsAvailable,
                provider.CreatedAt
            });
        }
        
        return Ok(enrichedProviders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProviderById(int id)
    {
        try
        {
            var provider = await _providerService.GetEnrichedProviderById(id);
            if (provider == null)
            {
                return NotFound(new { message = "Provider not found." });
            }
            return Ok(provider);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetProviderByUserId(int userId)
    {
        try
        {
            var provider = await _providerService.GetProviderByUserId(userId);
            return Ok(provider);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProviders([FromQuery] string term)
    {
        var providers = await _providerService.SearchProviders(term);
        return Ok(providers);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncProvider([FromBody] SyncProviderDto dto)
    {
        try
        {
            var provider = await _providerService.SyncFromAuthService(dto);
            return Ok(provider);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateProvider([FromBody] ProviderDto dto)
    {
        try
        {
            var provider = await _providerService.CreateProvider(dto);
            return CreatedAtAction(nameof(GetProviderById), new { id = provider.ProviderId }, provider);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProvider(int id, [FromBody] ProviderDto dto)
    {
        try
        {
            var provider = await _providerService.UpdateProvider(id, dto);
            return Ok(provider);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProvider(int id)
    {
        bool success = await _providerService.DeleteProvider(id);
        if (!success)
        {
            return NotFound(new { message = "Provider not found." });
        }
        return Ok(new { message = "Provider deleted successfully." });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/verify")]
    public async Task<IActionResult> VerifyProvider(int id)
    {
        try
        {
            var provider = await _providerService.VerifyProvider(id);
            return Ok(provider);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/unverify")]
    public async Task<IActionResult> UnverifyProvider(int id)
    {
        try
        {
            var provider = await _providerService.UnverifyProvider(id);
            return Ok(provider);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/verification-status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVerificationStatus(int id)
    {
        try
        {
            var provider = await _providerService.GetProviderById(id);
            if (provider == null)
            {
                return NotFound(new { message = "Provider not found." });
            }
            return Ok(new { providerId = provider.ProviderId, isVerified = provider.IsVerified });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/rating")]
    public async Task<IActionResult> UpdateRating(int id, [FromQuery] double rating)
    {
        var result = await _providerService.UpdateRating(id, rating);
        if (!result)
        {
            return NotFound(new { message = "Provider not found." });
        }
        return Ok(new { message = "Rating updated successfully." });
    }
}
