using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class SystemSetting : BaseEntity
{
    public string SettingKey { get; set; } = null!;
    public string SettingValue { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public long? UpdatedByUserId { get; set; }

    // Navigation property
    public virtual User? UpdatedByUser { get; set; }
}
