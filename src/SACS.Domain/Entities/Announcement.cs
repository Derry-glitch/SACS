using System;
using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class Announcement : BaseEntity
{
    public long InstitutionId { get; set; }
    public long? CourseOfferingId { get; set; }
    public long CreatedByUserId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    public bool IsPinned { get; set; } = false;
    public DateTime? ExpiresAt { get; set; }

    // Navigation properties
    public virtual Institution Institution { get; set; } = null!;
    public virtual CourseSemesterOffering? CourseOffering { get; set; }
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<AnnouncementRecipient> AnnouncementRecipients { get; set; } = new List<AnnouncementRecipient>();
}
