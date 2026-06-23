using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class Course : BaseEntity
{
    public long DepartmentId { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int CreditUnits { get; set; } = 3;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Department Department { get; set; } = null!;
    public virtual ICollection<CourseSemesterOffering> CourseOfferings { get; set; } = new List<CourseSemesterOffering>();
}
