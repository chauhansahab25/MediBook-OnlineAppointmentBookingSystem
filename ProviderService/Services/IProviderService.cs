using ProviderService.Entities;

namespace ProviderService.Services;

public interface IProviderService
{
    Task<Provider> RegisterProvider(Provider provider);

    Task<Provider?> GetProviderById(int id);

    Task<List<Provider>> GetBySpecialization(string specialization);

    Task<List<Provider>> SearchProviders(string text);

    Task<Provider> UpdateProvider(int id, Provider updated);

    Task<bool> VerifyProvider(int id);

    Task<bool> SetAvailability(int id, bool available);

    Task<bool> DeleteProvider(int id);

    Task<bool> UpdateRating(int id, double rating);

    Task<List<Provider>> GetAllProviders();
}