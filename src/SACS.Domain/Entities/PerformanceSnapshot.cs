using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class PerformanceSnapshot : BaseEntity
{
    public long StudentId { get; set; }
    public long SemesterId { get; set; }
    public long? CourseOfferingId { get; set; }
    public int AssignmentsSubmitted { get; set; } = 0;
    public int AssignmentsMissed { get; set; } = 0;
    public decimal? AverageQuizScore { get; set; }
    public int TotalStudyMinutes { get; set; } = 0;
    public decimal? OnTimeSubmissionRate { get; set; }
    public DateOnly SnapshotDate { get; set; }

    // Navigation properties
    public virtual StudentProfile Student { get; set; } = null!;
    public virtual Semester Semester { get; set; } = null!;
    public virtual CourseSemesterOffering? CourseOffering { get; set; }
}
