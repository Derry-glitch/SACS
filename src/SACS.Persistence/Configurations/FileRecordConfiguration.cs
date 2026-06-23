using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class FileRecordConfiguration : IEntityTypeConfiguration<FileRecord>
{
    public void Configure(EntityTypeBuilder<FileRecord> builder)
    {
                builder.ToTable("FileRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.FileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.BlobStorageUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.BlobContainer).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MimeType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DownloadCount).HasDefaultValue(0);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("UploadedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.DeletedAtUtc).HasColumnName("DeletedAt");

        builder.HasOne(x => x.Uploader)
            .WithMany(x => x.UploadedFiles)
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CourseOffering)
            .WithMany(x => x.FileRecords)
            .HasForeignKey(x => x.CourseOfferingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
    }
}
