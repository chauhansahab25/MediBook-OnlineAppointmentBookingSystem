using Microsoft.EntityFrameworkCore;
using ProviderService.Data;
using ProviderService.Entities;

namespace ProviderService.Repositories;

public class ProviderRepository : IProviderRepository
{
    private readonly AppDbContext _context;

    public ProviderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Provider?> FindByUserId(int userId)
    {
        return await _context.Providers
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<Provider?> FindById(int providerId)
    {
        return await _context.Providers
            .FindAsync(providerId);
    }

    public async Task<List<Provider>> FindBySpecialization(string specialization)
    {
        return await _context.Providers
            .Where(p => p.Specialization.Contains(specialization))
            .ToListAsync();
    }

    public async Task<List<Provider>> FindByIsVerified(bool isVerified)
    {
        return await _context.Providers
            .Where(p => p.IsVerified == isVerified)
            .ToListAsync();
    }

    public async Task<List<Provider>> FindByIsAvailable(bool isAvailable)
    {
        return await _context.Providers
            .Where(p => p.IsAvailable == isAvailable)
            .ToListAsync();
    }

    public async Task<List<Provider>> SearchByNameOrSpecialization(string text)
    {
        return await _context.Providers
            .Where(p => p.Specialization.Contains(text) ||
                        p.ClinicName.Contains(text))
            .ToListAsync();
    }

    public async Task<List<Provider>> FindByClinicAddress(string address)
    {
        return await _context.Providers
            .Where(p => p.ClinicAddress.Contains(address))
            .ToListAsync();
    }

    public async Task<int> CountBySpecialization(string specialization)
    {
        return await _context.Providers
            .CountAsync(p => p.Specialization == specialization);
    }

    public async Task<List<Provider>> GetAll()
    {
        return await _context.Providers.ToListAsync();
    }

    public async Task<Provider> Add(Provider provider)
    {
        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();
        return provider;
    }

    public async Task<Provider> Update(Provider provider)
    {
        _context.Providers.Update(provider);
        await _context.SaveChangesAsync();
        return provider;
    }

    public async Task<bool> Delete(int providerId)
    {
        var provider = await _context.Providers.FindAsync(providerId);

        if (provider == null)
        {
            return false;
        }

        _context.Providers.Remove(provider);
        await _context.SaveChangesAsync();
        return true;
    }
}