using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class EventAttachment : BaseEntity
{
    public long AcademicEventId { get; set; }
    public string FileName { get; set; } = null!;
    public string BlobStorageUrl { get; set; } = null!;
    public long FileSizeInBytes { get; set; }
    public string ContentType { get; set; } = null!;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual AcademicEvent AcademicEvent { get; set; } = null!;
}
