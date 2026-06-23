using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class ExtractedDeadline : BaseEntity
{
    public long IngestedMessageId { get; set; }
    public long? AcademicEventId { get; set; }
    public string Title { get; set; } = null!;
    public string? CourseCodeGuess { get; set; }
    public DateTime? ParsedDueDate { get; set; }
    public decimal ConfidenceScore { get; set; }
    public bool IsConfirmed { get; set; } = false;
    public bool IsRejected { get; set; } = false;
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual IngestedMessage IngestedMessage { get; set; } = null!;
    public virtual AcademicEvent? AcademicEvent { get; set; }
}
