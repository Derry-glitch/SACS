using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using SACS.Application.Common.Interfaces;

namespace SACS.Infrastructure.Notifications;

public class EmailNotificationProvider : INotificationProvider
{
    public NotificationChannel SupportedChannel => NotificationChannel.Email;

    public async Task SendAsync(NotificationTarget target, NotificationMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(target.Email))
        {
            return;
        }

        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("SACS Companion", "no-reply@sacs.com"));
        emailMessage.To.Add(new MailboxAddress("", target.Email));
        emailMessage.Subject = message.Title;

        var bodyBuilder = new BodyBuilder { HtmlBody = message.Body };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            // Connect to SMTP development sandbox (e.g. Mailhog or Papercut on localhost:25)
            await client.ConnectAsync("localhost", 25, false, cancellationToken);
            await client.SendAsync(emailMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailNotificationProvider] SMTP transmit failed: {ex.Message}");
        }
    }
}
