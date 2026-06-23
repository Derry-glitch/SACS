using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class AIGeneratedQuiz : BaseEntity
{
    public long UserId { get; set; }
    public long CourseOfferingId { get; set; }
    public string Title { get; set; } = null!;
    public string DifficultyLevel { get; set; } = "Medium"; // Easy, Medium, Hard
    public string QuizStructureJson { get; set; } = null!; // JSON payload containing questions/answers
    public decimal? ScoreObtained { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual StudentProfile Student { get; set; } = null!;
    public virtual CourseSemesterOffering CourseOffering { get; set; } = null!;
}
