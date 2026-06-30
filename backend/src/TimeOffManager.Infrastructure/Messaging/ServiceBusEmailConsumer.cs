using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Application.Notifications;
using TimeOffManager.Infrastructure.Email;

namespace TimeOffManager.Infrastructure.Messaging;

/// <summary>Consumes review notifications from Azure Service Bus and sends the email.
/// The Service Bus counterpart of <see cref="EmailNotificationConsumer"/>.</summary>
public sealed class ServiceBusEmailConsumer : BackgroundService
{
    private readonly ServiceBusOptions _options;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ServiceBusEmailConsumer> _logger;
    private ServiceBusClient? _client;
    private ServiceBusProcessor? _processor;

    public ServiceBusEmailConsumer(
        IOptions<ServiceBusOptions> options,
        IEmailSender emailSender,
        ILogger<ServiceBusEmailConsumer> logger)
    {
        _options = options.Value;
        _emailSender = emailSender;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client = new ServiceBusClient(_options.ConnectionString);
        _processor = _client.CreateProcessor(_options.QueueName, new ServiceBusProcessorOptions { AutoCompleteMessages = false });

        _processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var notification = JsonSerializer.Deserialize<RequestReviewedNotification>(args.Message.Body.ToString());
                if (notification is not null)
                    await _emailSender.SendAsync(ReviewedRequestEmail.From(notification), args.CancellationToken);

                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process Service Bus email notification");
                await args.DeadLetterMessageAsync(args.Message, cancellationToken: args.CancellationToken);
            }
        };

        _processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Service Bus processor error");
            return Task.CompletedTask;
        };

        await _processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("Email notification consumer (Service Bus) is listening on '{Queue}'", _options.QueueName);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_processor is not null)
            await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _processor?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _client?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        base.Dispose();
    }
}
