using ScheduleService.DTOs;
using ScheduleService.Entities;
using ScheduleService.Repositories;

namespace ScheduleService.Services;

public class ScheduleService : IScheduleService
{
    private readonly ISlotRepository _repo;

    public ScheduleService(ISlotRepository repo)
    {
        _repo = repo;
    }

    // ── Add single slot ───────────────────────────────────────────────────────

    public async Task<AvailabilitySlot> AddSlot(AddSlotDto dto)
    {
        var slot = new AvailabilitySlot
        {
            ProviderId = dto.ProviderId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            DurationMinutes = dto.DurationMinutes,
            Recurrence = dto.Recurrence,
            IsBooked = false,
            IsBlocked = false,
            CreatedAt = DateTime.UtcNow
        };

        return await _repo.Add(slot);
    }

    // ── Add multiple slots at once ────────────────────────────────────────────

    public async Task<List<AvailabilitySlot>> AddBulkSlots(List<BulkSlotDto> dtos)
    {
        var slots = dtos.Select(dto => new AvailabilitySlot
        {
            ProviderId = dto.ProviderId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            DurationMinutes = dto.DurationMinutes,
            Recurrence = "None",
            IsBooked = false,
            IsBlocked = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        return await _repo.AddRange(slots);
    }

    // ── Get all slots for a provider ──────────────────────────────────────────

    public async Task<List<AvailabilitySlot>> GetSlotsByProvider(int providerId)
    {
        return await _repo.FindByProviderId(providerId);
    }

    // ── Get only available (not booked, not blocked) slots ────────────────────

    public async Task<List<AvailabilitySlot>> GetAvailableSlots(int providerId, DateTime date)
    {
        return await _repo.FindAvailableByProviderAndDate(providerId, date);
    }

    // ── Get single slot by ID ─────────────────────────────────────────────────

    public async Task<AvailabilitySlot?> GetSlotById(int slotId)
    {
        return await _repo.FindById(slotId);
    }

    // ── Book a slot (Available → Booked) ─────────────────────────────────────

    public async Task<bool> BookSlot(int slotId)
    {
        var slot = await _repo.FindById(slotId);

        if (slot == null)
        {
            return false;
        }

        if (slot.IsBooked)
        {
            throw new InvalidOperationException("Slot is already booked.");
        }

        if (slot.IsBlocked)
        {
            throw new InvalidOperationException("Slot is blocked and cannot be booked.");
        }

        slot.IsBooked = true;
        await _repo.Update(slot);
        return true;
    }

    // ── Unbook a slot (Booked → Available) ───────────────────────────────────

    public async Task<bool> UnbookSlot(int slotId)
    {
        var slot = await _repo.FindById(slotId);

        if (slot == null)
        {
            return false;
        }

        slot.IsBooked = false;
        await _repo.Update(slot);
        return true;
    }

    // ── Block a slot (for leave etc.) ─────────────────────────────────────────

    public async Task<bool> BlockSlot(int slotId)
    {
        var slot = await _repo.FindById(slotId);

        if (slot == null)
        {
            return false;
        }

        if (slot.IsBooked)
        {
            throw new InvalidOperationException("Cannot block a slot that is already booked.");
        }

        slot.IsBlocked = true;
        await _repo.Update(slot);
        return true;
    }

    // ── Unblock a slot ────────────────────────────────────────────────────────

    public async Task<bool> UnblockSlot(int slotId)
    {
        var slot = await _repo.FindById(slotId);

        if (slot == null)
        {
            return false;
        }

        slot.IsBlocked = false;
        await _repo.Update(slot);
        return true;
    }

    // ── Delete a slot ─────────────────────────────────────────────────────────

    public async Task<bool> DeleteSlot(int slotId)
    {
        return await _repo.DeleteBySlotId(slotId);
    }

    // ── Update slot details ───────────────────────────────────────────────────

    public async Task<AvailabilitySlot> UpdateSlot(int slotId, UpdateSlotDto dto)
    {
        var slot = await _repo.FindById(slotId);

        if (slot == null)
        {
            throw new KeyNotFoundException("Slot not found.");
        }

        if (slot.IsBooked)
        {
            throw new InvalidOperationException("Cannot update a slot that is already booked.");
        }

        slot.Date = dto.Date;
        slot.StartTime = dto.StartTime;
        slot.EndTime = dto.EndTime;
        slot.DurationMinutes = dto.DurationMinutes;

        return await _repo.Update(slot);
    }

    // ── Generate recurring slots (Daily or Weekly) ────────────────────────────

    public async Task<List<AvailabilitySlot>> GenerateRecurringSlots(RecurringSlotDto dto)
    {
        var slots = new List<AvailabilitySlot>();

        DateTime current = dto.StartDate;

        while (current.Date <= dto.EndDate.Date)
        {
            var slot = new AvailabilitySlot
            {
                ProviderId = dto.ProviderId,
                Date = current,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                DurationMinutes = dto.DurationMinutes,
                Recurrence = dto.Pattern,
                IsBooked = false,
                IsBlocked = false,
                CreatedAt = DateTime.UtcNow
            };

            slots.Add(slot);

            // Move to next date based on pattern
            if (dto.Pattern == "Daily")
            {
                current = current.AddDays(1);
            }
            else if (dto.Pattern == "Weekly")
            {
                current = current.AddDays(7);
            }
            else
            {
                break;
            }
        }

        return await _repo.AddRange(slots);
    }
}
