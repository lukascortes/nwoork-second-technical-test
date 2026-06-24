using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Application.TimeOffRequests;

/// <summary>Employee-submitted request. The server assigns the owner and status,
/// so neither is accepted from the client (prevents over-posting / self-approval).</summary>
public sealed record CreateTimeOffRequestRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    LeaveType Type,
    string? Reason);

public sealed record UpdateRequestStatusRequest(RequestStatus Status);

/// <summary>Aggregate counts across all requests, for the admin metrics dashboard.</summary>
public sealed record RequestStatsDto(
    int Total,
    int Pending,
    int Approved,
    int Rejected,
    int Vacation,
    int Sick,
    int Other);

public sealed record UserSummaryDto(Guid Id, string Email, string FullName, UserRole Role)
{
    public static UserSummaryDto FromEntity(User user)
        => new(user.Id, user.Email, user.FullName, user.Role);
}

public sealed record TimeOffRequestDto(
    Guid Id,
    DateOnly StartDate,
    DateOnly EndDate,
    LeaveType Type,
    string? Reason,
    RequestStatus Status,
    int TotalDays,
    DateTime CreatedAt,
    DateTime? ReviewedAt,
    UserSummaryDto? User)
{
    public static TimeOffRequestDto FromEntity(TimeOffRequest r) => new(
        r.Id,
        r.StartDate,
        r.EndDate,
        r.Type,
        r.Reason,
        r.Status,
        r.TotalDays,
        r.CreatedAt,
        r.ReviewedAt,
        r.User is null ? null : UserSummaryDto.FromEntity(r.User));
}
