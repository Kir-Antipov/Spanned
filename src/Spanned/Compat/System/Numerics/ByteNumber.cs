namespace System.Numerics;

internal readonly struct ByteNumber : INumber<byte>
{
    public byte MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => byte.MinValue;
    }

    public byte MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => byte.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Add(byte left, byte right) => (byte)(left + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte AddChecked(byte left, byte right) => checked((byte)(left + right));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNegative(byte value) => false;
}
