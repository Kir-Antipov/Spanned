namespace System.Numerics;

internal readonly struct UInt16Number : INumber<ushort>
{
    public ushort MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ushort.MinValue;
    }

    public ushort MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ushort.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort Add(ushort left, ushort right) => (ushort)(left + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort AddChecked(ushort left, ushort right) => checked((ushort)(left + right));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNegative(ushort value) => false;
}
