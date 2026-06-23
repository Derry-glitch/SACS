using System;
using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class Semester : BaseEntity
{
    public long AcademicSessionId { get; set; }
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsCurrent { get; set; } = false;

    // Navigation properties
    public virtual AcademicSession AcademicSession { get; set; } = null!;
    public virtual ICollection<CourseSemesterOffering> CourseOfferings { get; set; } = new List<CourseSemesterOffering>();
    public virtual ICollection<StudyPlan> StudyPlans { get; set; } = new List<StudyPlan>();
    public virtual ICollection<PerformanceSnapshot> PerformanceSnapshots { get; set; } = new List<PerformanceSnapshot>();
}
