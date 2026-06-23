using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Common.Interfaces;
using SACS.Application.Events.DTOs;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Queries.GetCalendar;

public record GetUpcomingCalendarQuery : IRequest<IEnumerable<CalendarEventDto>>;

public class GetUpcomingCalendarQueryHandler : IRequestHandler<GetUpcomingCalendarQuery, IEnumerable<CalendarEventDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetUpcomingCalendarQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<CalendarEventDto>> Handle(GetUpcomingCalendarQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");
        var now = DateTime.UtcNow;

        var enrolledOfferings = await _unitOfWork.Repository<CourseEnrollment>()
            .Query()
            .Where(ce => ce.StudentId == currentUserId && ce.Status == "Active")
            .Select(ce => ce.CourseOfferingId)
            .ToListAsync(cancellationToken);

        var events = await _unitOfWork.Repository<AcademicEvent>()
            .Query()
            .Include(ae => ae.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(ae => enrolledOfferings.Contains(ae.CourseOfferingId) && ae.DueDate >= now)
            .OrderBy(ae => ae.DueDate)
            .ToListAsync(cancellationToken);

        return events.Select(ae => new CalendarEventDto
        {
            Id = ae.Id,
            Title = ae.Title,
            EventType = ae.EventType,
            DueDate = ae.DueDate,
            CourseName = ae.CourseOffering.Course.Code,
            Priority = ae.Priority,
            Venue = ae.Venue
        });
    }
}
