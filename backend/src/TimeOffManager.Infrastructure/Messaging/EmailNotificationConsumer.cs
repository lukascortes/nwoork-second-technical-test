using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Application.Notifications;
using TimeOffManager.Infrastructure.Email;

namespace TimeOffManager.Infrastructure.Messaging;

/// <summary>Background worker: consumes review notifications off the queue and
/// turns them into emails. This is the consumer side of the async pattern.</summary>
public sealed class EmailNotificationConsumer : BackgroundService
{
    private readonly RabbitMqConnection _connection;
    private readonly RabbitMqOptions _options;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailNotificationConsumer> _logger;
    private IModel? _channel;

    public EmailNotificationConsumer(
        RabbitMqConnection connection,
        IOptions<RabbitMqOptions> options,
        IEmailSender emailSender,
        ILogger<EmailNotificationConsumer> logger)
    {
        _connection = connection;
        _options = options.Value;
        _emailSender = emailSender;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _connection.GetConnection().CreateModel();
        _channel.QueueDeclare(_options.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var notification = JsonSerializer.Deserialize<RequestReviewedNotification>(ea.Body.Span);
                if (notification is not null)
                    await _emailSender.SendAsync(ReviewedRequestEmail.From(notification), stoppingToken);

                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process email notification");
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(_options.QueueName, autoAck: false, consumer);
        _logger.LogInformation("Email notification consumer (RabbitMQ) is listening on '{Queue}'", _options.QueueName);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }
}
