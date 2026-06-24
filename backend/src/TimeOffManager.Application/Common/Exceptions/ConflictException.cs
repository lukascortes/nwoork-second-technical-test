namespace TimeOffManager.Application.Common.Exceptions;

/// <summary>State conflict (e.g. duplicate email). Mapped to HTTP 409.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
