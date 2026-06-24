using FluentAssertions;
using TimeOffManager.Domain.Common;
using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Domain.UnitTests;

public class TimeOffRequestTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ReviewerId = Guid.NewGuid();
    private static readonly DateOnly Start = new(2026, 7, 1);
    private static readonly DateOnly End = new(2026, 7, 5);

    private static TimeOffRequest NewRequest()
        => TimeOffRequest.Create(UserId, Start, End, LeaveType.Vacation, "Beach");

    [Fact]
    public void Create_SetsPendingStatus_AndComputesInclusiveDays()
    {
        var request = NewRequest();

        request.Status.Should().Be(RequestStatus.Pending);
        request.TotalDays.Should().Be(5); // 1..5 inclusive
        request.Reason.Should().Be("Beach");
    }

    [Fact]
    public void Create_WithEndBeforeStart_Throws()
    {
        var act = () => TimeOffRequest.Create(UserId, End, Start, LeaveType.Vacation, null);

        act.Should().Throw<DomainException>().WithMessage("*End date*");
    }

    [Fact]
    public void Approve_FromPending_TransitionsAndRecordsReviewer()
    {
        var request = NewRequest();
        var when = DateTime.UtcNow;

        request.Approve(ReviewerId, when);

        request.Status.Should().Be(RequestStatus.Approved);
        request.ReviewedByUserId.Should().Be(ReviewerId);
        request.ReviewedAt.Should().Be(when);
    }

    [Fact]
    public void Approve_WhenAlreadyReviewed_Throws()
    {
        var request = NewRequest();
        request.Approve(ReviewerId, DateTime.UtcNow);

        var act = () => request.Reject(ReviewerId, DateTime.UtcNow);

        act.Should().Throw<DomainException>().WithMessage("*Only pending*");
    }

    [Theory]
    [InlineData("2026-07-05", "2026-07-10", true)]  // touches on the 5th
    [InlineData("2026-07-06", "2026-07-10", false)] // starts after
    [InlineData("2026-06-25", "2026-07-01", true)]  // touches on the 1st
    [InlineData("2026-06-01", "2026-06-30", false)] // entirely before
    public void OverlapsWith_DetectsIntersections(string start, string end, bool expected)
    {
        var request = NewRequest();

        var overlaps = request.OverlapsWith(DateOnly.Parse(start), DateOnly.Parse(end));

        overlaps.Should().Be(expected);
    }
}
