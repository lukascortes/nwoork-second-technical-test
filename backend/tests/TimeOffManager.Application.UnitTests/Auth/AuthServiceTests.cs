using FluentAssertions;
using NSubstitute;
using TimeOffManager.Application.Auth;
using TimeOffManager.Application.Common.Exceptions;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Application.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokens = Substitute.For<IJwtTokenGenerator>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private AuthService CreateSut() => new(
        _users, _hasher, _tokens, _uow,
        new LoginRequestValidator(),
        new RegisterRequestValidator());

    public AuthServiceTests()
    {
        _tokens.GenerateToken(Arg.Any<User>())
            .Returns(new TokenResult("jwt-token", DateTime.UtcNow.AddHours(1)));
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var user = User.Create("jane@corp.com", "stored-hash", "Jane", UserRole.Employee);
        _users.GetByEmailAsync("jane@corp.com").Returns(user);
        _hasher.Verify("Secret123", "stored-hash").Returns(true);

        var result = await CreateSut().LoginAsync(new LoginRequest("Jane@Corp.com", "Secret123"));

        result.AccessToken.Should().Be("jwt-token");
        result.Role.Should().Be(UserRole.Employee);
        result.Email.Should().Be("jane@corp.com");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsInvalidCredentials()
    {
        var user = User.Create("jane@corp.com", "stored-hash", "Jane", UserRole.Employee);
        _users.GetByEmailAsync("jane@corp.com").Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var act = () => CreateSut().LoginAsync(new LoginRequest("jane@corp.com", "wrong"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ThrowsInvalidCredentials_AndStillVerifies()
    {
        _users.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var act = () => CreateSut().LoginAsync(new LoginRequest("ghost@corp.com", "whatever1"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
        // Verifies against the dummy hash to keep timing constant (anti-enumeration).
        _hasher.Received(1).Verify("whatever1", Arg.Any<string>());
    }

    [Fact]
    public async Task Register_WithExistingEmail_ThrowsConflict()
    {
        _users.EmailExistsAsync("taken@corp.com").Returns(true);

        var act = () => CreateSut().RegisterAsync(new RegisterRequest("taken@corp.com", "Secret123", "Tay"));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Register_CreatesEmployee_AndPersists()
    {
        _users.EmailExistsAsync(Arg.Any<string>()).Returns(false);
        _hasher.Hash("Secret123").Returns("hashed");

        var result = await CreateSut().RegisterAsync(new RegisterRequest("new@corp.com", "Secret123", "New User"));

        result.Role.Should().Be(UserRole.Employee);
        await _users.Received(1).AddAsync(Arg.Is<User>(u => u.Email == "new@corp.com" && u.Role == UserRole.Employee));
        await _uow.Received(1).SaveChangesAsync();
    }
}
