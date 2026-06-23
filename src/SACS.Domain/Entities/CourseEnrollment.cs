using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class CourseEnrollment : BaseEntity
{
    public long CourseOfferingId { get; set; }
    public long StudentId { get; set; }
    public string Status { get; set; } = "Active"; // Active, Dropped, Audited
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual CourseSemesterOffering CourseOffering { get; set; } = null!;
    public virtual StudentProfile Student { get; set; } = null!;
}
