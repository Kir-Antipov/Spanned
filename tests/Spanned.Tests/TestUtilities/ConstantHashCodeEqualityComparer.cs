namespace Spanned.Tests.TestUtilities;

public sealed class ConstantHashCodeEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly IEqualityComparer<T> _comparer;

    public ConstantHashCodeEqualityComparer() => _comparer = EqualityComparer<T>.Default;

    public ConstantHashCodeEqualityComparer(IEqualityComparer<T> comparer) => _comparer = comparer;

    public bool Equals(T? x, T? y) => _comparer.Equals(x, y);

    public int GetHashCode(T obj) => 42;
}
