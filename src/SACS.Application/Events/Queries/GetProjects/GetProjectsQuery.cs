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

namespace SACS.Application.Events.Queries.GetProjects;

public record GetProjectsQuery : IRequest<IEnumerable<ProjectDto>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, IEnumerable<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetProjectsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");

        var enrolledCourseOfferingIds = await _unitOfWork.Repository<CourseEnrollment>()
            .Query()
            .Where(ce => ce.StudentId == currentUserId && ce.Status == "Active")
            .Select(ce => ce.CourseOfferingId)
            .ToListAsync(cancellationToken);

        var projects = await _unitOfWork.Repository<AcademicEvent>()
            .Query()
            .Include(ae => ae.Attachments)
            .Include(ae => ae.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(ae => ae.EventType == "Project" && enrolledCourseOfferingIds.Contains(ae.CourseOfferingId))
            .OrderBy(ae => ae.DueDate)
            .ToListAsync(cancellationToken);

        return projects.Select(ae =>
        {
            var meta = new EventMetadata();
            try
            {
                meta = JsonSerializer.Deserialize<EventMetadata>(ae.Description ?? "") ?? new EventMetadata();
            }
            catch { }

            return new ProjectDto
            {
                Id = ae.Id,
                Title = ae.Title,
                CourseOfferingId = ae.CourseOfferingId,
                CourseName = ae.CourseOffering.Course.Code,
                SupervisorName = meta.SupervisorName ?? "Unknown",
                SubmissionDate = ae.DueDate,
                ProgressPercentage = meta.ProgressPercentage ?? 0,
                Notes = meta.Notes,
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
