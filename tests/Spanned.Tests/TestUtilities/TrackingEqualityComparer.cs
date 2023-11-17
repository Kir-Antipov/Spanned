using System.Diagnostics.CodeAnalysis;

namespace Spanned.Tests.TestUtilities;

public sealed class TrackingEqualityComparer<T> : IEqualityComparer<T>
{
    public int EqualsCalls;
    public int GetHashCodeCalls;

    public bool Equals(T? x, T? y)
    {
        EqualsCalls++;
        return EqualityComparer<T>.Default.Equals(x, y);
    }

    public int GetHashCode([DisallowNull] T obj)
    {
        GetHashCodeCalls++;
        return EqualityComparer<T>.Default.GetHashCode(obj);
    }
}
