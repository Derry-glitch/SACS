using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Authentication.DTOs;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Common;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Authentication.Commands.RegisterStudent;

public class RegisterStudentCommandHandler : IRequestHandler<RegisterStudentCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterStudentCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(RegisterStudentCommand request, CancellationToken cancellationToken)
    {
        // 1. Email uniqueness validation
        var normalizedEmail = request.Email.ToUpperInvariant();
        var existingUser = await _unitOfWork.Repository<User>()
            .Query()
            .AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

        if (existingUser)
        {
            throw new ArgumentException("Email address is already in use.");
        }

        // 2. Validate Matriculation Number uniqueness
        var existingStudent = await _unitOfWork.Repository<StudentProfile>()
            .Query()
            .AnyAsync(s => s.MatriculationNumber == request.MatriculationNumber, cancellationToken);

        if (existingStudent)
        {
            throw new ArgumentException("Matriculation number is already registered.");
        }

        // 3. Verify Institution exists
        var institution = await _unitOfWork.Repository<Institution>()
            .GetByIdAsync(request.InstitutionId, cancellationToken);
        if (institution == null)
        {
            throw new ArgumentException("Institution not found.");
        }

        // 4. Verify Student Role exists
        var studentRole = await _unitOfWork.Repository<Role>()
            .Query()
            .FirstOrDefaultAsync(r => r.Name == Roles.Student, cancellationToken);

        if (studentRole == null)
        {
            throw new InvalidOperationException("Default Student role is not configured in the system.");
        }

        // 5. Database transaction safety
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var user = new User
            {
                InstitutionId = request.InstitutionId,
                Email = request.Email,
                NormalizedEmail = normalizedEmail,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };

            await _unitOfWork.Repository<User>().AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Assign role
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = studentRole.Id
            };
            await _unitOfWork.Repository<UserRole>().AddAsync(userRole, cancellationToken);

            // Create Student Profile
            var studentProfile = new StudentProfile
            {
                Id = user.Id, // maps to UserId
                MatriculationNumber = request.MatriculationNumber,
                AcademicLevel = request.AcademicLevel,
                CurrentGPA = 0.00m,
                CurrentCGPA = 0.00m
            };
            await _unitOfWork.Repository<StudentProfile>().AddAsync(studentProfile, cancellationToken);

            // Generate JWT Token & Refresh Token
            var rolesList = new[] { Roles.Student };
            var accessToken = _jwtTokenGenerator.GenerateToken(user, rolesList);
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

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

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
                    Role = Roles.Student,
                    InstitutionId = user.InstitutionId,
                    MatriculationNumber = studentProfile.MatriculationNumber
                }
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
