using System.Collections.Generic;

namespace SACS.Application.Common.Events;

public class StudyPlanGenerationEvent
{
    public string Name { get; set; } = null!;
    public Dictionary<string, double> AvailableFreeHours { get; set; } = new();
    public long UserId { get; set; }

    public StudyPlanGenerationEvent() { }

    public StudyPlanGenerationEvent(string name, Dictionary<string, double> availableFreeHours, long userId)
    {
        Name = name;
        AvailableFreeHours = availableFreeHours;
        UserId = userId;
    }
}
