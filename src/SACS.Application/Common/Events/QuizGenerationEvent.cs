namespace SACS.Application.Common.Events;

public class QuizGenerationEvent
{
    public long CourseOfferingId { get; set; }
    public string Title { get; set; } = null!;
    public string LectureNoteContent { get; set; } = null!;
    public string DifficultyLevel { get; set; } = null!;
    public long UserId { get; set; }

    public QuizGenerationEvent() { }

    public QuizGenerationEvent(long courseOfferingId, string title, string lectureNoteContent, string difficultyLevel, long userId)
    {
        CourseOfferingId = courseOfferingId;
        Title = title;
        LectureNoteContent = lectureNoteContent;
        DifficultyLevel = difficultyLevel;
        UserId = userId;
    }
}
