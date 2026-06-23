using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class CourseSemesterOffering : BaseEntity
{
    public long CourseId { get; set; }
    public long SemesterId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Course Course { get; set; } = null!;
    public virtual Semester Semester { get; set; } = null!;
    public virtual ICollection<CourseInstructor> Instructors { get; set; } = new List<CourseInstructor>();
    public virtual ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
    public virtual ICollection<AttendanceTracking> Attendances { get; set; } = new List<AttendanceTracking>();
    public virtual ICollection<AcademicEvent> AcademicEvents { get; set; } = new List<AcademicEvent>();
    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
    public virtual ICollection<StudyPlanEntry> StudyPlanEntries { get; set; } = new List<StudyPlanEntry>();
    public virtual ICollection<AIGeneratedQuiz> GeneratedQuizzes { get; set; } = new List<AIGeneratedQuiz>();
    public virtual ICollection<LectureNoteSummary> LectureNoteSummaries { get; set; } = new List<LectureNoteSummary>();
    public virtual ICollection<FileRecord> FileRecords { get; set; } = new List<FileRecord>();
    public virtual ICollection<PerformanceSnapshot> PerformanceSnapshots { get; set; } = new List<PerformanceSnapshot>();
}
