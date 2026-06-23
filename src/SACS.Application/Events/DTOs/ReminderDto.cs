using System;

namespace SACS.Application.Events.DTOs;

public class ReminderDto
{
    public long Id { get; set; }
    public long EventId { get; set; }
    public string EventTitle { get; set; } = null!;
    public string ReminderType { get; set; } = null!;
    public DateTime ScheduledTime { get; set; }
    public string Status { get; set; } = null!;
}
