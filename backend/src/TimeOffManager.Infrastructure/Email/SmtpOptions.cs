namespace TimeOffManager.Infrastructure.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string FromEmail { get; set; } = "noreply@timeoff.dev";
    public string FromName { get; set; } = "TimeOff Manager";
}
