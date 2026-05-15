using ScheduleService.DTOs;
using ScheduleService.Entities;
using ScheduleService.Repositories;

namespace ScheduleService.Services;

public class ScheduleService : IScheduleService
{
    private readonly ISlotRepository _repo;
    private readonly IHttpClientFactory _httpClientFactory;

    public ScheduleService(ISlotRepository repo, IHttpClientFactory httpClientFactory)
    {
        _repo = repo;
        _httpClientFactory = httpClientFactory;
    }

    // ── Add single slot ───────────────────────────────────────────────────────

    public async Task<AvailabilitySlot> AddSlot(AddSlotDto dto)
    {
        // Check if provider is verified before adding slots
        bool isVerified = await CheckProviderVerification(dto.ProviderId);

        if (!isVerified)
        {
            throw new InvalidOperationException("Provider is not verified by admin. Cannot add slots for unverified providers.");
        }

        var slot = new AvailabilitySlot
        {
            ProviderId = dto.ProviderId,
            Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
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
        if (dtos.Any())
        {
            int providerId = dtos.First().ProviderId;
            bool isVerified = await CheckProviderVerification(providerId);

            if (!isVerified)
            {
                throw new InvalidOperationException("Provider is not verified by admin. Cannot add slots for unverified providers.");
            }
        }

        var slots = dtos.Select(dto => new AvailabilitySlot
        {
            ProviderId = dto.ProviderId,
            Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
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

    public async Task<List<AvailabilitySlot>> GetSlotsByProviderAndDate(int providerId, DateTime date)
    {
        return await _repo.FindByProviderIdAndDate(providerId, date);
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
        // Check if provider is verified before generating recurring slots
        bool isVerified = await CheckProviderVerification(dto.ProviderId);

        if (!isVerified)
        {
            throw new InvalidOperationException("Provider is not verified by admin. Cannot add slots for unverified providers.");
        }

        var slots = new List<AvailabilitySlot>();

        DateTime current = dto.StartDate;

        while (current.Date <= dto.EndDate.Date)
        {
            var slot = new AvailabilitySlot
            {
                ProviderId = dto.ProviderId,
                Date = DateTime.SpecifyKind(current, DateTimeKind.Utc),
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

    // ── Private Helpers ───────────────────────────────────────────────────────

    // Calls Provider-Service to check if provider is verified
    private async Task<bool> CheckProviderVerification(int providerId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ProviderService");
            var response = await client.GetAsync($"/api/v1/providers/{providerId}/verification-status");

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<VerificationResponse>(content);
            return result?.isVerified ?? false;
        }
        catch
        {
            // If Provider-Service is not reachable, allow slot creation (fail-open)
            return true;
        }
    }
}
