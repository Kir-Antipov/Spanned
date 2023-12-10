namespace System.Numerics;

internal readonly struct Int64Number : INumber<long>
{
    public long MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => long.MinValue;
    }

    public long MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => long.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long Add(long left, long right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long AddChecked(long left, long right) => checked(left + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNegative(long value) => value < 0L;
}
