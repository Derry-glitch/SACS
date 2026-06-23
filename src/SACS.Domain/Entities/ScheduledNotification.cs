using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class ScheduledNotification : BaseEntity
{
    public long UserId { get; set; }
    public long AcademicEventId { get; set; }
    public string ReminderType { get; set; } = null!; // SevenDay, ThreeDay, TwentyFourHour, TwoHour
    public DateTime ScheduledTime { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Sent, Failed, Cancelled
    public DateTime? SentAt { get; set; }
    public string? FcmMessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; } = 0;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual AcademicEvent AcademicEvent { get; set; } = null!;
}
