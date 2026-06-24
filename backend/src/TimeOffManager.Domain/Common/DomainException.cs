namespace TimeOffManager.Domain.Common;

/// <summary>
/// Thrown when a domain invariant or business rule is violated.
/// Mapped to HTTP 400/422 by the API exception handler.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
