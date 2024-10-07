using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // Get unread notifications for a vendor
    [HttpGet("unread/{vendorId}")]
    public async Task<IActionResult> GetUnreadNotifications(string vendorId)
    {
        var notifications = await _notificationService.GetUnreadNotificationsAsync(vendorId);
        return Ok(notifications);
    }

    // Mark a notification as read
    [HttpPut("mark-as-read/{notificationId}")]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        await _notificationService.MarkAsReadAsync(notificationId);
        return Ok("Notification marked as read.");
    }
}
