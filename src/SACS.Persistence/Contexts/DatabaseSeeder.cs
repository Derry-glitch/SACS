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

        // 4. Seed basic courses and semester offerings for development/testing
        if (!await context.Courses.AnyAsync())
        {
            var institution = await context.Institutions.FirstAsync();
            var faculty = new Faculty { Name = "Science", Code = "SCI", InstitutionId = institution.Id };
            await context.Faculties.AddAsync(faculty);
            await context.SaveChangesAsync();

            var department = new Department { Name = "Computer Science", Code = "CSC", FacultyId = faculty.Id };
            await context.Departments.AddAsync(department);
            await context.SaveChangesAsync();

            var course1 = new Course { Code = "CSC201", Title = "Java Programming", DepartmentId = department.Id, CreditUnits = 3 };
            var course2 = new Course { Code = "CSC202", Title = "Data Structures", DepartmentId = department.Id, CreditUnits = 3 };
            var course3 = new Course { Code = "CSC301", Title = "Operating Systems", DepartmentId = department.Id, CreditUnits = 3 };
            await context.Courses.AddRangeAsync(course1, course2, course3);
            await context.SaveChangesAsync();

            var session = new AcademicSession
            {
                Name = "2025/2026",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(9)),
                InstitutionId = institution.Id,
                IsCurrent = true
            };
            await context.AcademicSessions.AddAsync(session);
            await context.SaveChangesAsync();

            var semester = new Semester
            {
                Name = "First Semester",
                StartDate = session.StartDate,
                EndDate = session.EndDate,
                AcademicSessionId = session.Id,
                IsCurrent = true
            };
            await context.Semesters.AddAsync(semester);
            await context.SaveChangesAsync();

            var offering1 = new CourseSemesterOffering { CourseId = course1.Id, SemesterId = semester.Id };
            var offering2 = new CourseSemesterOffering { CourseId = course2.Id, SemesterId = semester.Id };
            var offering3 = new CourseSemesterOffering { CourseId = course3.Id, SemesterId = semester.Id };
            await context.CourseSemesterOfferings.AddRangeAsync(offering1, offering2, offering3);
            await context.SaveChangesAsync();
        }
    }
}
