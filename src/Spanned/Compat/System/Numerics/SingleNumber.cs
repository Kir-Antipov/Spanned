namespace System.Numerics;

internal readonly struct SingleNumber : INumber<float>
{
    public float MinValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => float.MinValue;
    }

    public float MaxValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => float.MaxValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Add(float left, float right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float AddChecked(float left, float right) => left + right;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNegative(float value) => value < 0f;
}
