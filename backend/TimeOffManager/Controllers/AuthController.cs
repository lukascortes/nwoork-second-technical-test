using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeOffManager.Data;
using TimeOffManager.Models.Auth;
using TimeOffManager.Services;

namespace TimeOffManager.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwt;

    public AuthController(AppDbContext context, JwtService jwt)
    {
        _context = context;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);


        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var token = _jwt.GenerateToken(user);
        return Ok(new
        {
            token = token,
            role = user.Role.ToString(),
            userId = user.Id,
        });

    }
}
