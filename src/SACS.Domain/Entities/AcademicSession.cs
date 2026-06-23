using System;
using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class AcademicSession : BaseEntity
{
    public long InstitutionId { get; set; }
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsCurrent { get; set; } = false;

    // Navigation properties
    public virtual Institution Institution { get; set; } = null!;
    public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();
}
