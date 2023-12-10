namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// A vectorized solution to compute the sum of the values in the specified memory block.
    /// </summary>
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <returns>The sum of the values in the memory block.</returns>
    private static long LongSum(ref int searchSpace, int length)
    {
        long sum = 0;
        ref int current = ref searchSpace;
        ref int end = ref Unsafe.Add(ref current, length);

        if (Vector.IsHardwareAccelerated && length >= Vector<int>.Count)
        {
            Vector<long> sums = Vector<long>.Zero;
            ref int lastVectorStart = ref Unsafe.Add(ref current, length - Vector<int>.Count);

            do
            {
                Vector.Widen(new(MemoryMarshal.CreateSpan(ref current, Vector<int>.Count)), out Vector<long> currentA, out Vector<long> currentB);
                sums += currentA + currentB;
                current = ref Unsafe.Add(ref current, (nint)Vector<int>.Count);
            }
            while (!Unsafe.IsAddressGreaterThan(ref current, ref lastVectorStart));

            for (int i = 0; i < Vector<long>.Count; i++)
                sum += sums[i];
        }

        while (Unsafe.IsAddressLessThan(ref current, ref end))
        {
            sum += current;
            current = ref Unsafe.Add(ref current, (nint)1);
        }

        return sum;
    }

    /// <summary>
    /// Computes the sum of a span of <see cref="byte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="byte"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped Span<byte> span)
    {
        ulong sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <inheritdoc cref="LongSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped ReadOnlySpan<byte> span)
    {
        ulong sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <summary>
    /// Computes the sum of a span of <see cref="sbyte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="sbyte"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    [CLSCompliant(false)]
    public static long LongSum(this scoped Span<sbyte> span)
    {
        long sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <inheritdoc cref="LongSum(Span{sbyte})"/>
    [CLSCompliant(false)]
    public static long LongSum(this scoped ReadOnlySpan<sbyte> span)
    {
        long sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <summary>
    /// Computes the sum of a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    public static long LongSum(this scoped Span<short> span)
    {
        long sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <inheritdoc cref="LongSum(Span{short})"/>
    public static long LongSum(this scoped ReadOnlySpan<short> span)
    {
        long sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <summary>
    /// Computes the sum of a span of <see cref="ushort"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ushort"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped Span<ushort> span)
    {
        ulong sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <inheritdoc cref="LongSum(Span{ushort})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped ReadOnlySpan<ushort> span)
    {
        ulong sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <summary>
    /// Computes the sum of a span of <see cref="int"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="int"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    public static long LongSum(this scoped Span<int> span) => LongSum(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{int})"/>
    public static long LongSum(this scoped ReadOnlySpan<int> span) => LongSum(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="uint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="uint"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped Span<uint> span)
    {
        ulong sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <inheritdoc cref="LongSum(Span{uint})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped ReadOnlySpan<uint> span)
    {
        ulong sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <summary>
    /// Computes the sum of a span of <see cref="long"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="long"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    /// <exception cref="OverflowException">The addition operation in a checked context resulted in an overflow.</exception>
    public static long LongSum(this scoped Span<long> span) => Sum<long, SignedIntegerOverflowTracker<long, Int64Number>, Int64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{long})"/>
    public static long LongSum(this scoped ReadOnlySpan<long> span) => Sum<long, SignedIntegerOverflowTracker<long, Int64Number>, Int64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ulong"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ulong"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="LongSum(Span{long})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped Span<ulong> span) => Sum<ulong, UInt64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{ulong})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped ReadOnlySpan<ulong> span) => Sum<ulong, UInt64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="float"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="float"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    public static double LongSum(this scoped Span<float> span)
    {
        double sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <inheritdoc cref="LongSum(Span{float})"/>
    public static double LongSum(this scoped ReadOnlySpan<float> span)
    {
        double sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];

        return sum;
    }

    /// <summary>
    /// Computes the sum of a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    public static double LongSum(this scoped Span<double> span) => Sum<double, NullOverflowTracker<double>, DoubleNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{double})"/>
    public static double LongSum(this scoped ReadOnlySpan<double> span) => Sum<double, NullOverflowTracker<double>, DoubleNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="decimal"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="decimal"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="LongSum(Span{long})"/>
    public static decimal LongSum(this scoped Span<decimal> span) => Sum<decimal, DecimalNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{decimal})"/>
    public static decimal LongSum(this scoped ReadOnlySpan<decimal> span) => Sum<decimal, DecimalNumber>(ref MemoryMarshal.GetReference(span), span.Length);
}
