namespace TimeOffManager.Application.Users;

public sealed record VacationBalanceDto(
    int AnnualAllowance,
    int UsedDays,
    int PendingDays,
    int RemainingDays,
    int ProjectedRemainingDays);
