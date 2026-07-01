using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SACS.Domain.Entities;
using SACS.Persistence.Contexts;

namespace SACS.API.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : ApiControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public record CreateDepartmentRequest(string Name, string Code);

    public record CreateCourseRequest(
        long DepartmentId,
        string Code,
        string Title,
        string? Description,
        int CreditUnits
    );

    // 1. GET /api/Admin/all-students
    [HttpGet("all-students")]
    public async Task<IActionResult> GetAllStudents()
    {
        var students = await _context.StudentProfiles
            .Include(s => s.User)
            .Where(s => !s.IsDeleted && !s.User.IsDeleted)
            .Select(s => new
            {
                Id = s.Id,
                FirstName = s.User.FirstName,
                LastName = s.User.LastName,
                Email = s.User.Email,
                MatriculationNumber = s.MatriculationNumber,
                AcademicLevel = s.AcademicLevel,
                CurrentGPA = s.CurrentGPA,
                CurrentCGPA = s.CurrentCGPA
            })
            .ToListAsync();

        return Ok(students);
    }

    // 2. GET /api/Admin/all-lecturers
    [HttpGet("all-lecturers")]
    public async Task<IActionResult> GetAllLecturers()
    {
        var lecturers = await _context.LecturerProfiles
            .Include(l => l.User)
            .Where(l => !l.IsDeleted && !l.User.IsDeleted)
            .Select(l => new
            {
                Id = l.Id,
                FirstName = l.User.FirstName,
                LastName = l.User.LastName,
                Email = l.User.Email,
                StaffId = l.StaffId,
                OfficeLocation = l.OfficeLocation,
                AcademicTitle = l.AcademicTitle
            })
            .ToListAsync();

        return Ok(lecturers);
    }

    // 3. POST /api/Admin/create-department
    [HttpPost("create-department")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Name and Code are required.");
        }

        // Ensure default Faculty exists
        var faculty = await _context.Faculties.FirstOrDefaultAsync();
        if (faculty == null)
        {
            faculty = new Faculty
            {
                InstitutionId = CurrentInstitutionId > 0 ? CurrentInstitutionId : 1,
                Name = "Science and Technology",
                Code = "FST"
            };
            _context.Faculties.Add(faculty);
            await _context.SaveChangesAsync();
        }

        var department = new Department
        {
            FacultyId = faculty.Id,
            Name = request.Name,
            Code = request.Code
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            FacultyId = department.FacultyId
        });
    }

    // 4. POST /api/Admin/create-course
    [HttpPost("create-course")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest("Course Code and Title are required.");
        }

        var departmentExists = await _context.Departments.AnyAsync(d => d.Id == request.DepartmentId);
        if (!departmentExists)
        {
            return NotFound("Target Department not found.");
        }

        var course = new Course
        {
            DepartmentId = request.DepartmentId,
            Code = request.Code,
            Title = request.Title,
            Description = request.Description,
            CreditUnits = request.CreditUnits > 0 ? request.CreditUnits : 3,
            IsActive = true
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Id = course.Id,
            DepartmentId = course.DepartmentId,
            Code = course.Code,
            Title = course.Title,
            CreditUnits = course.CreditUnits,
            IsActive = course.IsActive
        });
    }

    // 5. DELETE /api/Admin/remove-student/{id}
    [HttpDelete("remove-student/{id}")]
    public async Task<IActionResult> RemoveStudent(long id)
    {
        var student = await _context.StudentProfiles
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
        {
            return NotFound("Student not found.");
        }

        student.IsDeleted = true;
        student.User.IsDeleted = true;
        student.DeletedAtUtc = DateTime.UtcNow;
        student.User.DeletedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Student suspended/removed successfully." });
    }

    // 6. GET /api/Admin/system-stats
    [HttpGet("system-stats")]
    public async Task<IActionResult> GetSystemStats()
    {
        var totalStudents = await _context.StudentProfiles.CountAsync(s => !s.IsDeleted);
        var totalLecturers = await _context.LecturerProfiles.CountAsync(l => !l.IsDeleted);
        var totalDepartments = await _context.Departments.CountAsync(d => !d.IsDeleted);
        var totalCourses = await _context.Courses.CountAsync(c => !c.IsDeleted);
        var totalAnnouncements = await _context.Announcements.CountAsync(a => !a.IsDeleted);
        var activeSessionsCount = AttendanceController.ActiveSessions.Count(s => s.Value.ExpiresAt > DateTime.UtcNow);

        return Ok(new
        {
            TotalStudents = totalStudents,
            TotalLecturers = totalLecturers,
            TotalDepartments = totalDepartments,
            TotalCourses = totalCourses,
            TotalAnnouncements = totalAnnouncements,
            ActiveSessionsCount = activeSessionsCount
        });
    }
}
