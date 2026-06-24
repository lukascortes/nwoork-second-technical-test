using FluentValidation;
using TimeOffManager.Application.Common.Exceptions;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;
using TimeOffManager.Domain.ValueObjects;

namespace TimeOffManager.Application.TimeOffRequests;

public sealed class TimeOffRequestService : ITimeOffRequestService
{
    private readonly ITimeOffRequestRepository _requests;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IValidator<CreateTimeOffRequestRequest> _createValidator;
    private readonly IValidator<UpdateRequestStatusRequest> _updateStatusValidator;

    public TimeOffRequestService(
        ITimeOffRequestRepository requests,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        IValidator<CreateTimeOffRequestRequest> createValidator,
        IValidator<UpdateRequestStatusRequest> updateStatusValidator)
    {
        _requests = requests;
        _users = users;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _createValidator = createValidator;
        _updateStatusValidator = updateStatusValidator;
    }

    public async Task<TimeOffRequestDto> CreateForEmployeeAsync(
        Guid employeeId,
        CreateTimeOffRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var hasOverlap = await _requests.HasOverlapAsync(
            employeeId, request.Type, request.StartDate, request.EndDate, cancellationToken);
        if (hasOverlap)
            throw new ConflictException(
                "You already have a pending or approved request of this type for these dates.");

        var entity = TimeOffRequest.Create(
            employeeId, request.StartDate, request.EndDate, request.Type, request.Reason);

        await _requests.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TimeOffRequestDto.FromEntity(entity);
    }

    public async Task<IReadOnlyList<TimeOffRequestDto>> GetMyRequestsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var items = await _requests.GetByUserAsync(employeeId, cancellationToken);
        return items.Select(TimeOffRequestDto.FromEntity).ToList();
    }

    public async Task<IReadOnlyList<TimeOffRequestDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _requests.GetAllWithUsersAsync(cancellationToken);
        return items.Select(TimeOffRequestDto.FromEntity).ToList();
    }

    public async Task<TimeOffRequestDto> UpdateStatusAsync(
        Guid requestId,
        Guid reviewerId,
        UpdateRequestStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateStatusValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = await _requests.GetByIdAsync(requestId, cancellationToken)
                     ?? throw new NotFoundException(nameof(TimeOffRequest), requestId);

        if (request.Status == RequestStatus.Approved)
        {
            await EnsureWithinVacationAllowanceAsync(entity, cancellationToken);
            entity.Approve(reviewerId, _clock.UtcNow);
        }
        else
        {
            entity.Reject(reviewerId, _clock.UtcNow);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TimeOffRequestDto.FromEntity(entity);
    }

    /// <summary>Blocks approval of a vacation request that would push the employee
    /// over their annual allowance.</summary>
    private async Task EnsureWithinVacationAllowanceAsync(TimeOffRequest request, CancellationToken cancellationToken)
    {
        if (request.Type != LeaveType.Vacation)
            return;

        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return;

        var siblings = await _requests.GetByUserAsync(request.UserId, cancellationToken);
        var alreadyUsed = siblings
            .Where(r => r.Id != request.Id && r.Type == LeaveType.Vacation && r.Status == RequestStatus.Approved)
            .Sum(r => r.TotalDays);

        var balance = new VacationBalance(user.AnnualVacationDays, alreadyUsed, PendingDays: 0);
        if (!balance.CanAccommodate(request.TotalDays))
            throw new ConflictException(
                $"Approving this request would exceed the employee's annual vacation allowance " +
                $"({user.AnnualVacationDays} days). Already used: {alreadyUsed}, requested: {request.TotalDays}.");
    }
}
