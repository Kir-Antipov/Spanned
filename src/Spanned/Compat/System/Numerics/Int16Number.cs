namespace System.Numerics;

internal readonly struct Int16Number : INumber<short>
{
    public short MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => short.MinValue;
    }

    public short MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => short.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short Add(short left, short right) => (short)(left + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short AddChecked(short left, short right) => checked((short)(left + right));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNegative(short value) => value < (short)0;
}
