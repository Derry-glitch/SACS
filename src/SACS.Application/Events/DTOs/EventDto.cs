using System;
using SACS.Domain.Common;

namespace SACS.Application.Events.DTOs;

public class EventDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public long CourseId { get; set; }
    public string CourseCode { get; set; } = null!;
    public AcademicEventType EventType { get; set; }
    public DateTime DueDateTime { get; set; }
    public string PriorityLevel { get; set; } = null!;
    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }
    public long CreatedByUserId { get; set; }

    // Type-specific properties
    public int? DurationMinutes { get; set; }
    public string? Venue { get; set; }
    public string? SeatNumber { get; set; }
    public string? SupervisorName { get; set; }
    public int? ProgressPercentage { get; set; }
    public string? StudyTopic { get; set; }
    public int? StudyDuration { get; set; }
}
