namespace TimeOffManager.Application.TimeOffRequests;

public interface ITimeOffRequestService
{
    Task<TimeOffRequestDto> CreateForEmployeeAsync(
        Guid employeeId,
        CreateTimeOffRequestRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeOffRequestDto>> GetMyRequestsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeOffRequestDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<RequestStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);

    Task<TimeOffRequestDto> UpdateStatusAsync(
        Guid requestId,
        Guid reviewerId,
        UpdateRequestStatusRequest request,
        CancellationToken cancellationToken = default);
}
