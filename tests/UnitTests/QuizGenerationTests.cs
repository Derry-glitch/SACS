using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SACS.Application.AI.Commands.GenerateQuiz;
using SACS.Application.Common.Events;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Persistence.Contexts;
using SACS.Persistence.Repositories;
using Xunit;

namespace UnitTests;

public class QuizGenerationTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

    public QuizGenerationTests()
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
        public Task<QuizGenerationResponseDto> GenerateQuizAsync(string content, string difficulty, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new QuizGenerationResponseDto
            {
                QuizTitle = "Sample ML Quiz",
                DifficultyLevel = difficulty,
                Questions = new()
                {
                    new()
                    {
                        QuestionText = "What is supervised learning?",
                        Options = new() { "A", "B", "C", "D" },
                        CorrectAnswer = "A",
                        Explanation = "Supervised learning relies on labeled data."
                    }
                }
            });
        }

        public Task<DeadlineExtractionResponseDto> ExtractDeadlinesAsync(string text, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SummaryResponseDto> SummarizeLectureNotesAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SummaryResponseDto> SummarizeTextAsync(string text, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<StudyPlanResponseDto> GenerateStudyPlanAsync(StudyPlanRequestDto request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
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

        return (context, uow, currentUserService);
    }

    [Fact]
    public async Task GenerateQuizCommand_ShouldEnqueueBackgroundJob()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var fakeEventBus = new FakeEventBus();
        var handler = new GenerateQuizCommandHandler(currentUserService, fakeEventBus);
        var command = new GenerateQuizCommand(10, "ML Practice Quiz", "This is some lecture note content about Machine Learning.", "Medium");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("EventPublished", result);
        Assert.Single(fakeEventBus.PublishedEvents);
        Assert.IsType<QuizGenerationEvent>(fakeEventBus.PublishedEvents[0]);
    }

    [Fact]
    public async Task ProcessQuizGenerationCommand_ShouldGenerateQuizAndPersist()
    {
        // Arrange
        var (context, uow, _) = await CreateTestContextAsync();
        var fakeAiClient = new FakeAiServiceClient();
        var handler = new ProcessQuizGenerationCommandHandler(uow, fakeAiClient);
        var command = new ProcessQuizGenerationCommand(10, "ML Quiz Title", "Some content about supervised learning.", "Medium", 1);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var savedQuizzes = await context.AIGeneratedQuizzes.Where(q => q.CourseOfferingId == 10).ToListAsync();
        Assert.Single(savedQuizzes);

        var quiz = savedQuizzes[0];
        Assert.Equal(1, quiz.UserId);
        Assert.Equal("ML Quiz Title", quiz.Title);
        Assert.Equal("Medium", quiz.DifficultyLevel);
        Assert.NotEmpty(quiz.QuizStructureJson);

        var structure = JsonSerializer.Deserialize<QuizGenerationResponseDto>(quiz.QuizStructureJson);
        Assert.NotNull(structure);
        Assert.Equal("Sample ML Quiz", structure.QuizTitle);
        Assert.Single(structure.Questions);
        Assert.Equal("What is supervised learning?", structure.Questions[0].QuestionText);
        Assert.Equal("A", structure.Questions[0].CorrectAnswer);
    }
}
