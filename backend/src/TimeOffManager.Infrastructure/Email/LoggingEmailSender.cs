using Microsoft.Extensions.Logging;
using TimeOffManager.Application.Common.Interfaces;

namespace TimeOffManager.Infrastructure.Email;

/// <summary>An <see cref="IEmailSender"/> that just logs — used for the cloud demo
/// when no real SMTP provider is configured (MailHog isn't available there).</summary>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger) => _logger = logger;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[Email] (logging provider) To={To} | Subject={Subject}", message.To, message.Subject);
        return Task.CompletedTask;
    }
}
