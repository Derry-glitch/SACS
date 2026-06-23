using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class EventAttachmentConfiguration : IEntityTypeConfiguration<EventAttachment>
{
    public void Configure(EntityTypeBuilder<EventAttachment> builder)
    {
                builder.ToTable("EventAttachments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.FileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.BlobStorageUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("UploadedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(x => x.AcademicEvent)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.AcademicEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
