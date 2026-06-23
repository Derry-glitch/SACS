using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class EventSubmission : BaseEntity
{
    public long AcademicEventId { get; set; }
    public long StudentId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Submitted"; // Submitted, Graded, Late, Resubmitted
    public decimal? ScoreObtained { get; set; }
    public string? LecturerFeedback { get; set; }
    public long? GradedByUserId { get; set; }
    public DateTime? GradedAt { get; set; }
    public bool IsLate { get; set; } = false;

    // Navigation properties
    public virtual AcademicEvent AcademicEvent { get; set; } = null!;
    public virtual StudentProfile Student { get; set; } = null!;
    public virtual User? Grader { get; set; }
}
