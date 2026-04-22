using NotificationService.DTOs;

namespace NotificationService.Services;

public interface INotificationService
{
    Task<NotificationResponseDto> Send(SendNotificationDto dto);

    Task<List<NotificationResponseDto>> SendBulk(SendBulkNotificationDto dto);

    Task<bool> MarkAsRead(int notificationId);

    Task MarkAllRead(int recipientId);

    Task<List<NotificationResponseDto>> GetByRecipient(int recipientId);

    Task<int> GetUnreadCount(int recipientId);

    Task<bool> DeleteNotification(int notificationId);

    Task<bool> SendEmail(SendEmailDto dto);

    Task<bool> SendSms(SendSmsDto dto);

    Task<List<NotificationResponseDto>> GetAll();
} 
