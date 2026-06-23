using System;
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
using SACS.Domain.Common;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Queries.GetEvents;

public record GetEventsQuery : IRequest<IEnumerable<EventDto>>;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, IEnumerable<EventDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetEventsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<EventDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");

        var enrolledOfferings = await _unitOfWork.Repository<CourseEnrollment>()
            .Query()
            .Where(ce => ce.StudentId == currentUserId && ce.Status == "Active")
            .Select(ce => ce.CourseOfferingId)
            .ToListAsync(cancellationToken);

        var events = await _unitOfWork.Repository<AcademicEvent>()
            .Query()
            .Include(ae => ae.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(ae => enrolledOfferings.Contains(ae.CourseOfferingId))
            .OrderBy(ae => ae.DueDate)
            .ToListAsync(cancellationToken);

        return events.Select(ae =>
        {
            Enum.TryParse<AcademicEventType>(ae.EventType, out var eventType);
            var meta = new EventMetadata();
            try
            {
                meta = JsonSerializer.Deserialize<EventMetadata>(ae.Description ?? "") ?? new EventMetadata();
            }
            catch { }

            return new EventDto
            {
                Id = ae.Id,
                Title = ae.Title,
                Description = ae.Description,
                CourseId = ae.CourseOfferingId,
                CourseCode = ae.CourseOffering.Course.Code,
                EventType = eventType,
                DueDateTime = ae.DueDate,
                PriorityLevel = ae.Priority,
                Notes = meta.Notes,
                AttachmentUrl = meta.AttachmentUrl,
                CreatedByUserId = ae.CreatedByUserId,
                DurationMinutes = meta.DurationMinutes,
                Venue = ae.Venue,
                SeatNumber = meta.SeatNumber,
                SupervisorName = meta.SupervisorName,
                ProgressPercentage = meta.ProgressPercentage,
                StudyTopic = meta.StudyTopic,
                StudyDuration = meta.StudyDuration
            };
        });
    }
}
