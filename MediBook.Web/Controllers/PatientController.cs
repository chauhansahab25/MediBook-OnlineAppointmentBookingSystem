using MediBook.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MediBook.Web.Controllers;

public class PatientController : Controller
{
    private readonly IApiService _api;
    private readonly IConfiguration _config;

    public PatientController(
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

    // ── Home ──────────────────────────────────────────────────────────────────

    public IActionResult Home()
    {
        return View();
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(
        string fullName, string email,
        string password, string? phone)
    {
        var result = await _api.PostAsync(
            Auth, "/api/v1/auth/register", new
            {
                FullName = fullName,
                Email = email,
                Password = password,
                Phone = phone,
                Role = "Patient"
            });

        if (result == null)
        {
            ViewBag.Error =
                "Registration failed. Email may already be taken.";
            return View();
        }

        return RedirectToAction("Login");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(
        string email, string password)
    {
        var result = await _api.PostAsync(
            Auth, "/api/v1/auth/login", new
            {
                Email = email,
                Password = password
            });

        if (result == null)
        {
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        // result is JsonElement? so we use result.Value
        string token = result.Value
            .GetProperty("token")
            .GetString() ?? string.Empty;

        string fullName = result.Value
            .GetProperty("fullName")
            .GetString() ?? string.Empty;

        string role = result.Value
            .GetProperty("role")
            .GetString() ?? "Patient";

        HttpContext.Session.SetString("JwtToken", token);
        HttpContext.Session.SetString("UserEmail", email);
        HttpContext.Session.SetString("UserFullName", fullName);
        HttpContext.Session.SetString("UserRole", role);

        return RedirectToAction("Home");
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    // ── View Profile ──────────────────────────────────────────────────────────

    public async Task<IActionResult> ViewProfile()
    {
        var profile = await _api.GetAsync(
            Auth, "/api/v1/auth/profile");
        return View(profile);
    }

    // ── Edit Profile ──────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var profile = await _api.GetAsync(
            Auth, "/api/v1/auth/profile");
        return View(profile);
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile(
        string fullName, string? phone,
        string? profilePicUrl)
    {
        await _api.PutAsync(
            Auth, "/api/v1/auth/profile", new
            {
                FullName = fullName,
                Phone = phone,
                ProfilePicUrl = profilePicUrl
            });

        return RedirectToAction("ViewProfile");
    }

    // ── Search Providers ──────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> SearchProviders(
        string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            var all = await _api.GetAsync(
                Provider, "/api/v1/providers");
            return View(all);
        }

        var results = await _api.GetAsync(
            Provider,
            $"/api/v1/providers/search?text={text}");
        return View(results);
    }

    // ── View Provider Profile ─────────────────────────────────────────────────

    public async Task<IActionResult> ViewProviderProfile(
        int providerId)
    {
        var provider = await _api.GetAsync(
            Provider,
            $"/api/v1/providers/{providerId}");

        var avgRating = await _api.GetAsync(
            Review,
            $"/api/v1/reviews/provider/{providerId}/avgrating");

        ViewBag.AvgRating = avgRating;
        ViewBag.ProviderId = providerId;
        return View(provider);
    }

    // ── View Available Slots ──────────────────────────────────────────────────

    public async Task<IActionResult> ViewAvailableSlots(
        int providerId, DateTime date)
    {
        var slots = await _api.GetAsync(
            Schedule,
            $"/api/v1/slots/provider/{providerId}/available" +
            $"?date={date:yyyy-MM-dd}");

        ViewBag.ProviderId = providerId;
        ViewBag.Date = date;
        return View(slots);
    }

    // ── Book Appointment ──────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult BookAppointment(
        int providerId, int slotId)
    {
        ViewBag.ProviderId = providerId;
        ViewBag.SlotId = slotId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> BookAppointment(
        int patientId, int providerId,
        int slotId, string serviceType,
        DateTime appointmentDate,
        TimeSpan startTime, TimeSpan endTime,
        string modeOfConsultation, string? notes)
    {
        var result = await _api.PostAsync(
            Appointment, "/api/v1/appointments", new
            {
                PatientId = patientId,
                ProviderId = providerId,
                SlotId = slotId,
                ServiceType = serviceType,
                AppointmentDate = appointmentDate,
                StartTime = startTime,
                EndTime = endTime,
                ModeOfConsultation = modeOfConsultation,
                Notes = notes
            });

        if (result == null)
        {
            ViewBag.Error =
                "Booking failed. Slot may already be taken.";
            return View();
        }

        return RedirectToAction("ViewMyAppointments",
            new { patientId });
    }

    // ── View My Appointments ──────────────────────────────────────────────────

    public async Task<IActionResult> ViewMyAppointments(
        int patientId)
    {
        var appointments = await _api.GetAsync(
            Appointment,
            $"/api/v1/appointments/patient/{patientId}");

        ViewBag.PatientId = patientId;
        return View(appointments);
    }

    // ── Cancel Appointment ────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> CancelAppointment(
        int appointmentId, int patientId)
    {
        await _api.PutAsync(
            Appointment,
            $"/api/v1/appointments/{appointmentId}/cancel");

        return RedirectToAction("ViewMyAppointments",
            new { patientId });
    }

    // ── Reschedule Appointment ────────────────────────────────────────────────

    [HttpGet]
    public IActionResult RescheduleAppointment(
        int appointmentId)
    {
        ViewBag.AppointmentId = appointmentId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RescheduleAppointment(
        int appointmentId, int patientId,
        int newSlotId, DateTime newDate,
        TimeSpan newStartTime, TimeSpan newEndTime)
    {
        await _api.PutAsync(
            Appointment,
            $"/api/v1/appointments/{appointmentId}/reschedule",
            new
            {
                NewSlotId = newSlotId,
                NewAppointmentDate = newDate,
                NewStartTime = newStartTime,
                NewEndTime = newEndTime
            });

        return RedirectToAction("ViewMyAppointments",
            new { patientId });
    }

    // ── View Medical Records ──────────────────────────────────────────────────

    public async Task<IActionResult> ViewMedicalRecords(
        int patientId)
    {
        var records = await _api.GetAsync(
            Record,
            $"/api/v1/records/patient/{patientId}");

        ViewBag.PatientId = patientId;
        return View(records);
    }

    // ── Make Payment ──────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult MakePayment(int appointmentId)
    {
        ViewBag.AppointmentId = appointmentId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> MakePayment(
        int appointmentId, int patientId,
        int providerId, decimal amount,
        string mode, string currency)
    {
        var result = await _api.PostAsync(
            Payment, "/api/v1/payments", new
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                ProviderId = providerId,
                Amount = amount,
                Mode = mode,
                Currency = currency
            });

        if (result == null)
        {
            ViewBag.Error = "Payment failed. Please try again.";
            return View();
        }

        return RedirectToAction("ViewPaymentHistory",
            new { patientId });
    }

    // ── View Payment History ──────────────────────────────────────────────────

    public async Task<IActionResult> ViewPaymentHistory(
        int patientId)
    {
        var payments = await _api.GetAsync(
            Payment,
            $"/api/v1/payments/patient/{patientId}");

        ViewBag.PatientId = patientId;
        return View(payments);
    }

    // ── Submit Review ─────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult SubmitReview(
        int appointmentId, int providerId)
    {
        ViewBag.AppointmentId = appointmentId;
        ViewBag.ProviderId = providerId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SubmitReview(
        int appointmentId, int patientId,
        int providerId, int rating,
        string? comment, bool isAnonymous)
    {
        await _api.PostAsync(
            Review, "/api/v1/reviews", new
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                ProviderId = providerId,
                Rating = rating,
                Comment = comment,
                IsAnonymous = isAnonymous
            });

        return RedirectToAction("ViewMyAppointments",
            new { patientId });
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