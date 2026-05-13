using Microsoft.EntityFrameworkCore;
using ScheduleService.Data;
using ScheduleService.Entities;

namespace ScheduleService.Repositories;

public class SlotRepository : ISlotRepository
{
    private readonly AppDbContext _context;

    public SlotRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailabilitySlot>> FindByProviderId(int providerId)
    {
        return await _context.AvailabilitySlots
            .Where(s => s.ProviderId == providerId)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<AvailabilitySlot>> FindByProviderIdAndDate(int providerId, DateTime date)
    {
        var startOfDay = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);
        return await _context.AvailabilitySlots
            .Where(s => s.ProviderId == providerId && s.Date >= startOfDay && s.Date < endOfDay)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<AvailabilitySlot>> FindAvailableByProviderAndDate(int providerId, DateTime date)
    {
        var startOfDay = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);
        return await _context.AvailabilitySlots
            .Where(s => s.ProviderId == providerId
                     && s.Date >= startOfDay && s.Date < endOfDay
                     && s.IsBooked == false
                     && s.IsBlocked == false)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<AvailabilitySlot>> FindByDateBetween(DateTime startDate, DateTime endDate)
    {
        var start = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var end = DateTime.SpecifyKind(endDate.Date, DateTimeKind.Utc).AddDays(1);
        return await _context.AvailabilitySlots
            .Where(s => s.Date >= start && s.Date < end)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<int> CountAvailableByProviderId(int providerId)
    {
        return await _context.AvailabilitySlots
            .CountAsync(s => s.ProviderId == providerId
                          && s.IsBooked == false
                          && s.IsBlocked == false);
    }

    public async Task<AvailabilitySlot?> FindById(int slotId)
    {
        return await _context.AvailabilitySlots.FindAsync(slotId);
    }

    public async Task<bool> DeleteBySlotId(int slotId)
    {
        var slot = await _context.AvailabilitySlots.FindAsync(slotId);

        if (slot == null)
        {
            return false;
        }

        _context.AvailabilitySlots.Remove(slot);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AvailabilitySlot> Add(AvailabilitySlot slot)
    {
        _context.AvailabilitySlots.Add(slot);
        await _context.SaveChangesAsync();
        return slot;
    }

    public async Task<List<AvailabilitySlot>> AddRange(List<AvailabilitySlot> slots)
    {
        _context.AvailabilitySlots.AddRange(slots);
        await _context.SaveChangesAsync();
        return slots;
    }

    public async Task<AvailabilitySlot> Update(AvailabilitySlot slot)
    {
        _context.AvailabilitySlots.Update(slot);
        await _context.SaveChangesAsync();
        return slot;
    }
} 
