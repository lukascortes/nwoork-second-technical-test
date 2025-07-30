namespace TimeOffManager.DTOs;

public class UserBasicDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // string instead of enum
}