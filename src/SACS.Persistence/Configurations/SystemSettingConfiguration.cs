using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
                builder.ToTable("SystemSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd(); // maps to integer IDENTITY PK
        builder.Property(x => x.SettingKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SettingValue).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("UpdatedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(x => x.UpdatedByUser)
            .WithMany(x => x.UpdatedSystemSettings)
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SettingKey).IsUnique();

        builder.Ignore(x => x.CreatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
