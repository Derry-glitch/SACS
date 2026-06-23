using System;
using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class AcademicEvent : BaseEntity
{
    public long CourseOfferingId { get; set; }
    public long CreatedByUserId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string EventType { get; set; } = null!; // Assignment, Quiz, Exam, Presentation, Project, MidTerm
    public DateTime DueDate { get; set; }
    public string? Venue { get; set; }
    public decimal? MaxScore { get; set; }
    public decimal? Weight { get; set; }
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Status { get; set; } = "Active"; // Active, Completed, Cancelled, Extended
    public string SourceType { get; set; } = "Manual"; // Manual, AIExtracted
    public bool IsVisibleToStudents { get; set; } = true;

    // Navigation properties
    public virtual CourseSemesterOffering CourseOffering { get; set; } = null!;
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<EventSubmission> Submissions { get; set; } = new List<EventSubmission>();
    public virtual ICollection<EventAttachment> Attachments { get; set; } = new List<EventAttachment>();
    public virtual ICollection<ScheduledNotification> ScheduledNotifications { get; set; } = new List<ScheduledNotification>();
    public virtual ICollection<ExtractedDeadline> ExtractedDeadlines { get; set; } = new List<ExtractedDeadline>();
}
