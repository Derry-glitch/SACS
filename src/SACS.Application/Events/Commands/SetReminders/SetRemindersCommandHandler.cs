using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Commands.SetReminders;

public class SetRemindersCommandHandler : IRequestHandler<SetRemindersCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SetRemindersCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(SetRemindersCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");

        var academicEvent = await _unitOfWork.Repository<AcademicEvent>()
            .GetByIdAsync(request.AcademicEventId, cancellationToken);

        if (academicEvent == null)
        {
            throw new KeyNotFoundException("Academic event not found.");
        }

        // Delete existing scheduled notifications for this event & user
        var oldNotifications = await _unitOfWork.Repository<ScheduledNotification>()
            .Query()
            .Where(sn => sn.UserId == currentUserId && sn.AcademicEventId == request.AcademicEventId)
            .ToListAsync(cancellationToken);

        _unitOfWork.Repository<ScheduledNotification>().RemoveRange(oldNotifications);

        // Schedule new notifications
        foreach (var reminder in request.Reminders)
        {
            DateTime? scheduledTime = reminder.ToLowerInvariant() switch
            {
                "1day" => academicEvent.DueDate.AddDays(-1),
                "3days" => academicEvent.DueDate.AddDays(-3),
                "1week" => academicEvent.DueDate.AddDays(-7),
                "custom" => request.CustomReminderTime,
                _ => null
            };

            if (scheduledTime.HasValue && scheduledTime.Value > DateTime.UtcNow)
            {
                var notification = new ScheduledNotification
                {
                    UserId = currentUserId,
                    AcademicEventId = request.AcademicEventId,
                    ReminderType = reminder,
                    ScheduledTime = scheduledTime.Value,
                    Status = "Pending"
                };

                await _unitOfWork.Repository<ScheduledNotification>().AddAsync(notification, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
