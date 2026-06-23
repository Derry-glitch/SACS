using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class Department : BaseEntity
{
    public long FacultyId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;

    // Navigation properties
    public virtual Faculty Faculty { get; set; } = null!;
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
