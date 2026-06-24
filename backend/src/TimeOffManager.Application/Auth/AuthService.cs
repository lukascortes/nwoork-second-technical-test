using FluentValidation;
using TimeOffManager.Application.Common.Exceptions;
using TimeOffManager.Application.Common.Interfaces;
using TimeOffManager.Domain.Entities;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Application.Auth;

public sealed class AuthService : IAuthService
{
    // A valid BCrypt hash, used to equalize timing when the email does not exist
    // (mitigates user-enumeration via response time). It never matches any password.
    private const string DummyHash = "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        IUnitOfWork unitOfWork,
        IValidator<LoginRequest> loginValidator,
        IValidator<RegisterRequest> registerValidator)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _unitOfWork = unitOfWork;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = User.NormalizeEmail(request.Email);
        var user = await _users.GetByEmailAsync(email, cancellationToken);

        // Always run a verification (against a dummy hash if needed) so the response
        // time does not reveal whether the account exists.
        var passwordOk = _passwordHasher.Verify(request.Password, user?.PasswordHash ?? DummyHash);
        if (user is null || !passwordOk)
            throw new InvalidCredentialsException();

        return BuildResponse(user);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = User.NormalizeEmail(request.Email);
        if (await _users.EmailExistsAsync(email, cancellationToken))
            throw new ConflictException("An account with this email already exists.");

        var user = User.Create(
            email,
            _passwordHasher.Hash(request.Password),
            request.FullName,
            UserRole.Employee); // self-registration is always an employee

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var token = _tokenGenerator.GenerateToken(user);
        return new AuthResponse(
            token.AccessToken,
            token.ExpiresAtUtc,
            user.Id,
            user.Email,
            user.FullName,
            user.Role);
    }
}
