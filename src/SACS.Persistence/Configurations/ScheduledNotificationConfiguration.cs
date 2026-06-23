using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class ScheduledNotificationConfiguration : IEntityTypeConfiguration<ScheduledNotification>
{
    public void Configure(EntityTypeBuilder<ScheduledNotification> builder)
    {
                builder.ToTable("ScheduledNotifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ReminderType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Pending");
        builder.Property(x => x.FcmMessageId).HasMaxLength(200);
        builder.Property(x => x.ErrorMessage).HasMaxLength(500);
        builder.Property(x => x.RetryCount).HasDefaultValue(0);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.ScheduledNotifications)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AcademicEvent)
            .WithMany(x => x.ScheduledNotifications)
            .HasForeignKey(x => x.AcademicEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
