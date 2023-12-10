namespace System.Numerics;

internal readonly struct Int32Number : INumber<int>
{
    public int MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => int.MinValue;
    }

    public int MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => int.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Add(int left, int right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int AddChecked(int left, int right) => checked(left + right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNegative(int value) => value < 0;
}
