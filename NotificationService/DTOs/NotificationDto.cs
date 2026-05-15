namespace NotificationService.DTOs;

public class SendNotificationDto
{
    public int RecipientId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Channel { get; set; } = "APP";
    public int? RelatedId { get; set; }
    public string? RelatedType { get; set; }
}

public class SendBulkNotificationDto
{
    public List<int> RecipientIds { get; set; } = new();
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Channel { get; set; } = "APP";
    public int? RelatedId { get; set; }
    public string? RelatedType { get; set; }
}

public class SendEmailDto
{
    public string ToEmail { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public class SendSmsDto
{
    public string ToPhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class NotificationResponseDto
{
    public int NotificationId { get; set; }
    public int RecipientId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public int? RelatedId { get; set; }
    public string? RelatedType { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
} 
