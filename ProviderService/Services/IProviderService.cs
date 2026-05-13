using ProviderService.DTOs;
using ProviderService.Entities;

namespace ProviderService.Services;

public interface IProviderService
{
    Task<Provider> RegisterProvider(Provider provider);
    Task<Provider> CreateProvider(ProviderDto dto);
    Task<Provider?> GetProviderById(int id);
    Task<EnrichedProviderDto?> GetEnrichedProviderById(int id);
    Task<List<Provider>> GetBySpecialization(string specialization);
    Task<List<Provider>> SearchProviders(string text);
    Task<Provider> UpdateProvider(int id, ProviderDto dto);
    Task<Provider> VerifyProvider(int id);
    Task<Provider> UnverifyProvider(int id);
    Task<bool> SetAvailability(int id, bool available);
    Task<bool> DeleteProvider(int id);
    Task<bool> UpdateRating(int id, double rating);
    Task<List<Provider>> GetAllProviders();
    Task<List<Provider>> GetAllProviders(bool verifiedOnly);
    Task<List<Provider>> GetPendingProviders();
    Task<bool> RejectProvider(int id);
    Task<bool> IsProviderVerified(int providerId);
    Task<Provider> SyncFromAuthService(SyncProviderDto dto);
    Task<Provider?> GetProviderByUserId(int userId);
}