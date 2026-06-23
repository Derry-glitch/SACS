using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class CourseSemesterOfferingConfiguration : IEntityTypeConfiguration<CourseSemesterOffering>
{
    public void Configure(EntityTypeBuilder<CourseSemesterOffering> builder)
    {
                builder.ToTable("CourseSemesterOfferings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasOne(x => x.Course)
            .WithMany(x => x.CourseOfferings)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Semester)
            .WithMany(x => x.CourseOfferings)
            .HasForeignKey(x => x.SemesterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CourseId, x.SemesterId }).IsUnique();

        builder.Ignore(x => x.CreatedAtUtc);
        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
