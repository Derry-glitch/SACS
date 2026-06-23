using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class Faculty : BaseEntity
{
    public long InstitutionId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;

    // Navigation properties
    public virtual Institution Institution { get; set; } = null!;
    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
}
