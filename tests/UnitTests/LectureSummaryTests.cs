using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SACS.Application.AI.Commands.SummarizeLectureNotes;
using SACS.Application.Common.Events;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Persistence.Contexts;
using SACS.Persistence.Repositories;
using Xunit;

namespace UnitTests;

public class LectureSummaryTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

    public LectureSummaryTests()
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

    private class FakeBlobStorageService : IBlobStorageService
    {
        public Task<string> UploadAsync(Stream stream, string fileName, string containerName, string contentType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"https://fakeblob.core.windows.net/{containerName}/{fileName}");
        }

        public Task<string> GeneratePresignedUrlAsync(string blobName, string containerName, TimeSpan expiry, CancellationToken cancellationToken = default)
        {
            return Task.FromResult($"https://fakeblob.core.windows.net/{containerName}/{blobName}?sas");
        }

        public Task DeleteAsync(string blobName, string containerName, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private class FakeAiServiceClient : IAiServiceClient
    {
        public Task<SummaryResponseDto> SummarizeLectureNotesAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new SummaryResponseDto
            {
                Summary = "This is a summarized output from mock service."
            });
        }

        public Task<DeadlineExtractionResponseDto> ExtractDeadlinesAsync(string text, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<QuizGenerationResponseDto> GenerateQuizAsync(string content, string difficulty, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<StudyPlanResponseDto> GenerateStudyPlanAsync(StudyPlanRequestDto request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private async Task<(ApplicationDbContext context, UnitOfWork uow, TestCurrentUserService currentUserService)> CreateTestContextAsync()
    {
        var currentUserService = new TestCurrentUserService();
        var context = new ApplicationDbContext(_dbContextOptions, currentUserService);
        await DatabaseSeeder.SeedAsync(context);
        var uow = new UnitOfWork(context);

        // Seed basic student user
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

        // Seed basic CourseSemesterOffering
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
    public async Task SummarizeLectureNotesCommand_ShouldUploadFileAndEnqueueSummary()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var fakeBlob = new FakeBlobStorageService();
        var fakeEventBus = new FakeEventBus();
        var handler = new SummarizeLectureNotesCommandHandler(uow, currentUserService, fakeBlob, fakeEventBus);

        using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Fake file content. Machine Learning lecture notes."));
        var command = new SummarizeLectureNotesCommand(
            FileStream: memoryStream,
            FileName: "ml_notes.txt",
            ContentType: "text/plain",
            CourseOfferingId: 10,
            FileSizeInBytes: memoryStream.Length
        );

        // Act
        var fileRecordId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(fileRecordId > 0);
        Assert.Single(fakeEventBus.PublishedEvents);
        Assert.IsType<LectureNoteSummarizationEvent>(fakeEventBus.PublishedEvents[0]);

        var fileRecord = await context.FileRecords.FirstOrDefaultAsync(f => f.Id == fileRecordId);
        Assert.NotNull(fileRecord);
        Assert.Equal("ml_notes.txt", fileRecord.FileName);
        Assert.Equal("lecturenotes", fileRecord.BlobContainer);
        Assert.Equal("LectureNote", fileRecord.Category);
        Assert.StartsWith("https://fakeblob.core.windows.net/lecturenotes/", fileRecord.BlobStorageUrl);
    }

    [Fact]
    public async Task ProcessLectureSummaryCommand_ShouldSummarizeAndPersistSummary()
    {
        // Arrange
        var (context, uow, _) = await CreateTestContextAsync();
        var fileRecord = new FileRecord
        {
            UploadedByUserId = 1,
            CourseOfferingId = 10,
            FileName = "ml_notes.txt",
            BlobStorageUrl = "https://fakeblob.core.windows.net/lecturenotes/ml_notes.txt",
            BlobContainer = "lecturenotes",
            FileSizeInBytes = 100,
            MimeType = "text/plain",
            Category = "LectureNote"
        };
        await context.FileRecords.AddAsync(fileRecord);
        await context.SaveChangesAsync();

        var fakeAiClient = new FakeAiServiceClient();
        var handler = new ProcessLectureSummaryCommandHandler(uow, fakeAiClient);
        var command = new ProcessLectureSummaryCommand(fileRecord.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var savedSummaries = await context.LectureNoteSummaries.Where(s => s.CourseOfferingId == 10).ToListAsync();
        Assert.Single(savedSummaries);

        var summary = savedSummaries[0];
        Assert.Equal(1, summary.UserId);
        Assert.Equal("ml_notes.txt", summary.SourceFileName);
        Assert.Equal(fileRecord.BlobStorageUrl, summary.OriginalFileUrl);
        Assert.Equal("This is a summarized output from mock service.", summary.SummaryText);
    }
}
