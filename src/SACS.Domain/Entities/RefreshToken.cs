using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public long UserId { get; set; }
    public string Token { get; set; } = null!;
    public string JwtId { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }

    // Navigation property
    public virtual User User { get; set; } = null!;
}
