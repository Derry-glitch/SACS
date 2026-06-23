using System;
using System.Collections.Generic;

namespace SACS.Application.Events.DTOs;

public class ProjectDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public long CourseOfferingId { get; set; }
    public string CourseName { get; set; } = null!;
    public string SupervisorName { get; set; } = null!;
    public DateTime SubmissionDate { get; set; }
    public int ProgressPercentage { get; set; }
    public string? Notes { get; set; }
    public List<AttachmentDto> Attachments { get; set; } = new();
}
