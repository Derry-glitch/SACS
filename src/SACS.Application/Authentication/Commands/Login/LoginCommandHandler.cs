using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Authentication.DTOs;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToUpperInvariant();
        
        var user = await _unitOfWork.Repository<User>()
            .Query()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.StudentProfile)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user == null || !user.IsActive || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Get Roles
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        // Generate tokens
        var accessToken = _jwtTokenGenerator.GenerateToken(user, roles);
        var refreshTokenString = _jwtTokenGenerator.GenerateRefreshToken();

        // Extract JTI from Access Token
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(accessToken);
        var jwtId = jwtToken.Id;

        // Save Refresh Token
        var refreshToken = new SACS.Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenString,
            JwtId = jwtId,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        await _unitOfWork.Repository<SACS.Domain.Entities.RefreshToken>().AddAsync(refreshToken, cancellationToken);

        // Update User last login
        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Repository<User>().Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
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
