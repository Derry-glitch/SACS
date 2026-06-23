using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Common;
using SACS.Domain.Entities;

namespace SACS.Persistence.Contexts;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Institution> Institutions => Set<Institution>();
    public DbSet<User> Users => Set<User>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<LecturerProfile> LecturerProfiles => Set<LecturerProfile>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<AcademicSession> AcademicSessions => Set<AcademicSession>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseSemesterOffering> CourseSemesterOfferings => Set<CourseSemesterOffering>();
    public DbSet<CourseInstructor> CourseInstructors => Set<CourseInstructor>();
    public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
    public DbSet<AttendanceTracking> AttendanceTrackings => Set<AttendanceTracking>();
    public DbSet<AcademicEvent> AcademicEvents => Set<AcademicEvent>();
    public DbSet<EventSubmission> EventSubmissions => Set<EventSubmission>();
    public DbSet<EventAttachment> EventAttachments => Set<EventAttachment>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<AnnouncementRecipient> AnnouncementRecipients => Set<AnnouncementRecipient>();
    public DbSet<IngestedMessage> IngestedMessages => Set<IngestedMessage>();
    public DbSet<ExtractedDeadline> ExtractedDeadlines => Set<ExtractedDeadline>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<ScheduledNotification> ScheduledNotifications => Set<ScheduledNotification>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
    public DbSet<StudyPlanEntry> StudyPlanEntries => Set<StudyPlanEntry>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<AIInteraction> AIInteractions => Set<AIInteraction>();
    public DbSet<AIGeneratedQuiz> AIGeneratedQuizzes => Set<AIGeneratedQuiz>();
    public DbSet<LectureNoteSummary> LectureNoteSummaries => Set<LectureNoteSummary>();
    public DbSet<FileRecord> FileRecords => Set<FileRecord>();
    public DbSet<StudentActivityLog> StudentActivityLogs => Set<StudentActivityLog>();
    public DbSet<PerformanceSnapshot> PerformanceSnapshots => Set<PerformanceSnapshot>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<SystemAuditLog> SystemAuditLogs => Set<SystemAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations from assembly SACS.Persistence
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply soft-delete query filters globally
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var isDeletedProp = entityType.FindProperty("IsDeleted");
                if (isDeletedProp != null)
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var body = System.Linq.Expressions.Expression.Equal(
                        System.Linq.Expressions.Expression.Property(parameter, "IsDeleted"),
                        System.Linq.Expressions.Expression.Constant(false)
                    );
                    var lambda = System.Linq.Expressions.Expression.Lambda(body, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId ?? "System";

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Metadata.FindProperty(nameof(BaseEntity.CreatedAtUtc)) != null)
                        entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                    if (entry.Metadata.FindProperty(nameof(BaseEntity.CreatedBy)) != null)
                        entry.Entity.CreatedBy ??= currentUserId;
                    break;

                case EntityState.Modified:
                    if (entry.Metadata.FindProperty(nameof(BaseEntity.UpdatedAtUtc)) != null)
                        entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    if (entry.Metadata.FindProperty(nameof(BaseEntity.UpdatedBy)) != null)
                        entry.Entity.UpdatedBy = currentUserId;
                    break;

                case EntityState.Deleted:
                    var isDeletedProp = entry.Metadata.FindProperty(nameof(BaseEntity.IsDeleted));
                    if (isDeletedProp != null)
                    {
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        if (entry.Metadata.FindProperty(nameof(BaseEntity.DeletedAtUtc)) != null)
                            entry.Entity.DeletedAtUtc = DateTime.UtcNow;
                        if (entry.Metadata.FindProperty(nameof(BaseEntity.UpdatedBy)) != null)
                            entry.Entity.UpdatedBy = currentUserId;
                        if (entry.Metadata.FindProperty(nameof(BaseEntity.UpdatedAtUtc)) != null)
                            entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    }
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
