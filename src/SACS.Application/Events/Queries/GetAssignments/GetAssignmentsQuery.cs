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

namespace SACS.Application.Events.Queries.GetAssignments;

public record GetAssignmentsQuery : IRequest<IEnumerable<AssignmentDto>>;

public class GetAssignmentsQueryHandler : IRequestHandler<GetAssignmentsQuery, IEnumerable<AssignmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAssignmentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<AssignmentDto>> Handle(GetAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");

        // Get student's enrolled courses
        var enrolledCourseOfferingIds = await _unitOfWork.Repository<CourseEnrollment>()
            .Query()
            .Where(ce => ce.StudentId == currentUserId && ce.Status == "Active")
            .Select(ce => ce.CourseOfferingId)
            .ToListAsync(cancellationToken);

        var assignments = await _unitOfWork.Repository<AcademicEvent>()
            .Query()
            .Include(ae => ae.Attachments)
            .Include(ae => ae.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(ae => ae.EventType == "Assignment" && enrolledCourseOfferingIds.Contains(ae.CourseOfferingId))
            .OrderBy(ae => ae.DueDate)
            .ToListAsync(cancellationToken);

        return assignments.Select(ae =>
        {
            string? rawDesc = ae.Description;
            try
            {
                var meta = JsonSerializer.Deserialize<EventMetadata>(ae.Description ?? "");
                rawDesc = meta?.RawDescription ?? ae.Description;
            }
            catch { }

            return new AssignmentDto
            {
                Id = ae.Id,
                Title = ae.Title,
                Description = rawDesc,
                CourseOfferingId = ae.CourseOfferingId,
                CourseName = ae.CourseOffering.Course.Code,
                DeadlineDate = ae.DueDate,
                Priority = ae.Priority,
                Attachments = ae.Attachments.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    BlobStorageUrl = a.BlobStorageUrl,
                    FileSizeInBytes = a.FileSizeInBytes,
                    ContentType = a.ContentType
                }).ToList()
            };
        });
    }
}
