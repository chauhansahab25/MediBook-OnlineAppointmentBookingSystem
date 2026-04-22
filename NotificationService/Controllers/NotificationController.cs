using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Services;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        _service = service;
    }

    /// <summary>Send a single notification</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Send([FromBody] SendNotificationDto dto)
    {
        var result = await _service.Send(dto);
        return Ok(result);
    }

    /// <summary>Send bulk notifications to multiple recipients (Admin broadcast)</summary>
    [HttpPost("bulk")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendBulk([FromBody] SendBulkNotificationDto dto)
    {
        var result = await _service.SendBulk(dto);
        return Ok(result);
    }

    /// <summary>Send an email notification directly</summary>
    [HttpPost("email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailDto dto)
    {
        var result = await _service.SendEmail(dto);

        if (!result)
        {
            return BadRequest(new { message = "Failed to send email." });
        }

        return Ok(new { message = "Email sent successfully." });
    }

    /// <summary>Send an SMS notification directly</summary>
    [HttpPost("sms")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendSms([FromBody] SendSmsDto dto)
    {
        var result = await _service.SendSms(dto);

        if (!result)
        {
            return BadRequest(new { message = "Failed to send SMS." });
        }

        return Ok(new { message = "SMS sent successfully." });
    }

    /// <summary>Get all notifications</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var notifications = await _service.GetAll();
        return Ok(notifications);
    }

    /// <summary>Get all notifications for a recipient</summary>
    [HttpGet("recipient/{recipientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByRecipient(int recipientId)
    {
        var notifications = await _service.GetByRecipient(recipientId);
        return Ok(notifications);
    }

    /// <summary>Get unread notification count for a recipient</summary>
    [HttpGet("recipient/{recipientId}/unread/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(int recipientId)
    {
        var count = await _service.GetUnreadCount(recipientId);
        return Ok(new { recipientId, unreadCount = count });
    }

    /// <summary>Mark a single notification as read</summary>
    [HttpPut("{notificationId}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var result = await _service.MarkAsRead(notificationId);

        if (!result)
        {
            return NotFound(new { message = "Notification not found." });
        }

        return Ok(new { message = "Notification marked as read." });
    }

    /// <summary>Mark all notifications as read for a recipient</summary>
    [HttpPut("recipient/{recipientId}/read/all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllRead(int recipientId)
    {
        await _service.MarkAllRead(recipientId);
        return Ok(new { message = "All notifications marked as read." });
    }

    /// <summary>Delete a notification</summary>
    [HttpDelete("{notificationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int notificationId)
    {
        var result = await _service.DeleteNotification(notificationId);

        if (!result)
        {
            return NotFound(new { message = "Notification not found." });
        }

        return Ok(new { message = "Notification deleted successfully." });
    }
} 
