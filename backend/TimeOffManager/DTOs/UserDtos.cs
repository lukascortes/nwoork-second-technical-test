namespace TimeOffManager.Models.DTOs;

public class UserCreateDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Role Role { get; set; }
}

public class UserUpdateDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public Role? Role { get; set; }
}

public class UserResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
}