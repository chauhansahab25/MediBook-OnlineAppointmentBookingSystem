using AppointmentService.DTOs;
using AppointmentService.Entities;
using AppointmentService.Repositories;
using System.Text.Json;

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

    public async Task<List<AppointmentResponseDto>> GetAll()
    {
        var appointments = await _repo.FindAll();
        var enrichedAppointments = new List<AppointmentResponseDto>();

        foreach (var appointment in appointments)
        {
            var dto = MapToResponse(appointment);
            try
            {
                await EnrichAppointmentData(dto);
            }
            catch
            {
                // If enrichment fails, continue with basic data
            }
            enrichedAppointments.Add(dto);
        }

        return enrichedAppointments;
    }

    public async Task<AppointmentResponseDto> BookAppointment(BookAppointmentDto dto)
    {
        // Check if provider is verified before booking
        bool isVerified = await CheckProviderVerification(dto.ProviderId);

        if (!isVerified)
        {
            throw new InvalidOperationException("Provider is not verified by admin. Cannot book appointments with unverified providers.");
        }

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
            AppointmentDate = DateTime.SpecifyKind(dto.AppointmentDate, DateTimeKind.Utc),
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = "Scheduled",
            Notes = dto.Notes,
            ModeOfConsultation = dto.ModeOfConsultation,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var saved = await _repo.Add(appointment);
        var response = MapToResponse(saved);
        await EnrichAppointmentData(response);
        return response;
    }

    // ── Get By ID ─────────────────────────────────────────────────────────────

    public async Task<AppointmentResponseDto?> GetById(int appointmentId)
    {
        var appointment = await _repo.FindById(appointmentId);

        if (appointment == null)
        {
            return null;
        }

        var dto = MapToResponse(appointment);
        await EnrichAppointmentData(dto);
        return dto;
    }

    public async Task<AppointmentResponseDto?> GetBySlotId(int slotId)
    {
        var appointment = await _repo.FindBySlotId(slotId);

        if (appointment == null)
        {
            return null;
        }

        var dto = MapToResponse(appointment);
        await EnrichAppointmentData(dto);
        return dto;
    }

    // ── Get By Patient ────────────────────────────────────────────────────────

    public async Task<List<AppointmentResponseDto>> GetByPatient(int patientId)
    {
        var appointments = await _repo.FindByPatientId(patientId);
        var enriched = new List<AppointmentResponseDto>();
        foreach (var a in appointments)
        {
            var dto = MapToResponse(a);
            await EnrichAppointmentData(dto);
            enriched.Add(dto);
        }
        return enriched;
    }

    // ── Get By Provider ───────────────────────────────────────────────────────

    public async Task<List<AppointmentResponseDto>> GetByProvider(int providerId)
    {
        var appointments = await _repo.FindByProviderId(providerId);
        var enriched = new List<AppointmentResponseDto>();
        foreach (var a in appointments)
        {
            var dto = MapToResponse(a);
            await EnrichAppointmentData(dto);
            enriched.Add(dto);
        }
        return enriched;
    }

    // ── Get By Provider And Date ──────────────────────────────────────────────

    public async Task<List<AppointmentResponseDto>> GetByProviderAndDate(
        int providerId, DateTime date)
    {
        var appointments = await _repo.FindByProviderIdAndAppointmentDate(providerId, date);
        var enriched = new List<AppointmentResponseDto>();
        foreach (var a in appointments)
        {
            var dto = MapToResponse(a);
            await EnrichAppointmentData(dto);
            enriched.Add(dto);
        }
        return enriched;
    }

    // ── Cancel Appointment ────────────────────────────────────────────────────

    public async Task<bool> CancelAppointment(int appointmentId, string cancelledBy = "Patient")
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
        appointment.CancelledBy = cancelledBy;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _repo.Update(appointment);

        // Process automatic refund if cancelled by provider
        if (cancelledBy == "Provider")
        {
            await ProcessAutomaticRefund(appointment);
        }

        return true;
    }

    // ── Process Automatic Refund ────────────────────────────────────────
    private async Task ProcessAutomaticRefund(Appointment appointment)
    {
        try
        {
            // Create refund record
            var client = _httpClientFactory.CreateClient("PaymentService");
            var refundData = new
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                ProviderId = appointment.ProviderId,
                Amount = 550m,
                Status = "Refunded",
                Mode = "Online",
                TransactionId = "refund_" + DateTime.UtcNow.Ticks,
                Currency = "INR",
                Notes = "Appointment cancelled by provider - full refund"
            };

            var response = await client.PostAsJsonAsync("/payments/refund", refundData);
            Console.WriteLine($"Automatic refund processed for appointment {appointment.AppointmentId}: {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process automatic refund for appointment {appointment.AppointmentId}: {ex.Message}");
        }
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
        appointment.AppointmentDate = DateTime.SpecifyKind(dto.NewAppointmentDate, DateTimeKind.Utc);
        appointment.StartTime = dto.NewStartTime;
        appointment.EndTime = dto.NewEndTime;
        appointment.UpdatedAt = DateTime.UtcNow;

        var updated = await _repo.Update(appointment);
        var response = MapToResponse(updated);
        await EnrichAppointmentData(response);
        return response;
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
        var enriched = new List<AppointmentResponseDto>();
        foreach (var a in appointments)
        {
            var dto = MapToResponse(a);
            await EnrichAppointmentData(dto);
            enriched.Add(dto);
        }
        return enriched;
    }

    // ── Get Appointment Count ─────────────────────────────────────────────────

    public async Task<int> GetAppointmentCount(int providerId)
    {
        return await _repo.CountByProviderId(providerId);
    }

    public async Task<bool> DeleteAppointment(int appointmentId)
    {
        var appointment = await _repo.FindById(appointmentId);
        if (appointment == null) return false;

        // Release the slot back in Schedule-Service before deleting the appointment
        await ReleaseSlot(appointment.SlotId);

        return await _repo.Delete(appointmentId);
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
            // If Provider-Service is not reachable, allow booking (fail-open)
            return true;
        }
    }

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
            CancelledBy = a.CancelledBy,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            PatientName = string.Empty,
            PatientEmail = string.Empty,
            ProviderName = string.Empty,
            ProviderEmail = string.Empty,
            Specialization = string.Empty,
            PaymentStatus = string.Empty,
            PaymentMode = string.Empty,
            TransactionId = string.Empty
        };
    }

    // Enriches appointment with patient and provider details
    private async Task EnrichAppointmentData(AppointmentResponseDto dto)
    {
        try
        {
            // Fetch patient details from AuthService
            try
            {
                var authClient = _httpClientFactory.CreateClient("AuthService");
                var patientResponse = await authClient.GetAsync($"/api/v1/auth/users/{dto.PatientId}");
                if (patientResponse.IsSuccessStatusCode)
                {
                    var patientJson = await patientResponse.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(patientJson))
                    {
                        var patient = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(patientJson);
                        if (patient.ValueKind != JsonValueKind.Null && patient.ValueKind != JsonValueKind.Undefined)
                        {
                            if (patient.TryGetProperty("fullName", out var fullNameProp))
                                dto.PatientName = fullNameProp.GetString();
                            if (patient.TryGetProperty("email", out var emailProp))
                                dto.PatientEmail = emailProp.GetString();
                        }
                    }
                }
            }
            catch
            {
                // If auth service fails, continue without patient data
            }

            // Fetch provider details from ProviderService
            try
            {
                var providerClient = _httpClientFactory.CreateClient("ProviderService");
                var providerResponse = await providerClient.GetAsync($"/api/v1/providers/{dto.ProviderId}");
                if (providerResponse.IsSuccessStatusCode)
                {
                    var providerJson = await providerResponse.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(providerJson))
                    {
                        var provider = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(providerJson);
                        if (provider.ValueKind != JsonValueKind.Null && provider.ValueKind != JsonValueKind.Undefined)
                        {
                            if (provider.TryGetProperty("fullName", out var fullNameProp))
                                dto.ProviderName = fullNameProp.GetString();
                            if (provider.TryGetProperty("email", out var emailProp))
                                dto.ProviderEmail = emailProp.GetString();
                            if (provider.TryGetProperty("specialization", out var specProp))
                                dto.Specialization = specProp.GetString();
                        }
                    }
                }
            }
            catch
            {
                // If provider service fails, continue without provider data
            }

            // Fetch Payment status from PaymentService
            var paymentClient = _httpClientFactory.CreateClient("PaymentService");
            try
            {
                var paymentResponse = await paymentClient.GetAsync($"/api/v1/payments/appointment/{dto.AppointmentId}");
                if (paymentResponse.IsSuccessStatusCode)
                {
                    var paymentJson = await paymentResponse.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(paymentJson))
                    {
                        var payment = JsonSerializer.Deserialize<JsonElement>(paymentJson);
                        if (payment.ValueKind != JsonValueKind.Null && payment.ValueKind != JsonValueKind.Undefined)
                        {
                            if (payment.TryGetProperty("status", out var statusProp))
                                dto.PaymentStatus = statusProp.GetString();
                            
                            if (payment.TryGetProperty("amount", out var amountProp))
                                dto.PaymentAmount = amountProp.GetDecimal();
                            
                            if (payment.TryGetProperty("mode", out var modeProp))
                                dto.PaymentMode = modeProp.GetString();
                            
                            if (payment.TryGetProperty("transactionId", out var txnProp))
                                dto.TransactionId = txnProp.GetString();
                        }
                    }
                }
            }
            catch
            {
                // If payment service fails, continue without payment data
            }
        }
        catch
        {
            // If enrichment fails, continue with basic data
        }
    }
} 
