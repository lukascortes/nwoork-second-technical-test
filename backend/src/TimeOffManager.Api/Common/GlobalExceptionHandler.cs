using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TimeOffManager.Application.Common.Exceptions;
using TimeOffManager.Domain.Common;

namespace TimeOffManager.Api.Common;

/// <summary>
/// Translates exceptions into RFC 7807 ProblemDetails responses, so the API never
/// leaks stack traces and every error has a consistent, typed shape.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException validation:
                await WriteValidationProblem(httpContext, validation, cancellationToken);
                return true;

            case NotFoundException:
                await WriteProblem(httpContext, StatusCodes.Status404NotFound, "Not Found", exception.Message, cancellationToken);
                return true;

            case ConflictException:
                await WriteProblem(httpContext, StatusCodes.Status409Conflict, "Conflict", exception.Message, cancellationToken);
                return true;

            case ForbiddenAccessException:
                await WriteProblem(httpContext, StatusCodes.Status403Forbidden, "Forbidden", exception.Message, cancellationToken);
                return true;

            case InvalidCredentialsException:
                await WriteProblem(httpContext, StatusCodes.Status401Unauthorized, "Unauthorized", exception.Message, cancellationToken);
                return true;

            case DomainException:
                await WriteProblem(httpContext, StatusCodes.Status422UnprocessableEntity, "Business rule violation", exception.Message, cancellationToken);
                return true;

            default:
                _logger.LogError(exception, "Unhandled exception processing {Path}", httpContext.Request.Path);
                await WriteProblem(httpContext, StatusCodes.Status500InternalServerError, "Internal Server Error",
                    "An unexpected error occurred.", cancellationToken);
                return true;
        }
    }

    private static async Task WriteValidationProblem(HttpContext ctx, ValidationException ex, CancellationToken ct)
    {
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        };
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsJsonAsync(problem, ct);
    }

    private static async Task WriteProblem(HttpContext ctx, int status, string title, string detail, CancellationToken ct)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };
        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(problem, ct);
    }
}
