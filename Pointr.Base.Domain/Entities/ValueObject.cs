using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pointr.Base.Domain.Entities;

public abstract class ValueObject
{
    /// <summary>
    /// Components that define structural equality for this value object.
    /// Order matters; two instances are equal if these sequences are equal.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        // Null or different runtime type → not equal
        if (obj is null || obj.GetType() != GetType())
            return false;

        // Structural equality: compare component sequences
        return GetEqualityComponents().SequenceEqual(((ValueObject)obj).GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        // Fold component hash codes into a single hash
        return GetEqualityComponents()
            .Aggregate(
                0,
                (currentHash, component) =>
                {
                    var componentHash = component?.GetHashCode() ?? 0;
                    return HashCode.Combine(currentHash, componentHash);
                }
            );
    }
}
