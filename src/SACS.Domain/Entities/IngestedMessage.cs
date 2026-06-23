using System;
using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class IngestedMessage : BaseEntity
{
    public long UserId { get; set; }
    public string RawContent { get; set; } = null!;
    public string SourceChannel { get; set; } = null!; // Telegram, Email, ManualPaste
    public string ProcessingStatus { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
    public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<ExtractedDeadline> ExtractedDeadlines { get; set; } = new List<ExtractedDeadline>();
}
