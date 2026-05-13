using ProviderService.Entities;

namespace ProviderService.Repositories;

public interface IProviderRepository
{
    Task<Provider?> FindByUserId(int userId);

    Task<Provider?> FindById(int providerId);

    Task<List<Provider>> FindBySpecialization(string specialization);

    Task<List<Provider>> FindByIsVerified(bool isVerified);

    Task<List<Provider>> FindByIsAvailable(bool isAvailable);

    Task<List<Provider>> SearchByNameOrSpecialization(string text);

    Task<List<Provider>> FindByClinicAddress(string address);

    Task<int> CountBySpecialization(string specialization);

    Task<List<Provider>> GetAll();

    Task<Provider> Add(Provider provider);

    Task<Provider> Update(Provider provider);

    Task<bool> Delete(int providerId);
}