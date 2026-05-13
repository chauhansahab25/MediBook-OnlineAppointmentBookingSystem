 using ScheduleService.Entities;

namespace ScheduleService.Repositories;

public interface ISlotRepository
{
    Task<List<AvailabilitySlot>> FindByProviderId(int providerId);

    Task<List<AvailabilitySlot>> FindByProviderIdAndDate(int providerId, DateTime date);

    Task<List<AvailabilitySlot>> FindAvailableByProviderAndDate(int providerId, DateTime date);

    Task<List<AvailabilitySlot>> FindByDateBetween(DateTime startDate, DateTime endDate);

    Task<int> CountAvailableByProviderId(int providerId);

    Task<AvailabilitySlot?> FindById(int slotId);

    Task<bool> DeleteBySlotId(int slotId);

    Task<AvailabilitySlot> Add(AvailabilitySlot slot);

    Task<List<AvailabilitySlot>> AddRange(List<AvailabilitySlot> slots);

    Task<AvailabilitySlot> Update(AvailabilitySlot slot);
}
