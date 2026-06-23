using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
                builder.ToTable("StudentProfiles");
        builder.HasKey(x => x.Id); // maps to UserId
        builder.Property(x => x.Id).HasColumnName("UserId").ValueGeneratedNever();
        builder.Property(x => x.MatriculationNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.AcademicLevel).IsRequired();
        builder.Property(x => x.CurrentGPA).HasPrecision(3, 2).HasDefaultValue(0.00);
        builder.Property(x => x.CurrentCGPA).HasPrecision(3, 2).HasDefaultValue(0.00);

        builder.HasOne(x => x.User)
            .WithOne(x => x.StudentProfile)
            .HasForeignKey<StudentProfile>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.MatriculationNumber).IsUnique();

        builder.Ignore(x => x.CreatedAtUtc);
        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
