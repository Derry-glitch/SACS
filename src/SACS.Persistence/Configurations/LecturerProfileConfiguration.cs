using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class LecturerProfileConfiguration : IEntityTypeConfiguration<LecturerProfile>
{
    public void Configure(EntityTypeBuilder<LecturerProfile> builder)
    {
                builder.ToTable("LecturerProfiles");
        builder.HasKey(x => x.Id); // maps to UserId
        builder.Property(x => x.Id).HasColumnName("UserId").ValueGeneratedNever();
        builder.Property(x => x.StaffId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.OfficeLocation).HasMaxLength(100);
        builder.Property(x => x.AcademicTitle).HasMaxLength(50);

        builder.HasOne(x => x.User)
            .WithOne(x => x.LecturerProfile)
            .HasForeignKey<LecturerProfile>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.StaffId).IsUnique();

        builder.Ignore(x => x.CreatedAtUtc);
        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
