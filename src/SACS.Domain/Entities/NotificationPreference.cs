using SACS.Domain.Common;

namespace SACS.Domain.Entities;

public class NotificationPreference : BaseEntity
{
    public long UserId { get; set; }
    public string ReminderType { get; set; } = null!; // SevenDay, ThreeDay, TwentyFourHour, TwoHour
    public bool IsEnabled { get; set; } = true;
    public string DeliveryChannel { get; set; } = "Both"; // Push, Email, Both, None

    // Navigation property
    public virtual User User { get; set; } = null!;
}
