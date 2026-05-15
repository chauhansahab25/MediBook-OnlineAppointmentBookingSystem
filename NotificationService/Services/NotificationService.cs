using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.SignalR;
using MimeKit;
using NotificationService.DTOs;
using NotificationService.Entities;
using NotificationService.Hubs;
using NotificationService.Repositories;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace NotificationService.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        INotificationRepository repo,
        IConfiguration configuration,
        IHubContext<NotificationHub> hubContext)
    {
        _repo = repo;
        _configuration = configuration;
        _hubContext = hubContext;
    }

    // ── Send Single Notification ──────────────────────────────────────────────

    public async Task<NotificationResponseDto> Send(SendNotificationDto dto)
    {
        var notification = new Notification
        {
            RecipientId = dto.RecipientId,
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            Channel = dto.Channel,
            RelatedId = dto.RelatedId,
            RelatedType = dto.RelatedType,
            IsRead = false,
            SentAt = DateTime.UtcNow
        };

        var saved = await _repo.Add(notification);

        // Push real-time notification via SignalR if channel is APP
        if (dto.Channel == "APP")
        {
            await _hubContext.Clients
                .Group($"user_{dto.RecipientId}")
                .SendAsync("ReceiveNotification", MapToResponse(saved));
        }

        // Send email if channel is EMAIL
        if (dto.Channel == "EMAIL")
        {
            await SendEmail(new SendEmailDto
            {
                ToEmail = string.Empty,
                ToName = string.Empty,
                Subject = dto.Title,
                Body = dto.Message
            });
        }

        return MapToResponse(saved);
    }

    // ── Send Bulk Notifications ───────────────────────────────────────────────

    public async Task<List<NotificationResponseDto>> SendBulk(SendBulkNotificationDto dto)
    {
        var notifications = dto.RecipientIds.Select(recipientId => new Notification
        {
            RecipientId = recipientId,
            Type = dto.Type,
            Title = dto.Title,
            Message = dto.Message,
            Channel = dto.Channel,
            RelatedId = dto.RelatedId,
            RelatedType = dto.RelatedType,
            IsRead = false,
            SentAt = DateTime.UtcNow
        }).ToList();

        var saved = await _repo.AddRange(notifications);

        // Push real-time to all recipients via SignalR
        foreach (var notification in saved)
        {
            await _hubContext.Clients
                .Group($"user_{notification.RecipientId}")
                .SendAsync("ReceiveNotification", MapToResponse(notification));
        }

        return saved.Select(MapToResponse).ToList();
    }

    // ── Mark Single Notification as Read ──────────────────────────────────────

    public async Task<bool> MarkAsRead(int notificationId)
    {
        var notification = await _repo.FindById(notificationId);

        if (notification == null)
        {
            return false;
        }

        notification.IsRead = true;
        await _repo.Update(notification);
        return true;
    }

    // ── Mark All Notifications as Read for a Recipient ────────────────────────

    public async Task MarkAllRead(int recipientId)
    {
        await _repo.UpdateAllReadByRecipientId(recipientId);
    }

    // ── Get All Notifications for a Recipient ─────────────────────────────────

    public async Task<List<NotificationResponseDto>> GetByRecipient(int recipientId)
    {
        var notifications = await _repo.FindByRecipientId(recipientId);
        return notifications.Select(MapToResponse).ToList();
    }

    // ── Get Unread Count ──────────────────────────────────────────────────────

    public async Task<int> GetUnreadCount(int recipientId)
    {
        return await _repo.CountByRecipientIdAndIsRead(recipientId, false);
    }

    // ── Delete Notification ───────────────────────────────────────────────────

    public async Task<bool> DeleteNotification(int notificationId)
    {
        return await _repo.DeleteByNotificationId(notificationId);
    }

    // ── Get All Notifications ─────────────────────────────────────────────────

    public async Task<List<NotificationResponseDto>> GetAll()
    {
        var notifications = await _repo.GetAll();
        return notifications.Select(MapToResponse).ToList();
    }

    // ── Send Email via MailKit ────────────────────────────────────────────────

    public async Task<bool> SendEmail(SendEmailDto dto)
    {
        try
        {
            string smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            int smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            string smtpUser = _configuration["Email:SmtpUser"] ?? string.Empty;
            string smtpPass = _configuration["Email:SmtpPass"] ?? string.Empty;
            string fromName = _configuration["Email:FromName"] ?? "MediBook";

            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(fromName, smtpUser));
            email.To.Add(new MailboxAddress(dto.ToName, dto.ToEmail));
            email.Subject = dto.Subject;

            email.Body = new TextPart("plain")
            {
                Text = dto.Body
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(smtpHost, smtpPort,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(smtpUser, smtpPass);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            return true;
        }
        catch
        {
            // Log and continue — email failure should not crash the service
            return false;
        }
    }

    // ── Send SMS via Twilio ───────────────────────────────────────────────────

    public async Task<bool> SendSms(SendSmsDto dto)
    {
        try
        {
            string accountSid = _configuration["Twilio:AccountSid"] ?? string.Empty;
            string authToken = _configuration["Twilio:AuthToken"] ?? string.Empty;
            string fromNumber = _configuration["Twilio:FromNumber"] ?? string.Empty;

            TwilioClient.Init(accountSid, authToken);

            await MessageResource.CreateAsync(
                body: dto.Message,
                from: new Twilio.Types.PhoneNumber(fromNumber),
                to: new Twilio.Types.PhoneNumber(dto.ToPhoneNumber)
            );

            return true;
        }
        catch
        {
            // Log and continue — SMS failure should not crash the service
            return false;
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private NotificationResponseDto MapToResponse(Notification n)
    {
        return new NotificationResponseDto
        {
            NotificationId = n.NotificationId,
            RecipientId = n.RecipientId,
            Type = n.Type,
            Title = n.Title,
            Message = n.Message,
            Channel = n.Channel,
            RelatedId = n.RelatedId,
            RelatedType = n.RelatedType,
            IsRead = n.IsRead,
            SentAt = n.SentAt
        };
    }
} 
