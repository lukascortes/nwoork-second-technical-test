namespace TimeOffManager.Models;

public enum LeaveType { Vacation, Sick, Other }
public enum RequestStatus { Pending, Approved, Rejected }

public class TimeOffRequest {
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public LeaveType Type { get; set; }
    public string? Reason { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
