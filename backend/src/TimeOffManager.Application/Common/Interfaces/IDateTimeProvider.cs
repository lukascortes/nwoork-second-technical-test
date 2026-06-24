namespace TimeOffManager.Application.Common.Interfaces;

/// <summary>Abstracts the system clock so time-dependent rules (e.g. "no past dates")
/// are deterministically testable.</summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}
