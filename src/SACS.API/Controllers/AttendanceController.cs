using System;
using System.Collections.Concurrent;
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
public class AttendanceController : ApiControllerBase
{
    private readonly ApplicationDbContext _context;

    // Static thread-safe dictionary to store active attendance sessions
    private static readonly ConcurrentDictionary<string, AttendanceSession> ActiveSessions = new();

    public AttendanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    public class AttendanceSession
    {
        public string SessionId { get; set; } = null!;
        public long CourseOfferingId { get; set; }
        public string Code { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public long CreatedByUserId { get; set; }
    }

    public record CreateSessionRequest(long CourseOfferingId, int DurationInMinutes);
    public record CheckInRequest(string Code);

    // 1. POST /api/Attendance/create-session
    [HttpPost("create-session")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        if (request == null || request.CourseOfferingId <= 0)
        {
            return BadRequest("Valid course offering ID is required.");
        }

        // Verify the offering exists
        var offering = await _context.CourseSemesterOfferings
            .Include(o => o.Course)
            .FirstOrDefaultAsync(o => o.Id == request.CourseOfferingId);

        if (offering == null)
        {
            return NotFound("Course offering not found.");
        }

        // Generate a unique 6-character alphanumeric code
        string code;
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        do
        {
            code = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        } while (ActiveSessions.Values.Any(s => s.Code == code && s.ExpiresAt > DateTime.UtcNow));

        var sessionId = Guid.NewGuid().ToString();
        var duration = request.DurationInMinutes > 0 ? request.DurationInMinutes : 15;

        var session = new AttendanceSession
        {
            SessionId = sessionId,
            CourseOfferingId = request.CourseOfferingId,
            Code = code,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(duration),
            CreatedByUserId = CurrentUserId
        };

        ActiveSessions[sessionId] = session;

        return Ok(new
        {
            SessionId = session.SessionId,
            CourseOfferingId = session.CourseOfferingId,
            CourseCode = offering.Course.Code,
            CourseTitle = offering.Course.Title,
            Code = session.Code,
            ExpiresAt = session.ExpiresAt
        });
    }

    // 2. POST /api/Attendance/check-in
    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Attendance code is required.");
        }

        var normalizedCode = request.Code.Trim().ToUpper();

        // Find active session matching code
        var session = ActiveSessions.Values
            .FirstOrDefault(s => s.Code == normalizedCode && s.ExpiresAt > DateTime.UtcNow);

        if (session == null)
        {
            return BadRequest("Invalid or expired attendance code.");
        }

        // Verify student profile exists for current user
        var student = await _context.StudentProfiles
            .FirstOrDefaultAsync(s => s.Id == CurrentUserId);

        if (student == null)
        {
            return BadRequest("Only students can check in to attendance sessions.");
        }

        // Check for duplicate attendance today
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var existing = await _context.AttendanceTrackings
            .AnyAsync(a => a.StudentId == CurrentUserId && 
                           a.CourseOfferingId == session.CourseOfferingId && 
                           a.Date == today);

        if (existing)
        {
            return BadRequest("You have already checked in for this course today.");
        }

        // Record check-in
        var record = new AttendanceTracking
        {
            CourseOfferingId = session.CourseOfferingId,
            StudentId = CurrentUserId,
            Date = today,
            Status = "Present",
            RecordedByUserId = session.CreatedByUserId,
            Notes = $"Checked in via code {session.Code} at {DateTime.UtcNow:HH:mm:ss} UTC"
        };

        _context.AttendanceTrackings.Add(record);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Check-in successful.",
            CourseOfferingId = record.CourseOfferingId,
            Date = record.Date,
            Status = record.Status
        });
    }

    // 3. GET /api/Attendance/history/{studentId}
    [HttpGet("history/{studentId}")]
    public async Task<IActionResult> GetHistory(long studentId)
    {
        var records = await _context.AttendanceTrackings
            .Include(a => a.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.Date)
            .Select(a => new
            {
                Id = a.Id,
                CourseOfferingId = a.CourseOfferingId,
                CourseCode = a.CourseOffering.Course.Code,
                CourseTitle = a.CourseOffering.Course.Title,
                Date = a.Date.ToString("yyyy-MM-dd"),
                Status = a.Status,
                Notes = a.Notes
            })
            .ToListAsync();

        // Calculate attendance rate (percentage present)
        double total = records.Count;
        double present = records.Count(r => r.Status == "Present" || r.Status == "Late");
        double percentage = total > 0 ? (present / total) * 100.0 : 100.0;

        return Ok(new
        {
            AttendancePercentage = Math.Round(percentage, 1),
            TotalClasses = total,
            ClassesAttended = present,
            Records = records
        });
    }

    // 4. GET /api/Attendance/session/{sessionId}
    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetSession(string sessionId)
    {
        if (!ActiveSessions.TryGetValue(sessionId, out var session))
        {
            return NotFound("Attendance session not found or expired.");
        }

        // Get matching tracking records for this course offering and date
        var sessionDate = DateOnly.FromDateTime(session.CreatedAt);
        
        var checkedInStudents = await _context.AttendanceTrackings
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Where(a => a.CourseOfferingId == session.CourseOfferingId && a.Date == sessionDate)
            .Select(a => new
            {
                StudentId = a.StudentId,
                FullName = $"{a.Student.User.FirstName} {a.Student.User.LastName}",
                MatriculationNumber = a.Student.MatriculationNumber,
                Status = a.Status,
                CheckInTime = a.Notes
            })
            .ToListAsync();

        return Ok(new
        {
            SessionId = session.SessionId,
            CourseOfferingId = session.CourseOfferingId,
            Code = session.Code,
            CreatedAt = session.CreatedAt,
            ExpiresAt = session.ExpiresAt,
            IsActive = session.ExpiresAt > DateTime.UtcNow,
            CheckedInCount = checkedInStudents.Count,
            CheckedInStudents = checkedInStudents
        });
    }
}
