using TimeOffManager.Models;
using TimeOffManager.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using TimeOffManager.Data;


namespace TimeOffManager.Services;

public class UserService
{
    private readonly AppDbContext _context;
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public UserService(AppDbContext context, DbContextOptions<AppDbContext> dbContextOptions)
    {
        _context = context;
        _dbContextOptions = dbContextOptions;
    }

    public async Task<UserResponseDto> CreateUser(UserCreateDto userDto)
    {
        var user = new User
        {
            Email = userDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
            Role = userDto.Role
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role
        };
    }

  
}