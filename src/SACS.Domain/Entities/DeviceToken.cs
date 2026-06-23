using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class DeviceToken : BaseEntity
{
    public long UserId { get; set; }
    public string Token { get; set; } = null!;
    public string? DeviceType { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual User User { get; set; } = null!;
}
