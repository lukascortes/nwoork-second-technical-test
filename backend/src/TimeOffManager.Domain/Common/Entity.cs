namespace TimeOffManager.Domain.Common;

/// <summary>
/// Base class for domain entities. Identity is a non-sequential <see cref="Guid"/>
/// (mitigates IDOR/enumeration on public endpoints). Equality is identity-based.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; }

    public override bool Equals(object? obj)
        => obj is Entity other
           && other.GetType() == GetType()
           && Id != default
           && other.Id == Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
