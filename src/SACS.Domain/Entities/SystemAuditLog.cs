using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class SystemAuditLog : BaseEntity
{
    public long? UserId { get; set; }
    public string ActionName { get; set; } = null!;
    public string TableName { get; set; } = null!;
    public long RecordId { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
