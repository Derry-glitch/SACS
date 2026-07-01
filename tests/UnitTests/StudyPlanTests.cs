using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SACS.Application.AI.Commands.GenerateStudyPlan;
using SACS.Application.Common.Events;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Persistence.Contexts;
using SACS.Persistence.Repositories;
using Xunit;

namespace UnitTests;

public class StudyPlanTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

    public StudyPlanTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    private class TestCurrentUserService : ICurrentUserService
    {
        public string? UserId { get; set; } = "1";
        public string? Email => "student@sacs.edu";
    }

    private class FakeEventBus : IEventBus
    {
        public List<object> PublishedEvents { get; } = new();

        public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        {
            PublishedEvents.Add(message);
            return Task.CompletedTask;
        }
    }

    private class FakeAiServiceClient : IAiServiceClient
    {
        public Task<StudyPlanResponseDto> GenerateStudyPlanAsync(StudyPlanRequestDto request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new StudyPlanResponseDto
            {
                PlanName = "My Study Plan",
                Entries = new()
                {
                    new()
                    {
                        DayOfWeek = "Monday",
                        Date = "2026-06-29",
                        StartTime = "18:00:00",
                        EndTime = "20:00:00",
                        CourseCode = "CSC301",
                        Topic = "Study Supervised Learning",
                        Priority = "High"
                    }
                }
            });
        }

        public Task<DeadlineExtractionResponseDto> ExtractDeadlinesAsync(string text, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SummaryResponseDto> SummarizeLectureNotesAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SummaryResponseDto> SummarizeTextAsync(string text, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<QuizGenerationResponseDto> GenerateQuizAsync(string content, string difficulty, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private async Task<(ApplicationDbContext context, UnitOfWork uow, TestCurrentUserService currentUserService)> CreateTestContextAsync()
    {
        var currentUserService = new TestCurrentUserService();
        var context = new ApplicationDbContext(_dbContextOptions, currentUserService);
        await DatabaseSeeder.SeedAsync(context);
        var uow = new UnitOfWork(context);

        // Seed basic user & profile
        var user = new User
        {
            Id = 1,
            Email = "student@sacs.edu",
            NormalizedEmail = "STUDENT@SACS.EDU",
            PasswordHash = "hashed",
            FirstName = "John",
            LastName = "Doe"
        };
        await context.Users.AddAsync(user);

        var profile = new StudentProfile
        {
            Id = 1, // mapped to UserId
            MatriculationNumber = "MAT-1122",
            AcademicLevel = 300
        };
        await context.StudentProfiles.AddAsync(profile);
        await context.SaveChangesAsync();

        // Seed Course offering
        var institution = await context.Institutions.FirstAsync();
        var faculty = new Faculty { Name = "Science", Code = "SCI", InstitutionId = institution.Id };
        await context.Faculties.AddAsync(faculty);
        await context.SaveChangesAsync();

        var department = new Department { Name = "Computer Science", Code = "CSC", FacultyId = faculty.Id };
        await context.Departments.AddAsync(department);
        await context.SaveChangesAsync();

        var course = new Course { Code = "CSC301", Title = "Machine Learning", DepartmentId = department.Id, CreditUnits = 3 };
        await context.Courses.AddAsync(course);
        await context.SaveChangesAsync();

        var session = new AcademicSession
        {
            Name = "2026/2027",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(11)),
            InstitutionId = institution.Id,
            IsCurrent = true
        };
        await context.AcademicSessions.AddAsync(session);
        await context.SaveChangesAsync();

        var semester = new Semester
        {
            Name = "Semester 1",
            StartDate = session.StartDate,
            EndDate = session.EndDate,
            AcademicSessionId = session.Id,
            IsCurrent = true
        };
        await context.Semesters.AddAsync(semester);
        await context.SaveChangesAsync();

        var offering = new CourseSemesterOffering { Id = 10, CourseId = course.Id, SemesterId = semester.Id };
        await context.CourseSemesterOfferings.AddAsync(offering);
        await context.SaveChangesAsync();

        // Enroll Student
        var enrollment = new CourseEnrollment
        {
            StudentId = 1,
            CourseOfferingId = offering.Id,
            Status = "Active"
        };
        await context.CourseEnrollments.AddAsync(enrollment);
        await context.SaveChangesAsync();

        return (context, uow, currentUserService);
    }

    [Fact]
    public async Task GenerateStudyPlanCommand_ShouldEnqueueBackgroundJob()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var fakeEventBus = new FakeEventBus();
        var handler = new GenerateStudyPlanCommandHandler(currentUserService, fakeEventBus);
        var command = new GenerateStudyPlanCommand("Weekly Study Plan", new() { { "Monday", 2.0 } });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("EventPublished", result);
        Assert.Single(fakeEventBus.PublishedEvents);
        Assert.IsType<StudyPlanGenerationEvent>(fakeEventBus.PublishedEvents[0]);
    }

    [Fact]
    public async Task ProcessStudyPlanGenerationCommand_ShouldGeneratePlanAndPersistPlanAndEntries()
    {
        // Arrange
        var (context, uow, _) = await CreateTestContextAsync();
        var fakeAiClient = new FakeAiServiceClient();
        var handler = new ProcessStudyPlanGenerationCommandHandler(uow, fakeAiClient);
        var command = new ProcessStudyPlanGenerationCommand("Weekly Study Plan", new() { { "Monday", 2.0 } }, 1);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var savedPlans = await context.StudyPlans.Include(p => p.Entries).Where(p => p.UserId == 1).ToListAsync();
        Assert.Single(savedPlans);

        var plan = savedPlans[0];
        Assert.Equal("Weekly Study Plan", plan.Name);
        Assert.True(plan.IsActive);
        Assert.Single(plan.Entries);

        var entry = plan.Entries.First();
        Assert.Equal(10, entry.CourseOfferingId); // mapped to CSC301 offering
        Assert.Equal(DateOnly.Parse("2026-06-29"), entry.Date);
        Assert.Equal(TimeOnly.Parse("18:00:00"), entry.StartTime);
        Assert.Equal(TimeOnly.Parse("20:00:00"), entry.EndTime);
        Assert.Equal("Study Supervised Learning", entry.TopicToStudy);
        Assert.Equal("High", entry.Priority);
    }
}
