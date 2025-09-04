using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.Base.Domain.Entities;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    /// <summary>
    /// Stable identity of the entity. Two entities are equal iff they share the same runtime type and Id.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        // Delegate to typed equality for consistency
        return Equals(obj as Entity<TId>);
    }

    public bool Equals(Entity<TId>? other)
    {
        // Same reference → equal
        if (ReferenceEquals(this, other))
            return true;

        // Null → not equal
        if (other is null)
            return false;

        // Different runtime type → not equal (prevents cross-type equality on same Id)
        if (GetType() != other.GetType())
            return false;

        // Compare by Id
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
    {
        // Include runtime type to avoid collisions across different entity types with same Id
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null)
            return right is null;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }
}
