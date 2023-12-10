namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Computes the sum of the values in the specified memory block.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TNumber">The type of the values.</typeparam>
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <returns>The sum of the values in the memory block.</returns>
    /// <exception cref="OverflowException">The addition operation in a checked context resulted in an overflow.</exception>
    private static T Sum<T, TNumber>(ref T searchSpace, int length)
        where T : struct
        where TNumber : struct, INumber<T>
    {
        T sum = default;
        for (int i = 0; i < length; i++)
            sum = default(TNumber).AddChecked(sum, Unsafe.Add(ref searchSpace, i));

        return sum;
    }

    /// <summary>
    /// Computes the sum of the values in the specified memory block.
    /// </summary>
    /// <remarks>
    /// This method does not account for overflow.
    /// Use with caution and ensure that values in the span cannot cause an overflow if it is not desirable.
    /// </remarks>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TNumber">The type of the values.</typeparam>
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <returns>The sum of the values in the memory block.</returns>
    private static T UnsafeSum<T, TNumber>(ref T searchSpace, int length)
        where T : struct
        where TNumber : struct, INumber<T>
    {
        T sum = default;
        for (int i = 0; i < length; i++)
            sum = default(TNumber).Add(sum, Unsafe.Add(ref searchSpace, i));

        return sum;
    }

    /// <summary>
    /// Computes the sum of a span of <see cref="byte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="byte"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    /// <exception cref="OverflowException">The addition operation in a checked context resulted in an overflow.</exception>
    public static byte Sum(this scoped Span<byte> span) => Sum<byte, ByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static byte Sum(this scoped ReadOnlySpan<byte> span) => Sum<byte, ByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="sbyte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="sbyte"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    [CLSCompliant(false)]
    public static sbyte Sum(this scoped Span<sbyte> span) => Sum<sbyte, SByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{sbyte})"/>
    [CLSCompliant(false)]
    public static sbyte Sum(this scoped ReadOnlySpan<sbyte> span) => Sum<sbyte, SByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static short Sum(this scoped Span<short> span) => Sum<short, Int16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{short})"/>
    public static short Sum(this scoped ReadOnlySpan<short> span) => Sum<short, Int16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ushort"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ushort"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ushort Sum(this scoped Span<ushort> span) => Sum<ushort, UInt16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{ushort})"/>
    [CLSCompliant(false)]
    public static ushort Sum(this scoped ReadOnlySpan<ushort> span) => Sum<ushort, UInt16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="int"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="int"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static int Sum(this scoped Span<int> span) => Sum<int, Int32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{int})"/>
    public static int Sum(this scoped ReadOnlySpan<int> span) => Sum<int, Int32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="uint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="uint"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    [CLSCompliant(false)]
    public static uint Sum(this scoped Span<uint> span) => Sum<uint, UInt32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{uint})"/>
    [CLSCompliant(false)]
    public static uint Sum(this scoped ReadOnlySpan<uint> span) => Sum<uint, UInt32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="long"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="long"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static long Sum(this scoped Span<long> span) => Sum<long, Int64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{long})"/>
    public static long Sum(this scoped ReadOnlySpan<long> span) => Sum<long, Int64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ulong"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ulong"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ulong Sum(this scoped Span<ulong> span) => Sum<ulong, UInt64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{ulong})"/>
    [CLSCompliant(false)]
    public static ulong Sum(this scoped ReadOnlySpan<ulong> span) => Sum<ulong, UInt64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="float"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="float"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the sequence.</returns>
    public static float Sum(this scoped Span<float> span) => UnsafeSum<float, SingleNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{float})"/>
    public static float Sum(this scoped ReadOnlySpan<float> span) => UnsafeSum<float, SingleNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the sequence.</returns>
    public static double Sum(this scoped Span<double> span) => UnsafeSum<double, DoubleNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{double})"/>
    public static double Sum(this scoped ReadOnlySpan<double> span) => UnsafeSum<double, DoubleNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="decimal"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="decimal"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static decimal Sum(this scoped Span<decimal> span) => UnsafeSum<decimal, DecimalNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{decimal})"/>
    public static decimal Sum(this scoped ReadOnlySpan<decimal> span) => UnsafeSum<decimal, DecimalNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="byte"/> values.
    /// </summary>
    /// <remarks>
    /// This method does not account for overflow.
    /// Use with caution and ensure that values in the span cannot cause an overflow if it is not desirable.
    /// </remarks>
    /// <param name="span">A span of <see cref="byte"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    public static byte UnsafeSum(this scoped Span<byte> span) => UnsafeSum<byte, ByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static byte UnsafeSum(this scoped ReadOnlySpan<byte> span) => UnsafeSum<byte, ByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="sbyte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="sbyte"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static sbyte UnsafeSum(this scoped Span<sbyte> span) => UnsafeSum<sbyte, SByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{sbyte})"/>
    [CLSCompliant(false)]
    public static sbyte UnsafeSum(this scoped ReadOnlySpan<sbyte> span) => UnsafeSum<sbyte, SByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static short UnsafeSum(this scoped Span<short> span) => UnsafeSum<short, Int16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{short})"/>
    public static short UnsafeSum(this scoped ReadOnlySpan<short> span) => UnsafeSum<short, Int16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ushort"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ushort"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ushort UnsafeSum(this scoped Span<ushort> span) => UnsafeSum<ushort, UInt16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{ushort})"/>
    [CLSCompliant(false)]
    public static ushort UnsafeSum(this scoped ReadOnlySpan<ushort> span) => UnsafeSum<ushort, UInt16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="int"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="int"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static int UnsafeSum(this scoped Span<int> span) => UnsafeSum<int, Int32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{int})"/>
    public static int UnsafeSum(this scoped ReadOnlySpan<int> span) => UnsafeSum<int, Int32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="uint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="uint"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static uint UnsafeSum(this scoped Span<uint> span) => UnsafeSum<uint, UInt32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{uint})"/>
    [CLSCompliant(false)]
    public static uint UnsafeSum(this scoped ReadOnlySpan<uint> span) => UnsafeSum<uint, UInt32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="long"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="long"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static long UnsafeSum(this scoped Span<long> span) => UnsafeSum<long, Int64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{long})"/>
    public static long UnsafeSum(this scoped ReadOnlySpan<long> span) => UnsafeSum<long, Int64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ulong"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ulong"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ulong UnsafeSum(this scoped Span<ulong> span) => UnsafeSum<ulong, UInt64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{ulong})"/>
    [CLSCompliant(false)]
    public static ulong UnsafeSum(this scoped ReadOnlySpan<ulong> span) => UnsafeSum<ulong, UInt64Number>(ref MemoryMarshal.GetReference(span), span.Length);
}
