using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SACS.Application.AI.Commands.ExtractDeadline;
using SACS.Application.Common.Events;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Persistence.Contexts;
using SACS.Persistence.Repositories;
using Xunit;

namespace UnitTests;

public class DeadlineExtractionTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

    public DeadlineExtractionTests()
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
        public Task<DeadlineExtractionResponseDto> ExtractDeadlinesAsync(string text, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DeadlineExtractionResponseDto
            {
                Deadlines = new List<ExtractedDeadlineItemDto>
                {
                    new()
                    {
                        Title = "Machine Learning Assignment",
                        CourseCodeGuess = "ML101",
                        ParsedDueDate = new DateTime(2026, 7, 14, 23, 59, 0, DateTimeKind.Utc),
                        ConfidenceScore = 0.95m
                    }
                }
            });
        }

        public Task<SummaryResponseDto> SummarizeLectureNotesAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<SummaryResponseDto> SummarizeTextAsync(string text, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<QuizGenerationResponseDto> GenerateQuizAsync(string content, string difficulty, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<StudyPlanResponseDto> GenerateStudyPlanAsync(StudyPlanRequestDto request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private async Task<(ApplicationDbContext context, UnitOfWork uow, TestCurrentUserService currentUserService)> CreateTestContextAsync()
    {
        var currentUserService = new TestCurrentUserService();
        var context = new ApplicationDbContext(_dbContextOptions, currentUserService);
        await DatabaseSeeder.SeedAsync(context);
        var uow = new UnitOfWork(context);

        // Seed a student user
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
        await context.SaveChangesAsync();

        return (context, uow, currentUserService);
    }

    [Fact]
    public async Task ExtractDeadlineCommand_ShouldCreateIngestedMessageAndEnqueueJob()
    {
        // Arrange
        var (context, uow, currentUserService) = await CreateTestContextAsync();
        var fakeEventBus = new FakeEventBus();
        var handler = new ExtractDeadlineCommandHandler(uow, currentUserService, fakeEventBus);
        var command = new ExtractDeadlineCommand(
            RawContent: "Machine Learning assignment should be submitted on 14 July before 11:59 PM.",
            SourceChannel: "Telegram"
        );

        // Act
        var ingestedMessageId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(ingestedMessageId > 0);
        Assert.Single(fakeEventBus.PublishedEvents);
        Assert.IsType<DeadlineExtractionEvent>(fakeEventBus.PublishedEvents[0]);

        var savedMessage = await context.IngestedMessages.FirstOrDefaultAsync(m => m.Id == ingestedMessageId);
        Assert.NotNull(savedMessage);
        Assert.Equal("Pending", savedMessage.ProcessingStatus);
        Assert.Equal(command.RawContent, savedMessage.RawContent);
        Assert.Equal("Telegram", savedMessage.SourceChannel);
    }

    [Fact]
    public async Task ProcessDeadlineExtractionCommand_ShouldCallAiServiceAndPersistDeadline()
    {
        // Arrange
        var (context, uow, _) = await CreateTestContextAsync();
        var message = new IngestedMessage
        {
            UserId = 1,
            RawContent = "Machine Learning assignment should be submitted on 14 July before 11:59 PM.",
            SourceChannel = "Telegram",
            ProcessingStatus = "Pending"
        };
        await context.IngestedMessages.AddAsync(message);
        await context.SaveChangesAsync();

        var fakeAiClient = new FakeAiServiceClient();
        var handler = new ProcessDeadlineExtractionCommandHandler(uow, fakeAiClient);
        var command = new ProcessDeadlineExtractionCommand(message.Id);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedMessage = await context.IngestedMessages.FirstOrDefaultAsync(m => m.Id == message.Id);
        Assert.NotNull(updatedMessage);
        Assert.Equal("Completed", updatedMessage.ProcessingStatus);
        Assert.NotNull(updatedMessage.ProcessedAt);

        var extractedDeadlines = await context.ExtractedDeadlines.Where(d => d.IngestedMessageId == message.Id).ToListAsync();
        Assert.Single(extractedDeadlines);

        var deadline = extractedDeadlines[0];
        Assert.Equal("Machine Learning Assignment", deadline.Title);
        Assert.Equal("ML101", deadline.CourseCodeGuess);
        Assert.Equal(new DateTime(2026, 7, 14, 23, 59, 0, DateTimeKind.Utc), deadline.ParsedDueDate);
        Assert.Equal(0.95m, deadline.ConfidenceScore);
    }
}
