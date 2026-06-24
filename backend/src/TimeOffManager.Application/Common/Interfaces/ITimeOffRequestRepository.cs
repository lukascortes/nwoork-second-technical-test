using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Application.Common.Interfaces;

public interface ITimeOffRequestRepository
{
    Task<TimeOffRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeOffRequest>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>All requests including their <see cref="User"/> (admin view), newest first.</summary>
    Task<IReadOnlyList<TimeOffRequest>> GetAllWithUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>True if the user already has a non-rejected request of the same type
    /// whose date range intersects [start, end].</summary>
    Task<bool> HasOverlapAsync(
        Guid userId,
        LeaveType type,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default);

    Task AddAsync(TimeOffRequest request, CancellationToken cancellationToken = default);
}
