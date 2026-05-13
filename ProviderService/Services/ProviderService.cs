using ProviderService.DTOs;
using ProviderService.Entities;
using ProviderService.Repositories;
using System.Text.Json;

namespace ProviderService.Services;

public class ProviderService : IProviderService
{
    private readonly IProviderRepository _repo;
    private readonly IHttpClientFactory _httpClientFactory;

    public ProviderService(IProviderRepository repo, IHttpClientFactory httpClientFactory)
    {
        _repo = repo;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Provider> RegisterProvider(Provider provider)
    {
        provider.CreatedAt = DateTime.UtcNow;
        provider.IsVerified = false;
        provider.IsAvailable = true;
        provider.AvgRating = 0;

        return await _repo.Add(provider);
    }

    public async Task<Provider> CreateProvider(ProviderDto dto)
    {
        var provider = new Provider
        {
            UserId = dto.UserId,
            Specialization = dto.Specialization,
            Qualification = dto.Qualification,
            ExperienceYears = dto.ExperienceYears,
            Bio = dto.Bio,
            ClinicName = dto.ClinicName,
            ClinicAddress = dto.ClinicAddress,
            AvgRating = 0,
            IsVerified = false,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _repo.Add(provider);
    }

    public async Task<Provider?> GetProviderById(int id)
    {
        return await _repo.FindById(id);
    }

    public async Task<EnrichedProviderDto?> GetEnrichedProviderById(int id)
    {
        var provider = await _repo.FindById(id);
        if (provider == null) return null;

        var dto = new EnrichedProviderDto
        {
            ProviderId = provider.ProviderId,
            UserId = provider.UserId,
            Specialization = provider.Specialization,
            Qualification = provider.Qualification,
            ExperienceYears = provider.ExperienceYears,
            Bio = provider.Bio,
            ClinicName = provider.ClinicName,
            ClinicAddress = provider.ClinicAddress,
            AvgRating = provider.AvgRating,
            IsVerified = provider.IsVerified,
            IsAvailable = provider.IsAvailable,
            CreatedAt = provider.CreatedAt
        };

        // Fetch user details from AuthService
        try
        {
            var client = _httpClientFactory.CreateClient("AuthService");
            var response = await client.GetAsync($"/api/v1/auth/users/{provider.UserId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<JsonElement>(json);
                if (user.TryGetProperty("fullName", out var nameElement))
                    dto.FullName = nameElement.GetString() ?? "N/A";
                if (user.TryGetProperty("email", out var emailElement))
                    dto.Email = emailElement.GetString() ?? "N/A";
            }
        }
        catch
        {
            dto.FullName = $"Provider #{provider.ProviderId}";
            dto.Email = "N/A";
        }

        return dto;
    }

    public async Task<List<Provider>> GetBySpecialization(string specialization)
    {
        return await _repo.FindBySpecialization(specialization);
    }

    public async Task<List<Provider>> SearchProviders(string text)
    {
        return await _repo.SearchByNameOrSpecialization(text);
    }

    public async Task<Provider> UpdateProvider(int id, ProviderDto dto)
    {
        var provider = await _repo.FindById(id);

        if (provider == null)
        {
            throw new KeyNotFoundException("Provider not found.");
        }

        provider.Specialization = dto.Specialization;
        provider.Qualification = dto.Qualification;
        provider.ExperienceYears = dto.ExperienceYears;
        provider.Bio = dto.Bio;
        provider.ClinicName = dto.ClinicName;
        provider.ClinicAddress = dto.ClinicAddress;

        return await _repo.Update(provider);
    }

    public async Task<Provider> VerifyProvider(int id)
    {
        var provider = await _repo.FindById(id);

        if (provider == null)
        {
            throw new KeyNotFoundException("Provider not found.");
        }

        provider.IsVerified = true;
        return await _repo.Update(provider);
    }

    public async Task<Provider> UnverifyProvider(int id)
    {
        var provider = await _repo.FindById(id);

        if (provider == null)
        {
            throw new KeyNotFoundException("Provider not found.");
        }

        provider.IsVerified = false;
        return await _repo.Update(provider);
    }

    public async Task<bool> SetAvailability(int id, bool available)
    {
        var provider = await _repo.FindById(id);

        if (provider == null)
        {
            return false;
        }

        provider.IsAvailable = available;
        await _repo.Update(provider);
        return true;
    }

    public async Task<bool> DeleteProvider(int id)
    {
        return await _repo.Delete(id);
    }

    public async Task<bool> UpdateRating(int id, double rating)
    {
        var provider = await _repo.FindById(id);

        if (provider == null)
        {
            return false;
        }

        provider.AvgRating = rating;
        await _repo.Update(provider);
        return true;
    }

    public async Task<List<Provider>> GetAllProviders()
    {
        return await GetAllProviders(true);
    }

    public async Task<List<Provider>> GetAllProviders(bool verifiedOnly)
    {
        if (verifiedOnly)
        {
            return await _repo.FindByIsVerified(true);
        }
        return await _repo.GetAll();
    }

    public async Task<List<Provider>> GetPendingProviders()
    {
        return await _repo.FindByIsVerified(false);
    }

    public async Task<bool> RejectProvider(int id)
    {
        var provider = await _repo.FindById(id);

        if (provider == null)
        {
            return false;
        }

        // Delete the provider to reject them
        return await _repo.Delete(id);
    }

    public async Task<bool> IsProviderVerified(int providerId)
    {
        var provider = await _repo.FindById(providerId);
        return provider?.IsVerified ?? false;
    }

    public async Task<Provider> SyncFromAuthService(SyncProviderDto dto)
    {
        // Check if provider already exists
        var existingProvider = await _repo.FindByUserId(dto.UserId);
        if (existingProvider != null)
        {
            throw new InvalidOperationException("Provider already exists for this user.");
        }

        var provider = new Provider
        {
            UserId = dto.UserId,
            Specialization = "General Practitioner", // Default for synced providers
            Qualification = "TBD", // To be filled by provider
            ExperienceYears = 0,
            Bio = $"Provider profile for {dto.FullName} - pending completion",
            ClinicName = "",
            ClinicAddress = "",
            AvgRating = 0,
            IsVerified = false,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _repo.Add(provider);
    }

    public async Task<Provider?> GetProviderByUserId(int userId)
    {
        return await _repo.FindByUserId(userId);
    }
}