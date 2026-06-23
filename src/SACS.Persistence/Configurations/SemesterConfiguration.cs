using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SACS.Domain.Entities;

namespace SACS.Persistence.Configurations;

public class SemesterConfiguration : IEntityTypeConfiguration<Semester>
{
    public void Configure(EntityTypeBuilder<Semester> builder)
    {
                builder.ToTable("Semesters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsCurrent).HasDefaultValue(false);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("CreatedAt").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

        builder.HasOne(x => x.AcademicSession)
            .WithMany(x => x.Semesters)
            .HasForeignKey(x => x.AcademicSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.UpdatedAtUtc);
        builder.Ignore(x => x.CreatedBy);
        builder.Ignore(x => x.UpdatedBy);
        builder.Ignore(x => x.IsDeleted);
        builder.Ignore(x => x.DeletedAtUtc);
    }
}
