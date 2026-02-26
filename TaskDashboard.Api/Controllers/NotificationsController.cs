using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskDashboard.Api.Data;
using TaskDashboard.Api.Models;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context) => _context = context;

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult> GetNotifications()
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult> GetUnreadCount()
    {
        var count = await _context.Notifications.CountAsync(n => n.UserId == UserId && !n.IsRead);
        return Ok(new { count });
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == UserId);
        if (notification == null) return NotFound();
        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == UserId && !n.IsRead)
            .ToListAsync();
        notifications.ForEach(n => n.IsRead = true);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult> CreateNotification([FromBody] NotificationDto dto)
    {
        if (string.IsNullOrEmpty(UserId)) return Unauthorized();

        var notification = new Notification
        {
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            UserId = UserId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        try
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNotifications), new { id = notification.Id }, notification);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public class NotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
}
