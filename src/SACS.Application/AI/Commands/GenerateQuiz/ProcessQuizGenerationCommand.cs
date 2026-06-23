using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.AI.Commands.GenerateQuiz;

public record ProcessQuizGenerationCommand(
    long CourseOfferingId,
    string Title,
    string LectureNoteContent,
    string DifficultyLevel,
    long UserId
) : IRequest;

public class ProcessQuizGenerationCommandHandler : IRequestHandler<ProcessQuizGenerationCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiServiceClient _aiServiceClient;

    public ProcessQuizGenerationCommandHandler(IUnitOfWork unitOfWork, IAiServiceClient aiServiceClient)
    {
        _unitOfWork = unitOfWork;
        _aiServiceClient = aiServiceClient;
    }

    public async Task Handle(ProcessQuizGenerationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var responseDto = await _aiServiceClient.GenerateQuizAsync(
                request.LectureNoteContent,
                request.DifficultyLevel,
                cancellationToken);

            var quizStructureJson = JsonSerializer.Serialize(responseDto);

            var aiGeneratedQuiz = new AIGeneratedQuiz
            {
                UserId = request.UserId,
                CourseOfferingId = request.CourseOfferingId,
                Title = request.Title,
                DifficultyLevel = request.DifficultyLevel,
                QuizStructureJson = quizStructureJson,
                GeneratedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AIGeneratedQuiz>().AddAsync(aiGeneratedQuiz, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // Logging or error handling - let the background worker fail/retry
            throw;
        }
    }
}
