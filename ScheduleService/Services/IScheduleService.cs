using ScheduleService.DTOs;
using ScheduleService.Entities;

namespace ScheduleService.Services;

public interface IScheduleService
{
    Task<AvailabilitySlot> AddSlot(AddSlotDto dto);

    Task<List<AvailabilitySlot>> AddBulkSlots(List<BulkSlotDto> dtos);

    Task<List<AvailabilitySlot>> GetSlotsByProvider(int providerId);

    Task<List<AvailabilitySlot>> GetAvailableSlots(int providerId, DateTime date);

    Task<AvailabilitySlot?> GetSlotById(int slotId);

    Task<bool> BookSlot(int slotId);

    Task<bool> UnbookSlot(int slotId);

    Task<bool> BlockSlot(int slotId);

    Task<bool> UnblockSlot(int slotId);

    Task<bool> DeleteSlot(int slotId);

    Task<AvailabilitySlot> UpdateSlot(int slotId, UpdateSlotDto dto);

    Task<List<AvailabilitySlot>> GenerateRecurringSlots(RecurringSlotDto dto);
} 
