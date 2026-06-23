using System;
using System.Collections.Generic;
using MediatR;

namespace SACS.Application.Events.Commands.SetReminders;

public record SetRemindersCommand(
    long AcademicEventId,
    List<string> Reminders,
    DateTime? CustomReminderTime = null
) : IRequest;
