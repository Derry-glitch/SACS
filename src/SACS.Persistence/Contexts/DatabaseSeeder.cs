using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SACS.Domain.Common;
using SACS.Domain.Entities;

namespace SACS.Persistence.Contexts;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 1. Ensure Database is created and migrations are applied
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        // 2. Seed Default Institution
        if (!await context.Institutions.AnyAsync())
        {
            var defaultInstitution = new Institution
            {
                Name = "SACS Academy",
                Code = "SACS",
                Domain = "sacs.edu",
                TimeZone = "Africa/Lagos",
                IsActive = true
            };
            await context.Institutions.AddAsync(defaultInstitution);
            await context.SaveChangesAsync();
        }

        // 3. Seed Roles
        var requiredRoles = new[]
        {
            new Role { Name = Roles.Student, Description = "Default Student Role" },
            new Role { Name = Roles.Lecturer, Description = "Default Lecturer Role" },
            new Role { Name = Roles.Admin, Description = "Default Administrator Role" }
        };

        foreach (var role in requiredRoles)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == role.Name))
            {
                await context.Roles.AddAsync(role);
            }
        }

        await context.SaveChangesAsync();
    }
}
