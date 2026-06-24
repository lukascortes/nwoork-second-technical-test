using FluentValidation;
using TimeOffManager.Application.Common.Exceptions;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Domain.Entities;

namespace TimeOffManager.Application.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;

    public UserService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _users.GetAllAsync(cancellationToken);
        return users.Select(UserDto.FromEntity).ToList();
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), id);
        return UserDto.FromEntity(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = User.NormalizeEmail(request.Email);
        if (await _users.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException("An account with this email already exists.");

        var user = User.Create(
            email,
            _passwordHasher.Hash(request.Password),
            request.FullName,
            request.Role,
            request.AnnualVacationDays);

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.FromEntity(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var user = await _users.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), id);

        if (request.Email is not null)
        {
            var newEmail = User.NormalizeEmail(request.Email);
            if (newEmail != user.Email && await _users.EmailExistsAsync(newEmail, cancellationToken))
                throw new ConflictException("An account with this email already exists.");
            user.ChangeEmail(newEmail);
        }

        if (request.FullName is not null)
            user.Rename(request.FullName);

        if (request.Role is not null)
            user.ChangeRole(request.Role.Value);

        if (request.AnnualVacationDays is not null)
            user.SetAnnualVacationDays(request.AnnualVacationDays.Value);

        if (request.Password is not null)
            user.ChangePassword(_passwordHasher.Hash(request.Password));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UserDto.FromEntity(user);
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        if (id == currentUserId)
            throw new ConflictException("You cannot delete your own account.");

        var user = await _users.GetByIdAsync(id, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), id);

        _users.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
