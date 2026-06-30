using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using TimeOffManager.Application.Common.Interfaces;

namespace TimeOffManager.Infrastructure.Messaging;

/// <summary>Azure Service Bus implementation of <see cref="IMessagePublisher"/>.
/// Same port as the RabbitMQ publisher — selected by configuration for the cloud.</summary>
public sealed class ServiceBusMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public ServiceBusMessagePublisher(IOptions<ServiceBusOptions> options)
    {
        var value = options.Value;
        _client = new ServiceBusClient(value.ConnectionString);
        _sender = _client.CreateSender(value.QueueName);
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        var body = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(body) { ContentType = "application/json" };
        await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}
