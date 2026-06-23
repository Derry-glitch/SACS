using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class AttendanceTracking : BaseEntity
{
    public long CourseOfferingId { get; set; }
    public long StudentId { get; set; }
    public DateOnly Date { get; set; }
    public string Status { get; set; } = null!; // Present, Absent, Late, Excused
    public long RecordedByUserId { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual CourseSemesterOffering CourseOffering { get; set; } = null!;
    public virtual StudentProfile Student { get; set; } = null!;
    public virtual User Recorder { get; set; } = null!;
}
