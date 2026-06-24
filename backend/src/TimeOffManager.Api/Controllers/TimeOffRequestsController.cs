using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeOffManager.Api.Common;
using TimeOffManager.Application.TimeOffRequests;
using TimeOffManager.Application.Users;

namespace TimeOffManager.Api.Controllers;

[ApiController]
[Route("api/timeoffrequests")]
[Authorize]
public sealed class TimeOffRequestsController : ControllerBase
{
    private readonly ITimeOffRequestService _requests;
    private readonly IUserService _users;

    public TimeOffRequestsController(ITimeOffRequestService requests, IUserService users)
    {
        _requests = requests;
        _users = users;
    }

    /// <summary>Employee submits a new request. Owner and status are assigned server-side.</summary>
    [HttpPost]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<TimeOffRequestDto>> Create(
        CreateTimeOffRequestRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _requests.CreateForEmployeeAsync(User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetMine), null, created);
    }

    /// <summary>Employee lists their own requests.</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<IReadOnlyList<TimeOffRequestDto>>> GetMine(CancellationToken cancellationToken)
        => Ok(await _requests.GetMyRequestsAsync(User.GetUserId(), cancellationToken));

    /// <summary>Employee's own vacation-day balance.</summary>
    [HttpGet("balance")]
    [Authorize(Roles = "Employee")]
    public async Task<ActionResult<VacationBalanceDto>> GetMyBalance(CancellationToken cancellationToken)
        => Ok(await _users.GetVacationBalanceAsync(User.GetUserId(), cancellationToken));

    /// <summary>Admin lists every request, newest first.</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<TimeOffRequestDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _requests.GetAllAsync(cancellationToken));

    /// <summary>Admin approves or rejects a pending request.</summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<TimeOffRequestDto>> UpdateStatus(
        Guid id,
        UpdateRequestStatusRequest request,
        CancellationToken cancellationToken)
        => Ok(await _requests.UpdateStatusAsync(id, User.GetUserId(), request, cancellationToken));
}
