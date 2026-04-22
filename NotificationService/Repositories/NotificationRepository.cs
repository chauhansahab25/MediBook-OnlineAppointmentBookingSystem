using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Entities;

namespace NotificationService.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> FindById(int notificationId)
    {
        return await _context.Notifications.FindAsync(notificationId);
    }

    public async Task<List<Notification>> FindByRecipientId(int recipientId)
    {
        return await _context.Notifications
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();
    }

    public async Task<List<Notification>> FindByRecipientIdAndIsRead(
        int recipientId, bool isRead)
    {
        return await _context.Notifications
            .Where(n => n.RecipientId == recipientId && n.IsRead == isRead)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();
    }

    public async Task<int> CountByRecipientIdAndIsRead(int recipientId, bool isRead)
    {
        return await _context.Notifications
            .CountAsync(n => n.RecipientId == recipientId && n.IsRead == isRead);
    }

    public async Task<List<Notification>> FindByType(string type)
    {
        return await _context.Notifications
            .Where(n => n.Type == type)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();
    }

    public async Task<List<Notification>> FindByRelatedId(int relatedId)
    {
        return await _context.Notifications
            .Where(n => n.RelatedId == relatedId)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();
    }

    public async Task<bool> DeleteByNotificationId(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);

        if (notification == null)
        {
            return false;
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Notification>> GetAll()
    {
        return await _context.Notifications
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();
    }

    public async Task<Notification> Add(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<List<Notification>> AddRange(List<Notification> notifications)
    {
        _context.Notifications.AddRange(notifications);
        await _context.SaveChangesAsync();
        return notifications;
    }

    public async Task<Notification> Update(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task UpdateAllReadByRecipientId(int recipientId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.RecipientId == recipientId && n.IsRead == false)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
    }
}
