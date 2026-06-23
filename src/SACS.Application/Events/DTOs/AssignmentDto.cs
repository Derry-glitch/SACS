using System;
using System.Collections.Generic;

namespace SACS.Application.Events.DTOs;

public class AssignmentDto
{
    public long Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public long CourseOfferingId { get; set; }
    public string CourseName { get; set; } = null!;
    public DateTime DeadlineDate { get; set; }
    public string Priority { get; set; } = null!;
    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class AttachmentDto
{
    public long Id { get; set; }
    public string FileName { get; set; } = null!;
    public string BlobStorageUrl { get; set; } = null!;
    public long FileSizeInBytes { get; set; }
    public string ContentType { get; set; } = null!;
}
