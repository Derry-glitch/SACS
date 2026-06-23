using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class LectureNoteSummary : BaseEntity
{
    public long UserId { get; set; }
    public long CourseOfferingId { get; set; }
    public string SourceFileName { get; set; } = null!;
    public string OriginalFileUrl { get; set; } = null!;
    public string SummaryText { get; set; } = null!;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual CourseSemesterOffering CourseOffering { get; set; } = null!;
}
