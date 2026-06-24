namespace TimeOffManager.Application.Common.Exceptions;

/// <summary>Login failed. Mapped to HTTP 401. Message is intentionally generic
/// so it does not reveal whether the email exists.</summary>
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid email or password.")
    {
    }
}
