namespace TimeOffManager.Application.Common.Exceptions;

/// <summary>Authenticated but not allowed to perform the action. Mapped to HTTP 403.</summary>
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "You are not allowed to perform this action.")
        : base(message)
    {
    }
}
