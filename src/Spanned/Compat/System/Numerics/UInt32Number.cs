namespace System.Numerics;

internal readonly struct UInt32Number : INumber<uint>
{
    public uint MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => uint.MinValue;
    }

    public uint MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => uint.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Add(uint left, uint right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint AddChecked(uint left, uint right) => checked(left + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNegative(uint value) => false;
}
