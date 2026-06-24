using FluentAssertions;
using TimeOffManager.Domain.ValueObjects;

namespace TimeOffManager.Domain.UnitTests;

public class VacationBalanceTests
{
    [Fact]
    public void Computes_Remaining_And_Projected()
    {
        var balance = new VacationBalance(AnnualAllowance: 20, UsedDays: 8, PendingDays: 5);

        balance.RemainingDays.Should().Be(12);
        balance.ProjectedRemainingDays.Should().Be(7);
    }

    [Theory]
    [InlineData(20, 15, 5, true)]   // 15 + 5 = 20, exactly fits
    [InlineData(20, 15, 6, false)]  // 15 + 6 = 21, exceeds
    [InlineData(20, 0, 20, true)]   // empty balance, full request
    public void CanAccommodate_RespectsAllowance(int allowance, int used, int additional, bool expected)
    {
        var balance = new VacationBalance(allowance, used, PendingDays: 0);

        balance.CanAccommodate(additional).Should().Be(expected);
    }
}
