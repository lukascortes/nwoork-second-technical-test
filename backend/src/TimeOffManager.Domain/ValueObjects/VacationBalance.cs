namespace TimeOffManager.Domain.ValueObjects;

/// <summary>
/// An employee's vacation-day balance for the year. Pure calculation over the
/// annual allowance, days already taken (approved) and days awaiting approval.
/// </summary>
public sealed record VacationBalance(int AnnualAllowance, int UsedDays, int PendingDays)
{
    /// <summary>Days left after subtracting approved vacation.</summary>
    public int RemainingDays => AnnualAllowance - UsedDays;

    /// <summary>Days left if every pending request were also approved.</summary>
    public int ProjectedRemainingDays => AnnualAllowance - UsedDays - PendingDays;

    /// <summary>True if approving <paramref name="additionalDays"/> stays within the allowance.</summary>
    public bool CanAccommodate(int additionalDays) => UsedDays + additionalDays <= AnnualAllowance;
}
