namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Returns the minimum value in a memory block.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="searchSpace">The reference to the start of the search space.</param>
    /// <param name="length">The length of the search space.</param>
    /// <param name="comparer">The <see cref="IComparer{T}"/> to compare values.</param>
    /// <returns>The minimum value in the memory block.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="searchSpace"/> contains no elements.</exception>
    /// <exception cref="ArgumentException">
    /// No object in <paramref name="searchSpace"/> implements the <see cref="IComparable"/> or
    /// <see cref="IComparable{T}"/> interface.
    /// </exception>
    private static T? Min<T>(ref T searchSpace, int length, IComparer<T>? comparer)
    {
        if (typeof(T).IsValueType && comparer is not null && comparer == Comparer<T>.Default)
        {
            // We want to inline `Comparer<T>.Default.Compare` for value types.
            comparer = null;
        }

        if (typeof(T) == typeof(byte) && comparer is null)
            return (T)(object)Extremum<byte, Minimum<byte>>(ref Unsafe.As<T, byte>(ref searchSpace), length);

        if (typeof(T) == typeof(sbyte) && comparer is null)
            return (T)(object)Extremum<sbyte, Minimum<sbyte>>(ref Unsafe.As<T, sbyte>(ref searchSpace), length);

        if (typeof(T) == typeof(short) && comparer is null)
            return (T)(object)Extremum<short, Minimum<short>>(ref Unsafe.As<T, short>(ref searchSpace), length);

        if (typeof(T) == typeof(ushort) && comparer is null)
            return (T)(object)Extremum<ushort, Minimum<ushort>>(ref Unsafe.As<T, ushort>(ref searchSpace), length);

        if (typeof(T) == typeof(int) && comparer is null)
            return (T)(object)Extremum<int, Minimum<int>>(ref Unsafe.As<T, int>(ref searchSpace), length);

        if (typeof(T) == typeof(uint) && comparer is null)
            return (T)(object)Extremum<uint, Minimum<uint>>(ref Unsafe.As<T, uint>(ref searchSpace), length);

        if (typeof(T) == typeof(long) && comparer is null)
            return (T)(object)Extremum<long, Minimum<long>>(ref Unsafe.As<T, long>(ref searchSpace), length);

        if (typeof(T) == typeof(ulong) && comparer is null)
            return (T)(object)Extremum<ulong, Minimum<ulong>>(ref Unsafe.As<T, ulong>(ref searchSpace), length);

        if (length == 0)
        {
            if (default(T) is null)
                return default;
            else
                ThrowHelper.ThrowInvalidOperationException_NoElements();
        }

        int i = 1;
        T minValue = searchSpace;

        // Instead of `typeof(T).IsValueType`, test for reference types and
        // nullable value types.
        // `Enumerable.Min` filters out null values for some reason, even though
        // they are considered the smallest possible value everywhere else.
        if (default(T) is not null)
        {
            if (comparer is null)
            {
                for (; i < length; i++)
                {
                    T currentValue = Unsafe.Add(ref searchSpace, i);

                    // Let JIT de-virtualize `Compare` calls for value types.
                    if (Comparer<T>.Default.Compare(currentValue, minValue) < 0)
                        minValue = currentValue;
                }
            }
            else
            {
                for (; i < length; i++)
                {
                    T currentValue = Unsafe.Add(ref searchSpace, i);
                    if (comparer.Compare(currentValue, minValue) < 0)
                        minValue = currentValue;
                }
            }
        }
        else
        {
            comparer ??= Comparer<T>.Default;

            for (; minValue is null && i < length; i++)
                minValue = Unsafe.Add(ref searchSpace, i);

            for (; i < length; i++)
            {
                T currentValue = Unsafe.Add(ref searchSpace, i);
                if (currentValue is not null && comparer.Compare(currentValue, minValue) < 0)
                    minValue = currentValue;
            }
        }

        return minValue;
    }

    /// <summary>
    /// Returns the minimum value in a memory block.
    /// </summary>
    /// <remarks>
    /// If <typeparamref name="T"/> is an IEEE 754 floating-point type, this method does not account
    /// for <c>NaN</c> values, so it will produce incorrect results if a <c>NaN</c> is present in the span.
    /// Use with caution and ensure the span does not contain any <c>NaN</c> values if accurate minimum
    /// computation is required.
    /// </remarks>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="searchSpace">The reference to the start of the search space.</param>
    /// <param name="length">The length of the search space.</param>
    /// <returns>The minimum value in the memory block.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="searchSpace"/> contains no elements.</exception>
    private static T? UnsafeMin<T>(ref T searchSpace, int length)
    {
        if (typeof(T) == typeof(float))
            return (T)(object)Extremum<float, Minimum<float>>(ref Unsafe.As<T, float>(ref searchSpace), length);

        if (typeof(T) == typeof(double))
            return (T)(object)Extremum<double, Minimum<double>>(ref Unsafe.As<T, double>(ref searchSpace), length);

        return Min(ref searchSpace, length, comparer: null);
    }

    /// <summary>
    /// Provides methods for calculating the minimum value between two values or vectors
    /// of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    private readonly struct Minimum<T> : IExtremum<T> where T : struct, IComparable<T>
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
        public bool Compare(T left, T right) => left.CompareTo(right) < 0;

        /// <summary>
        /// Computes the minimum of two vectors on a per-element basis.
        /// </summary>
        /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
        /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
        /// <returns>
        /// A vector whose elements are the minimum of the corresponding elements in left and right.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<T> Compare(Vector<T> left, Vector<T> right) => Vector.Min(left, right);
    }

    /// <summary>
    /// Returns the minimum value in a span of <see cref="byte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="byte"/> values to determine the minimum value of.</param>
    /// <returns>The minimum value in the span.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="span"/> contains no elements.</exception>
    public static byte Min(this scoped Span<byte> span) => Extremum<byte, Minimum<byte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{byte})"/>
    public static byte Min(this scoped ReadOnlySpan<byte> span) => Extremum<byte, Minimum<byte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="sbyte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="sbyte"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    [CLSCompliant(false)]
    public static sbyte Min(this scoped Span<sbyte> span) => Extremum<sbyte, Minimum<sbyte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{sbyte})"/>
    [CLSCompliant(false)]
    public static sbyte Min(this scoped ReadOnlySpan<sbyte> span) => Extremum<sbyte, Minimum<sbyte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    public static short Min(this scoped Span<short> span) => Extremum<short, Minimum<short>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{short})"/>
    public static short Min(this scoped ReadOnlySpan<short> span) => Extremum<short, Minimum<short>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="ushort"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ushort"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    [CLSCompliant(false)]
    public static ushort Min(this scoped Span<ushort> span) => Extremum<ushort, Minimum<ushort>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{ushort})"/>
    [CLSCompliant(false)]
    public static ushort Min(this scoped ReadOnlySpan<ushort> span) => Extremum<ushort, Minimum<ushort>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="int"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="int"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    public static int Min(this scoped Span<int> span) => Extremum<int, Minimum<int>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{int})"/>
    public static int Min(this scoped ReadOnlySpan<int> span) => Extremum<int, Minimum<int>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="uint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="uint"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    [CLSCompliant(false)]
    public static uint Min(this scoped Span<uint> span) => Extremum<uint, Minimum<uint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{uint})"/>
    [CLSCompliant(false)]
    public static uint Min(this scoped ReadOnlySpan<uint> span) => Extremum<uint, Minimum<uint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="long"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="long"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    public static long Min(this scoped Span<long> span) => Extremum<long, Minimum<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{long})"/>
    public static long Min(this scoped ReadOnlySpan<long> span) => Extremum<long, Minimum<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="ulong"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ulong"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    [CLSCompliant(false)]
    public static ulong Min(this scoped Span<ulong> span) => Extremum<ulong, Minimum<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{ulong})"/>
    [CLSCompliant(false)]
    public static ulong Min(this scoped ReadOnlySpan<ulong> span) => Extremum<ulong, Minimum<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="float"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="float"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    public static float Min(this scoped Span<float> span) => Min(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <inheritdoc cref="Min(Span{float})"/>
    public static float Min(this scoped ReadOnlySpan<float> span) => Min(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    public static double Min(this scoped Span<double> span) => Min(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <inheritdoc cref="Min(Span{double})"/>
    public static double Min(this scoped ReadOnlySpan<double> span) => Min(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="decimal"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="decimal"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    public static decimal Min(this scoped Span<decimal> span) => Min(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <inheritdoc cref="Min(Span{decimal})"/>
    public static decimal Min(this scoped ReadOnlySpan<decimal> span) => Min(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <summary>
    /// Returns the minimum value in a generic span.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="span">A span of values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    public static T? Min<T>(this scoped Span<T> span) => Min(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <inheritdoc cref="Min{T}(Span{T})"/>
    public static T? Min<T>(this scoped ReadOnlySpan<T> span) => Min(ref MemoryMarshal.GetReference(span), span.Length, comparer: null);

    /// <summary>
    /// Returns the minimum value in a generic span.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="span">A span of values to determine the minimum value of.</param>
    /// <param name="comparer">The <see cref="IComparer{T}"/> to compare values.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    /// <exception cref="ArgumentException">
    /// No object in <paramref name="span"/> implements the <see cref="IComparable"/> or
    /// <see cref="IComparable{T}"/> interface.
    /// </exception>
    public static T? Min<T>(this scoped Span<T> span, IComparer<T>? comparer) => Min(ref MemoryMarshal.GetReference(span), span.Length, comparer);

    /// <inheritdoc cref="Min{T}(Span{T}, IComparer{T}?)"/>
    public static T? Min<T>(this scoped ReadOnlySpan<T> span, IComparer<T>? comparer) => Min(ref MemoryMarshal.GetReference(span), span.Length, comparer);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="float"/> values.
    /// </summary>
    /// <remarks>
    /// This method does not account for <c>NaN</c> values, so it will produce incorrect results if
    /// a <c>NaN</c> is present in the span.
    /// Use with caution and ensure the span does not contain any <c>NaN</c> values if accurate minimum
    /// computation is required.
    /// </remarks>
    /// <param name="span">A span of <see cref="float"/> values to determine the minimum value of.</param>
    /// <returns>The minimum value in the span.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="span"/> contains no elements.</exception>
    public static float UnsafeMin(this scoped Span<float> span) => Extremum<float, Minimum<float>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeMin(Span{float})"/>
    public static float UnsafeMin(this scoped ReadOnlySpan<float> span) => Extremum<float, Minimum<float>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="UnsafeMin(Span{float})"/>
    public static double UnsafeMin(this scoped Span<double> span) => Extremum<double, Minimum<double>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeMin(Span{double})"/>
    public static double UnsafeMin(this scoped ReadOnlySpan<double> span) => Extremum<double, Minimum<double>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a generic span.
    /// </summary>
    /// <remarks>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// If <typeparamref name="T"/> is an IEEE 754 floating-point type, this method does not account
    /// for <c>NaN</c> values, so it will produce incorrect results if a <c>NaN</c> is present in the span.
    /// Use with caution and ensure the span does not contain any <c>NaN</c> values if accurate minimum
    /// computation is required.
    /// </remarks>
    /// <param name="span">A span of values to determine the minimum value of.</param>
    /// <returns>The minimum value in the span.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="span"/> contains no elements.</exception>
    public static T? UnsafeMin<T>(this scoped Span<T> span) => UnsafeMin(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeMin{T}(Span{T})"/>
    public static T? UnsafeMin<T>(this scoped ReadOnlySpan<T> span) => UnsafeMin(ref MemoryMarshal.GetReference(span), span.Length);
}
