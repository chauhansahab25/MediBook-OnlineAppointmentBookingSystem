using ProviderService.Entities;
using ProviderService.Repositories;

namespace ProviderService.Services;

public class ProviderService : IProviderService
{
    private readonly IProviderRepository _repo;

    public ProviderService(IProviderRepository repo)
    {
        _repo = repo;
    }

    public async Task<Provider> RegisterProvider(Provider provider)
    {
        provider.CreatedAt = DateTime.UtcNow;
        provider.IsVerified = false;
        provider.IsAvailable = true;
        provider.AvgRating = 0;

        return await _repo.Add(provider);
    }

    public async Task<Provider?> GetProviderById(int id)
    {
        return await _repo.FindById(id);
    }

    public async Task<List<Provider>> GetBySpecialization(string specialization)
    {
        return await _repo.FindBySpecialization(specialization);
    }

    public async Task<List<Provider>> SearchProviders(string text)
    {
        return await _repo.SearchByNameOrSpecialization(text);
    }

    public async Task<Provider> UpdateProvider(int id, Provider updated)
    {
        var provider = await _repo.FindById(id);

        if (provider == null)
        {
            throw new KeyNotFoundException("Provider not found.");
        }

        provider.Specialization = updated.Specialization;
        provider.Qualification = updated.Qualification;
        provider.ExperienceYears = updated.ExperienceYears;
        provider.Bio = updated.Bio;
        provider.ClinicName = updated.ClinicName;
        provider.ClinicAddress = updated.ClinicAddress;

        return await _repo.Update(provider);
    }

    public async Task<bool> VerifyProvider(int id)
    {
        var provider = await _repo.FindById(id);

        if (provider == null)
        {
            return false;
        }

        provider.IsVerified = true;
        await _repo.Update(provider);
        return true;
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
        return await _repo.GetAll();
    }
}