using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class AnnouncementRecipient : BaseEntity
{
    public long AnnouncementId { get; set; }
    public long UserId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Navigation properties
    public virtual Announcement Announcement { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
