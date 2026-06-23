using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
                builder.ToTable("DeviceTokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Token).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DeviceType).HasMaxLength(50);
        builder.Property(x => x.RegisteredAt).HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasOne(x => x.User)
            .WithMany(x => x.DeviceTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.CreatedAtUtc);
        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
