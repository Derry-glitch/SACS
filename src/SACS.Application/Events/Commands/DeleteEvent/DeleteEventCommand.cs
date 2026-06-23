using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Commands.DeleteEvent;

public record DeleteEventCommand(long Id) : IRequest;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEventCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var existingEvent = await _unitOfWork.Repository<AcademicEvent>().GetByIdAsync(request.Id, cancellationToken);
        if (existingEvent == null)
        {
            throw new KeyNotFoundException("Academic event not found.");
        }

        _unitOfWork.Repository<AcademicEvent>().Remove(existingEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
