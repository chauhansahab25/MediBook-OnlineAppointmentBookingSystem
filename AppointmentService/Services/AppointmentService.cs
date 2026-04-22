using AppointmentService.DTOs;
using AppointmentService.Entities;
using AppointmentService.Repositories;

namespace AppointmentService.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _repo;
    private readonly IHttpClientFactory _httpClientFactory;

    public AppointmentService(
        IAppointmentRepository repo,
        IHttpClientFactory httpClientFactory)
    {
        _repo = repo;
        _httpClientFactory = httpClientFactory;
    }

    // ── Book Appointment ──────────────────────────────────────────────────────

    public async Task<AppointmentResponseDto> BookAppointment(BookAppointmentDto dto)
    {
        // Call Schedule-Service to mark slot as booked
        bool slotBooked = await MarkSlotAsBooked(dto.SlotId);

        if (!slotBooked)
        {
            throw new InvalidOperationException("Failed to book the slot. It may already be taken.");
        }

        var appointment = new Appointment
        {
            PatientId = dto.PatientId,
            ProviderId = dto.ProviderId,
            SlotId = dto.SlotId,
            ServiceType = dto.ServiceType,
            AppointmentDate = dto.AppointmentDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = "Scheduled",
            Notes = dto.Notes,
            ModeOfConsultation = dto.ModeOfConsultation,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var saved = await _repo.Add(appointment);
        return MapToResponse(saved);
    }

    // ── Get By ID ─────────────────────────────────────────────────────────────

    public async Task<AppointmentResponseDto?> GetById(int appointmentId)
    {
        var appointment = await _repo.FindById(appointmentId);

        if (appointment == null)
        {
            return null;
        }

        return MapToResponse(appointment);
    }

    // ── Get By Patient ────────────────────────────────────────────────────────

    public async Task<List<AppointmentResponseDto>> GetByPatient(int patientId)
    {
        var appointments = await _repo.FindByPatientId(patientId);
        return appointments.Select(MapToResponse).ToList();
    }

    // ── Get By Provider ───────────────────────────────────────────────────────

    public async Task<List<AppointmentResponseDto>> GetByProvider(int providerId)
    {
        var appointments = await _repo.FindByProviderId(providerId);
        return appointments.Select(MapToResponse).ToList();
    }

    // ── Get By Provider And Date ──────────────────────────────────────────────

    public async Task<List<AppointmentResponseDto>> GetByProviderAndDate(
        int providerId, DateTime date)
    {
        var appointments = await _repo.FindByProviderIdAndAppointmentDate(providerId, date);
        return appointments.Select(MapToResponse).ToList();
    }

    // ── Cancel Appointment ────────────────────────────────────────────────────

    public async Task<bool> CancelAppointment(int appointmentId)
    {
        var appointment = await _repo.FindById(appointmentId);

        if (appointment == null)
        {
            return false;
        }

        if (appointment.Status == "Cancelled")
        {
            throw new InvalidOperationException("Appointment is already cancelled.");
        }

        if (appointment.Status == "Completed")
        {
            throw new InvalidOperationException("Cannot cancel a completed appointment.");
        }

        // Release the slot back in Schedule-Service
        await ReleaseSlot(appointment.SlotId);

        appointment.Status = "Cancelled";
        appointment.UpdatedAt = DateTime.UtcNow;

        await _repo.Update(appointment);
        return true;
    }

    // ── Reschedule Appointment ────────────────────────────────────────────────

    public async Task<AppointmentResponseDto> RescheduleAppointment(
        int appointmentId, RescheduleAppointmentDto dto)
    {
        var appointment = await _repo.FindById(appointmentId);

        if (appointment == null)
        {
            throw new KeyNotFoundException("Appointment not found.");
        }

        if (appointment.Status != "Scheduled")
        {
            throw new InvalidOperationException("Only scheduled appointments can be rescheduled.");
        }

        // Release old slot
        await ReleaseSlot(appointment.SlotId);

        // Book new slot
        bool newSlotBooked = await MarkSlotAsBooked(dto.NewSlotId);

        if (!newSlotBooked)
        {
            throw new InvalidOperationException("Failed to book the new slot.");
        }

        appointment.SlotId = dto.NewSlotId;
        appointment.AppointmentDate = dto.NewAppointmentDate;
        appointment.StartTime = dto.NewStartTime;
        appointment.EndTime = dto.NewEndTime;
        appointment.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.Update(appointment);
        return MapToResponse(updated);
    }

    // ── Complete Appointment ──────────────────────────────────────────────────

    public async Task<bool> CompleteAppointment(int appointmentId)
    {
        var appointment = await _repo.FindById(appointmentId);

        if (appointment == null)
        {
            return false;
        }

        if (appointment.Status != "Scheduled")
        {
            throw new InvalidOperationException("Only scheduled appointments can be completed.");
        }

        appointment.Status = "Completed";
        appointment.UpdatedAt = DateTime.UtcNow;

        await _repo.Update(appointment);
        return true;
    }

    // ── Update Status ─────────────────────────────────────────────────────────

    public async Task<bool> UpdateStatus(int appointmentId, string status)
    {
        var appointment = await _repo.FindById(appointmentId);

        if (appointment == null)
        {
            return false;
        }

        appointment.Status = status;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _repo.Update(appointment);
        return true;
    }

    // ── Get Upcoming By Patient ───────────────────────────────────────────────

    public async Task<List<AppointmentResponseDto>> GetUpcomingByPatient(int patientId)
    {
        var appointments = await _repo.FindUpcomingByPatientId(patientId);
        return appointments.Select(MapToResponse).ToList();
    }

    // ── Get Appointment Count ─────────────────────────────────────────────────

    public async Task<int> GetAppointmentCount(int providerId)
    {
        return await _repo.CountByProviderId(providerId);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    // Calls Schedule-Service to mark slot as booked
    private async Task<bool> MarkSlotAsBooked(int slotId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ScheduleService");
            var response = await client.PutAsync($"/api/v1/slots/{slotId}/book", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            // If Schedule-Service is not reachable, still allow booking
            return true;
        }
    }

    // Calls Schedule-Service to release slot back to available
    private async Task<bool> ReleaseSlot(int slotId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ScheduleService");
            var response = await client.PutAsync($"/api/v1/slots/{slotId}/unbook", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return true;
        }
    }

    // Maps entity to response DTO
    private AppointmentResponseDto MapToResponse(Appointment a)
    {
        return new AppointmentResponseDto
        {
            AppointmentId = a.AppointmentId,
            PatientId = a.PatientId,
            ProviderId = a.ProviderId,
            SlotId = a.SlotId,
            ServiceType = a.ServiceType,
            AppointmentDate = a.AppointmentDate,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status,
            Notes = a.Notes,
            ModeOfConsultation = a.ModeOfConsultation,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        };
    }
} 
