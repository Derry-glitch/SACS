using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class AIInteractionConfiguration : IEntityTypeConfiguration<AIInteraction>
{
    public void Configure(EntityTypeBuilder<AIInteraction> builder)
    {
                builder.ToTable("AIInteractions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RequestType).HasMaxLength(30).IsRequired();
        builder.Property(x => x.PromptText).IsRequired();
        builder.Property(x => x.ResponseText).IsRequired();
        builder.Property(x => x.TokensUsed).HasDefaultValue(0);
        builder.Property(x => x.ModelUsed).HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(x => x.ChatSession)
            .WithMany(x => x.AIInteractions)
            .HasForeignKey(x => x.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
