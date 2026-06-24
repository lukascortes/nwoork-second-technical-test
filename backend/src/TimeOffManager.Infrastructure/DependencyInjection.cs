using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Infrastructure.Persistence;
using TimeOffManager.Infrastructure.Persistence.Repositories;
using TimeOffManager.Infrastructure.Security;
using TimeOffManager.Infrastructure.Time;

namespace TimeOffManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
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

        return services;
    }
}
