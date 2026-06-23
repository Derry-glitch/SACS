using System;

namespace SACS.Application.Events.DTOs;

public class CalendarEventDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string EventType { get; set; } = null!; // Assignment, Quiz, Exam, Project
    public DateTime DueDate { get; set; }
    public string CourseName { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public string? Venue { get; set; }
}
