using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class IngestedMessageConfiguration : IEntityTypeConfiguration<IngestedMessage>
{
    public void Configure(EntityTypeBuilder<IngestedMessage> builder)
    {
                builder.ToTable("IngestedMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RawContent).IsRequired();
        builder.Property(x => x.SourceChannel).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProcessingStatus).HasMaxLength(30).IsRequired().HasDefaultValue("Pending");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("IngestedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.ProcessedAt);
        builder.Property(x => x.ErrorMessage).HasMaxLength(500);

        builder.HasOne(x => x.User)
            .WithMany(x => x.IngestedMessages)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
