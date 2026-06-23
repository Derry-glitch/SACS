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

public record GetWeeklyCalendarQuery(DateTime? Date) : IRequest<IEnumerable<CalendarEventDto>>;

public class GetWeeklyCalendarQueryHandler : IRequestHandler<GetWeeklyCalendarQuery, IEnumerable<CalendarEventDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetWeeklyCalendarQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<CalendarEventDto>> Handle(GetWeeklyCalendarQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");
        var targetDate = request.Date ?? DateTime.UtcNow;
        
        int diff = (7 + (targetDate.DayOfWeek - DayOfWeek.Sunday)) % 7;
        var start = targetDate.AddDays(-1 * diff).Date;
        var end = start.AddDays(7).AddTicks(-1);

        var enrolledOfferings = await _unitOfWork.Repository<CourseEnrollment>()
            .Query()
            .Where(ce => ce.StudentId == currentUserId && ce.Status == "Active")
            .Select(ce => ce.CourseOfferingId)
            .ToListAsync(cancellationToken);

        var events = await _unitOfWork.Repository<AcademicEvent>()
            .Query()
            .Include(ae => ae.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(ae => enrolledOfferings.Contains(ae.CourseOfferingId) && ae.DueDate >= start && ae.DueDate <= end)
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
