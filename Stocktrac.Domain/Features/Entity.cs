using System.Reflection.Metadata.Ecma335;

namespace Stocktrac.Domain.Features;

public abstract class Entity
{
    public virtual long Id { get; protected set; }
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == 0 || other.Id == 0)
            return false;

        return Id == other.Id;
    }

    public static bool operator ==(Entity? a, Entity? b) =>
        a is null
            ? b is null
            : b is not null && a.Equals(b);


    public static bool operator !=(Entity a, Entity b) =>
        !(a == b);

    public override int GetHashCode() =>
        HashCode.Combine(GetType(), Id);
}
