using MediBook.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MediBook.Web.Controllers;

public class AdminController : Controller
{
    private readonly IApiService _api;
    private readonly IConfiguration _config;

    public AdminController(
        IApiService api, IConfiguration config)
    {
        _api = api;
        _config = config;
    }

    private string Auth =>
        _config["ServiceUrls:AuthService"]!;
    private string Provider =>
        _config["ServiceUrls:ProviderService"]!;
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

    // ── Admin Dashboard ───────────────────────────────────────────────────────

    public IActionResult AdminDashboard()
    {
        return View();
    }

    // ── Manage Users ──────────────────────────────────────────────────────────

    public async Task<IActionResult> ManageUsers(
        string role = "Patient")
    {
        var users = await _api.GetAsync(
            Auth,
            $"/api/v1/auth/users?role={role}");

        ViewBag.CurrentRole = role;
        return View(users);
    }

    // ── Suspend User ──────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> SuspendUser(
        int userId, string role)
    {
        await _api.PutAsync(
            Auth,
            $"/api/v1/auth/deactivate/{userId}");

        return RedirectToAction("ManageUsers",
            new { role });
    }

    // ── Delete User ───────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> DeleteUser(
        int userId, string role)
    {
        await _api.DeleteAsync(
            Auth,
            $"/api/v1/auth/users/{userId}");

        return RedirectToAction("ManageUsers",
            new { role });
    }

    // ── Manage Providers ──────────────────────────────────────────────────────

    public async Task<IActionResult> ManageProviders()
    {
        var providers = await _api.GetAsync(
            Provider, "/api/v1/providers");

        return View(providers);
    }

    // ── Verify Provider ───────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> VerifyProvider(
        int providerId)
    {
        await _api.PutAsync(
            Provider,
            $"/api/v1/providers/{providerId}/verify");

        return RedirectToAction("ManageProviders");
    }

    // ── View All Appointments ─────────────────────────────────────────────────

    public async Task<IActionResult> ViewAllAppointments()
    {
        var appointments = await _api.GetAsync(
            Appointment, "/api/v1/appointments");

        return View(appointments);
    }

    // ── View All Payments ─────────────────────────────────────────────────────

    public async Task<IActionResult> ViewAllPayments(
        DateTime? startDate, DateTime? endDate)
    {
        var start = (startDate ?? DateTime.UtcNow.AddMonths(-1))
            .ToString("yyyy-MM-dd");

        var end = (endDate ?? DateTime.UtcNow)
            .ToString("yyyy-MM-dd");

        var payments = await _api.GetAsync(
            Payment,
            $"/api/v1/payments/history" +
            $"?startDate={start}&endDate={end}");

        ViewBag.StartDate = start;
        ViewBag.EndDate = end;
        return View(payments);
    }

    // ── View Platform Analytics ───────────────────────────────────────────────

    public async Task<IActionResult> ViewPlatformAnalytics()
    {
        var providers = await _api.GetAsync(
            Provider, "/api/v1/providers");

        var reviews = await _api.GetAsync(
            Review, "/api/v1/reviews");

        var notifications = await _api.GetAsync(
            Notification, "/api/v1/notifications");

        ViewBag.Providers = providers;
        ViewBag.Reviews = reviews;
        ViewBag.Notifications = notifications;

        return View();
    }

    // ── View All Reviews ──────────────────────────────────────────────────────

    public async Task<IActionResult> ViewAllReviews()
    {
        var reviews = await _api.GetAsync(
            Review, "/api/v1/reviews");

        return View(reviews);
    }

    // ── Moderate Review ───────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> ModerateReview(
        int reviewId)
    {
        await _api.DeleteAsync(
            Review,
            $"/api/v1/reviews/{reviewId}");

        return RedirectToAction("ViewAllReviews");
    }

    // ── Send Platform Notification ────────────────────────────────────────────

    [HttpGet]
    public IActionResult SendPlatformNotification()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendPlatformNotification(
        string recipientIdsRaw, string type,
        string title, string message, string channel)
    {
        var recipientIds = recipientIdsRaw
            .Split(',')
            .Where(x => int.TryParse(x.Trim(), out _))
            .Select(x => int.Parse(x.Trim()))
            .ToList();

        await _api.PostAsync(
            Notification,
            "/api/v1/notifications/bulk", new
            {
                RecipientIds = recipientIds,
                Type = type,
                Title = title,
                Message = message,
                Channel = channel
            });

        TempData["Success"] =
            "Platform notification sent successfully.";

        return RedirectToAction("AdminDashboard");
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

    // ── Generate Revenue Report ───────────────────────────────────────────────

    public async Task<IActionResult> GenerateRevenueReport(
        int providerId)
    {
        var revenue = await _api.GetAsync(
            Payment,
            $"/api/v1/payments/provider/{providerId}/revenue");

        var paymentHistory = await _api.GetAsync(
            Payment,
            $"/api/v1/payments/history" +
            $"?startDate=" +
            $"{DateTime.UtcNow.AddMonths(-12):yyyy-MM-dd}" +
            $"&endDate={DateTime.UtcNow:yyyy-MM-dd}");

        ViewBag.ProviderId = providerId;
        ViewBag.PaymentHistory = paymentHistory;

        return View(revenue);
    }
}