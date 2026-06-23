using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Persistence.Contexts;
using SACS.Persistence.Repositories;

namespace UnitTests;

public class PersistenceTests
{
    private class TestCurrentUserService : ICurrentUserService
    {
        public string? UserId => "TestUser";
        public string? Email => "test@sacs.com";
    }

    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options, new TestCurrentUserService());
    }

    [Fact]
    public async Task AddEntity_ShouldPopulateAuditFields()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        using var uow = new UnitOfWork(context);

        var institution = new Institution
        {
            Name = "Test University",
            Code = "TESTUNI",
            TimeZone = "Africa/Lagos",
            IsActive = true
        };

        // Act
        await uow.Repository<Institution>().AddAsync(institution);
        await uow.SaveChangesAsync();

        // Assert
        Assert.NotEqual(0, institution.Id);
        Assert.True((DateTime.UtcNow - institution.CreatedAtUtc).TotalSeconds < 5);
    }

    [Fact]
    public async Task SoftDeleteEntity_ShouldExcludeFromQueryResults()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        using var uow = new UnitOfWork(context);

        var institution = new Institution
        {
            Name = "Test University",
            Code = "TESTUNI"
        };
        await uow.Repository<Institution>().AddAsync(institution);
        await uow.SaveChangesAsync();

        var user = new User
        {
            InstitutionId = institution.Id,
            Email = "student@sacs.com",
            NormalizedEmail = "STUDENT@SACS.COM",
            PasswordHash = "hashed",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };
        await uow.Repository<User>().AddAsync(user);
        await uow.SaveChangesAsync();

        // Act - Delete user
        uow.Repository<User>().Remove(user);
        await uow.SaveChangesAsync();

        // Assert
        // The user should not be visible using normal queries because of global query filter
        var activeUsers = await uow.Repository<User>().GetAllAsync();
        Assert.Empty(activeUsers);

        // Verify that in the database, the record is actually soft-deleted (IsDeleted is true)
        var deletedUser = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(deletedUser);
        Assert.True(deletedUser.IsDeleted);
        Assert.NotNull(deletedUser.DeletedAtUtc);
    }
}
