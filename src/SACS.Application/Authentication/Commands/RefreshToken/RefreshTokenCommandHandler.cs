using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SACS.Application.Authentication.DTOs;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Get principal from expired token
        var principal = _jwtTokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            throw new SecurityTokenException("Invalid access token.");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) 
                          ?? principal.FindFirst("sub");

        if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
        {
            throw new SecurityTokenException("Invalid access token claims.");
        }

        // 2. Fetch the refresh token from DB
        var savedRefreshToken = await _unitOfWork.Repository<SACS.Domain.Entities.RefreshToken>()
            .Query()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (savedRefreshToken == null)
        {
            throw new SecurityTokenException("Refresh token does not exist.");
        }

        // 3. Detect token replay attack
        if (savedRefreshToken.RevokedAt != null)
        {
            // Replay attack! Invalidate ALL active refresh tokens for the user
            var activeTokens = await _unitOfWork.Repository<SACS.Domain.Entities.RefreshToken>()
                .Query()
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.ReasonRevoked = "Replay attack detected (re-use of revoked refresh token)";
                _unitOfWork.Repository<SACS.Domain.Entities.RefreshToken>().Update(token);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new SecurityTokenException("Compromised session. Replay attack detected. All sessions invalidated.");
        }

        // 4. Validate token expiration
        if (savedRefreshToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new SecurityTokenException("Refresh token has expired.");
        }

        // 5. Verify it belongs to the correct user
        if (savedRefreshToken.UserId != userId)
        {
            throw new SecurityTokenException("Refresh token does not match user session.");
        }

        // 6. Get User with Profile and Roles
        var user = await _unitOfWork.Repository<User>()
            .Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.StudentProfile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new SecurityTokenException("User is inactive or not found.");
        }

        // 7. Revoke the old token
        var newRefreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();
        
        savedRefreshToken.RevokedAt = DateTime.UtcNow;
        savedRefreshToken.ReasonRevoked = "Replaced by new token";
        savedRefreshToken.ReplacedByToken = newRefreshTokenString;
        _unitOfWork.Repository<SACS.Domain.Entities.RefreshToken>().Update(savedRefreshToken);

        // 8. Generate new token pair
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var newAccessToken = _jwtTokenGenerator.GenerateToken(user, roles);

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(newAccessToken);
        var newJwtId = jwtToken.Id;

        // 9. Save new refresh token
        var newRefreshToken = new SACS.Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenString,
            JwtId = newJwtId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _unitOfWork.Repository<SACS.Domain.Entities.RefreshToken>().AddAsync(newRefreshToken, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenString,
            ExpiresAt = jwtToken.ValidTo,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? "Student",
                InstitutionId = user.InstitutionId,
                MatriculationNumber = user.StudentProfile?.MatriculationNumber
            }
        };
    }
}
