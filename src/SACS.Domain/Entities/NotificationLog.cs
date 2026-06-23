using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class NotificationLog : BaseEntity
{
    public long UserId { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string ChannelUsed { get; set; } = null!; // Push, Email
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }

    // Navigation property
    public virtual User User { get; set; } = null!;
}
