using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Common.Interfaces;
using SACS.Application.Common.Models;
using SACS.Application.Events.DTOs;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Queries.GetExams;

public record GetExamsQuery : IRequest<IEnumerable<ExamDto>>;

public class GetExamsQueryHandler : IRequestHandler<GetExamsQuery, IEnumerable<ExamDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetExamsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<ExamDto>> Handle(GetExamsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");

        var enrolledCourseOfferingIds = await _unitOfWork.Repository<CourseEnrollment>()
            .Query()
            .Where(ce => ce.StudentId == currentUserId && ce.Status == "Active")
            .Select(ce => ce.CourseOfferingId)
            .ToListAsync(cancellationToken);

        var exams = await _unitOfWork.Repository<AcademicEvent>()
            .Query()
            .Include(ae => ae.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(ae => ae.EventType == "Exam" && enrolledCourseOfferingIds.Contains(ae.CourseOfferingId))
            .OrderBy(ae => ae.DueDate)
            .ToListAsync(cancellationToken);

        return exams.Select(ae =>
        {
            var meta = new EventMetadata();
            try
            {
                meta = JsonSerializer.Deserialize<EventMetadata>(ae.Description ?? "") ?? new EventMetadata();
            }
            catch { }

            return new ExamDto
            {
                Id = ae.Id,
                Title = ae.Title,
                CourseOfferingId = ae.CourseOfferingId,
                CourseName = ae.CourseOffering.Course.Code,
                ExamDate = ae.DueDate,
                Venue = ae.Venue,
                DurationMinutes = meta.DurationMinutes ?? 0,
                SeatNumber = meta.SeatNumber,
                Notes = meta.Notes
            };
        });
    }
}
