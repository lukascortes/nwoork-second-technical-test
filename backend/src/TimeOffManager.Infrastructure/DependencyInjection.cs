using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Infrastructure.Email;
using TimeOffManager.Infrastructure.Messaging;
using TimeOffManager.Infrastructure.Persistence;
using TimeOffManager.Infrastructure.Persistence.Repositories;
using TimeOffManager.Infrastructure.Security;
using TimeOffManager.Infrastructure.Time;

namespace TimeOffManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool registerHostedServices = true)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITimeOffRequestRepository, TimeOffRequestRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // Fail fast at startup if the signing key is missing or too weak (no insecure fallback).
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && o.Key.Length >= 32,
                "Jwt:Key is required and must be at least 32 characters.")
            .Validate(o => o.AccessTokenMinutes > 0,
                "Jwt:AccessTokenMinutes must be a positive value.")
            .ValidateOnStart();

        AddMessaging(services, configuration, registerHostedServices);
        AddEmail(services, configuration);

        return services;
    }

    /// <summary>Selects the message broker adapter by configuration. Same
    /// <see cref="IMessagePublisher"/> port — RabbitMQ locally, Azure Service Bus in the cloud.</summary>
    private static void AddMessaging(IServiceCollection services, IConfiguration configuration, bool registerHostedServices)
    {
        var provider = configuration["Messaging:Provider"] ?? "RabbitMq";

        if (provider.Equals("AzureServiceBus", StringComparison.OrdinalIgnoreCase))
        {
            services.AddOptions<ServiceBusOptions>().Bind(configuration.GetSection(ServiceBusOptions.SectionName));
            services.AddSingleton<IMessagePublisher, ServiceBusMessagePublisher>();
            if (registerHostedServices)
                services.AddHostedService<ServiceBusEmailConsumer>();
        }
        else
        {
            services.AddOptions<RabbitMqOptions>().Bind(configuration.GetSection(RabbitMqOptions.SectionName));
            services.AddSingleton<RabbitMqConnection>();
            services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
            if (registerHostedServices)
                services.AddHostedService<EmailNotificationConsumer>();
        }
    }

    /// <summary>Selects the email adapter by configuration: real SMTP (MailHog/SendGrid)
    /// or a logging sender for the cloud demo when no SMTP is wired up.</summary>
    private static void AddEmail(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SmtpOptions>().Bind(configuration.GetSection(SmtpOptions.SectionName));

        var provider = configuration["Email:Provider"] ?? "Smtp";
        if (provider.Equals("Logging", StringComparison.OrdinalIgnoreCase))
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
        else
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
    }
}
