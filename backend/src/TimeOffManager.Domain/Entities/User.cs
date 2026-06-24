using TimeOffManager.Domain.Common;
using TimeOffManager.Domain.Enums;

namespace TimeOffManager.Domain.Entities;

/// <summary>
/// A system user (employee or administrator). Email is always stored normalized
/// (trimmed + lower-case) so authentication lookups are deterministic.
/// </summary>
public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public int AnnualVacationDays { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<TimeOffRequest> _requests = new();
    public IReadOnlyCollection<TimeOffRequest> Requests => _requests.AsReadOnly();

    private User() { } // EF Core

    private User(
        Guid id,
        string email,
        string passwordHash,
        string fullName,
        UserRole role,
        int annualVacationDays,
        DateTime createdAt)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        FullName = fullName;
        Role = role;
        AnnualVacationDays = annualVacationDays;
        CreatedAt = createdAt;
    }

    public static User Create(
        string email,
        string passwordHash,
        string fullName,
        UserRole role,
        int annualVacationDays = 20)
    {
        email = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");
        if (annualVacationDays < 0)
            throw new DomainException("Annual vacation days cannot be negative.");

        return new User(
            Guid.NewGuid(),
            email,
            passwordHash,
            fullName.Trim(),
            role,
            annualVacationDays,
            DateTime.UtcNow);
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash is required.");
        PasswordHash = newPasswordHash;
    }

    public void ChangeEmail(string email)
    {
        email = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        Email = email;
    }

    public void Rename(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");
        FullName = fullName.Trim();
    }

    public void ChangeRole(UserRole role) => Role = role;

    public void SetAnnualVacationDays(int days)
    {
        if (days < 0)
            throw new DomainException("Annual vacation days cannot be negative.");
        AnnualVacationDays = days;
    }

    public static string NormalizeEmail(string email)
        => (email ?? string.Empty).Trim().ToLowerInvariant();
}
