using TimeOffManager.Application.Common.Interfaces;

namespace TimeOffManager.Api.IntegrationTests;

/// <summary>Test double so integration tests never reach RabbitMQ.</summary>
internal sealed class NoOpMessagePublisher : IMessagePublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        => Task.CompletedTask;
}
