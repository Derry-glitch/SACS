using System;

namespace SACS.Application.Events.DTOs;

public class QuizDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public long CourseOfferingId { get; set; }
    public string CourseName { get; set; } = null!;
    public DateTime Date { get; set; }
    public int DurationMinutes { get; set; }
    public string? ReminderWindow { get; set; }
    public string? Notes { get; set; }
}
