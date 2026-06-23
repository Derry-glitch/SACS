using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SACS.Application.Authentication.Commands.Login;
using SACS.Application.Authentication.Commands.Logout;
using SACS.Application.Authentication.Commands.RefreshToken;
using SACS.Application.Authentication.Commands.RegisterStudent;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Common;
using SACS.Domain.Entities;
using SACS.Infrastructure.Identity;
using SACS.Persistence.Contexts;
using SACS.Persistence.Repositories;
using Xunit;

namespace UnitTests;

public class AuthenticationTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthenticationTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _passwordHasher = new PasswordHasher();

        var jwtOptions = Options.Create(new JwtOptions
        {
            Secret = "SuperSecretKeyForSacsDevelopmentNeedsToBeAtLeast32BytesLong!",
            Issuer = "SACS",
            Audience = "SACS-Students",
            ExpiryMinutes = 60
        });
        _jwtTokenGenerator = new JwtTokenGenerator(jwtOptions);
    }

    private class TestCurrentUserService : ICurrentUserService
    {
        public string? UserId => "1";
        public string? Email => "test@sacs.edu";
    }

    private async Task<(ApplicationDbContext, UnitOfWork)> CreateContextAndUnitOfWorkAsync()
    {
        var context = new ApplicationDbContext(_dbContextOptions, new TestCurrentUserService());
        await DatabaseSeeder.SeedAsync(context);
        return (context, new UnitOfWork(context));
    }

    [Fact]
    public async Task Register_ShouldCreateUserAndStudentProfile_WhenValid()
    {
        // Arrange
        var (context, uow) = await CreateContextAndUnitOfWorkAsync();
        var institution = await context.Institutions.FirstAsync();
        
        var handler = new RegisterStudentCommandHandler(uow, _passwordHasher, _jwtTokenGenerator);
        var command = new RegisterStudentCommand(
            Email: "student@sacs.edu",
            Password: "password123",
            FirstName: "John",
            LastName: "Doe",
            MatriculationNumber: "MAT-12345",
            AcademicLevel: 100,
            InstitutionId: institution.Id
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.Equal("student@sacs.edu", result.User.Email);
        Assert.Equal(Roles.Student, result.User.Role);
        Assert.Equal("MAT-12345", result.User.MatriculationNumber);

        var savedUser = await context.Users.Include(u => u.StudentProfile).FirstOrDefaultAsync(u => u.Email == "student@sacs.edu");
        Assert.NotNull(savedUser);
        Assert.NotNull(savedUser.StudentProfile);
        Assert.Equal("MAT-12345", savedUser.StudentProfile.MatriculationNumber);
    }

    [Fact]
    public async Task Register_ShouldThrowException_WhenEmailDuplicate()
    {
        // Arrange
        var (context, uow) = await CreateContextAndUnitOfWorkAsync();
        var institution = await context.Institutions.FirstAsync();
        
        var handler = new RegisterStudentCommandHandler(uow, _passwordHasher, _jwtTokenGenerator);
        var command = new RegisterStudentCommand(
            Email: "student@sacs.edu",
            Password: "password123",
            FirstName: "John",
            LastName: "Doe",
            MatriculationNumber: "MAT-12345",
            AcademicLevel: 100,
            InstitutionId: institution.Id
        );

        // Act & Assert first registration
        await handler.Handle(command, CancellationToken.None);

        // Second registration with same email
        var duplicateCommand = command with { MatriculationNumber = "MAT-54321" };
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(duplicateCommand, CancellationToken.None));
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsValid()
    {
        // Arrange
        var (context, uow) = await CreateContextAndUnitOfWorkAsync();
        var institution = await context.Institutions.FirstAsync();
        
        // Seed a user
        var user = new User
        {
            Email = "user@sacs.edu",
            NormalizedEmail = "USER@SACS.EDU",
            PasswordHash = _passwordHasher.HashPassword("securepass"),
            FirstName = "Test",
            LastName = "User",
            InstitutionId = institution.Id
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var handler = new LoginCommandHandler(uow, _passwordHasher, _jwtTokenGenerator);
        var command = new LoginCommand("user@sacs.edu", "securepass");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact]
    public async Task Login_ShouldThrowException_WhenPasswordInvalid()
    {
        // Arrange
        var (context, uow) = await CreateContextAndUnitOfWorkAsync();
        var institution = await context.Institutions.FirstAsync();
        
        // Seed a user
        var user = new User
        {
            Email = "user@sacs.edu",
            NormalizedEmail = "USER@SACS.EDU",
            PasswordHash = _passwordHasher.HashPassword("securepass"),
            FirstName = "Test",
            LastName = "User",
            InstitutionId = institution.Id
        };
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var handler = new LoginCommandHandler(uow, _passwordHasher, _jwtTokenGenerator);
        var command = new LoginCommand("user@sacs.edu", "wrongpass");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshToken_ShouldRotateTokens_WhenValid()
    {
        // Arrange
        var (context, uow) = await CreateContextAndUnitOfWorkAsync();
        var institution = await context.Institutions.FirstAsync();

        // Register a student first to generate valid initial tokens
        var registerHandler = new RegisterStudentCommandHandler(uow, _passwordHasher, _jwtTokenGenerator);
        var regCommand = new RegisterStudentCommand(
            Email: "student@sacs.edu",
            Password: "password123",
            FirstName: "John",
            LastName: "Doe",
            MatriculationNumber: "MAT-12345",
            AcademicLevel: 100,
            InstitutionId: institution.Id
        );
        var regResult = await registerHandler.Handle(regCommand, CancellationToken.None);

        var refreshHandler = new RefreshTokenCommandHandler(uow, _jwtTokenGenerator);
        var refreshCommand = new RefreshTokenCommand(regResult.AccessToken, regResult.RefreshToken);

        // Act
        var result = await refreshHandler.Handle(refreshCommand, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(regResult.AccessToken, result.AccessToken);
        Assert.NotEqual(regResult.RefreshToken, result.RefreshToken);

        // Check that the old refresh token is marked as revoked in database
        var revokedToken = await context.RefreshTokens.FirstAsync(rt => rt.Token == regResult.RefreshToken);
        Assert.NotNull(revokedToken.RevokedAt);
        Assert.Equal("Replaced by new token", revokedToken.ReasonRevoked);
        Assert.Equal(result.RefreshToken, revokedToken.ReplacedByToken);
    }

    [Fact]
    public async Task RefreshToken_ShouldRevokeAllSessions_WhenReplayAttackDetected()
    {
        // Arrange
        var (context, uow) = await CreateContextAndUnitOfWorkAsync();
        var institution = await context.Institutions.FirstAsync();

        // Register student
        var registerHandler = new RegisterStudentCommandHandler(uow, _passwordHasher, _jwtTokenGenerator);
        var regCommand = new RegisterStudentCommand(
            Email: "student@sacs.edu",
            Password: "password123",
            FirstName: "John",
            LastName: "Doe",
            MatriculationNumber: "MAT-12345",
            AcademicLevel: 100,
            InstitutionId: institution.Id
        );
        var regResult = await registerHandler.Handle(regCommand, CancellationToken.None);

        var refreshHandler = new RefreshTokenCommandHandler(uow, _jwtTokenGenerator);
        var refreshCommand = new RefreshTokenCommand(regResult.AccessToken, regResult.RefreshToken);

        // First refresh succeeds and rotates the token
        var firstResult = await refreshHandler.Handle(refreshCommand, CancellationToken.None);

        // Second refresh using the SAME old token (Replay Attack!)
        var replayCommand = new RefreshTokenCommand(regResult.AccessToken, regResult.RefreshToken);

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenException>(() => refreshHandler.Handle(replayCommand, CancellationToken.None));

        // Assert that the active tokens for the user have been revoked
        var activeTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == regResult.User.Id && rt.RevokedAt == null)
            .ToListAsync();

        Assert.Empty(activeTokens);
    }

    [Fact]
    public async Task Logout_ShouldRevokeToken_WhenCalled()
    {
        // Arrange
        var (context, uow) = await CreateContextAndUnitOfWorkAsync();
        var institution = await context.Institutions.FirstAsync();

        var registerHandler = new RegisterStudentCommandHandler(uow, _passwordHasher, _jwtTokenGenerator);
        var regCommand = new RegisterStudentCommand(
            Email: "student@sacs.edu",
            Password: "password123",
            FirstName: "John",
            LastName: "Doe",
            MatriculationNumber: "MAT-12345",
            AcademicLevel: 100,
            InstitutionId: institution.Id
        );
        var regResult = await registerHandler.Handle(regCommand, CancellationToken.None);

        var logoutHandler = new LogoutCommandHandler(uow);
        var logoutCommand = new LogoutCommand(regResult.RefreshToken);

        // Act
        await logoutHandler.Handle(logoutCommand, CancellationToken.None);

        // Assert
        var token = await context.RefreshTokens.FirstAsync(rt => rt.Token == regResult.RefreshToken);
        Assert.NotNull(token.RevokedAt);
        Assert.Equal("Logout", token.ReasonRevoked);
    }
}
