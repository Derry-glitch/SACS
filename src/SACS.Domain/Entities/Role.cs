using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
