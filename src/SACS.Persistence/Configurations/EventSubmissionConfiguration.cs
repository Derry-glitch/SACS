using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class EventSubmissionConfiguration : IEntityTypeConfiguration<EventSubmission>
{
    public void Configure(EntityTypeBuilder<EventSubmission> builder)
    {
                builder.ToTable("EventSubmissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Submitted");
        builder.Property(x => x.ScoreObtained).HasPrecision(5, 2);
        builder.Property(x => x.IsLate).HasDefaultValue(false);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("SubmittedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
        builder.Property(x => x.GradedAt);

        builder.HasOne(x => x.AcademicEvent)
            .WithMany(x => x.Submissions)
            .HasForeignKey(x => x.AcademicEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Student)
            .WithMany(x => x.Submissions)
            .HasForeignKey(x => x.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Grader)
            .WithMany()
            .HasForeignKey(x => x.GradedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
