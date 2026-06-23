using System.Collections.Generic;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class Institution : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Domain { get; set; }
    public string? LogoUrl { get; set; }
    public string TimeZone { get; set; } = "Africa/Lagos";
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
    public virtual ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
    public virtual ICollection<AcademicSession> AcademicSessions { get; set; } = new List<AcademicSession>();
    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
}
