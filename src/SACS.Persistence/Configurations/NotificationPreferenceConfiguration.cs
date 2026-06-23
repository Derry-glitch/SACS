using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
                builder.ToTable("NotificationPreferences");
        builder.HasKey(x => new { x.UserId, x.ReminderType });
        builder.Property(x => x.ReminderType).HasMaxLength(30);
        builder.Property(x => x.IsEnabled).HasDefaultValue(true);
        builder.Property(x => x.DeliveryChannel).HasMaxLength(20).IsRequired().HasDefaultValue("Both");

        builder.HasOne(x => x.User)
            .WithMany(x => x.NotificationPreferences)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.Id);
        builder.Ignore(x => x.CreatedAtUtc);
        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
