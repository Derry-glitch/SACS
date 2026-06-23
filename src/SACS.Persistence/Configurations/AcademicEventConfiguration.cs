using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class AcademicEventConfiguration : IEntityTypeConfiguration<AcademicEvent>
{
    public void Configure(EntityTypeBuilder<AcademicEvent> builder)
    {
                builder.ToTable("AcademicEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Venue).HasMaxLength(200);
        builder.Property(x => x.MaxScore).HasPrecision(5, 2);
        builder.Property(x => x.Weight).HasPrecision(5, 2);
        builder.Property(x => x.Priority).HasMaxLength(15).IsRequired().HasDefaultValue("Medium");
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Active");
        builder.Property(x => x.SourceType).HasMaxLength(20).IsRequired().HasDefaultValue("Manual");
        builder.Property(x => x.IsVisibleToStudents).HasDefaultValue(true);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UpdatedAt");
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.Property(x => x.DeletedAtUtc).HasColumnName("DeletedAt");

        builder.HasOne(x => x.CourseOffering)
            .WithMany(x => x.AcademicEvents)
            .HasForeignKey(x => x.CourseOfferingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Creator)
            .WithMany(x => x.CreatedEvents)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
    }
}
