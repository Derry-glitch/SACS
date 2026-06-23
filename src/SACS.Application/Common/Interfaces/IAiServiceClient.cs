using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SACS.Application.Common.Interfaces;

public interface IAiServiceClient
{
    Task<DeadlineExtractionResponseDto> ExtractDeadlinesAsync(string text, CancellationToken cancellationToken = default);
    Task<SummaryResponseDto> SummarizeLectureNotesAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task<QuizGenerationResponseDto> GenerateQuizAsync(string content, string difficulty, CancellationToken cancellationToken = default);
    Task<StudyPlanResponseDto> GenerateStudyPlanAsync(StudyPlanRequestDto request, CancellationToken cancellationToken = default);
}

public class ExtractedDeadlineItemDto
{
    public string Title { get; set; } = null!;
    public string? CourseCodeGuess { get; set; }
    public DateTime? ParsedDueDate { get; set; }
    public decimal ConfidenceScore { get; set; }
}

public class DeadlineExtractionResponseDto
{
    public List<ExtractedDeadlineItemDto> Deadlines { get; set; } = new();
}

public class SummaryResponseDto
{
    public string Summary { get; set; } = null!;
}

public class QuizQuestionDto
{
    public string QuestionText { get; set; } = null!;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = null!;
    public string? Explanation { get; set; }
}

public class QuizGenerationResponseDto
{
    public string QuizTitle { get; set; } = null!;
    public string DifficultyLevel { get; set; } = null!;
    public List<QuizQuestionDto> Questions { get; set; } = new();
}

public class DeadlineInputDto
{
    public string CourseCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string DueDate { get; set; } = null!; // ISO String
    public string Priority { get; set; } = null!;
}

public class StudyPlanRequestDto
{
    public List<string> Courses { get; set; } = new();
    public List<DeadlineInputDto> Deadlines { get; set; } = new();
    public Dictionary<string, double> FreeStudyHours { get; set; } = new();
}

public class StudyPlanEntryItemDto
{
    public string DayOfWeek { get; set; } = null!;
    public string Date { get; set; } = null!; // YYYY-MM-DD
    public string StartTime { get; set; } = null!; // HH:MM:SS
    public string EndTime { get; set; } = null!; // HH:MM:SS
    public string CourseCode { get; set; } = null!;
    public string Topic { get; set; } = null!;
    public string Priority { get; set; } = null!;
}

public class StudyPlanResponseDto
{
    public string PlanName { get; set; } = null!;
    public List<StudyPlanEntryItemDto> Entries { get; set; } = new();
}
