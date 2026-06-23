using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SACS.Application.AI.Commands.ExtractDeadline;
using SACS.Application.AI.Commands.GenerateQuiz;
using SACS.Application.AI.Commands.GenerateStudyPlan;
using SACS.Application.AI.Commands.SummarizeLectureNotes;
using SACS.Application.Common.Events;

namespace SACS.BackgroundJobs.Services;

public class AzureServiceBusConsumer : BackgroundService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AzureServiceBusConsumer> _logger;

    private ServiceBusProcessor? _deadlineProcessor;
    private ServiceBusProcessor? _summaryProcessor;
    private ServiceBusProcessor? _quizProcessor;
    private ServiceBusProcessor? _studyPlanProcessor;

    public AzureServiceBusConsumer(
        ServiceBusClient serviceBusClient,
        IServiceProvider serviceProvider,
        ILogger<AzureServiceBusConsumer> logger)
    {
        _serviceBusClient = serviceBusClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Initializing Azure Service Bus Processors...");

            _deadlineProcessor = _serviceBusClient.CreateProcessor("deadlineextractionevent", new ServiceBusProcessorOptions());
            _summaryProcessor = _serviceBusClient.CreateProcessor("lecturenotesummarizationevent", new ServiceBusProcessorOptions());
            _quizProcessor = _serviceBusClient.CreateProcessor("quizgenerationevent", new ServiceBusProcessorOptions());
            _studyPlanProcessor = _serviceBusClient.CreateProcessor("studyplangenerationevent", new ServiceBusProcessorOptions());

            _deadlineProcessor.ProcessMessageAsync += HandleDeadlineExtractionMessage;
            _deadlineProcessor.ProcessErrorAsync += ErrorHandler;

            _summaryProcessor.ProcessMessageAsync += HandleLectureSummaryMessage;
            _summaryProcessor.ProcessErrorAsync += ErrorHandler;

            _quizProcessor.ProcessMessageAsync += HandleQuizGenerationMessage;
            _quizProcessor.ProcessErrorAsync += ErrorHandler;

            _studyPlanProcessor.ProcessMessageAsync += HandleStudyPlanGenerationMessage;
            _studyPlanProcessor.ProcessErrorAsync += ErrorHandler;

            await _deadlineProcessor.StartProcessingAsync(stoppingToken);
            await _summaryProcessor.StartProcessingAsync(stoppingToken);
            await _quizProcessor.StartProcessingAsync(stoppingToken);
            await _studyPlanProcessor.StartProcessingAsync(stoppingToken);

            _logger.LogInformation("Azure Service Bus Processors started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start Azure Service Bus Processors. This is expected if running locally with a placeholder connection string.");
        }

        // Keep the background task alive
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(5000, stoppingToken);
        }

        // Cleanup
        if (_deadlineProcessor != null) await _deadlineProcessor.StopProcessingAsync();
        if (_summaryProcessor != null) await _summaryProcessor.StopProcessingAsync();
        if (_quizProcessor != null) await _quizProcessor.StopProcessingAsync();
        if (_studyPlanProcessor != null) await _studyPlanProcessor.StopProcessingAsync();
    }

    private async Task HandleDeadlineExtractionMessage(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        _logger.LogInformation("Received DeadlineExtractionEvent: {Body}", body);

        var ev = JsonSerializer.Deserialize<DeadlineExtractionEvent>(body);
        if (ev != null)
        {
            using var scope = _serviceProvider.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new ProcessDeadlineExtractionCommand(ev.IngestedMessageId));
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private async Task HandleLectureSummaryMessage(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        _logger.LogInformation("Received LectureNoteSummarizationEvent: {Body}", body);

        var ev = JsonSerializer.Deserialize<LectureNoteSummarizationEvent>(body);
        if (ev != null)
        {
            using var scope = _serviceProvider.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new ProcessLectureSummaryCommand(ev.FileRecordId));
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private async Task HandleQuizGenerationMessage(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        _logger.LogInformation("Received QuizGenerationEvent: {Body}", body);

        var ev = JsonSerializer.Deserialize<QuizGenerationEvent>(body);
        if (ev != null)
        {
            using var scope = _serviceProvider.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new ProcessQuizGenerationCommand(
                ev.CourseOfferingId,
                ev.Title,
                ev.LectureNoteContent,
                ev.DifficultyLevel,
                ev.UserId
            ));
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private async Task HandleStudyPlanGenerationMessage(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        _logger.LogInformation("Received StudyPlanGenerationEvent: {Body}", body);

        var ev = JsonSerializer.Deserialize<StudyPlanGenerationEvent>(body);
        if (ev != null)
        {
            using var scope = _serviceProvider.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new ProcessStudyPlanGenerationCommand(
                ev.Name,
                ev.AvailableFreeHours,
                ev.UserId
            ));
        }

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Error processing Service Bus Message in queue {QueueName}", args.EntityPath);
        return Task.CompletedTask;
    }
}
