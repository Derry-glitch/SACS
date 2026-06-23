using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class LecturerProfile : BaseEntity
{
    // The Id property from BaseEntity will be mapped to the UserId column and acts as the primary key.
    
    public string StaffId { get; set; } = null!;
    public string? OfficeLocation { get; set; }
    public string? AcademicTitle { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<CourseInstructor> CourseInstructors { get; set; } = new List<CourseInstructor>();
}
