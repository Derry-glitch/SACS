using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SACS.Application.Common.Interfaces;

namespace SACS.Application.AI.Commands.GenerateQuiz;

public record GenerateQuizCommand(
    long CourseOfferingId,
    string Title,
    string LectureNoteContent,
    string DifficultyLevel
) : IRequest<string>;

public class GenerateQuizCommandValidator : AbstractValidator<GenerateQuizCommand>
{
    public GenerateQuizCommandValidator()
    {
        RuleFor(x => x.CourseOfferingId).GreaterThan(0).WithMessage("Valid course offering ID is required.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Quiz title is required.");
        RuleFor(x => x.LectureNoteContent).NotEmpty().WithMessage("Lecture note content is required.");
        RuleFor(x => x.DifficultyLevel).Must(d => d == "Easy" || d == "Medium" || d == "Hard")
            .WithMessage("Difficulty level must be 'Easy', 'Medium', or 'Hard'.");
    }
}

public class GenerateQuizCommandHandler : IRequestHandler<GenerateQuizCommand, string>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IBackgroundJobService _backgroundJobService;

    public GenerateQuizCommandHandler(
        ICurrentUserService currentUserService,
        IBackgroundJobService backgroundJobService)
    {
        _currentUserService = currentUserService;
        _backgroundJobService = backgroundJobService;
    }

    public Task<string> Handle(GenerateQuizCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userId = long.Parse(userIdStr);

        // Queue the background quiz generation job
        var jobId = _backgroundJobService.Enqueue<ISender>(sender =>
            sender.Send(new ProcessQuizGenerationCommand(
                request.CourseOfferingId,
                request.Title,
                request.LectureNoteContent,
                request.DifficultyLevel,
                userId
            ), CancellationToken.None));

        return Task.FromResult(jobId);
    }
}
