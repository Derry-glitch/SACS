using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SACS.Application.Common.Events;
using SACS.Application.Common.Interfaces;

namespace SACS.Application.AI.Commands.GenerateStudyPlan;

public record GenerateStudyPlanCommand(
    string Name,
    Dictionary<string, double> AvailableFreeHours
) : IRequest<string>;

public class GenerateStudyPlanCommandValidator : AbstractValidator<GenerateStudyPlanCommand>
{
    public GenerateStudyPlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Plan name is required.");
        RuleFor(x => x.AvailableFreeHours).NotEmpty().WithMessage("Available free study hours are required.");
    }
}

public class GenerateStudyPlanCommandHandler : IRequestHandler<GenerateStudyPlanCommand, string>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IEventBus _eventBus;

    public GenerateStudyPlanCommandHandler(
        ICurrentUserService currentUserService,
        IEventBus eventBus)
    {
        _currentUserService = currentUserService;
        _eventBus = eventBus;
    }

    public async Task<string> Handle(GenerateStudyPlanCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userId = long.Parse(userIdStr);

        // Publish event to Azure Service Bus
        var busEvent = new StudyPlanGenerationEvent(
            request.Name,
            request.AvailableFreeHours,
            userId
        );

        await _eventBus.PublishAsync(busEvent, cancellationToken);

        return "EventPublished";
      }
  }
