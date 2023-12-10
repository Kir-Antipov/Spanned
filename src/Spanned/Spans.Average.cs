namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Computes the average of a span of <see cref="byte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="byte"/> values to calculate the average of.</param>
    /// <returns>The average of the span of values.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="span"/> contains no elements.</exception>
    public static double Average(this scoped Span<byte> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="Average(Span{byte})"/>
    public static double Average(this scoped ReadOnlySpan<byte> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="sbyte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="sbyte"/> values to calculate the average of.</param>
    /// <inheritdoc cref="Average(Span{byte})"/>
    [CLSCompliant(false)]
    public static double Average(this scoped Span<sbyte> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="Average(Span{sbyte})"/>
    [CLSCompliant(false)]
    public static double Average(this scoped ReadOnlySpan<sbyte> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to calculate the average of.</param>
    /// <inheritdoc cref="Average(Span{byte})"/>
    public static double Average(this scoped Span<short> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="Average(Span{short})"/>
    public static double Average(this scoped ReadOnlySpan<short> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="ushort"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ushort"/> values to calculate the average of.</param>
    /// <inheritdoc cref="Average(Span{byte})"/>
    [CLSCompliant(false)]
    public static double Average(this scoped Span<ushort> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="Average(Span{ushort})"/>
    [CLSCompliant(false)]
    public static double Average(this scoped ReadOnlySpan<ushort> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="int"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="int"/> values to calculate the average of.</param>
    /// <inheritdoc cref="Average(Span{byte})"/>
    public static double Average(this scoped Span<int> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="Average(Span{int})"/>
    public static double Average(this scoped ReadOnlySpan<int> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="uint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="uint"/> values to calculate the average of.</param>
    /// <inheritdoc cref="Average(Span{byte})"/>
    [CLSCompliant(false)]
    public static double Average(this scoped Span<uint> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="Average(Span{uint})"/>
    [CLSCompliant(false)]
    public static double Average(this scoped ReadOnlySpan<uint> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="long"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="long"/> values to calculate the average of.</param>
    /// <returns>The average of the span of values.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="span"/> contains no elements.</exception>
    /// <exception cref="OverflowException">The addition operation in a checked context resulted in an overflow.</exception>
    public static double Average(this scoped Span<long> span)
    {
        if (!span.IsEmpty)
            return span.Sum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="Average(Span{long})"/>
    public static double Average(this scoped ReadOnlySpan<long> span)
    {
        if (!span.IsEmpty)
            return span.Sum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="ulong"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ulong"/> values to calculate the average of.</param>
    /// <inheritdoc cref="Average(Span{long})"/>
    [CLSCompliant(false)]
    public static double Average(this scoped Span<ulong> span)
    {
        if (!span.IsEmpty)
            return span.Sum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="Average(Span{ulong})"/>
    [CLSCompliant(false)]
    public static double Average(this scoped ReadOnlySpan<ulong> span)
    {
        if (!span.IsEmpty)
            return span.Sum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="float"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="float"/> values to calculate the average of.</param>
    /// <inheritdoc cref="Average(Span{byte})"/>
    public static double Average(this scoped Span<float> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="Average(Span{float})"/>
    public static double Average(this scoped ReadOnlySpan<float> span)
    {
        if (!span.IsEmpty)
            return span.LongSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to calculate the average of.</param>
    /// <inheritdoc cref="Average(Span{byte})"/>
    public static double Average(this scoped Span<double> span)
    {
        if (!span.IsEmpty)
            return span.Sum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="Average(Span{double})"/>
    public static double Average(this scoped ReadOnlySpan<double> span)
    {
        if (!span.IsEmpty)
            return span.Sum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="decimal"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="decimal"/> values to calculate the average of.</param>
    /// <inheritdoc cref="Average(Span{long})"/>
    public static decimal Average(this scoped Span<decimal> span)
    {
        if (!span.IsEmpty)
            return span.Sum() / (decimal)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="Average(Span{decimal})"/>
    public static decimal Average(this scoped ReadOnlySpan<decimal> span)
    {
        if (!span.IsEmpty)
            return span.Sum() / (decimal)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="byte"/> values.
    /// </summary>
    /// <remarks>
    /// This method does not account for overflow.
    /// Use with caution and ensure that values in the span cannot cause an overflow if it is not desirable.
    /// </remarks>
    /// <param name="span">A span of <see cref="byte"/> values to calculate the average of.</param>
    /// <returns>The average of the span of values.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="span"/> contains no elements.</exception>
    public static double UnsafeAverage(this scoped Span<byte> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="UnsafeAverage(Span{byte})"/>
    public static double UnsafeAverage(this scoped ReadOnlySpan<byte> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="sbyte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="sbyte"/> values to calculate the average of.</param>
    /// <inheritdoc cref="UnsafeAverage(Span{byte})"/>
    [CLSCompliant(false)]
    public static double UnsafeAverage(this scoped Span<sbyte> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="UnsafeAverage(Span{sbyte})"/>
    [CLSCompliant(false)]
    public static double UnsafeAverage(this scoped ReadOnlySpan<sbyte> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to calculate the average of.</param>
    /// <inheritdoc cref="UnsafeAverage(Span{byte})"/>
    public static double UnsafeAverage(this scoped Span<short> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="UnsafeAverage(Span{short})"/>
    public static double UnsafeAverage(this scoped ReadOnlySpan<short> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="ushort"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ushort"/> values to calculate the average of.</param>
    /// <inheritdoc cref="UnsafeAverage(Span{byte})"/>
    [CLSCompliant(false)]
    public static double UnsafeAverage(this scoped Span<ushort> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="UnsafeAverage(Span{ushort})"/>
    [CLSCompliant(false)]
    public static double UnsafeAverage(this scoped ReadOnlySpan<ushort> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="int"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="int"/> values to calculate the average of.</param>
    /// <inheritdoc cref="UnsafeAverage(Span{byte})"/>
    public static double UnsafeAverage(this scoped Span<int> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="UnsafeAverage(Span{int})"/>
    public static double UnsafeAverage(this scoped ReadOnlySpan<int> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="uint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="uint"/> values to calculate the average of.</param>
    /// <inheritdoc cref="UnsafeAverage(Span{byte})"/>
    [CLSCompliant(false)]
    public static double UnsafeAverage(this scoped Span<uint> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="UnsafeAverage(Span{uint})"/>
    [CLSCompliant(false)]
    public static double UnsafeAverage(this scoped ReadOnlySpan<uint> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="long"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="long"/> values to calculate the average of.</param>
    /// <inheritdoc cref="UnsafeAverage(Span{byte})"/>
    public static double UnsafeAverage(this scoped Span<long> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="UnsafeAverage(Span{long})"/>
    public static double UnsafeAverage(this scoped ReadOnlySpan<long> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <summary>
    /// Computes the average of a span of <see cref="ulong"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ulong"/> values to calculate the average of.</param>
    /// <inheritdoc cref="UnsafeAverage(Span{byte})"/>
    [CLSCompliant(false)]
    public static double UnsafeAverage(this scoped Span<ulong> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }

    /// <inheritdoc cref="UnsafeAverage(Span{ulong})"/>
    [CLSCompliant(false)]
    public static double UnsafeAverage(this scoped ReadOnlySpan<ulong> span)
    {
        if (!span.IsEmpty)
            return span.UnsafeSum() / (double)span.Length;

        ThrowHelper.ThrowInvalidOperationException_NoElements();
        return 0;
    }
}
