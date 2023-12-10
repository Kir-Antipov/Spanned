namespace System.Numerics;

internal readonly struct DecimalNumber : INumber<decimal>
{
    public decimal MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => decimal.MinValue;
    }

    public decimal MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => decimal.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public decimal Add(decimal left, decimal right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public decimal AddChecked(decimal left, decimal right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNegative(decimal value) => value < 0m;
}
