using MediBook.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MediBook.Web.Controllers;

public class ProviderController : Controller
{
    private readonly IApiService _api;
    private readonly IConfiguration _config;

    public ProviderController(
        IApiService api, IConfiguration config)
    {
        _api = api;
        _config = config;
    }

    private string Auth =>
        _config["ServiceUrls:AuthService"]!;
    private string Provider =>
        _config["ServiceUrls:ProviderService"]!;
    private string Schedule =>
        _config["ServiceUrls:ScheduleService"]!;
    private string Appointment =>
        _config["ServiceUrls:AppointmentService"]!;
    private string Payment =>
        _config["ServiceUrls:PaymentService"]!;
    private string Notification =>
        _config["ServiceUrls:NotificationService"]!;
    private string Review =>
        _config["ServiceUrls:ReviewService"]!;
    private string Record =>
        _config["ServiceUrls:MedicalRecordService"]!;

    // ── Provider Dashboard ────────────────────────────────────────────────────

    public IActionResult ProviderDashboard()
    {
        return View();
    }

    // ── View Profile ──────────────────────────────────────────────────────────

    public async Task<IActionResult> ViewProfile(
        int providerId)
    {
        var profile = await _api.GetAsync(
            Provider,
            $"/api/v1/providers/{providerId}");

        ViewBag.ProviderId = providerId;
        return View(profile);
    }

    // ── Edit Profile ──────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> EditProfile(
        int providerId)
    {
        var profile = await _api.GetAsync(
            Provider,
            $"/api/v1/providers/{providerId}");

        ViewBag.ProviderId = providerId;
        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile(
        int providerId, string specialization,
        string qualification, int experienceYears,
        string? bio, string clinicName,
        string clinicAddress)
    {
        await _api.PutAsync(
            Provider,
            $"/api/v1/providers/{providerId}", new
            {
                Specialization = specialization,
                Qualification = qualification,
                ExperienceYears = experienceYears,
                Bio = bio,
                ClinicName = clinicName,
                ClinicAddress = clinicAddress
            });

        return RedirectToAction("ViewProfile",
            new { providerId });
    }

    // ── Manage Availability ───────────────────────────────────────────────────

    public async Task<IActionResult> ManageAvailability(
        int providerId)
    {
        var slots = await _api.GetAsync(
            Schedule,
            $"/api/v1/slots/provider/{providerId}");

        ViewBag.ProviderId = providerId;
        return View(slots);
    }

    // ── Add Single Slot ───────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult AddSlot(int providerId)
    {
        ViewBag.ProviderId = providerId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddSlot(
        int providerId, DateTime date,
        TimeSpan startTime, TimeSpan endTime,
        int durationMinutes, string recurrence)
    {
        await _api.PostAsync(
            Schedule, "/api/v1/slots", new
            {
                ProviderId = providerId,
                Date = date,
                StartTime = startTime,
                EndTime = endTime,
                DurationMinutes = durationMinutes,
                Recurrence = recurrence
            });

        return RedirectToAction("ManageAvailability",
            new { providerId });
    }

    // ── Add Bulk Slots ────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult AddBulkSlots(int providerId)
    {
        ViewBag.ProviderId = providerId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddBulkSlots(
        int providerId, DateTime startDate,
        DateTime endDate, TimeSpan startTime,
        TimeSpan endTime, int durationMinutes,
        string pattern)
    {
        await _api.PostAsync(
            Schedule,
            "/api/v1/slots/generateRecurring", new
            {
                ProviderId = providerId,
                StartDate = startDate,
                EndDate = endDate,
                StartTime = startTime,
                EndTime = endTime,
                DurationMinutes = durationMinutes,
                Pattern = pattern
            });

        return RedirectToAction("ManageAvailability",
            new { providerId });
    }

    // ── Block Slot ────────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> BlockSlot(
        int slotId, int providerId)
    {
        await _api.PutAsync(
            Schedule,
            $"/api/v1/slots/{slotId}/block");

        return RedirectToAction("ManageAvailability",
            new { providerId });
    }

    // ── View Today's Appointments ─────────────────────────────────────────────

    public async Task<IActionResult> ViewTodayAppointments(
        int providerId)
    {
        var today = DateTime.UtcNow.Date
            .ToString("yyyy-MM-dd");

        var appointments = await _api.GetAsync(
            Appointment,
            $"/api/v1/appointments/provider/{providerId}" +
            $"/date?date={today}");

        ViewBag.ProviderId = providerId;
        ViewBag.Today = today;
        return View(appointments);
    }

    // ── View All Appointments ─────────────────────────────────────────────────

    public async Task<IActionResult> ViewAllAppointments(
        int providerId)
    {
        var appointments = await _api.GetAsync(
            Appointment,
            $"/api/v1/appointments/provider/{providerId}");

        ViewBag.ProviderId = providerId;
        return View(appointments);
    }

    // ── Complete Appointment ──────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> CompleteAppointment(
        int appointmentId, int providerId)
    {
        await _api.PutAsync(
            Appointment,
            $"/api/v1/appointments/{appointmentId}/complete");

        return RedirectToAction("ViewAllAppointments",
            new { providerId });
    }

    // ── Create Medical Record ─────────────────────────────────────────────────

    [HttpGet]
    public IActionResult CreateMedicalRecord(
        int appointmentId, int patientId,
        int providerId)
    {
        ViewBag.AppointmentId = appointmentId;
        ViewBag.PatientId = patientId;
        ViewBag.ProviderId = providerId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicalRecord(
        int appointmentId, int patientId,
        int providerId, string diagnosis,
        string? prescription, string? notes,
        DateTime? followUpDate)
    {
        await _api.PostAsync(
            Record, "/api/v1/records", new
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                ProviderId = providerId,
                Diagnosis = diagnosis,
                Prescription = prescription,
                Notes = notes,
                FollowUpDate = followUpDate
            });

        return RedirectToAction("ViewAllAppointments",
            new { providerId });
    }

    // ── Edit Medical Record ───────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> EditMedicalRecord(
        int recordId, int providerId)
    {
        var record = await _api.GetAsync(
            Record,
            $"/api/v1/records/{recordId}");

        ViewBag.RecordId = recordId;
        ViewBag.ProviderId = providerId;
        return View(record);
    }

    [HttpPost]
    public async Task<IActionResult> EditMedicalRecord(
        int recordId, int providerId,
        string diagnosis, string? prescription,
        string? notes, DateTime? followUpDate)
    {
        await _api.PutAsync(
            Record,
            $"/api/v1/records/{recordId}", new
            {
                Diagnosis = diagnosis,
                Prescription = prescription,
                Notes = notes,
                FollowUpDate = followUpDate
            });

        return RedirectToAction("ViewAllAppointments",
            new { providerId });
    }

    // ── View Earnings ─────────────────────────────────────────────────────────

    public async Task<IActionResult> ViewEarnings(
        int providerId)
    {
        var revenue = await _api.GetAsync(
            Payment,
            $"/api/v1/payments/provider/{providerId}/revenue");

        ViewBag.ProviderId = providerId;
        return View(revenue);
    }

    // ── View Reviews ──────────────────────────────────────────────────────────

    public async Task<IActionResult> ViewReviews(
        int providerId)
    {
        var reviews = await _api.GetAsync(
            Review,
            $"/api/v1/reviews/provider/{providerId}");

        var avgRating = await _api.GetAsync(
            Review,
            $"/api/v1/reviews/provider/{providerId}/avgrating");

        ViewBag.AvgRating = avgRating;
        ViewBag.ProviderId = providerId;
        return View(reviews);
    }

    // ── View Notifications ────────────────────────────────────────────────────

    public async Task<IActionResult> ViewNotifications(
        int recipientId)
    {
        var notifications = await _api.GetAsync(
            Notification,
            $"/api/v1/notifications/recipient/{recipientId}");

        var unreadCount = await _api.GetAsync(
            Notification,
            $"/api/v1/notifications/recipient" +
            $"/{recipientId}/unread/count");

        ViewBag.UnreadCount = unreadCount;
        ViewBag.RecipientId = recipientId;
        return View(notifications);
    }
}