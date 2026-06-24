namespace TimeOffManager.Application.Common.Interfaces;

/// <summary>Publishes a message to the broker. Implemented by Infrastructure
/// (RabbitMQ today, Azure Service Bus tomorrow) — the application stays unaware.</summary>
public interface IMessagePublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}
