using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Students.DTOs;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Students.Queries.VerifyStudent;

public record VerifyStudentQuery(string MatriculationNumber) : IRequest<StudentVerificationDto>;

public class VerifyStudentQueryHandler : IRequestHandler<VerifyStudentQuery, StudentVerificationDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public VerifyStudentQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StudentVerificationDto> Handle(VerifyStudentQuery request, CancellationToken cancellationToken)
    {
        var student = await _unitOfWork.Repository<StudentProfile>()
            .Query()
            .Include(sp => sp.User)
                .ThenInclude(u => u.Institution)
            .Include(sp => sp.CourseEnrollments)
                .ThenInclude(ce => ce.CourseOffering)
                    .ThenInclude(co => co.Course)
                        .ThenInclude(c => c.Department)
            .FirstOrDefaultAsync(sp => sp.MatriculationNumber == request.MatriculationNumber, cancellationToken);

        if (student == null)
        {
            return new StudentVerificationDto
            {
                IsVerified = false,
                MatriculationNumber = request.MatriculationNumber,
                FirstName = "",
                LastName = "",
                Department = "",
                AcademicLevel = 0,
                InstitutionName = "",
                ProfileImageUrl = null
            };
        }

        // Try to find the department name from enrolled courses, or fallback to Computer Science
        var deptName = student.CourseEnrollments
            .Select(ce => ce.CourseOffering?.Course?.Department?.Name)
            .FirstOrDefault(name => name != null) ?? "Computer Science";

        return new StudentVerificationDto
        {
            IsVerified = true,
            MatriculationNumber = student.MatriculationNumber,
            FirstName = student.User.FirstName,
            LastName = student.User.LastName,
            Department = deptName,
            AcademicLevel = student.AcademicLevel,
            InstitutionName = student.User.Institution.Name,
            ProfileImageUrl = student.User.ProfileImageUrl
        };
    }
}
