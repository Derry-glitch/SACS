using System;
using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class FileRecord : BaseEntity
{
    public long UploadedByUserId { get; set; }
    public long? CourseOfferingId { get; set; }
    public string FileName { get; set; } = null!;
    public string BlobStorageUrl { get; set; } = null!;
    public string BlobContainer { get; set; } = null!; // e.g. lecturenotes, submissions
    public long FileSizeInBytes { get; set; }
    public string MimeType { get; set; } = null!;
    public string Category { get; set; } = null!; // LectureNote, Assignment, Submission, Resource
    public string? Description { get; set; }
    public int DownloadCount { get; set; } = 0;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User Uploader { get; set; } = null!;
    public virtual CourseSemesterOffering? CourseOffering { get; set; }
}
