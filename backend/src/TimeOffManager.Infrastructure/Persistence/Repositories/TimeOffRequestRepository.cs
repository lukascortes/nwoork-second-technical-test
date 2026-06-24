using Microsoft.EntityFrameworkCore;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Infrastructure.Persistence.Repositories;

public sealed class TimeOffRequestRepository : ITimeOffRequestRepository
{
    private readonly AppDbContext _db;

    public TimeOffRequestRepository(AppDbContext db) => _db = db;

    public Task<TimeOffRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.TimeOffRequests.FirstOrDefaultAsync(r => r.Id == id, cancellationToken); // tracked for status updates

    public async Task<IReadOnlyList<TimeOffRequest>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _db.TimeOffRequests
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TimeOffRequest>> GetAllWithUsersAsync(CancellationToken cancellationToken = default)
        => await _db.TimeOffRequests
            .AsNoTracking()
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TimeOffRequest>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.TimeOffRequests.AsNoTracking().ToListAsync(cancellationToken);

    public Task<bool> HasOverlapAsync(
        Guid userId,
        LeaveType type,
        DateOnly start,
        DateOnly end,
        CancellationToken cancellationToken = default)
        => _db.TimeOffRequests.AnyAsync(
            r => r.UserId == userId
                 && r.Status != RequestStatus.Rejected
                 && r.Type == type
                 && r.StartDate <= end
                 && r.EndDate >= start,
            cancellationToken);

    public async Task AddAsync(TimeOffRequest request, CancellationToken cancellationToken = default)
        => await _db.TimeOffRequests.AddAsync(request, cancellationToken);
}
