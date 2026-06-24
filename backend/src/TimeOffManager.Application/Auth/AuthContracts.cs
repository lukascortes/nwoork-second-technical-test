using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Application.Auth;

public sealed record LoginRequest(string Email, string Password);

/// <summary>Self-service registration. Intentionally does NOT accept a role:
/// new accounts are always created as <see cref="UserRole.Employee"/>.</summary>
public sealed record RegisterRequest(string Email, string Password, string FullName);

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string Email,
    string FullName,
    UserRole Role);
