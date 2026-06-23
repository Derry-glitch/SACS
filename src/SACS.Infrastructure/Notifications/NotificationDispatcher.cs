using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Infrastructure.Notifications;

public class NotificationDispatcher : INotificationDispatcher
{
    private readonly IEnumerable<INotificationProvider> _providers;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationDispatcher(
        IEnumerable<INotificationProvider> providers,
        IUnitOfWork unitOfWork)
    {
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task DispatchAsync(long userId, NotificationMessage message, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>().Query()
            .Include(u => u.DeviceTokens)
            .Include(u => u.NotificationPreferences)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return;
        }

        var channelsToNotify = new HashSet<NotificationChannel>();
        
        var preferences = user.NotificationPreferences.Where(p => p.IsEnabled);
        foreach (var pref in preferences)
        {
            switch (pref.DeliveryChannel)
            {
                case "Push":
                    channelsToNotify.Add(NotificationChannel.Push);
                    break;
                case "Email":
                    channelsToNotify.Add(NotificationChannel.Email);
                    break;
                case "Both":
                    channelsToNotify.Add(NotificationChannel.Push);
                    channelsToNotify.Add(NotificationChannel.Email);
                    break;
            }
        }

        if (!user.NotificationPreferences.Any())
        {
            channelsToNotify.Add(NotificationChannel.Push);
            channelsToNotify.Add(NotificationChannel.Email);
        }

        var target = new NotificationTarget
        {
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DeviceTokens = user.DeviceTokens
                .Where(t => t.IsActive)
                .Select(t => t.Token)
                .ToList()
        };

        foreach (var channel in channelsToNotify)
        {
            var provider = _providers.FirstOrDefault(p => p.SupportedChannel == channel);
            if (provider != null)
            {
                try
                {
                    await provider.SendAsync(target, message, cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NotificationDispatcher] Provider for {channel} failed: {ex.Message}");
                }

                var log = new NotificationLog
                {
                    UserId = userId,
                    Title = message.Title,
                    Body = message.Body,
                    ChannelUsed = channel.ToString(),
                    SentAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<NotificationLog>().AddAsync(log, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
