using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class AIGeneratedQuizConfiguration : IEntityTypeConfiguration<AIGeneratedQuiz>
{
    public void Configure(EntityTypeBuilder<AIGeneratedQuiz> builder)
    {
                builder.ToTable("AIGeneratedQuizzes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DifficultyLevel).HasMaxLength(20).IsRequired().HasDefaultValue("Medium");
        builder.Property(x => x.QuizStructureJson).IsRequired();
        builder.Property(x => x.ScoreObtained).HasPrecision(5, 2);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("GeneratedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(x => x.Student)
            .WithMany(x => x.GeneratedQuizzes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CourseOffering)
            .WithMany(x => x.GeneratedQuizzes)
            .HasForeignKey(x => x.CourseOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
