namespace TimeOffManager.DTOs;

public class TimeOffRequestDto
{
    public int Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public string? Type { get; set; } // string in place of enum
    public string? Status { get; set; } // string in place of enum
    public DateTime CreatedAt { get; set; }
    public UserBasicDto? User { get; set; } // Basic user information only
}