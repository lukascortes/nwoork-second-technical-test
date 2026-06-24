using FluentAssertions;
using NSubstitute;
using TimeOffManager.Application.Common.Exceptions;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Application.Users;
using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Application.UnitTests.Users;

public class UserServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private UserService CreateSut() => new(
        _users, _hasher, _uow,
        new CreateUserRequestValidator(),
        new UpdateUserRequestValidator());

    public UserServiceTests() => _hasher.Hash(Arg.Any<string>()).Returns("hashed");

    private static CreateUserRequest ValidCreate(string email = "new@corp.com") =>
        new(email, "Passw0rd1", "New User", UserRole.Employee);

    [Fact]
    public async Task Create_WithExistingEmail_ThrowsConflict()
    {
        _users.EmailExistsAsync("new@corp.com").Returns(true);

        var act = () => CreateSut().CreateAsync(ValidCreate());

        await act.Should().ThrowAsync<ConflictException>();
        await _users.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task Create_Valid_HashesPasswordAndPersists()
    {
        _users.EmailExistsAsync(Arg.Any<string>()).Returns(false);

        var result = await CreateSut().CreateAsync(ValidCreate());

        result.Email.Should().Be("new@corp.com");
        result.Role.Should().Be(UserRole.Employee);
        await _users.Received(1).AddAsync(Arg.Is<User>(u => u.PasswordHash == "hashed"));
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Delete_OwnAccount_ThrowsConflict()
    {
        var id = Guid.NewGuid();

        var act = () => CreateSut().DeleteAsync(id, id);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*your own*");
        _users.DidNotReceive().Remove(Arg.Any<User>());
    }

    [Fact]
    public async Task Delete_WhenNotFound_ThrowsNotFound()
    {
        _users.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

        var act = () => CreateSut().DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_Valid_RemovesAndSaves()
    {
        var user = User.Create("victim@corp.com", "h", "Victim", UserRole.Employee);
        _users.GetByIdAsync(user.Id).Returns(user);

        await CreateSut().DeleteAsync(user.Id, Guid.NewGuid());

        _users.Received(1).Remove(user);
        await _uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Update_ChangingEmailToOneTaken_ThrowsConflict()
    {
        var user = User.Create("old@corp.com", "h", "User", UserRole.Employee);
        _users.GetByIdAsync(user.Id).Returns(user);
        _users.EmailExistsAsync("taken@corp.com").Returns(true);

        var act = () => CreateSut().UpdateAsync(user.Id,
            new UpdateUserRequest("taken@corp.com", null, null, null, null));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Update_PartialFullName_AppliesAndSaves()
    {
        var user = User.Create("u@corp.com", "h", "Old Name", UserRole.Employee);
        _users.GetByIdAsync(user.Id).Returns(user);

        var result = await CreateSut().UpdateAsync(user.Id,
            new UpdateUserRequest(null, null, "New Name", null, null));

        result.FullName.Should().Be("New Name");
        await _uow.Received(1).SaveChangesAsync();
    }
}
