namespace System.Numerics;

internal readonly struct DoubleNumber : INumber<double>
{
    public double MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => double.MinValue;
    }

    public double MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => double.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Add(double left, double right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double AddChecked(double left, double right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNegative(double value) => value < 0d;
}
