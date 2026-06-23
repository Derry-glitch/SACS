using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class CourseInstructorConfiguration : IEntityTypeConfiguration<CourseInstructor>
{
    public void Configure(EntityTypeBuilder<CourseInstructor> builder)
    {
                builder.ToTable("CourseInstructors");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Role).HasMaxLength(30).IsRequired().HasDefaultValue("Primary");
        builder.Property(x => x.AssignedAt).HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(x => x.CourseOffering)
            .WithMany(x => x.Instructors)
            .HasForeignKey(x => x.CourseOfferingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Lecturer)
            .WithMany(x => x.CourseInstructors)
            .HasForeignKey(x => x.LecturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CourseOfferingId, x.LecturerId }).IsUnique();

        builder.Ignore(x => x.CreatedAtUtc);
        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
