namespace TimeOffManager.Models;

public enum Role { Admin, Employee }

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; }

    public ICollection<TimeOffRequest>? Requests { get; set; }
}