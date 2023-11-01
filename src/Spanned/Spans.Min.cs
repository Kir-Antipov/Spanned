namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Performs a vectorized search for the minimum value in a memory block that contains IEEE 754 floats.
    /// </summary>
    /// <typeparam name="TFloat">The IEEE 754 floating-point type.</typeparam>
    /// <typeparam name="TInteger">
    /// The unsigned integer type <typeparamref name="TFloat"/> can be interpreted as.
    /// </typeparam>
    /// <param name="searchSpace">The reference to the start of the search space.</param>
    /// <param name="length">The length of the search space.</param>
    /// <returns>The minimum value in the memory block.</returns>
    /// <exception cref="NotSupportedException"><typeparamref name="TFloat"/> or <typeparamref name="TInteger"/> is not supported.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="searchSpace"/> contains no elements.</exception>
    private static TFloat MinFloat<TFloat, TInteger>(ref TFloat searchSpace, int length)
        where TFloat : struct, IFloatingPointIeee754<TFloat>
        where TInteger : struct, IUnsignedNumber<TInteger>
    {
        // We need to special case IEEE 754 floats due to the NaN concept, which, I'm sure,
        // we all appreciate dearly. Since NaN != NaN, NaN != x, !(NaN < x), !(NaN > x),
        // it's impossible to rely on the regular comparison rules while processing
        // a sequence that may contain a result of a floating point exception.
        // This is the reason .NET doesn't vectorize Min/Max operations on float inputs.
        //
        // However, we can define Min for floats as `Contains(x, NaN) ? NaN : Min(x)`,
        // since in case the input does not contain NaNs, comparison rules are perfectly
        // predictable. Also, we do not really care about the order of the operations -
        // in case the search space contains a NaN, we discard the result of Min() anyways,
        // whether it's valid or not; otherwise, its result is **guaranteed** to be valid.
        // Therefore, it's possible to search for the minimum value and for a NaN
        // simultaneously in a given sequence.
        //
        // The next problem we face is actually searching for a NaN because it is not
        // a single value; it's actually a range of values. According to IEEE 754, any
        // value that follows the following format is considered a NaN:
        //   - sign     = 0 or 1.
        //   - exponent = all 1 bits.
        //   - mantissa = at least one 1 bit.
        // So, double-precision (1 bit sign, 11 bit exponent, 52 bit mantissa) floats
        // (which I will be using for my examples, but the logic stays the same for any
        // other precision level, including, but not limited to single-precision floats),
        // can throw 2^53 - 2 = 9007199254740990 different NaNs at you. Which is... a lot.
        // We are definitely not hardcoding that! Let's take a closer look at the binary
        // representation of the floats in question and see if there's something interesting.
        //
        //       s (sign); e (exponent); m (mantissa)
        //    *: seeeeeeeeeeemmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm
        //  NaN: 0111111111111000000000000000000000000000000000000000000000000000
        // +Inf: 0111111111110000000000000000000000000000000000000000000000000000
        // +Max: 0111111111101111111111111111111111111111111111111111111111111111
        //  ...: ???????????0????????????????????????????????????????????????????
        // -Max: 1111111111101111111111111111111111111111111111111111111111111111
        // -Inf: 1111111111110000000000000000000000000000000000000000000000000000
        //  NaN: 1111111111111000000000000000000000000000000000000000000000000000
        //
        // And something interesting there is indeed. If we treat these values as unsigned
        // integers, any NaN that has its sign bit set to 1 is greater than any valid value.
        // What will happen if we set the sign bit to 1 for every value? Valid positive floats
        // will begin to collide with their negative counterparts, but there is no positive or
        // negative NaN; they are all just NaNs. Therefore, logically speaking, a NaN is not
        // affected* by such an operation. Thus, any NaN (treated as an unsigned integer) is
        // guaranteed to be greater than any valid float value (treated as an unsigned integer)
        // in a sequence where every element has its sign bit set to 1.
        //
        // Knowing all that, we can search for a min float while simultaneously searching for
        // a max unsigned integer (ignoring the most significant bit) in the same memory block.
        // When we are done, if the maximum unsigned integer, converted back to an IEEE 754 float,
        // turns out to be a NaN, we return it instead of the found min float; otherwise, we return
        // the min float we have found, since in this case, as stated before, it's guaranteed that
        // the input was not poisoned by NaNs.
        //
        // Following this logic, the search can now be vectorized.
        //
        // *P.S. - Some runtimes (not the .NET-related ones, though) differentiate between
        // qNaN and sNaN. The distinction is based on the mantissa value rather than
        // the sign bit. Therefore, we are still okay with the sign bit flipping trick.

        Debug.Assert(Vector<TFloat>.IsSupported && Vector<TInteger>.IsSupported);
        Debug.Assert(Unsafe.SizeOf<TFloat>() == Unsafe.SizeOf<TInteger>());

        if (length == 0)
            ThrowHelper.ThrowInvalidOperationException_NoElements();

        // Note, we use `<=` instead of `<`, because in the end we need to
        // manually process every lane of the resulting vector.
        // Therefore, when `length == Vector.Count` a vectorized solution
        // would just end up performing unnecessary operations, becoming
        // slower than the regular loop without those.
        if (!Vector128.IsHardwareAccelerated || length <= Vector128<TFloat>.Count)
        {
            ref TFloat current = ref searchSpace;
            ref TFloat end = ref Unsafe.Add(ref searchSpace, length);

            TFloat minValue = current;
            current = ref Unsafe.Add(ref current, (nint)1);

            while (Unsafe.IsAddressLessThan(ref current, ref end))
            {
                if (current < minValue)
                {
                    minValue = current;
                }
                else if (TFloat.IsNaN(current))
                {
                    // Short-circuit, as we define NaN as the smallest value possible.
                    return current;
                }

                current = ref Unsafe.Add(ref current, (nint)1);
            }
            return minValue;
        }
        else if (!Vector256.IsHardwareAccelerated || length <= Vector256<TFloat>.Count)
        {
            ref TFloat current = ref searchSpace;
            ref TFloat lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector128<TFloat>.Count);

            // Set the most significant bit. 1 << (bit_sizeof(T) - 1), e.g., 1 << 63 for 64 bits.
            Vector128<TInteger> sign = Vector128<TInteger>.One << (Unsafe.SizeOf<TInteger>() * 8 - 1);

            Vector128<TFloat> min = Vector128.LoadUnsafe(ref current);
            Vector128<TInteger> max = min.As<TFloat, TInteger>() | sign;
            current = ref Unsafe.Add(ref current, (nint)Vector128<TFloat>.Count);

            while (Unsafe.IsAddressLessThan(ref current, ref lastVectorStart))
            {
                min = Vector128.Min(min, Vector128.LoadUnsafe(ref current));
                max = Vector128.Max(max, Vector128.LoadUnsafe(ref current).As<TFloat, TInteger>() | sign);
                current = ref Unsafe.Add(ref current, (nint)Vector128<TFloat>.Count);
            }
            min = Vector128.Min(min, Vector128.LoadUnsafe(ref lastVectorStart));
            max = Vector128.Max(max, Vector128.LoadUnsafe(ref lastVectorStart).As<TFloat, TInteger>() | sign);

            // Use a "magic value" instead of min[0], so we don't need to introduce
            // additional branching for a NaN check, similar to the one contained
            // in the following for loop.
            //
            // No value is greater than positive infinity. Therefore, it is guaranteed
            // to be replaced by an actual value from the vector, provided it contains
            // anything other than positive infinity. Otherwise, PositiveInfinity
            // is already the value we are looking for.
            TFloat minValue = TFloat.PositiveInfinity;

            for (int i = 0; i < Vector128<TFloat>.Count; i++)
            {
                // Do NOT invert this if.
                // We want the branch predictor to prefer the non-NaN case.
                if (!TFloat.IsNaN(Unsafe.BitCast<TInteger, TFloat>(max[i])))
                {
                    if (min[i] < minValue)
                        minValue = min[i];

                    continue;
                }

                return Unsafe.BitCast<TInteger, TFloat>(max[i]);
            }
            return minValue;
        }
        else if (!Vector512.IsHardwareAccelerated || length <= Vector512<TFloat>.Count)
        {
            ref TFloat current = ref searchSpace;
            ref TFloat lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector256<TFloat>.Count);

            // Set the most significant bit. 1 << (bit_sizeof(T) - 1), e.g., 1 << 63 for 64 bits.
            Vector256<TInteger> sign = Vector256<TInteger>.One << (Unsafe.SizeOf<TInteger>() * 8 - 1);

            Vector256<TFloat> min = Vector256.LoadUnsafe(ref current);
            Vector256<TInteger> max = min.As<TFloat, TInteger>() | sign;
            current = ref Unsafe.Add(ref current, (nint)Vector256<TFloat>.Count);

            while (Unsafe.IsAddressLessThan(ref current, ref lastVectorStart))
            {
                min = Vector256.Min(min, Vector256.LoadUnsafe(ref current));
                max = Vector256.Max(max, Vector256.LoadUnsafe(ref current).As<TFloat, TInteger>() | sign);
                current = ref Unsafe.Add(ref current, (nint)Vector256<TFloat>.Count);
            }
            min = Vector256.Min(min, Vector256.LoadUnsafe(ref lastVectorStart));
            max = Vector256.Max(max, Vector256.LoadUnsafe(ref lastVectorStart).As<TFloat, TInteger>() | sign);

            // Use a "magic value" instead of min[0], so we don't need to introduce
            // additional branching for a NaN check, similar to the one contained
            // in the following for loop.
            //
            // No value is greater than positive infinity. Therefore, it is guaranteed
            // to be replaced by an actual value from the vector, provided it contains
            // anything other than positive infinity. Otherwise, PositiveInfinity
            // is already the value we are looking for.
            TFloat minValue = TFloat.PositiveInfinity;

            for (int i = 0; i < Vector256<TFloat>.Count; i++)
            {
                // Do NOT invert this if.
                // We want the branch predictor to prefer the non-NaN branch.
                if (!TFloat.IsNaN(Unsafe.BitCast<TInteger, TFloat>(max[i])))
                {
                    if (min[i] < minValue)
                        minValue = min[i];

                    continue;
                }

                return Unsafe.BitCast<TInteger, TFloat>(max[i]);
            }
            return minValue;
        }
        else
        {
            ref TFloat current = ref searchSpace;
            ref TFloat lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector512<TFloat>.Count);

            // Set the most significant bit. 1 << (bit_sizeof(T) - 1), e.g., (1 << 63) for 64 bits.
            Vector512<TInteger> sign = Vector512<TInteger>.One << (Unsafe.SizeOf<TInteger>() * 8 - 1);

            Vector512<TFloat> min = Vector512.LoadUnsafe(ref current);
            Vector512<TInteger> max = min.As<TFloat, TInteger>() | sign;
            current = ref Unsafe.Add(ref current, (nint)Vector512<TFloat>.Count);

            while (Unsafe.IsAddressLessThan(ref current, ref lastVectorStart))
            {
                min = Vector512.Min(min, Vector512.LoadUnsafe(ref current));
                max = Vector512.Max(max, Vector512.LoadUnsafe(ref current).As<TFloat, TInteger>() | sign);
                current = ref Unsafe.Add(ref current, (nint)Vector512<TFloat>.Count);
            }
            min = Vector512.Min(min, Vector512.LoadUnsafe(ref lastVectorStart));
            max = Vector512.Max(max, Vector512.LoadUnsafe(ref lastVectorStart).As<TFloat, TInteger>() | sign);

            // Use a "magic value" instead of min[0], so we don't need to introduce
            // additional branching for a NaN check, similar to the one contained
            // in the following for loop.
            //
            // No value is greater than positive infinity. Therefore, it is guaranteed
            // to be replaced by an actual value from the vector, provided it contains
            // anything other than positive infinity. Otherwise, PositiveInfinity
            // is already the value we are looking for.
            TFloat minValue = TFloat.PositiveInfinity;

            for (int i = 0; i < Vector512<TFloat>.Count; i++)
            {
                // Do NOT invert this if.
                // We want the branch predictor to prefer the non-NaN branch.
                if (!TFloat.IsNaN(Unsafe.BitCast<TInteger, TFloat>(max[i])))
                {
                    if (min[i] < minValue)
                        minValue = min[i];

                    continue;
                }

                return Unsafe.BitCast<TInteger, TFloat>(max[i]);
            }
            return minValue;
        }
    }

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

        if (typeof(T) == typeof(nint) && comparer is null)
            return (T)(object)Extremum<nint, Minimum<nint>>(ref Unsafe.As<T, nint>(ref searchSpace), length);

        if (typeof(T) == typeof(nuint) && comparer is null)
            return (T)(object)Extremum<nuint, Minimum<nuint>>(ref Unsafe.As<T, nuint>(ref searchSpace), length);

        if (typeof(T) == typeof(float) && comparer is null)
            return (T)(object)MinFloat<float, uint>(ref Unsafe.As<T, float>(ref searchSpace), length);

        if (typeof(T) == typeof(double) && comparer is null)
            return (T)(object)MinFloat<double, ulong>(ref Unsafe.As<T, double>(ref searchSpace), length);

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
        T minValue = searchSpace;

        // Instead of `typeof(T).IsValueType`, test for reference types and
        // nullable value types.
        // `Enumerable.Min` filters out null values for some reason, even though
        // they are considered the smallest possible value everywhere else.
        if (default(T) is not null)
        {
            if (comparer is null)
            {
                for (; i < end; i++)
                {
                    T currentValue = Unsafe.Add(ref searchSpace, i);

                    // Let JIT de-virtualize `Compare` calls for value types.
                    if (Comparer<T>.Default.Compare(currentValue, minValue) < 0)
                        minValue = currentValue;
                }
            }
            else
            {
                for (; i < end; i++)
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

            for (; minValue is null && i < end; i++)
                minValue = Unsafe.Add(ref searchSpace, i);

            for (; i < end; i++)
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
    private readonly struct Minimum<T> : IExtremum<T> where T : struct, IBinaryNumber<T>
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
        public static bool Compare(T left, T right) => left < right;

        /// <summary>
        /// Computes the minimum of two vectors on a per-element basis.
        /// </summary>
        /// <param name="left">The vector to compare with <paramref name="right"/>.</param>
        /// <param name="right">The vector to compare with <paramref name="left"/>.</param>
        /// <returns>
        /// A vector whose elements are the minimum of the corresponding elements in left and right.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> Compare(Vector128<T> left, Vector128<T> right) => Vector128.Min(left, right);

        /// <inheritdoc cref="Compare(Vector128{T}, Vector128{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<T> Compare(Vector256<T> left, Vector256<T> right) => Vector256.Min(left, right);

        /// <inheritdoc cref="Compare(Vector128{T}, Vector128{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<T> Compare(Vector512<T> left, Vector512<T> right) => Vector512.Min(left, right);
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
    /// Returns the minimum value in a span of <see cref="nint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="nint"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    public static nint Min(this scoped Span<nint> span) => Extremum<nint, Minimum<nint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{nint})"/>
    public static nint Min(this scoped ReadOnlySpan<nint> span) => Extremum<nint, Minimum<nint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="nuint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="nuint"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    [CLSCompliant(false)]
    public static nuint Min(this scoped Span<nuint> span) => Extremum<nuint, Minimum<nuint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{nuint})"/>
    [CLSCompliant(false)]
    public static nuint Min(this scoped ReadOnlySpan<nuint> span) => Extremum<nuint, Minimum<nuint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="float"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="float"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    public static float Min(this scoped Span<float> span) => MinFloat<float, uint>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{float})"/>
    public static float Min(this scoped ReadOnlySpan<float> span) => MinFloat<float, uint>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Returns the minimum value in a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to determine the minimum value of.</param>
    /// <inheritdoc cref="Min(Span{byte})"/>
    public static double Min(this scoped Span<double> span) => MinFloat<double, ulong>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Min(Span{double})"/>
    public static double Min(this scoped ReadOnlySpan<double> span) => MinFloat<double, ulong>(ref MemoryMarshal.GetReference(span), span.Length);

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
