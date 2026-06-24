using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TimeOffManager.Application.Auth;
using TimeOffManager.Application.TimeOffRequests;
using TimeOffManager.Application.Users;

namespace TimeOffManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registers every IValidator<T> in this assembly (transient).
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITimeOffRequestService, TimeOffRequestService>();

        return services;
    }
}
