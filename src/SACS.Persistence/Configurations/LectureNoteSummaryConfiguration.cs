using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class LectureNoteSummaryConfiguration : IEntityTypeConfiguration<LectureNoteSummary>
{
    public void Configure(EntityTypeBuilder<LectureNoteSummary> builder)
    {
                builder.ToTable("LectureNoteSummaries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.SourceFileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.OriginalFileUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.SummaryText).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("GeneratedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CourseOffering)
            .WithMany(x => x.LectureNoteSummaries)
            .HasForeignKey(x => x.CourseOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
