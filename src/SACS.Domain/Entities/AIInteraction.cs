using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class AIInteraction : BaseEntity
{
    public long ChatSessionId { get; set; }
    public string RequestType { get; set; } = null!; // Chat, Summarization, StudyPlan, QuizGen
    public string PromptText { get; set; } = null!;
    public string ResponseText { get; set; } = null!;
    public int TokensUsed { get; set; }
    public string ModelUsed { get; set; } = null!; // e.g. gemini-1.5-pro
    public int LatencyInMs { get; set; }

    // Navigation property
    public virtual ChatSession ChatSession { get; set; } = null!;
}
