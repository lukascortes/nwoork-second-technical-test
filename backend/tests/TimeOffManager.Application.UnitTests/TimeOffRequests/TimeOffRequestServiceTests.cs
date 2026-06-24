using FluentAssertions;
using FluentValidation;
using NSubstitute;
using TimeOffManager.Application.Common.Exceptions;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Application.TimeOffRequests;
using TimeOffManager.Domain.Common;
using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Application.UnitTests.TimeOffRequests;

public class TimeOffRequestServiceTests
{
    private static readonly DateOnly Today = new(2026, 7, 1);
    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _reviewerId = Guid.NewGuid();

    private readonly ITimeOffRequestRepository _repo = Substitute.For<ITimeOffRequestRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _clock = new FixedClock(Today);
    private readonly IMessagePublisher _publisher = Substitute.For<IMessagePublisher>();

    private TimeOffRequestService CreateSut() => new(
        _repo, _users, _uow, _clock, _publisher,
        new CreateTimeOffRequestValidator(_clock),
        new UpdateRequestStatusValidator());

    private CreateTimeOffRequestRequest ValidCreate() =>
        new(Today.AddDays(5), Today.AddDays(7), LeaveType.Vacation, "Trip");

    [Fact]
    public async Task Create_WithPastStartDate_ThrowsValidation()
    {
        var request = new CreateTimeOffRequestRequest(Today.AddDays(-1), Today.AddDays(2), LeaveType.Vacation, null);

        var act = () => CreateSut().CreateForEmployeeAsync(_employeeId, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Create_WithOverlap_ThrowsConflict()
    {
        _repo.HasOverlapAsync(_employeeId, LeaveType.Vacation, Arg.Any<DateOnly>(), Arg.Any<DateOnly>())
            .Returns(true);

        var act = () => CreateSut().CreateForEmployeeAsync(_employeeId, ValidCreate());

        await act.Should().ThrowAsync<ConflictException>();
        await _repo.DidNotReceive().AddAsync(Arg.Any<TimeOffRequest>());
    }

    [Fact]
    public async Task Create_Valid_PersistsAsPending()
    {
        _repo.HasOverlapAsync(_employeeId, LeaveType.Vacation, Arg.Any<DateOnly>(), Arg.Any<DateOnly>())
            .Returns(false);

        var result = await CreateSut().CreateForEmployeeAsync(_employeeId, ValidCreate());

        result.Status.Should().Be(RequestStatus.Pending);
        result.TotalDays.Should().Be(3);
        await _repo.Received(1).AddAsync(Arg.Is<TimeOffRequest>(r => r.UserId == _employeeId));
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateStatus_WhenNotFound_ThrowsNotFound()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((TimeOffRequest?)null);

        var act = () => CreateSut().UpdateStatusAsync(Guid.NewGuid(), _reviewerId,
            new UpdateRequestStatusRequest(RequestStatus.Approved));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateStatus_RejectPending_SetsRejected()
    {
        var entity = TimeOffRequest.Create(_employeeId, Today.AddDays(5), Today.AddDays(6), LeaveType.Vacation, null);
        _repo.GetByIdAsync(entity.Id).Returns(entity);

        var result = await CreateSut().UpdateStatusAsync(entity.Id, _reviewerId,
            new UpdateRequestStatusRequest(RequestStatus.Rejected));

        result.Status.Should().Be(RequestStatus.Rejected);
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task UpdateStatus_OnAlreadyReviewedRequest_ThrowsDomain()
    {
        var entity = TimeOffRequest.Create(_employeeId, Today.AddDays(5), Today.AddDays(6), LeaveType.Vacation, null);
        entity.Approve(_reviewerId, DateTime.UtcNow);
        _repo.GetByIdAsync(entity.Id).Returns(entity);

        var act = () => CreateSut().UpdateStatusAsync(entity.Id, _reviewerId,
            new UpdateRequestStatusRequest(RequestStatus.Rejected));

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task UpdateStatus_WithPendingTarget_ThrowsValidation()
    {
        var act = () => CreateSut().UpdateStatusAsync(Guid.NewGuid(), _reviewerId,
            new UpdateRequestStatusRequest(RequestStatus.Pending));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Approve_VacationExceedingAllowance_ThrowsConflict()
    {
        var user = User.Create("e@corp.com", "h", "E", UserRole.Employee, annualVacationDays: 5);
        var entity = TimeOffRequest.Create(user.Id, Today.AddDays(5), Today.AddDays(12), LeaveType.Vacation, null); // 8 days
        _repo.GetByIdAsync(entity.Id).Returns(entity);
        _users.GetByIdAsync(entity.UserId).Returns(user);
        _repo.GetByUserAsync(entity.UserId).Returns(new List<TimeOffRequest> { entity });

        var act = () => CreateSut().UpdateStatusAsync(entity.Id, _reviewerId,
            new UpdateRequestStatusRequest(RequestStatus.Approved));

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*allowance*");
    }

    [Fact]
    public async Task Approve_VacationWithinAllowance_Succeeds()
    {
        var user = User.Create("e@corp.com", "h", "E", UserRole.Employee, annualVacationDays: 20);
        var entity = TimeOffRequest.Create(user.Id, Today.AddDays(5), Today.AddDays(7), LeaveType.Vacation, null); // 3 days
        _repo.GetByIdAsync(entity.Id).Returns(entity);
        _users.GetByIdAsync(entity.UserId).Returns(user);
        _repo.GetByUserAsync(entity.UserId).Returns(new List<TimeOffRequest> { entity });

        var result = await CreateSut().UpdateStatusAsync(entity.Id, _reviewerId,
            new UpdateRequestStatusRequest(RequestStatus.Approved));

        result.Status.Should().Be(RequestStatus.Approved);
    }

    private sealed class FixedClock : IDateTimeProvider
    {
        public FixedClock(DateOnly today)
        {
            Today = today;
            UtcNow = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        }

        public DateTime UtcNow { get; }
        public DateOnly Today { get; }
    }
}
