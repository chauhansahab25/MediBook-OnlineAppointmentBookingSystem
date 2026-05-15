using NotificationService.Entities;

namespace NotificationService.Repositories;

public interface INotificationRepository
{
    Task<Notification?> FindById(int notificationId);

    Task<List<Notification>> FindByRecipientId(int recipientId);

    Task<List<Notification>> FindByRecipientIdAndIsRead(int recipientId, bool isRead);

    Task<int> CountByRecipientIdAndIsRead(int recipientId, bool isRead);

    Task<List<Notification>> FindByType(string type);

    Task<List<Notification>> FindByRelatedId(int relatedId);

    Task<bool> DeleteByNotificationId(int notificationId);

    Task<List<Notification>> GetAll();

    Task<Notification> Add(Notification notification);

    Task<List<Notification>> AddRange(List<Notification> notifications);

    Task<Notification> Update(Notification notification);

    Task UpdateAllReadByRecipientId(int recipientId);
} 
