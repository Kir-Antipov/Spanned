namespace Spanned.Tests.TestUtilities;

public sealed class EquatableBackwardsOrder : IEquatable<EquatableBackwardsOrder>, IComparable<EquatableBackwardsOrder>, IComparable
{
    private readonly int _value;

    public EquatableBackwardsOrder(int value) => _value = value;

    public int CompareTo(EquatableBackwardsOrder? other) => other is null ? 1 : other._value - _value;

    public override int GetHashCode() => _value;

    public override bool Equals(object? obj) => obj is EquatableBackwardsOrder other && Equals(other);

    public bool Equals(EquatableBackwardsOrder? other) => other is not null && _value == other._value;

    int IComparable.CompareTo(object? obj)
    {
        if (obj?.GetType() == typeof(EquatableBackwardsOrder))
            return ((EquatableBackwardsOrder)obj)._value - _value;

        return -1;
    }
}
