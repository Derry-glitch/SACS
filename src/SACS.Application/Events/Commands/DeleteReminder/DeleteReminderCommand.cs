using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Commands.DeleteReminder;

public record DeleteReminderCommand(long Id) : IRequest;

public class DeleteReminderCommandHandler : IRequestHandler<DeleteReminderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteReminderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteReminderCommand request, CancellationToken cancellationToken)
    {
        var existingReminder = await _unitOfWork.Repository<ScheduledNotification>().GetByIdAsync(request.Id, cancellationToken);
        if (existingReminder == null)
        {
            throw new KeyNotFoundException("Reminder not found.");
        }

        _unitOfWork.Repository<ScheduledNotification>().Remove(existingReminder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
