namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Fills a generic span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the <paramref name="span"/>.</typeparam>
    /// <typeparam name="TNumber">The type of elements in the <paramref name="span"/>.</typeparam>
    /// <param name="span">The span to fill with sequential values.</param>
    /// <param name="offset">The starting value for the sequential filling.</param>
    /// <param name="step">The increment value for the sequential filling.</param>
    private static void FillSequential<T, TNumber>(this scoped Span<T> span, T offset, T step)
        where T : struct
        where TNumber : struct, INumber<T>
    {
        if (span.IsEmpty)
            return;

        T previous = offset;
        span[0] = previous;

        for (int i = 1; i < span.Length; i++)
        {
            previous = default(TNumber).Add(previous, step);
            span[i] = previous;
        }
    }

    /// <summary>
    /// Fills a <see cref="byte"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<byte> span) => FillSequential<byte, ByteNumber>(span, (byte)0, (byte)1);

    /// <summary>
    /// Fills a <see cref="byte"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    /// <param name="offset">The starting value for the sequential filling.</param>
    /// <param name="step">The increment value for the sequential filling.</param>
    public static void FillSequential(this scoped Span<byte> span, byte offset = 0, byte step = 1) => FillSequential<byte, ByteNumber>(span, offset, step);

    /// <summary>
    /// Fills a <see cref="sbyte"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<sbyte> span) => FillSequential<sbyte, SByteNumber>(span, (sbyte)0, (sbyte)1);

    /// <summary>
    /// Fills a <see cref="sbyte"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<sbyte> span, sbyte offset = 0, sbyte step = 1) => FillSequential<sbyte, SByteNumber>(span, offset, step);

    /// <summary>
    /// Fills a <see cref="short"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<short> span) => FillSequential<short, Int16Number>(span, (short)0, (short)1);

    /// <summary>
    /// Fills a <see cref="short"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<short> span, short offset = 0, short step = 1) => FillSequential<short, Int16Number>(span, offset, step);

    /// <summary>
    /// Fills a <see cref="ushort"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<ushort> span) => FillSequential<ushort, UInt16Number>(span, (ushort)0, (ushort)1);

    /// <summary>
    /// Fills a <see cref="ushort"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<ushort> span, ushort offset = 0, ushort step = 1) => FillSequential<ushort, UInt16Number>(span, offset, step);

    /// <summary>
    /// Fills a <see cref="int"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<int> span) => FillSequential<int, Int32Number>(span, 0, 1);

    /// <summary>
    /// Fills a <see cref="int"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<int> span, int offset = 0, int step = 1) => FillSequential<int, Int32Number>(span, offset, step);

    /// <summary>
    /// Fills a <see cref="uint"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<uint> span) => FillSequential<uint, UInt32Number>(span, 0u, 1u);

    /// <summary>
    /// Fills a <see cref="uint"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<uint> span, uint offset = 0, uint step = 1) => FillSequential<uint, UInt32Number>(span, offset, step);

    /// <summary>
    /// Fills a <see cref="long"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<long> span) => FillSequential<long, Int64Number>(span, 0L, 1L);

    /// <summary>
    /// Fills a <see cref="long"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<long> span, long offset = 0, long step = 1) => FillSequential<long, Int64Number>(span, offset, step);

    /// <summary>
    /// Fills a <see cref="ulong"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<ulong> span) => FillSequential<ulong, UInt64Number>(span, 0ul, 1ul);

    /// <summary>
    /// Fills a <see cref="ulong"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<ulong> span, ulong offset = 0, ulong step = 1) => FillSequential<ulong, UInt64Number>(span, offset, step);

    /// <summary>
    /// Fills a <see cref="float"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<float> span) => FillSequential<float, SingleNumber>(span, 0f, 1f);

    /// <summary>
    /// Fills a <see cref="float"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<float> span, float offset = 0, float step = 1) => FillSequential<float, SingleNumber>(span, offset, step);

    /// <summary>
    /// Fills a <see cref="double"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<double> span) => FillSequential<double, DoubleNumber>(span, 0d, 1d);

    /// <summary>
    /// Fills a <see cref="double"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<double> span, double offset = 0, double step = 1) => FillSequential<double, DoubleNumber>(span, offset, step);

    /// <summary>
    /// Fills a <see cref="decimal"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<decimal> span) => FillSequential<decimal, DecimalNumber>(span, 0m, 1m);

    /// <summary>
    /// Fills a <see cref="decimal"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<decimal> span, decimal offset = 0, decimal step = 1) => FillSequential<decimal, DecimalNumber>(span, offset, step);
}
