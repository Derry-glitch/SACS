using System;
using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class User : BaseEntity
{
    public long InstitutionId { get; set; }
    public string Email { get; set; } = null!;
    public string NormalizedEmail { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual Institution Institution { get; set; } = null!;
    public virtual StudentProfile? StudentProfile { get; set; }
    public virtual LecturerProfile? LecturerProfile { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<DeviceToken> DeviceTokens { get; set; } = new List<DeviceToken>();
    public virtual ICollection<NotificationPreference> NotificationPreferences { get; set; } = new List<NotificationPreference>();
    public virtual ICollection<ScheduledNotification> ScheduledNotifications { get; set; } = new List<ScheduledNotification>();
    public virtual ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
    public virtual ICollection<FileRecord> UploadedFiles { get; set; } = new List<FileRecord>();
    public virtual ICollection<StudentActivityLog> ActivityLogs { get; set; } = new List<StudentActivityLog>();
    public virtual ICollection<SystemSetting> UpdatedSystemSettings { get; set; } = new List<SystemSetting>();
    public virtual ICollection<AnnouncementRecipient> AnnouncementRecipients { get; set; } = new List<AnnouncementRecipient>();
    public virtual ICollection<Announcement> CreatedAnnouncements { get; set; } = new List<Announcement>();
    public virtual ICollection<IngestedMessage> IngestedMessages { get; set; } = new List<IngestedMessage>();
    public virtual ICollection<AttendanceTracking> RecordedAttendances { get; set; } = new List<AttendanceTracking>();
    public virtual ICollection<AcademicEvent> CreatedEvents { get; set; } = new List<AcademicEvent>();
}
