using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class PerformanceSnapshotConfiguration : IEntityTypeConfiguration<PerformanceSnapshot>
{
    public void Configure(EntityTypeBuilder<PerformanceSnapshot> builder)
    {
                builder.ToTable("PerformanceSnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.AverageQuizScore).HasPrecision(5, 2);
        builder.Property(x => x.OnTimeSubmissionRate).HasPrecision(5, 2);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(x => x.Student)
            .WithMany(x => x.PerformanceSnapshots)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Semester)
            .WithMany(x => x.PerformanceSnapshots)
            .HasForeignKey(x => x.SemesterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CourseOffering)
            .WithMany(x => x.PerformanceSnapshots)
            .HasForeignKey(x => x.CourseOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
