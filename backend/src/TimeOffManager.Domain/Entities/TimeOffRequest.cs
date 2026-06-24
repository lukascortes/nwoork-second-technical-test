using TimeOffManager.Domain.Common;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Domain.Entities;

/// <summary>
/// A time-off request. Encapsulates its own state machine: a request starts as
/// <see cref="RequestStatus.Pending"/> and can only transition to Approved/Rejected
/// once, and only from Pending. Dates are <see cref="DateOnly"/> (no time-zone ambiguity).
/// </summary>
public class TimeOffRequest : Entity
{
    public Guid UserId { get; private set; }
    public User? User { get; private set; }

    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public LeaveType Type { get; private set; }
    public string? Reason { get; private set; }
    public RequestStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }

    /// <summary>Inclusive number of calendar days covered by the request.</summary>
    public int TotalDays => EndDate.DayNumber - StartDate.DayNumber + 1;

    private TimeOffRequest() { } // EF Core

    private TimeOffRequest(
        Guid id,
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        LeaveType type,
        string? reason,
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        StartDate = startDate;
        EndDate = endDate;
        Type = type;
        Reason = reason;
        Status = RequestStatus.Pending;
        CreatedAt = createdAt;
    }

    public static TimeOffRequest Create(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        LeaveType type,
        string? reason)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required.");
        if (endDate < startDate)
            throw new DomainException("End date cannot be before start date.");

        return new TimeOffRequest(
            Guid.NewGuid(),
            userId,
            startDate,
            endDate,
            type,
            string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            DateTime.UtcNow);
    }

    public void Approve(Guid reviewerId, DateTime reviewedAtUtc)
        => Transition(RequestStatus.Approved, reviewerId, reviewedAtUtc);

    public void Reject(Guid reviewerId, DateTime reviewedAtUtc)
        => Transition(RequestStatus.Rejected, reviewerId, reviewedAtUtc);

    private void Transition(RequestStatus target, Guid reviewerId, DateTime reviewedAtUtc)
    {
        if (Status != RequestStatus.Pending)
            throw new DomainException($"Only pending requests can be modified (current status: {Status}).");
        if (reviewerId == Guid.Empty)
            throw new DomainException("Reviewer is required.");

        Status = target;
        ReviewedByUserId = reviewerId;
        ReviewedAt = reviewedAtUtc;
    }

    /// <summary>True when this request's date range intersects [start, end].</summary>
    public bool OverlapsWith(DateOnly start, DateOnly end)
        => StartDate <= end && EndDate >= start;
}
