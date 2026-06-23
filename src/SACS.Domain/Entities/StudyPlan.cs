using System;
using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class StudyPlan : BaseEntity
{
    public long UserId { get; set; }
    public long SemesterId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string? PreferencesJson { get; set; }

    // Navigation properties
    public virtual StudentProfile Student { get; set; } = null!;
    public virtual Semester Semester { get; set; } = null!;
    public virtual ICollection<StudyPlanEntry> Entries { get; set; } = new List<StudyPlanEntry>();
}
