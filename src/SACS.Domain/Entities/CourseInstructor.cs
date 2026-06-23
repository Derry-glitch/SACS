using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class CourseInstructor : BaseEntity
{
    public long CourseOfferingId { get; set; }
    public long LecturerId { get; set; }
    public string Role { get; set; } = "Primary"; // Primary, Assistant
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual CourseSemesterOffering CourseOffering { get; set; } = null!;
    public virtual LecturerProfile Lecturer { get; set; } = null!;
}
