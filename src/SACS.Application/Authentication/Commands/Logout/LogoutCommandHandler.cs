using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Authentication.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var savedRefreshToken = await _unitOfWork.Repository<SACS.Domain.Entities.RefreshToken>()
            .Query()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (savedRefreshToken != null && savedRefreshToken.RevokedAt == null)
        {
            savedRefreshToken.RevokedAt = DateTime.UtcNow;
            savedRefreshToken.ReasonRevoked = "Logout";
            _unitOfWork.Repository<SACS.Domain.Entities.RefreshToken>().Update(savedRefreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
