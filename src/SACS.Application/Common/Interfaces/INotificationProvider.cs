using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SACS.Application.Common.Interfaces;

public enum NotificationChannel
{
    Push,
    Email,
    SMS
}

public class NotificationTarget
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public List<string> DeviceTokens { get; set; } = new();
}

public class NotificationMessage
{
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public Dictionary<string, string>? Payload { get; set; }
}

public interface INotificationProvider
{
    NotificationChannel SupportedChannel { get; }
    Task SendAsync(NotificationTarget target, NotificationMessage message, CancellationToken cancellationToken = default);
}
