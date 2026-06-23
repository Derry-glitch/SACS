using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using SACS.Application.Common.Interfaces;

namespace SACS.Infrastructure.Notifications;

public class FirebaseNotificationProvider : INotificationProvider
{
    public NotificationChannel SupportedChannel => NotificationChannel.Push;

    public async Task SendAsync(NotificationTarget target, NotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (target.DeviceTokens == null || !target.DeviceTokens.Any())
        {
            return;
        }

        var multicastMessage = new MulticastMessage
        {
            Tokens = target.DeviceTokens,
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = message.Title,
                Body = message.Body
            },
            Data = message.Payload
        };

        try
        {
            // FirebaseAdmin requires initialization via FirebaseApp.Create() at startup.
            // We use a try-catch to allow clean execution even if configuration keys are not yet deployed.
            var messagingInstance = FirebaseMessaging.DefaultInstance;
            if (messagingInstance != null)
            {
                await messagingInstance.SendEachForMulticastAsync(multicastMessage, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log fallback error in console/diagnostics
            Console.WriteLine($"[FirebaseNotificationProvider] Failed to dispatch push: {ex.Message}");
        }
    }
}
