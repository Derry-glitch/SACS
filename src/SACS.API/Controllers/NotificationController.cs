using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SACS.Domain.Entities;
using SACS.Persistence.Contexts;

namespace SACS.API.Controllers;

[Authorize]
public class NotificationController : ApiControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationController(ApplicationDbContext context)
    {
        _context = context;
    }

    public record SendNotificationRequest(long UserId, string Title, string Body);

    // 1. POST /api/Notifications/send
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Recipient User ID, title, and body are required.");
        }

        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
        if (!userExists)
        {
            return NotFound("Target user not found.");
        }

        var notification = new NotificationLog
        {
            UserId = request.UserId,
            Title = request.Title,
            Body = request.Body,
            ChannelUsed = "System",
            SentAt = DateTime.UtcNow
        };

        _context.NotificationLogs.Add(notification);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Body = notification.Body,
            SentAt = notification.SentAt
        });
    }

    // 2. GET /api/Notifications/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserNotifications(long userId)
    {
        if (userId != CurrentUserId)
        {
            var isStaff = await _context.LecturerProfiles.AnyAsync(l => l.Id == CurrentUserId);
            if (!isStaff)
            {
                return Forbid("You cannot access another user's notifications.");
            }
        }

        var notifications = await _context.NotificationLogs
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.SentAt)
            .Select(n => new
            {
                Id = n.Id,
                UserId = n.UserId,
                Title = n.Title,
                Body = n.Body,
                SentAt = n.SentAt,
                IsRead = n.ReadAt != null,
                ReadAt = n.ReadAt
            })
            .ToListAsync();

        var unreadCount = notifications.Count(n => !n.IsRead);

        return Ok(new
        {
            UnreadCount = unreadCount,
            Notifications = notifications
        });
    }

    // 3. PUT /api/Notifications/mark-read/{id}
    [HttpPut("mark-read/{id}")]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        var notification = await _context.NotificationLogs.FindAsync(id);
        if (notification == null)
        {
            return NotFound("Notification not found.");
        }

        if (notification.UserId != CurrentUserId)
        {
            return Forbid("You cannot modify another user's notifications.");
        }

        if (notification.ReadAt == null)
        {
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Ok(new { Message = "Notification marked as read successfully." });
    }
}
