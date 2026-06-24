using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SACS.Application.Common.Events;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.AI.Commands.ExtractDeadline;

public record ExtractDeadlineCommand(string RawContent, string SourceChannel) : IRequest<long>;

public class ExtractDeadlineCommandValidator : AbstractValidator<ExtractDeadlineCommand>
{
    public ExtractDeadlineCommandValidator()
    {
        RuleFor(x => x.RawContent).NotEmpty().WithMessage("Raw content cannot be empty.");
        RuleFor(x => x.SourceChannel).NotEmpty().WithMessage("Source channel cannot be empty.");
    }
}

public class ExtractDeadlineCommandHandler : IRequestHandler<ExtractDeadlineCommand, long>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEventBus _eventBus;

    public ExtractDeadlineCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEventBus eventBus)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _eventBus = eventBus;
    }

    public async Task<long> Handle(ExtractDeadlineCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userId = long.Parse(userIdStr);

        var ingestedMessage = new IngestedMessage
        {
            UserId = userId,
            RawContent = request.RawContent,
            SourceChannel = request.SourceChannel,
            ProcessingStatus = "Pending",
            IngestedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<IngestedMessage>().AddAsync(ingestedMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event to Azure Service Bus
        await _eventBus.PublishAsync(new DeadlineExtractionEvent(ingestedMessage.Id), cancellationToken);

        return ingestedMessage.Id;
    }
}
