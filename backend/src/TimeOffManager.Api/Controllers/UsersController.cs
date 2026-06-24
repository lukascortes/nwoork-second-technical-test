using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeOffManager.Api.Common;
using TimeOffManager.Application.Users;

namespace TimeOffManager.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")] // user management is admin-only (closes the anonymous-CRUD hole)
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _users.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _users.GetByIdAsync(id, cancellationToken));

    [HttpGet("{id:guid}/vacation-balance")]
    public async Task<ActionResult<VacationBalanceDto>> GetVacationBalance(Guid id, CancellationToken cancellationToken)
        => Ok(await _users.GetVacationBalanceAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _users.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken)
        => Ok(await _users.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _users.DeleteAsync(id, User.GetUserId(), cancellationToken);
        return NoContent();
    }
}
