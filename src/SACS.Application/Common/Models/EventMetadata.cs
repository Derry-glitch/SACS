namespace SACS.Application.Common.Models;

public class EventMetadata
{
    public string? Notes { get; set; }
    public string? AttachmentUrl { get; set; }
    
    // Quiz / Exam
    public int? DurationMinutes { get; set; }

    // Exam
    public string? SeatNumber { get; set; }

    // Project
    public string? SupervisorName { get; set; }
    public int? ProgressPercentage { get; set; }

    // Study Session
    public string? StudyTopic { get; set; }
    public int? StudyDuration { get; set; } // in minutes
}
