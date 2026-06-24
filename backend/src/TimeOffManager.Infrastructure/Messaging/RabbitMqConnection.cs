using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace TimeOffManager.Infrastructure.Messaging;

/// <summary>Owns a single, lazily-established RabbitMQ connection shared by the
/// publisher and the consumer. Retries on startup so the API tolerates the broker
/// coming up slightly later.</summary>
public sealed class RabbitMqConnection : IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly object _gate = new();
    private IConnection? _connection;

    public RabbitMqConnection(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnection> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public IConnection GetConnection()
    {
        if (_connection is { IsOpen: true })
            return _connection;

        lock (_gate)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                DispatchConsumersAsync = true
            };

            const int maxAttempts = 10;
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    _logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}", _options.Host, _options.Port);
                    return _connection;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    _logger.LogWarning(ex, "RabbitMQ connection attempt {Attempt}/{Max} failed; retrying...", attempt, maxAttempts);
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                }
            }
        }
    }

    public void Dispose() => _connection?.Dispose();
}
