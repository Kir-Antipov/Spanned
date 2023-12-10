namespace System.Numerics;

internal readonly struct UInt64Number : INumber<ulong>
{
    public ulong MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ulong.MinValue;
    }

    public ulong MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ulong.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Add(ulong left, ulong right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong AddChecked(ulong left, ulong right) => checked(left + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNegative(ulong value) => false;
}
