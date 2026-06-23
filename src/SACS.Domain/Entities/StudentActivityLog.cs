using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class StudentActivityLog : BaseEntity
{
    public long UserId { get; set; }
    public string ActivityType { get; set; } = null!; // ViewLectureNote, StartQuiz, CreateStudyPlan
    public string? EntityAffected { get; set; }
    public long? EntityId { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual User User { get; set; } = null!;
}
