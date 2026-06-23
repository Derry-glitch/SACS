using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Commands.DeleteAssignment;

public record DeleteAssignmentCommand(long Id) : IRequest;

public class DeleteAssignmentCommandHandler : IRequestHandler<DeleteAssignmentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAssignmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteAssignmentCommand request, CancellationToken cancellationToken)
    {
        var existingEvent = await _unitOfWork.Repository<AcademicEvent>().GetByIdAsync(request.Id, cancellationToken);
        if (existingEvent == null || existingEvent.EventType != "Assignment")
        {
            throw new KeyNotFoundException("Assignment not found.");
        }

        _unitOfWork.Repository<AcademicEvent>().Remove(existingEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
