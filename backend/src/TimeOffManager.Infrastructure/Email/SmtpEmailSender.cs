using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TimeOffManager.Application.Common.Interfaces;

namespace TimeOffManager.Infrastructure.Email;

/// <summary>Sends mail over SMTP. In dev/demo this points at MailHog, which catches
/// messages and shows them in a web UI (no real mail leaves the machine).</summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;
        email.Body = new TextPart("plain") { Text = message.Body };

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.None, cancellationToken);
        await client.SendAsync(email, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        _logger.LogInformation("Email '{Subject}' sent to {To}", message.Subject, message.To);
    }
}
