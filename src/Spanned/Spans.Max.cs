namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Returns the maximum value in a memory block.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="searchSpace">The reference to the start of the search space.</param>
    /// <param name="length">The length of the search space.</param>
    /// <param name="comparer">The <see cref="IComparer{T}"/> to compare values.</param>
    /// <returns>The maximum value in the memory block.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="searchSpace"/> contains no elements.</exception>
    /// <exception cref="ArgumentException">
    /// No object in <paramref name="searchSpace"/> implements the <see cref="IComparable"/> or
    /// <see cref="IComparable{T}"/> interface.
    /// </exception>
    private static T? Max<T>(ref T searchSpace, int length, IComparer<T>? comparer)
    {
        if (typeof(T).IsValueType && comparer is not null && comparer == Comparer<T>.Default)
        {
            // We want to inline `Comparer<T>.Default.Compare` for value types.
            comparer = null;
        }

        if (typeof(T) == typeof(byte) && comparer is null)
            return (T)(object)Extremum<byte, Maximum<byte>>(ref Unsafe.As<T, byte>(ref searchSpace), length);

        if (typeof(T) == typeof(sbyte) && comparer is null)
            return (T)(object)Extremum<sbyte, Maximum<sbyte>>(ref Unsafe.As<T, sbyte>(ref searchSpace), length);

        if (typeof(T) == typeof(short) && comparer is null)
            return (T)(object)Extremum<short, Maximum<short>>(ref Unsafe.As<T, short>(ref searchSpace), length);

        if (typeof(T) == typeof(ushort) && comparer is null)
            return (T)(object)Extremum<ushort, Maximum<ushort>>(ref Unsafe.As<T, ushort>(ref searchSpace), length);

        if (typeof(T) == typeof(int) && comparer is null)
            return (T)(object)Extremum<int, Maximum<int>>(ref Unsafe.As<T, int>(ref searchSpace), length);

        if (typeof(T) == typeof(uint) && comparer is null)
            return (T)(object)Extremum<uint, Maximum<uint>>(ref Unsafe.As<T, uint>(ref searchSpace), length);

        if (typeof(T) == typeof(long) && comparer is null)
            return (T)(object)Extremum<long, Maximum<long>>(ref Unsafe.As<T, long>(ref searchSpace), length);

        if (typeof(T) == typeof(ulong) && comparer is null)
            return (T)(object)Extremum<ulong, Maximum<ulong>>(ref Unsafe.As<T, ulong>(ref searchSpace), length);

        if (length == 0)
        {
            if (default(T) is null)
                return default;
            else
                ThrowHelper.ThrowInvalidOperationException_NoElements();
        }

        int i = 1;
        T maxValue = searchSpace;

        // Instead of `typeof(T).IsValueType`, test for reference types and
        // nullable value types.
        // `Enumerable.Max` filters out null values for some reason.
        if (default(T) is not null)
        {
            if (comparer is null)
            {
                for (; i < length; i++)
                {
                    T currentValue = Unsafe.Add(ref searchSpace, i);

                    // Let JIT de-virtualize `Compare` calls for value types.
                    if (Comparer<T>.Default.Compare(currentValue, maxValue) > 0)
                        maxValue = currentValue;
                }
            }
            else
            {
                for (; i < length; i++)
                {
                    T currentValue = Unsafe.Add(ref searchSpace, i);
                    if (comparer.Compare(currentValue, maxValue) > 0)
                        maxValue = currentValue;
                }
            }
        }
        else
        {
            comparer ??= Comparer<T>.Default;

            for (; maxValue is null && i < length; i++)
                maxValue = Unsafe.Add(ref searchSpace, i);

            for (; i < length; i++)
            {
                T currentValue = Unsafe.Add(ref searchSpace, i);
                if (currentValue is not null && comparer.Compare(currentValue, maxValue) > 0)
                    maxValue = currentValue;
            }
        }

        return maxValue;
    }

    /// <summary>
    /// Returns the maximum value in a memory block.
    /// </summary>
    /// <remarks>
    /// If <typeparamref name="T"/> is an IEEE 754 floating-point type, this method does not account
    /// for <c>NaN</c> values, so it will produce incorrect results if a <c>NaN</c> is present in the span.
    /// Use with caution and ensure the span does not contain any <c>NaN</c> values if accurate maximum
    /// computation is required.
    /// </remarks>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="searchSpace">The reference to the start of the search space.</param>
    /// <param name="length">The length of the search space.</param>
    /// <returns>The maximum value in the memory block.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="searchSpace"/> contains no elements.</exception>
    private static T? UnsafeMax<T>(ref T searchSpace, int length)
    {
        if (typeof(T) == typeof(float))
            return (T)(object)Extremum<float, Maximum<float>>(ref Unsafe.As<T, float>(ref searchSpace), length);

        if (typeof(T) == typeof(double))
            return (T)(object)Extremum<double, Maximum<double>>(ref Unsafe.As<T, double>(ref searchSpace), length);

        return Max(ref searchSpace, length, comparer: null);
    }

    /// <summary>
    /// Provides methods for calculating the maximum value between two values or vectors
    /// of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    private readonly struct Maximum<T> : IExtremum<T> where T : struct, IComparable<T>
    {
        /// <summary>
        /// Compares two values to determine which is less.
        /// </summary>
        /// <param name="left">The value to compare with <paramref name="right"/>.</param>
        /// <param name="right">The value to compare with <paramref name="left"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Compare(T left, T right) => left.CompareTo(right) > 0;

        /// <summary>
        /// Computes the minimum of two vectors on a per-element basis.
        /// </summary>
        /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
        /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
        /// <returns>
        /// A vector whose elements are the minimum of the corresponding elements in left and right.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<T> Compare(Vector<T> left, Vector<T> right) => Vector.Max(left, right);
    }

    /// <summary>
    /// Returns the maximum value in a span of <see cref="byte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="byte"/> values to determine the maximum value of.</param>
    /// <returns>The maximum value in the span.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="span"/> contains no elements.</exception>
    public static byte Max(this scoped Span<byte> span) => Extremum<byte, Maximum<byte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{byte})"/>
    public static byte Max(this scoped ReadOnlySpan<byte> span) => Extremum<byte, Maximum<byte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="sbyte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="sbyte"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    [CLSCompliant(false)]
    public static sbyte Max(this scoped Span<sbyte> span) => Extremum<sbyte, Maximum<sbyte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{byte})"/>
    [CLSCompliant(false)]
    public static sbyte Max(this scoped ReadOnlySpan<sbyte> span) => Extremum<sbyte, Maximum<sbyte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    public static short Max(this scoped Span<short> span) => Extremum<short, Maximum<short>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{short})"/>
    public static short Max(this scoped ReadOnlySpan<short> span) => Extremum<short, Maximum<short>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="ushort"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ushort"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    [CLSCompliant(false)]
    public static ushort Max(this scoped Span<ushort> span) => Extremum<ushort, Maximum<ushort>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{ushort})"/>
    [CLSCompliant(false)]
    public static ushort Max(this scoped ReadOnlySpan<ushort> span) => Extremum<ushort, Maximum<ushort>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="int"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="int"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    public static int Max(this scoped Span<int> span) => Extremum<int, Maximum<int>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{int})"/>
    public static int Max(this scoped ReadOnlySpan<int> span) => Extremum<int, Maximum<int>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="uint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="uint"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    [CLSCompliant(false)]
    public static uint Max(this scoped Span<uint> span) => Extremum<uint, Maximum<uint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{uint})"/>
    [CLSCompliant(false)]
    public static uint Max(this scoped ReadOnlySpan<uint> span) => Extremum<uint, Maximum<uint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="long"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="long"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    public static long Max(this scoped Span<long> span) => Extremum<long, Maximum<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{long})"/>
    public static long Max(this scoped ReadOnlySpan<long> span) => Extremum<long, Maximum<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="ulong"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ulong"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    [CLSCompliant(false)]
    public static ulong Max(this scoped Span<ulong> span) => Extremum<ulong, Maximum<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{ulong})"/>
    [CLSCompliant(false)]
    public static ulong Max(this scoped ReadOnlySpan<ulong> span) => Extremum<ulong, Maximum<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="float"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="float"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    public static float Max(this scoped Span<float> span) => Max(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <inheritdoc cref="Max(Span{float})"/>
    public static float Max(this scoped ReadOnlySpan<float> span) => Max(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    public static double Max(this scoped Span<double> span) => Max(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <inheritdoc cref="Max(Span{double})"/>
    public static double Max(this scoped ReadOnlySpan<double> span) => Max(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="decimal"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="decimal"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    public static decimal Max(this scoped Span<decimal> span) => Max(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <inheritdoc cref="Max(Span{decimal})"/>
    public static decimal Max(this scoped ReadOnlySpan<decimal> span) => Max(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <summary>
    /// Returns the maximum value in a generic span.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="span">A span of values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    public static T? Max<T>(this scoped Span<T> span) => Max(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <inheritdoc cref="Max{T}(Span{T})"/>
    public static T? Max<T>(this scoped ReadOnlySpan<T> span) => Max(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <summary>
    /// Returns the maximum value in a generic span.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="span">A span of values to determine the maximum value of.</param>
    /// <param name="comparer">The <see cref="IComparer{T}"/> to compare values.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    /// <exception cref="ArgumentException">
    /// No object in <paramref name="span"/> implements the <see cref="IComparable"/> or
    /// <see cref="IComparable{T}"/> interface.
    /// </exception>
    public static T? Max<T>(this scoped Span<T> span, IComparer<T>? comparer) => Max(ref MemoryMarshal.GetReference(span), span.Length, comparer);

    /// <inheritdoc cref="Max{T}(Span{T}, IComparer{T}?)"/>
    public static T? Max<T>(this scoped ReadOnlySpan<T> span, IComparer<T>? comparer) => Max(ref MemoryMarshal.GetReference(span), span.Length, comparer);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="float"/> values.
    /// </summary>
    /// <remarks>
    /// This method does not account for <c>NaN</c> values, so it will produce incorrect results if
    /// a <c>NaN</c> is present in the span.
    /// Use with caution and ensure the span does not contain any <c>NaN</c> values if accurate maximum
    /// computation is required.
    /// </remarks>
    /// <param name="span">A span of <see cref="float"/> values to determine the maximum value of.</param>
    /// <returns>The maximum value in the span.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="span"/> contains no elements.</exception>
    public static float UnsafeMax(this scoped Span<float> span) => Extremum<float, Maximum<float>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeMax(Span{float})"/>
    public static float UnsafeMax(this scoped ReadOnlySpan<float> span) => Extremum<float, Maximum<float>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="UnsafeMax(Span{float})"/>
    public static double UnsafeMax(this scoped Span<double> span) => Extremum<double, Maximum<double>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeMax(Span{double})"/>
    public static double UnsafeMax(this scoped ReadOnlySpan<double> span) => Extremum<double, Maximum<double>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a generic span.
    /// </summary>
    /// <remarks>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// If <typeparamref name="T"/> is an IEEE 754 floating-point type, this method does not account
    /// for <c>NaN</c> values, so it will produce incorrect results if a <c>NaN</c> is present in the span.
    /// Use with caution and ensure the span does not contain any <c>NaN</c> values if accurate maximum
    /// computation is required.
    /// </remarks>
    /// <param name="span">A span of values to determine the maximum value of.</param>
    /// <returns>The maximum value in the span.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="span"/> contains no elements.</exception>
    public static T? UnsafeMax<T>(this scoped Span<T> span) => UnsafeMax(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeMax{T}(Span{T})"/>
    public static T? UnsafeMax<T>(this scoped ReadOnlySpan<T> span) => UnsafeMax(ref MemoryMarshal.GetReference(span), span.Length);
}
