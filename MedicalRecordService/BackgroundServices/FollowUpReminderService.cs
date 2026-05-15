using MedicalRecordService.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.BackgroundServices;

// Runs automatically in the background every day
// Checks for records with follow-up dates due tomorrow
// and calls the Notification-Service to send reminders

public class FollowUpReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FollowUpReminderService> _logger;

    public FollowUpReminderService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<FollowUpReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FollowUpReminderService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckFollowUps();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FollowUpReminderService.");
            }

            // Run once every 24 hours
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task CheckFollowUps()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Find all records with follow-up date set to tomorrow
        DateTime tomorrow = DateTime.UtcNow.AddDays(1).Date;

        var records = await context.MedicalRecords
            .Where(r => r.FollowUpDate.HasValue
                     && r.FollowUpDate.Value.Date == tomorrow)
            .ToListAsync();

        _logger.LogInformation(
            "Found {Count} follow-up records due tomorrow.", records.Count);

        // For each record, call the Notification-Service
        string notificationServiceUrl = _configuration["ServiceUrls:NotificationService"]
            ?? "http://localhost:5006";

        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(notificationServiceUrl);

        foreach (var record in records)
        {
            try
            {
                var payload = new
                {
                    RecipientId = record.PatientId,
                    Type = "FOLLOWUP",
                    Title = "Follow-Up Reminder",
                    Message = $"You have a follow-up appointment scheduled for tomorrow " +
                              $"({tomorrow:dd MMM yyyy}). Please contact your provider.",
                    Channel = "APP",
                    RelatedId = record.AppointmentId,
                    RelatedType = "Appointment"
                };

                await httpClient.PostAsJsonAsync("/api/v1/notifications", payload);

                _logger.LogInformation(
                    "Follow-up reminder sent for PatientId: {PatientId}", record.PatientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send follow-up reminder for RecordId: {RecordId}",
                    record.RecordId);
            }
        }
    }
}
