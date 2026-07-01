using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Students.Queries.VerifyStudent;
using SACS.Domain.Entities;
using SACS.Persistence.Contexts;

namespace SACS.API.Controllers;

[Authorize(Roles = "Lecturer")]
public class LecturerController : ApiControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ISender _sender;

    public LecturerController(ApplicationDbContext context, ISender sender)
    {
        _context = context;
        _sender = sender;
    }

    public record CreateAnnouncementRequest(
        string Title,
        string Message,
        string? Department,
        string? TargetAudience,
        string Priority // Normal or Urgent
    );

    public record CreateSessionRequest(long CourseOfferingId, int DurationInMinutes);

    public record VerifyStudentIdRequest(string MatriculationNumber);

    // 1. POST /api/Lecturer/create-announcement
    [HttpPost("create-announcement")]
    public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementRequest request)
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

        // Resolve recipients
        var query = _context.StudentProfiles.AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.Department))
        {
            var deptNormalized = request.Department.Trim().ToLower();
            query = query.Where(s => s.CourseEnrollments.Any(ce => 
                ce.CourseOffering.Course.Department.Name.ToLower().Contains(deptNormalized) ||
                ce.CourseOffering.Course.Department.Code.ToLower().Contains(deptNormalized)));
        }

        var studentRecipients = await query.ToListAsync();
        if (!studentRecipients.Any())
        {
            studentRecipients = await _context.StudentProfiles.ToListAsync();
        }

        foreach (var student in studentRecipients)
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
                Title = $"Lecturer Announcement: {announcement.Title}",
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
            RecipientsCount = studentRecipients.Count
        });
    }

    // 2. POST /api/Lecturer/create-attendance-session
    [HttpPost("create-attendance-session")]
    public async Task<IActionResult> CreateAttendanceSession([FromBody] CreateSessionRequest request)
    {
        if (request == null || request.CourseOfferingId <= 0)
        {
            return BadRequest("Course offering ID is required.");
        }

        var offering = await _context.CourseSemesterOfferings
            .Include(co => co.Course)
            .FirstOrDefaultAsync(co => co.Id == request.CourseOfferingId);

        if (offering == null)
        {
            return NotFound("Course offering not found.");
        }

        // Generate 6-digit session code
        var rand = new Random();
        var code = rand.Next(100000, 999999).ToString();
        var sessionId = Guid.NewGuid().ToString();

        var session = new AttendanceController.AttendanceSession
        {
            SessionId = sessionId,
            CourseOfferingId = request.CourseOfferingId,
            Code = code,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(request.DurationInMinutes > 0 ? request.DurationInMinutes : 15),
            CreatedByUserId = CurrentUserId
        };

        AttendanceController.ActiveSessions[code] = session;

        return Ok(new
        {
            SessionId = session.SessionId,
            CourseOfferingId = session.CourseOfferingId,
            CourseCode = offering.Course.Code,
            CourseTitle = offering.Course.Title,
            Code = session.Code,
            CreatedAt = session.CreatedAt,
            ExpiresAt = session.ExpiresAt
        });
    }

    // 3. GET /api/Lecturer/course-attendance/{courseId}
    [HttpGet("course-attendance/{courseId}")]
    public async Task<IActionResult> GetCourseAttendance(long courseId)
    {
        var records = await _context.AttendanceTrackings
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(a => a.CourseOffering.CourseId == courseId || a.CourseOfferingId == courseId)
            .OrderByDescending(a => a.Date)
            .Select(a => new
            {
                Id = a.Id,
                StudentId = a.StudentId,
                StudentName = $"{a.Student.User.FirstName} {a.Student.User.LastName}",
                MatriculationNumber = a.Student.MatriculationNumber,
                Date = a.Date,
                Status = a.Status,
                Notes = a.Notes
            })
            .ToListAsync();

        return Ok(records);
    }

    // 4. GET /api/Lecturer/students/{courseId}
    [HttpGet("students/{courseId}")]
    public async Task<IActionResult> GetCourseStudents(long courseId)
    {
        var students = await _context.CourseEnrollments
            .Include(ce => ce.Student)
                .ThenInclude(s => s.User)
            .Where(ce => ce.CourseOffering.CourseId == courseId || ce.CourseOfferingId == courseId)
            .Select(ce => new
            {
                Id = ce.Student.Id,
                FirstName = ce.Student.User.FirstName,
                LastName = ce.Student.User.LastName,
                MatriculationNumber = ce.Student.MatriculationNumber,
                AcademicLevel = ce.Student.AcademicLevel,
                CurrentGPA = ce.Student.CurrentGPA
            })
            .ToListAsync();

        return Ok(students);
    }

    // 5. POST /api/Lecturer/verify-student-id
    [HttpPost("verify-student-id")]
    public async Task<IActionResult> VerifyStudentId([FromBody] VerifyStudentIdRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MatriculationNumber))
        {
            return BadRequest("Matriculation number is required.");
        }

        var result = await _sender.Send(new VerifyStudentQuery(request.MatriculationNumber));
        return Ok(result);
    }
}
