using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Application.Users;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    int AnnualVacationDays,
    DateTime CreatedAt)
{
    public static UserDto FromEntity(User user) => new(
        user.Id,
        user.Email,
        user.FullName,
        user.Role,
        user.AnnualVacationDays,
        user.CreatedAt);
}

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string FullName,
    UserRole Role,
    int AnnualVacationDays = 20);

/// <summary>Partial update — only non-null fields are applied.</summary>
public sealed record UpdateUserRequest(
    string? Email,
    string? Password,
    string? FullName,
    UserRole? Role,
    int? AnnualVacationDays);
