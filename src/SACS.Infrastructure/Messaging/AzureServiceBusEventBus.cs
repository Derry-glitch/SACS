using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using SACS.Application.Common.Interfaces;

namespace SACS.Infrastructure.Messaging;

public class AzureServiceBusEventBus : IEventBus
{
    private readonly ServiceBusClient _serviceBusClient;

    public AzureServiceBusEventBus(ServiceBusClient serviceBusClient)
    {
        _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        var queueOrTopicName = typeof(T).Name.ToLower();
        var sender = _serviceBusClient.CreateSender(queueOrTopicName);

        var json = JsonSerializer.Serialize(message);
        var busMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json"
        };

        await sender.SendMessageAsync(busMessage, cancellationToken);
    }
}
