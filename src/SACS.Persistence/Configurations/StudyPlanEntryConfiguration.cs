using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class StudyPlanEntryConfiguration : IEntityTypeConfiguration<StudyPlanEntry>
{
    public void Configure(EntityTypeBuilder<StudyPlanEntry> builder)
    {
                builder.ToTable("StudyPlanEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.TopicToStudy).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Priority).HasMaxLength(15).IsRequired().HasDefaultValue("Medium");
        builder.Property(x => x.IsCompleted).HasDefaultValue(false);

        builder.HasOne(x => x.StudyPlan)
            .WithMany(x => x.Entries)
            .HasForeignKey(x => x.StudyPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CourseOffering)
            .WithMany(x => x.StudyPlanEntries)
            .HasForeignKey(x => x.CourseOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.CreatedAtUtc);
        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
