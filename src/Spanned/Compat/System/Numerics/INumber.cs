namespace System.Numerics;

internal interface INumber<T> where T : struct
{
    T MinValue { get; }

    T MaxValue { get; }

    bool IsNegative(T value);

    T Add(T left, T right);

    T AddChecked(T left, T right);
}
