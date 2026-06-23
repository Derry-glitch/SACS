using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class ExtractedDeadlineConfiguration : IEntityTypeConfiguration<ExtractedDeadline>
{
    public void Configure(EntityTypeBuilder<ExtractedDeadline> builder)
    {
                builder.ToTable("ExtractedDeadlines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.CourseCodeGuess).HasMaxLength(50);
        builder.Property(x => x.ConfidenceScore).HasPrecision(3, 2).IsRequired();
        builder.Property(x => x.IsConfirmed).HasDefaultValue(false);
        builder.Property(x => x.IsRejected).HasDefaultValue(false);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("ExtractedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(x => x.IngestedMessage)
            .WithMany(x => x.ExtractedDeadlines)
            .HasForeignKey(x => x.IngestedMessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AcademicEvent)
            .WithMany(x => x.ExtractedDeadlines)
            .HasForeignKey(x => x.AcademicEventId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
