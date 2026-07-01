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
public class AnnouncementsController : ApiControllerBase
{
    private readonly ApplicationDbContext _context;

    public AnnouncementsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public record CreateAnnouncementRequest(
        string Title,
        string Message,
        string? Department,
        string? TargetAudience,
        string Priority // Normal or Urgent
    );

    // 1. POST /api/Announcements/create
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] CreateAnnouncementRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Title and Message are required.");
        }

        var announcement = new Announcement
        {
            InstitutionId = CurrentInstitutionId > 0 ? CurrentInstitutionId : 1,
            Title = request.Title,
            Content = request.Message,
            Priority = request.Priority ?? "Normal",
            CreatedByUserId = CurrentUserId,
            IsPinned = request.Priority?.ToLower() == "urgent"
        };

        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        // Find affected student profiles
        var query = _context.StudentProfiles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            var deptNormalized = request.Department.Trim().ToLower();
            query = query.Where(s => s.CourseEnrollments.Any(ce => 
                ce.CourseOffering.Course.Department.Name.ToLower().Contains(deptNormalized) ||
                ce.CourseOffering.Course.Department.Code.ToLower().Contains(deptNormalized)));
        }

        var recipientStudents = await query.ToListAsync();
        if (!recipientStudents.Any())
        {
            // Fallback: send to all students
            recipientStudents = await _context.StudentProfiles.ToListAsync();
        }

        // Add recipients and notifications
        foreach (var student in recipientStudents)
        {
            var recipient = new AnnouncementRecipient
            {
                AnnouncementId = announcement.Id,
                UserId = student.Id,
                IsRead = false
            };
            _context.AnnouncementRecipients.Add(recipient);

            var notification = new NotificationLog
            {
                UserId = student.Id,
                Title = $"New Announcement: {announcement.Title}",
                Body = announcement.Content,
                ChannelUsed = "System",
                SentAt = DateTime.UtcNow
            };
            _context.NotificationLogs.Add(notification);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Message = announcement.Content,
            Priority = announcement.Priority,
            CreatedAt = announcement.CreatedAtUtc,
            RecipientsCount = recipientStudents.Count
        });
    }

    // 2. GET /api/Announcements/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var isStudent = await _context.StudentProfiles.AnyAsync(s => s.Id == CurrentUserId);
        if (isStudent)
        {
            var studentAnnouncements = await _context.AnnouncementRecipients
                .Include(r => r.Announcement)
                    .ThenInclude(a => a.Creator)
                .Where(r => r.UserId == CurrentUserId && !r.Announcement.IsDeleted)
                .OrderByDescending(r => r.Announcement.CreatedAtUtc)
                .Select(r => new
                {
                    Id = r.Announcement.Id,
                    Title = r.Announcement.Title,
                    Message = r.Announcement.Content,
                    Priority = r.Announcement.Priority,
                    CreatedAt = r.Announcement.CreatedAtUtc,
                    CreatorName = $"{r.Announcement.Creator.FirstName} {r.Announcement.Creator.LastName}",
                    IsRead = r.IsRead
                })
                .ToListAsync();
            return Ok(studentAnnouncements);
        }
        else
        {
            var allAnnouncements = await _context.Announcements
                .Include(a => a.Creator)
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAtUtc)
                .Select(a => new
                {
                    Id = a.Id,
                    Title = a.Title,
                    Message = a.Content,
                    Priority = a.Priority,
                    CreatedAt = a.CreatedAtUtc,
                    CreatorName = $"{a.Creator.FirstName} {a.Creator.LastName}",
                    IsRead = true
                })
                .ToListAsync();
            return Ok(allAnnouncements);
        }
    }

    // 3. GET /api/Announcements/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var announcement = await _context.Announcements
            .Include(a => a.Creator)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (announcement == null)
        {
            return NotFound("Announcement not found.");
        }

        // Mark as read for current student recipient if applicable
        var recipient = await _context.AnnouncementRecipients
            .FirstOrDefaultAsync(r => r.AnnouncementId == id && r.UserId == CurrentUserId);

        if (recipient != null && !recipient.IsRead)
        {
            recipient.IsRead = true;
            recipient.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Ok(new
        {
            Id = announcement.Id,
            Title = announcement.Title,
            Message = announcement.Content,
            Priority = announcement.Priority,
            CreatedAt = announcement.CreatedAtUtc,
            CreatorName = $"{announcement.Creator.FirstName} {announcement.Creator.LastName}"
        });
    }

    // 4. DELETE /api/Announcements/delete/{id}
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var announcement = await _context.Announcements.FindAsync(id);
        if (announcement == null)
        {
            return NotFound("Announcement not found.");
        }

        announcement.IsDeleted = true;
        announcement.DeletedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Announcement deleted successfully." });
    }
}
