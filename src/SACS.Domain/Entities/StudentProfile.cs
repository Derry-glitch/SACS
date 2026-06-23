using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class StudentProfile : BaseEntity
{
    // The Id property from BaseEntity will be mapped to the UserId column and acts as the primary key.
    
    public string MatriculationNumber { get; set; } = null!;
    public int AcademicLevel { get; set; }
    public decimal CurrentGPA { get; set; } = 0.00m;
    public decimal CurrentCGPA { get; set; } = 0.00m;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();
    public virtual ICollection<AttendanceTracking> Attendances { get; set; } = new List<AttendanceTracking>();
    public virtual ICollection<StudyPlan> StudyPlans { get; set; } = new List<StudyPlan>();
    public virtual ICollection<AIGeneratedQuiz> GeneratedQuizzes { get; set; } = new List<AIGeneratedQuiz>();
    public virtual ICollection<PerformanceSnapshot> PerformanceSnapshots { get; set; } = new List<PerformanceSnapshot>();
    public virtual ICollection<EventSubmission> Submissions { get; set; } = new List<EventSubmission>();
}
