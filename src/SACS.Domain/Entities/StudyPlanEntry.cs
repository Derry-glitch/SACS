using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class StudyPlanEntry : BaseEntity
{
    public long StudyPlanId { get; set; }
    public long CourseOfferingId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string TopicToStudy { get; set; } = null!;
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual StudyPlan StudyPlan { get; set; } = null!;
    public virtual CourseSemesterOffering CourseOffering { get; set; } = null!;
}
