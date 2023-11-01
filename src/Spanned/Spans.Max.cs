namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Performs a vectorized search for the maximum value in a memory block that contains IEEE 754 floats.
    /// </summary>
    /// <typeparam name="TFloat">The IEEE 754 floating-point type.</typeparam>
    /// <typeparam name="TInteger">
    /// The unsigned integer type <typeparamref name="TFloat"/> can be interpreted as.
    /// </typeparam>
    /// <param name="searchSpace">The reference to the start of the search space.</param>
    /// <param name="length">The length of the search space.</param>
    /// <returns>The maximum value in the memory block.</returns>
    /// <exception cref="NotSupportedException"><typeparamref name="TFloat"/> or <typeparamref name="TInteger"/> is not supported.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="searchSpace"/> contains no elements.</exception>
    private static TFloat MaxFloat<TFloat, TInteger>(ref TFloat searchSpace, int length)
        where TFloat : struct, IFloatingPointIeee754<TFloat>
        where TInteger : struct, IUnsignedNumber<TInteger>
    {
        // Unlike with `MinFloat` (refer to the comment on that method for more info),
        // even if the input is poisoned by NaNs, we still need to return a meaningful
        // value most of the time.
        // Merely identifying NaN's presence in the given memory block is not enough,
        // as it only informs us that the comparison logic was probably compromised
        // somewhere down the line.
        // Thus, in this case, we need to ensure that NaN cannot break the comparison.
        //
        // To achieve this, we must find a non-NaN value in the input (any NaN value
        // is considered to be less than any non-NaN value). If there's no such value,
        // return NaN, indicating that the sequence only contains NaNs. Otherwise, we
        // can safely filter out NaN values from the input, replacing them with the
        // found valid value to ensure the deterministic nature of comparison operations.
        //
        // To detect NaNs in a vector, we can use the same trick as in `MinFloat`:
        // any NaN, treated as an unsigned integer with its most significant bit set to 1,
        // is guaranteed to be greater than any other value represented the same way.
        // Also, it's guaranteed to be at least 1 greater than negative infinity casted to its
        // unsigned integer form.
        //
        // Unfortunately, this method is slower than `MinFloat`. While it still benefits from
        // vectorization, the situation would be in a much better shape without the need to deal
        // with the NaN concept every iteration. So, there are a few things we could consider:
        //
        //   1) Leave everything as is. In this case, every scenario (i.e., whether the input
        //      contains NaNs or not) will see slight performance improvements.
        //   2) Use a slightly tweaked `MinFloat` implementation to search for a maximum value,
        //      but instead of returning a NaN value upon its detection, reset the search state
        //      and fallback to a regular loop. This way, inputs that do not contain NaNs will
        //      see huge performance improvements, while those that do contain at least a single
        //      NaN will see a 15-20% performance degradation. Is it worth it? In the end of the
        //      day, NaN is a result of a floating-point exception, and exceptions are not designed
        //      to be fast and efficient. How often do you see a NaN value present in a valid input?
        //   3) Almost the same as (2), but instead of using a regular loop as our fallback, the
        //      current `MaxFloat` implementation could be used instead. This way, valid inputs will
        //      once again see huge performance improvements, while inputs that contain NaNs, which
        //      are processed twice but with vectorized solutions that are at least 2x faster than
        //      a naive loop, will see little to no performance improvements and definitely should not
        //      see any degradations. Seems like a perfect solution if we don't need to think about
        //      assembly size. Is it worth it to keep a huge, chunky method that only deals with
        //      NaN-containing inputs? Once again, we need a definitive answer on how often one can
        //      observe a NaN value in a valid input. And without any statistics on this matter, your
        //      guess is as good as mine. Therefore, while I would love to choose one of (2) and (3),
        //      at the moment I need to go with (1), as it's the safest option out of all three.

        Debug.Assert(Vector<TFloat>.IsSupported && Vector<TInteger>.IsSupported);
        Debug.Assert(Unsafe.SizeOf<TFloat>() == Unsafe.SizeOf<TInteger>());

        if (length == 0)
            ThrowHelper.ThrowInvalidOperationException_NoElements();

        ref TFloat current = ref searchSpace;
        ref TFloat end = ref Unsafe.Add(ref current, length);

        // Find a non-NaN value.
        TFloat nonNaN;
        do
        {
            nonNaN = current;
            current = ref Unsafe.Add(ref current, (nint)1);
        }
        while (TFloat.IsNaN(nonNaN) && Unsafe.IsAddressLessThan(ref current, ref end));

        if (Unsafe.AreSame(ref current, ref end))
            return nonNaN;

        // Note, we use `<=` instead of `<`, because in the end we need to
        // manually process every lane of the resulting vector.
        // Therefore, when `length == Vector.Count` a vectorized solution
        // would just end up performing unnecessary operations, becoming
        // slower than the regular loop without those.
        if (!Vector128.IsHardwareAccelerated || length <= Vector128<TFloat>.Count)
        {
            TFloat maxValue = nonNaN;
            do
            {
                // Once we've found a value that is not NaN, we don't need to worry
                // about NaN weirdness anymore. This is because `NaN > maxValue` is always
                // false, and it mimics the desired outcome in this case, similar to
                // the situation where `!(minValue > maxValue)`.
                if (current > maxValue)
                    maxValue = current;

                current = ref Unsafe.Add(ref current, (nint)1);
            }
            while (Unsafe.IsAddressLessThan(ref current, ref end));

            return maxValue;
        }
        else if (!Vector256.IsHardwareAccelerated || length <= Vector256<TFloat>.Count)
        {
            ref TFloat lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector128<TFloat>.Count);

            // Set the most significant bit. 1 << (bit_sizeof(T) - 1), e.g., 1 << 63 for 64 bits.
            Vector128<TInteger> sign = Vector128<TInteger>.One << (Unsafe.SizeOf<TInteger>() * 8 - 1);

            // (*(TInteger*)&NaN | sign) > *(TInteger*)&NegativeInfinity
            Vector128<TInteger> limit = Vector128.Create(Unsafe.BitCast<TFloat, TInteger>(TFloat.NegativeInfinity));

            // Fill the initial vector with non-NaN values present in the search space.
            Vector128<TFloat> max = Vector128.Create(nonNaN);

            while (Unsafe.IsAddressLessThan(ref current, ref lastVectorStart))
            {
                Vector128<TFloat> vector = Vector128.LoadUnsafe(ref current);
                Vector128<TFloat> isNaN = Vector128.GreaterThan(vector.As<TFloat, TInteger>() | sign, limit).As<TInteger, TFloat>();

                max = Vector128.Max(max, Vector128.ConditionalSelect(isNaN, max, vector));
                current = ref Unsafe.Add(ref current, (nint)Vector128<TFloat>.Count);
            }
            Vector128<TFloat> lastVector = Vector128.LoadUnsafe(ref lastVectorStart);
            Vector128<TFloat> lastIsNaN = Vector128.GreaterThan(lastVector.As<TFloat, TInteger>() | sign, limit).As<TInteger, TFloat>();
            max = Vector128.Max(max, Vector128.ConditionalSelect(lastIsNaN, max, lastVector));

            // `max[0]` is guaranteed to be non-NaN.
            // We filtered out those pesky NaNs earlier.
            TFloat maxValue = max[0];
            for (int i = 1; i < Vector128<TFloat>.Count; i++)
            {
                if (max[i] > maxValue)
                    maxValue = max[i];
            }
            return maxValue;
        }
        else if (!Vector512.IsHardwareAccelerated || length <= Vector512<TFloat>.Count)
        {
            ref TFloat lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector256<TFloat>.Count);

            // Set the most significant bit. 1 << (bit_sizeof(T) - 1), e.g., 1 << 63 for 64 bits.
            Vector256<TInteger> sign = Vector256<TInteger>.One << (Unsafe.SizeOf<TInteger>() * 8 - 1);

            // (*(TInteger*)&NaN | sign) > *(TInteger*)&NegativeInfinity
            Vector256<TInteger> limit = Vector256.Create(Unsafe.BitCast<TFloat, TInteger>(TFloat.NegativeInfinity));

            // Fill the initial vector with non-NaN values present in the search space.
            Vector256<TFloat> max = Vector256.Create(nonNaN);

            while (Unsafe.IsAddressLessThan(ref current, ref lastVectorStart))
            {
                Vector256<TFloat> vector = Vector256.LoadUnsafe(ref current);
                Vector256<TFloat> isNaN = Vector256.GreaterThan(vector.As<TFloat, TInteger>() | sign, limit).As<TInteger, TFloat>();

                max = Vector256.Max(max, Vector256.ConditionalSelect(isNaN, max, vector));
                current = ref Unsafe.Add(ref current, (nint)Vector256<TFloat>.Count);
            }
            Vector256<TFloat> lastVector = Vector256.LoadUnsafe(ref lastVectorStart);
            Vector256<TFloat> lastIsNaN = Vector256.GreaterThan(lastVector.As<TFloat, TInteger>() | sign, limit).As<TInteger, TFloat>();
            max = Vector256.Max(max, Vector256.ConditionalSelect(lastIsNaN, max, lastVector));

            // `max[0]` is guaranteed to be non-NaN.
            // We filtered out those pesky NaNs earlier.
            TFloat maxValue = max[0];
            for (int i = 1; i < Vector256<TFloat>.Count; i++)
            {
                if (max[i] > maxValue)
                    maxValue = max[i];
            }
            return maxValue;
        }
        else
        {
            ref TFloat lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector512<TFloat>.Count);

            // Set the most significant bit. 1 << (bit_sizeof(T) - 1), e.g., 1 << 63 for 64 bits.
            Vector512<TInteger> sign = Vector512<TInteger>.One << (Unsafe.SizeOf<TInteger>() * 8 - 1);

            // (*(TInteger*)&NaN | sign) > *(TInteger*)&NegativeInfinity
            Vector512<TInteger> limit = Vector512.Create(Unsafe.BitCast<TFloat, TInteger>(TFloat.NegativeInfinity));

            // Fill the initial vector with non-NaN values present in the search space.
            Vector512<TFloat> max = Vector512.Create(nonNaN);

            while (Unsafe.IsAddressLessThan(ref current, ref lastVectorStart))
            {
                Vector512<TFloat> vector = Vector512.LoadUnsafe(ref current);
                Vector512<TFloat> isNaN = Vector512.GreaterThan(vector.As<TFloat, TInteger>() | sign, limit).As<TInteger, TFloat>();

                max = Vector512.Max(max, Vector512.ConditionalSelect(isNaN, max, vector));
                current = ref Unsafe.Add(ref current, (nint)Vector512<TFloat>.Count);
            }
            Vector512<TFloat> lastVector = Vector512.LoadUnsafe(ref lastVectorStart);
            Vector512<TFloat> lastIsNaN = Vector512.GreaterThan(lastVector.As<TFloat, TInteger>() | sign, limit).As<TInteger, TFloat>();
            max = Vector512.Max(max, Vector512.ConditionalSelect(lastIsNaN, max, lastVector));

            // `max[0]` is guaranteed to be non-NaN.
            // We filtered out those pesky NaNs earlier.
            TFloat maxValue = max[0];
            for (int i = 1; i < Vector512<TFloat>.Count; i++)
            {
                if (max[i] > maxValue)
                    maxValue = max[i];
            }
            return maxValue;
        }
    }

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

        if (typeof(T) == typeof(nint) && comparer is null)
            return (T)(object)Extremum<nint, Maximum<nint>>(ref Unsafe.As<T, nint>(ref searchSpace), length);

        if (typeof(T) == typeof(nuint) && comparer is null)
            return (T)(object)Extremum<nuint, Maximum<nuint>>(ref Unsafe.As<T, nuint>(ref searchSpace), length);

        if (typeof(T) == typeof(float) && comparer is null)
            return (T)(object)MaxFloat<float, uint>(ref Unsafe.As<T, float>(ref searchSpace), length);

        if (typeof(T) == typeof(double) && comparer is null)
            return (T)(object)MaxFloat<double, ulong>(ref Unsafe.As<T, double>(ref searchSpace), length);

        if (length == 0)
        {
            if (default(T) is null)
                return default;
            else
                ThrowHelper.ThrowInvalidOperationException_NoElements();
        }

        // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations.
        nint i = 1;
        nint end = (nint)length;
        T maxValue = searchSpace;

        // Instead of `typeof(T).IsValueType`, test for reference types and
        // nullable value types.
        // `Enumerable.Max` filters out null values for some reason.
        if (default(T) is not null)
        {
            if (comparer is null)
            {
                for (; i < end; i++)
                {
                    T currentValue = Unsafe.Add(ref searchSpace, i);

                    // Let JIT de-virtualize `Compare` calls for value types.
                    if (Comparer<T>.Default.Compare(currentValue, maxValue) > 0)
                        maxValue = currentValue;
                }
            }
            else
            {
                for (; i < end; i++)
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

            for (; maxValue is null && i < end; i++)
                maxValue = Unsafe.Add(ref searchSpace, i);

            for (; i < end; i++)
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
    private readonly struct Maximum<T> : IExtremum<T> where T : struct, IBinaryNumber<T>
    {
        /// <summary>
        /// Compares two values to determine which is greater.
        /// </summary>
        /// <param name="left">The value to compare with <paramref name="right"/>.</param>
        /// <param name="right">The value to compare with <paramref name="left"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Compare(T left, T right) => left > right;

        /// <summary>
        /// Computes the maximum of two vectors on a per-element basis.
        /// </summary>
        /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
        /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
        /// <returns>
        /// A vector whose elements are the maximum of the corresponding elements in left and right.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> Compare(Vector128<T> left, Vector128<T> right) => Vector128.Max(left, right);

        /// <inheritdoc cref="Compare(Vector128{T}, Vector128{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<T> Compare(Vector256<T> left, Vector256<T> right) => Vector256.Max(left, right);

        /// <inheritdoc cref="Compare(Vector128{T}, Vector128{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<T> Compare(Vector512<T> left, Vector512<T> right) => Vector512.Max(left, right);
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
    /// Returns the maximum value in a span of <see cref="nint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="nint"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    public static nint Max(this scoped Span<nint> span) => Extremum<nint, Maximum<nint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{nint})"/>
    public static nint Max(this scoped ReadOnlySpan<nint> span) => Extremum<nint, Maximum<nint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="nuint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="nuint"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    [CLSCompliant(false)]
    public static nuint Max(this scoped Span<nuint> span) => Extremum<nuint, Maximum<nuint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{nuint})"/>
    [CLSCompliant(false)]
    public static nuint Max(this scoped ReadOnlySpan<nuint> span) => Extremum<nuint, Maximum<nuint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="float"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="float"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    public static float Max(this scoped Span<float> span) => MaxFloat<float, uint>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{float})"/>
    public static float Max(this scoped ReadOnlySpan<float> span) => MaxFloat<float, uint>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the maximum value in a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to determine the maximum value of.</param>
    /// <inheritdoc cref="Max(Span{byte})"/>
    public static double Max(this scoped Span<double> span) => MaxFloat<double, ulong>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Max(Span{double})"/>
    public static double Max(this scoped ReadOnlySpan<double> span) => MaxFloat<double, ulong>(ref MemoryMarshal.GetReference(span), span.Length);

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
