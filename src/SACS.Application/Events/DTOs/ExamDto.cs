using System;

namespace SACS.Application.Events.DTOs;

public class ExamDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public long CourseOfferingId { get; set; }
    public string CourseName { get; set; } = null!;
    public DateTime ExamDate { get; set; }
    public string? Venue { get; set; }
    public int DurationMinutes { get; set; }
    public string? SeatNumber { get; set; }
    public string? Notes { get; set; }
}
