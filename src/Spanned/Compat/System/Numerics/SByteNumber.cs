namespace System.Numerics;

internal readonly struct SByteNumber : INumber<sbyte>
{
    public sbyte MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => sbyte.MinValue;
    }

    public sbyte MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => sbyte.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte Add(sbyte left, sbyte right) => (sbyte)(left + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte AddChecked(sbyte left, sbyte right) => checked((sbyte)(left + right));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNegative(sbyte value) => value < (sbyte)0;
}
