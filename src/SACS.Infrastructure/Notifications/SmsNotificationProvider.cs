using System;
using System.Threading;
using System.Threading.Tasks;
using SACS.Application.Common.Interfaces;

namespace SACS.Infrastructure.Notifications;

public class SmsNotificationProvider : INotificationProvider
{
    public NotificationChannel SupportedChannel => NotificationChannel.SMS;

    public Task SendAsync(NotificationTarget target, NotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(target.PhoneNumber))
        {
            return Task.CompletedTask;
        }

        // Future Integration: Twilio/Infobip/local gateways
        Console.WriteLine($"[SmsNotificationProvider PLACEHOLDER] SMS to {target.PhoneNumber}: {message.Title} - {message.Body}");
        
        return Task.CompletedTask;
    }
}
