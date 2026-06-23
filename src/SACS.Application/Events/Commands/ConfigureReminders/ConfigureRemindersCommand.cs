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

namespace SACS.Application.Events.Commands.ConfigureReminders;

public record ConfigureRemindersCommand(
    List<ReminderItemRequestDto> Reminders,
    long? EventId = null,
    DateTime? CustomReminderTime = null
) : IRequest;

public record ReminderItemRequestDto(string ReminderType, bool IsEnabled);

public class ConfigureRemindersCommandHandler : IRequestHandler<ConfigureRemindersCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ConfigureRemindersCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ConfigureRemindersCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");

        if (request.EventId.HasValue)
        {
            // Event-specific configuration: update/schedule ScheduledNotifications directly
            var academicEvent = await _unitOfWork.Repository<AcademicEvent>().GetByIdAsync(request.EventId.Value, cancellationToken);
            if (academicEvent == null)
            {
                throw new KeyNotFoundException("Academic event not found.");
            }

            // Remove existing reminders for this event & user
            var oldNotifications = await _unitOfWork.Repository<ScheduledNotification>()
                .Query()
                .Where(sn => sn.UserId == currentUserId && sn.AcademicEventId == request.EventId.Value)
                .ToListAsync(cancellationToken);

            _unitOfWork.Repository<ScheduledNotification>().RemoveRange(oldNotifications);

            // Add new enabled scheduled notifications
            foreach (var reminder in request.Reminders.Where(r => r.IsEnabled))
            {
                DateTime? scheduledTime = reminder.ReminderType.ToLowerInvariant() switch
                {
                    "twentyfourhour" or "24hours" => academicEvent.DueDate.AddDays(-1),
                    "threeday" or "72hours" => academicEvent.DueDate.AddDays(-3),
                    "sevenday" or "7days" => academicEvent.DueDate.AddDays(-7),
                    "custom" => request.CustomReminderTime,
                    _ => null
                };

                if (scheduledTime.HasValue && scheduledTime.Value > DateTime.UtcNow)
                {
                    var notification = new ScheduledNotification
                    {
                        UserId = currentUserId,
                        AcademicEventId = academicEvent.Id,
                        ReminderType = reminder.ReminderType,
                        ScheduledTime = scheduledTime.Value,
                        Status = "Pending"
                    };
                    await _unitOfWork.Repository<ScheduledNotification>().AddAsync(notification, cancellationToken);
                }
            }
        }
        else
        {
            // Global preferences configuration: update NotificationPreference table
            var oldPrefs = await _unitOfWork.Repository<NotificationPreference>()
                .Query()
                .Where(np => np.UserId == currentUserId)
                .ToListAsync(cancellationToken);

            _unitOfWork.Repository<NotificationPreference>().RemoveRange(oldPrefs);

            foreach (var reminder in request.Reminders)
            {
                var pref = new NotificationPreference
                {
                    UserId = currentUserId,
                    ReminderType = reminder.ReminderType,
                    IsEnabled = reminder.IsEnabled,
                    DeliveryChannel = "Both"
                };
                await _unitOfWork.Repository<NotificationPreference>().AddAsync(pref, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
