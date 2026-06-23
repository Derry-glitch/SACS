namespace SACS.Application.Common.Models;

public class EventMetadata
{
    public int? DurationMinutes { get; set; }
    public string? ReminderWindow { get; set; }
    public string? SeatNumber { get; set; }
    public string? SupervisorName { get; set; }
    public int? ProgressPercentage { get; set; }
    public string? RawDescription { get; set; }
    public string? Notes { get; set; }
}
