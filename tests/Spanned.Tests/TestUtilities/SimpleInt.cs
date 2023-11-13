using System.Collections;

namespace Spanned.Tests.TestUtilities;

public struct SimpleInt : IStructuralComparable, IStructuralEquatable, IComparable, IComparable<SimpleInt>
{
    public SimpleInt(int t) => Val = t;

    public int Val { get; set; }

    public readonly int CompareTo(SimpleInt other) => other.Val - Val;

    public readonly int CompareTo(object? obj)
    {
        if (obj?.GetType() == typeof(SimpleInt))
            return ((SimpleInt)obj).Val - Val;

        return -1;
    }

    public readonly int CompareTo(object? other, IComparer comparer)
    {
        if (other?.GetType() == typeof(SimpleInt))
            return ((SimpleInt)other).Val - Val;

        return -1;
    }

    public readonly bool Equals(object? other, IEqualityComparer comparer)
    {
        if (other?.GetType() == typeof(SimpleInt))
            return ((SimpleInt)other).Val == Val;

        return false;
    }

    public readonly int GetHashCode(IEqualityComparer comparer) => comparer.GetHashCode(Val);
}
