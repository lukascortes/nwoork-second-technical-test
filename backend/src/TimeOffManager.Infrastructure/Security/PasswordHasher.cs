using TimeOffManager.Application.Common.Interfaces;

namespace TimeOffManager.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    // Work factor 12 ≈ 250ms/hash on commodity hardware — a deliberate brute-force brake.
    private const int WorkFactor = 12;

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Malformed stored hash → treat as a failed verification, never throw.
            return false;
        }
    }
}
