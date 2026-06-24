using FluentAssertions;
using TimeOffManager.Domain.Common;
using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Domain.UnitTests;

public class UserTests
{
    [Fact]
    public void Create_NormalizesEmail_ToLowerTrimmed()
    {
        var user = User.Create("  John@Example.COM ", "hash", "John Doe", UserRole.Employee);

        user.Email.Should().Be("john@example.com");
        user.Role.Should().Be(UserRole.Employee);
        user.AnnualVacationDays.Should().Be(20);
        user.Id.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankEmail_Throws(string email)
    {
        var act = () => User.Create(email, "hash", "John Doe", UserRole.Employee);

        act.Should().Throw<DomainException>().WithMessage("*Email*");
    }

    [Fact]
    public void Create_WithBlankPasswordHash_Throws()
    {
        var act = () => User.Create("john@example.com", "  ", "John Doe", UserRole.Employee);

        act.Should().Throw<DomainException>().WithMessage("*Password*");
    }

    [Fact]
    public void Create_WithNegativeVacationDays_Throws()
    {
        var act = () => User.Create("john@example.com", "hash", "John Doe", UserRole.Employee, -1);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ChangeEmail_NormalizesValue()
    {
        var user = User.Create("john@example.com", "hash", "John Doe", UserRole.Employee);

        user.ChangeEmail(" NEW@Mail.com ");

        user.Email.Should().Be("new@mail.com");
    }
}
