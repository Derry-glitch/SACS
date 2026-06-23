using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class StudentActivityLogConfiguration : IEntityTypeConfiguration<StudentActivityLog>
{
    public void Configure(EntityTypeBuilder<StudentActivityLog> builder)
    {
                builder.ToTable("StudentActivityLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ActivityType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EntityAffected).HasMaxLength(50);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("Timestamp").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.ActivityLogs)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
