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

namespace SACS.Application.Events.Queries.GetMyReminders;

public record GetMyRemindersQuery : IRequest<IEnumerable<ReminderDto>>;

public class GetMyRemindersQueryHandler : IRequestHandler<GetMyRemindersQuery, IEnumerable<ReminderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyRemindersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<ReminderDto>> Handle(GetMyRemindersQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");

        var reminders = await _unitOfWork.Repository<ScheduledNotification>()
            .Query()
            .Include(sn => sn.AcademicEvent)
            .Where(sn => sn.UserId == currentUserId)
            .OrderBy(sn => sn.ScheduledTime)
            .ToListAsync(cancellationToken);

        return reminders.Select(sn => new ReminderDto
        {
            Id = sn.Id,
            EventId = sn.AcademicEventId,
            EventTitle = sn.AcademicEvent.Title,
            ReminderType = sn.ReminderType,
            ScheduledTime = sn.ScheduledTime,
            Status = sn.Status
        });
    }
}
