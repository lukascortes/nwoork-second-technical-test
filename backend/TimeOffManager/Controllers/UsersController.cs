using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TimeOffManager.Data;
using TimeOffManager.Models;
using TimeOffManager.Models.DTOs;
using TimeOffManager.Validators;

namespace TimeOffManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;
    public UsersController(AppDbContext context, DbContextOptions<AppDbContext> dbContextOptions)
    {
        _context = context;
        _dbContextOptions = dbContextOptions;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
    {
        return await _context.Users
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role
            })
            .ToListAsync();
    }

    // GET: api/Users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role
        };
    }

    // POST: api/Users
    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> CreateUser(UserCreateDto userDto)
    {
        var user = new User
        {
            Email = userDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
            Role = userDto.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role
        });
    }

    // PUT: api/Users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UserUpdateDto userDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        if (userDto.Email != null)
        {
            user.Email = userDto.Email;
        }

        if (userDto.Password != null)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
        }

        if (userDto.Role != null)
        {
            user.Role = userDto.Role.Value;
        }

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}