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

public record GetCalendarQuery(string ViewType, DateTime Date) : IRequest<IEnumerable<CalendarEventDto>>;

public class GetCalendarQueryHandler : IRequestHandler<GetCalendarQuery, IEnumerable<CalendarEventDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCalendarQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<CalendarEventDto>> Handle(GetCalendarQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");

        // Enrolled courses
        var enrolledCourseOfferingIds = await _unitOfWork.Repository<CourseEnrollment>()
            .Query()
            .Where(ce => ce.StudentId == currentUserId && ce.Status == "Active")
            .Select(ce => ce.CourseOfferingId)
            .ToListAsync(cancellationToken);

        var query = _unitOfWork.Repository<AcademicEvent>()
            .Query()
            .Include(ae => ae.CourseOffering)
                .ThenInclude(co => co.Course)
            .Where(ae => enrolledCourseOfferingIds.Contains(ae.CourseOfferingId));

        DateTime startDate;
        DateTime endDate;

        switch (request.ViewType.ToLowerInvariant())
        {
            case "day":
                startDate = request.Date.Date;
                endDate = request.Date.Date.AddDays(1).AddTicks(-1);
                query = query.Where(ae => ae.DueDate >= startDate && ae.DueDate <= endDate);
                break;

            case "week":
                // Start of week (Sunday)
                int diff = (7 + (request.Date.DayOfWeek - DayOfWeek.Sunday)) % 7;
                startDate = request.Date.AddDays(-1 * diff).Date;
                endDate = startDate.AddDays(7).AddTicks(-1);
                query = query.Where(ae => ae.DueDate >= startDate && ae.DueDate <= endDate);
                break;

            case "month":
                startDate = new DateTime(request.Date.Year, request.Date.Month, 1);
                endDate = startDate.AddMonths(1).AddTicks(-1);
                query = query.Where(ae => ae.DueDate >= startDate && ae.DueDate <= endDate);
                break;

            case "upcoming":
            default:
                query = query.Where(ae => ae.DueDate >= DateTime.UtcNow);
                break;
        }

        var events = await query.OrderBy(ae => ae.DueDate).ToListAsync(cancellationToken);

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
