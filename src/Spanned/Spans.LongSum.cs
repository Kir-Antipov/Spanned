namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// A vectorized solution to compute the sum of the values in the specified memory block.
    /// </summary>
    /// <typeparam name="TSource">The type of the values.</typeparam>
    /// <typeparam name="TAccumulator">The type used for intermediate accumulation.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TVectorAddition">The type defines a mechanism for computing the sum of two vectors.</typeparam>
    /// <typeparam name="TVectorSummation">The type defines a mechanism for computing the sum of all elements in a vector.</typeparam>
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <returns>The sum of the values in the memory block.</returns>
    /// <exception cref="NotSupportedException"><typeparamref name="TSource"/> or <typeparamref name="TAccumulator"/> is not supported.</exception>
    private static TResult LongSum<TSource, TAccumulator, TResult, TVectorAddition, TVectorSummation>(ref TSource searchSpace, int length)
        where TSource : struct, IBinaryNumber<TSource>
        where TAccumulator : struct, IBinaryNumber<TAccumulator>
        where TResult : struct, IBinaryNumber<TResult>
        where TVectorAddition : IVectorAddition<TAccumulator, TSource, TAccumulator>
        where TVectorSummation : IVectorSummation<TAccumulator, TResult>
    {
        Debug.Assert(Vector<TSource>.IsSupported && Vector<TAccumulator>.IsSupported);

        TResult sum = TResult.Zero;
        ref TSource current = ref searchSpace;
        ref TSource end = ref Unsafe.Add(ref current, length);

        if (!Vector128.IsHardwareAccelerated || length < Vector128<TSource>.Count)
        {
            // The operation cannot be vectorized.
            // Skip to the manual loop.
        }
        else if (!Vector256.IsHardwareAccelerated || length < Vector256<TSource>.Count)
        {
            Vector128<TAccumulator> sums = Vector128<TAccumulator>.Zero;
            ref TSource lastVectorStart = ref Unsafe.Add(ref current, length - Vector128<TSource>.Count);

            do
            {
                sums = TVectorAddition.Add(sums, Vector128.LoadUnsafe(ref current));
                current = ref Unsafe.Add(ref current, (nint)Vector128<TSource>.Count);
            }
            while (!Unsafe.IsAddressGreaterThan(ref current, ref lastVectorStart));

            sum = TVectorSummation.Sum(sums);
        }
        else if (!Vector512.IsHardwareAccelerated || length < Vector512<TSource>.Count)
        {
            Vector256<TAccumulator> sums = Vector256<TAccumulator>.Zero;
            ref TSource lastVectorStart = ref Unsafe.Add(ref current, length - Vector256<TSource>.Count);

            do
            {
                sums = TVectorAddition.Add(sums, Vector256.LoadUnsafe(ref current));
                current = ref Unsafe.Add(ref current, (nint)Vector256<TSource>.Count);
            }
            while (!Unsafe.IsAddressGreaterThan(ref current, ref lastVectorStart));

            sum = TVectorSummation.Sum(sums);
        }
        else
        {
            Vector512<TAccumulator> sums = Vector512<TAccumulator>.Zero;
            ref TSource lastVectorStart = ref Unsafe.Add(ref current, length - Vector512<TSource>.Count);

            do
            {
                sums = TVectorAddition.Add(sums, Vector512.LoadUnsafe(ref current));
                current = ref Unsafe.Add(ref current, (nint)Vector512<TSource>.Count);
            }
            while (!Unsafe.IsAddressGreaterThan(ref current, ref lastVectorStart));

            sum = TVectorSummation.Sum(sums);
        }

        while (Unsafe.IsAddressLessThan(ref current, ref end))
        {
            sum += TResult.CreateChecked(current);
            current = ref Unsafe.Add(ref current, (nint)1);
        }

        return sum;
    }

    /// <summary>
    /// A vectorized solution to compute the sum of the values in the specified memory block.
    /// </summary>
    /// <typeparam name="TSource">The type of the values.</typeparam>
    /// <typeparam name="TAccumulator1">The most optimal type used for intermediate accumulation.</typeparam>
    /// <typeparam name="TAccumulator2">
    /// The least optimal type used for intermediate accumulation.
    /// However, it's guaranteed to be reliable.
    /// </typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TVectorAddition1">The type defines a mechanism for computing the sum of two vectors.</typeparam>
    /// <typeparam name="TVectorSummation1">The type defines a mechanism for computing the sum of all elements in a vector.</typeparam>
    /// <typeparam name="TVectorAddition2">The type defines a mechanism for computing the sum of two vectors.</typeparam>
    /// <typeparam name="TVectorSummation2">The type defines a mechanism for computing the sum of all elements in a vector.</typeparam>
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <returns>The sum of the values in the memory block.</returns>
    /// <exception cref="NotSupportedException">
    /// <typeparamref name="TSource"/>, or <typeparamref name="TAccumulator1"/>, or
    /// <typeparamref name="TAccumulator2"/> is not supported.
    /// </exception>
    private static TResult LongSum<
            TSource, TAccumulator1, TAccumulator2, TResult,
            TVectorAddition1, TVectorSummation1,
            TVectorAddition2, TVectorSummation2>(ref TSource searchSpace, int length)
        where TSource : struct, IBinaryNumber<TSource>, IMinMaxValue<TSource>
        where TAccumulator1 : struct, IBinaryNumber<TAccumulator1>, IMinMaxValue<TAccumulator1>
        where TAccumulator2 : struct, IBinaryNumber<TAccumulator2>, IMinMaxValue<TAccumulator2>
        where TResult : struct, IBinaryNumber<TResult>
        where TVectorAddition1 : IVectorAddition<TAccumulator1, TSource, TAccumulator1>
        where TVectorSummation1 : IVectorSummation<TAccumulator1, TResult>
        where TVectorAddition2 : IVectorAddition<TAccumulator2, TSource, TAccumulator2>
        where TVectorSummation2 : IVectorSummation<TAccumulator2, TResult>
    {
        // See the next overload for more information on how and why this works.

        Debug.Assert(Unsafe.SizeOf<TAccumulator1>() < Unsafe.SizeOf<TAccumulator2>());

        if (IsSafeAccumulator<TSource, TAccumulator1>(length))
        {
            return LongSum<TSource, TAccumulator1, TResult, TVectorAddition1, TVectorSummation1>(ref searchSpace, length);
        }
        else
        {
            return LongSum<TSource, TAccumulator2, TResult, TVectorAddition2, TVectorSummation2>(ref searchSpace, length);
        }
    }

    /// <summary>
    /// A vectorized solution to compute the sum of the values in the specified memory block.
    /// </summary>
    /// <typeparam name="TSource">The type of the values.</typeparam>
    /// <typeparam name="TAccumulator1">The most optimal type used for intermediate accumulation.</typeparam>
    /// <typeparam name="TAccumulator2">
    /// The type used for intermediate accumulation that is slightly less optimal than
    /// <typeparamref name="TAccumulator1"/>, but is still decent enough.
    /// </typeparam>
    /// <typeparam name="TAccumulator3">
    /// The least optimal type used for intermediate accumulation.
    /// However, it's guaranteed to be reliable.
    /// </typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <typeparam name="TVectorAddition1">The type defines a mechanism for computing the sum of two vectors.</typeparam>
    /// <typeparam name="TVectorSummation1">The type defines a mechanism for computing the sum of all elements in a vector.</typeparam>
    /// <typeparam name="TVectorAddition2">The type defines a mechanism for computing the sum of two vectors.</typeparam>
    /// <typeparam name="TVectorSummation2">The type defines a mechanism for computing the sum of all elements in a vector.</typeparam>
    /// <typeparam name="TVectorAddition3">The type defines a mechanism for computing the sum of two vectors.</typeparam>
    /// <typeparam name="TVectorSummation3">The type defines a mechanism for computing the sum of all elements in a vector.</typeparam>
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <returns>The sum of the values in the memory block.</returns>
    /// <exception cref="NotSupportedException">
    /// <typeparamref name="TSource"/>, or <typeparamref name="TAccumulator1"/>, or
    /// <typeparamref name="TAccumulator2"/>, or <typeparamref name="TAccumulator3"/>
    /// is not supported.
    /// </exception>
    private static TResult LongSum<
            TSource, TAccumulator1, TAccumulator2, TAccumulator3, TResult,
            TVectorAddition1, TVectorSummation1,
            TVectorAddition2, TVectorSummation2,
            TVectorAddition3, TVectorSummation3>(ref TSource searchSpace, int length)
        where TSource : struct, IBinaryNumber<TSource>, IMinMaxValue<TSource>
        where TAccumulator1 : struct, IBinaryNumber<TAccumulator1>, IMinMaxValue<TAccumulator1>
        where TAccumulator2 : struct, IBinaryNumber<TAccumulator2>, IMinMaxValue<TAccumulator2>
        where TAccumulator3 : struct, IBinaryNumber<TAccumulator3>, IMinMaxValue<TAccumulator3>
        where TResult : struct, IBinaryNumber<TResult>
        where TVectorAddition1 : IVectorAddition<TAccumulator1, TSource, TAccumulator1>
        where TVectorSummation1 : IVectorSummation<TAccumulator1, TResult>
        where TVectorAddition2 : IVectorAddition<TAccumulator2, TSource, TAccumulator2>
        where TVectorSummation2 : IVectorSummation<TAccumulator2, TResult>
        where TVectorAddition3 : IVectorAddition<TAccumulator3, TSource, TAccumulator3>
        where TVectorSummation3 : IVectorSummation<TAccumulator3, TResult>
    {
        // The logic behind the entire `LongSum` concept lies in the fact that we can safely
        // compute the sum for every integer type using a 64-bit integer as an accumulator
        // (well, except for the 64-bit integers themselves).
        //
        // The condition `(long)uint.MaxValue * (long)length < long.MaxValue` is always `true`,
        // since `length` cannot exceed `int.MaxValue`.
        //
        // The issue here is that widening a byte vector to a long vector is a relatively expensive
        // operation, which compromises the performance benefits offered by vectorization.
        // To optimize this, we can extend the original concept. For instance, we can determine
        // how many 8-bit integers can reliably fit into a 16-bit integer:
        // `ushort.MaxValue / byte.MaxValue == 257`. While this might seem like not a lot, operating
        // on vectors changes the dynamics. A 128-bit vector can accommodate 8 16-bit integers, each
        // operating as a standalone, independent accumulator (that's the whole point of vectorization),
        // resulting in `257 * 8 == 2056` (or even 4112 with 256-bit vectors) 8-bit integers that we can
        // process using only a single widening operation.
        //
        // Being able to process 2056 or 4112 values covers a significant range, considering that 1024,
        // 2048, and 4096 are all common sizes for byte buffers. In case we encounter a larger size, we
        // can utilize our favorite 32-bit integers as accumulators, providing us with
        // `uint.MaxValue / byte.MaxValue * Vector128<uint>.Count == 67372036` (or 134744072 with 256-bit
        // vectors) 8-bit integers that we can process using 2 widening operations per iteration.
        //
        // Finally, if someone is processing literally billions of bytes, we fallback to
        // the reliable 64-bit accumulator, because, as mentioned before, it can handle it all.
        // Even if it involves 3 widening operations per iteration, it remains slightly faster
        // than a regular loop.

        Debug.Assert(Unsafe.SizeOf<TAccumulator1>() < Unsafe.SizeOf<TAccumulator2>());
        Debug.Assert(Unsafe.SizeOf<TAccumulator2>() < Unsafe.SizeOf<TAccumulator3>());

        if (IsSafeAccumulator<TSource, TAccumulator1>(length))
        {
            return LongSum<TSource, TAccumulator1, TResult, TVectorAddition1, TVectorSummation1>(ref searchSpace, length);
        }
        else if (IsSafeAccumulator<TSource, TAccumulator2>(length))
        {
            return LongSum<TSource, TAccumulator2, TResult, TVectorAddition2, TVectorSummation2>(ref searchSpace, length);
        }
        else
        {
            return LongSum<TSource, TAccumulator3, TResult, TVectorAddition3, TVectorSummation3>(ref searchSpace, length);
        }
    }

    /// <summary>
    /// Computes the sum of the values in the specified memory block.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source span.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <returns>The sum of the values in the memory block.</returns>
    /// <exception cref="OverflowException">The addition operation in a checked context resulted in an overflow.</exception>
    private static TResult LongSum<TSource, TResult>(ref TSource searchSpace, int length)
        where TSource : INumberBase<TSource>
        where TResult : INumberBase<TResult>
    {
        if (typeof(TSource) == typeof(TResult))
            return (TResult)(object)Sum(ref searchSpace, length);

        if (typeof(TSource) == typeof(byte) && typeof(TResult) == typeof(ulong))
            return (TResult)(object)LongSum<byte, ushort, uint, ulong, ulong, UInt16ByteVectorAddition, UInt16UInt64VectorSummation, UInt32ByteVectorAddition, UInt32UInt64VectorSummation, UInt64ByteVectorAddition, VectorSummation<ulong>>(ref Unsafe.As<TSource, byte>(ref searchSpace), length);

        if (typeof(TSource) == typeof(sbyte) && typeof(TResult) == typeof(long))
            return (TResult)(object)LongSum<sbyte, short, int, long, long, Int16SByteVectorAddition, Int16Int64VectorSummation, Int32SByteVectorAddition, Int32Int64VectorSummation, Int64SByteVectorAddition, VectorSummation<long>>(ref Unsafe.As<TSource, sbyte>(ref searchSpace), length);

        if (typeof(TSource) == typeof(short) && typeof(TResult) == typeof(long))
            return (TResult)(object)LongSum<short, int, long, long, Int32Int16VectorAddition, Int32Int64VectorSummation, Int64Int16VectorAddition, VectorSummation<long>>(ref Unsafe.As<TSource, short>(ref searchSpace), length);

        if (typeof(TSource) == typeof(ushort) && typeof(TResult) == typeof(ulong))
            return (TResult)(object)LongSum<ushort, uint, ulong, ulong, UInt32UInt16VectorAddition, UInt32UInt64VectorSummation, UInt64UInt16VectorAddition, VectorSummation<ulong>>(ref Unsafe.As<TSource, ushort>(ref searchSpace), length);

        if (typeof(TSource) == typeof(int) && typeof(TResult) == typeof(long))
            return (TResult)(object)LongSum<int, long, long, Int64Int32VectorAddition, VectorSummation<long>>(ref Unsafe.As<TSource, int>(ref searchSpace), length);

        if (typeof(TSource) == typeof(uint) && typeof(TResult) == typeof(ulong))
            return (TResult)(object)LongSum<uint, ulong, ulong, UInt64UInt32VectorAddition, VectorSummation<ulong>>(ref Unsafe.As<TSource, uint>(ref searchSpace), length);

        if (typeof(TSource) == typeof(nint) && typeof(TResult) == typeof(long))
            return (TResult)(object)(Unsafe.SizeOf<nint>() == sizeof(int) ? LongSum<int, long, long, Int64Int32VectorAddition, VectorSummation<long>>(ref Unsafe.As<TSource, int>(ref searchSpace), length) : (long)Sum<nint, SignedIntegerOverflowTracker<nint>>(ref Unsafe.As<TSource, nint>(ref searchSpace), length));

        if (typeof(TSource) == typeof(nuint) && typeof(TResult) == typeof(ulong))
            return (TResult)(object)(Unsafe.SizeOf<nuint>() == sizeof(uint) ? LongSum<uint, ulong, ulong, UInt64UInt32VectorAddition, VectorSummation<ulong>>(ref Unsafe.As<TSource, uint>(ref searchSpace), length) : (ulong)Sum<nuint, UnsignedIntegerOverflowTracker<nuint>>(ref Unsafe.As<TSource, nuint>(ref searchSpace), length));

        if (typeof(TSource) == typeof(float) && typeof(TResult) == typeof(double))
            return (TResult)(object)LongSum<float, double, double, DoubleSingleVectorAddition, VectorSummation<double>>(ref Unsafe.As<TSource, float>(ref searchSpace), length);

        TResult sum = TResult.Zero;
        nint end = (nint)length;

        for (nint i = 0; i < end; i++)
            sum = checked(sum + TResult.CreateChecked(Unsafe.Add(ref searchSpace, i)));

        return sum;
    }

    /// <summary>
    /// Computes the sum of a span of <see cref="byte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="byte"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped Span<byte> span) => LongSum<byte, ushort, uint, ulong, ulong, UInt16ByteVectorAddition, UInt16UInt64VectorSummation, UInt32ByteVectorAddition, UInt32UInt64VectorSummation, UInt64ByteVectorAddition, VectorSummation<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped ReadOnlySpan<byte> span) => LongSum<byte, ushort, uint, ulong, ulong, UInt16ByteVectorAddition, UInt16UInt64VectorSummation, UInt32ByteVectorAddition, UInt32UInt64VectorSummation, UInt64ByteVectorAddition, VectorSummation<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="sbyte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="sbyte"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    [CLSCompliant(false)]
    public static long LongSum(this scoped Span<sbyte> span) => LongSum<sbyte, short, int, long, long, Int16SByteVectorAddition, Int16Int64VectorSummation, Int32SByteVectorAddition, Int32Int64VectorSummation, Int64SByteVectorAddition, VectorSummation<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{sbyte})"/>
    [CLSCompliant(false)]
    public static long LongSum(this scoped ReadOnlySpan<sbyte> span) => LongSum<sbyte, short, int, long, long, Int16SByteVectorAddition, Int16Int64VectorSummation, Int32SByteVectorAddition, Int32Int64VectorSummation, Int64SByteVectorAddition, VectorSummation<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    public static long LongSum(this scoped Span<short> span) => LongSum<short, int, long, long, Int32Int16VectorAddition, Int32Int64VectorSummation, Int64Int16VectorAddition, VectorSummation<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{short})"/>
    public static long LongSum(this scoped ReadOnlySpan<short> span) => LongSum<short, int, long, long, Int32Int16VectorAddition, Int32Int64VectorSummation, Int64Int16VectorAddition, VectorSummation<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ushort"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ushort"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped Span<ushort> span) => LongSum<ushort, uint, ulong, ulong, UInt32UInt16VectorAddition, UInt32UInt64VectorSummation, UInt64UInt16VectorAddition, VectorSummation<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{ushort})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped ReadOnlySpan<ushort> span) => LongSum<ushort, uint, ulong, ulong, UInt32UInt16VectorAddition, UInt32UInt64VectorSummation, UInt64UInt16VectorAddition, VectorSummation<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="int"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="int"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    public static long LongSum(this scoped Span<int> span) => LongSum<int, long, long, Int64Int32VectorAddition, VectorSummation<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{int})"/>
    public static long LongSum(this scoped ReadOnlySpan<int> span) => LongSum<int, long, long, Int64Int32VectorAddition, VectorSummation<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="uint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="uint"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped Span<uint> span) => LongSum<uint, ulong, ulong, UInt64UInt32VectorAddition, VectorSummation<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{uint})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped ReadOnlySpan<uint> span) => LongSum<uint, ulong, ulong, UInt64UInt32VectorAddition, VectorSummation<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="long"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="long"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    /// <exception cref="OverflowException">The addition operation in a checked context resulted in an overflow.</exception>
    public static long LongSum(this scoped Span<long> span) => Sum<long, SignedIntegerOverflowTracker<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{long})"/>
    public static long LongSum(this scoped ReadOnlySpan<long> span) => Sum<long, SignedIntegerOverflowTracker<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ulong"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ulong"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="LongSum(Span{long})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped Span<ulong> span) => Sum<ulong, UnsignedIntegerOverflowTracker<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{ulong})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped ReadOnlySpan<ulong> span) => Sum<ulong, UnsignedIntegerOverflowTracker<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="nint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="nint"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="LongSum(Span{long})"/>
    public static long LongSum(this scoped Span<nint> span) => Unsafe.SizeOf<nint>() == sizeof(int) ? LongSum<int, long, long, Int64Int32VectorAddition, VectorSummation<long>>(ref Unsafe.As<nint, int>(ref MemoryMarshal.GetReference(span)), span.Length) : (long)Sum<nint, SignedIntegerOverflowTracker<nint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{nint})"/>
    public static long LongSum(this scoped ReadOnlySpan<nint> span) => Unsafe.SizeOf<nint>() == sizeof(int) ? LongSum<int, long, long, Int64Int32VectorAddition, VectorSummation<long>>(ref Unsafe.As<nint, int>(ref MemoryMarshal.GetReference(span)), span.Length) : (long)Sum<nint, SignedIntegerOverflowTracker<nint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="nuint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="nuint"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="LongSum(Span{long})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped Span<nuint> span) => Unsafe.SizeOf<nuint>() == sizeof(uint) ? LongSum<uint, ulong, ulong, UInt64UInt32VectorAddition, VectorSummation<ulong>>(ref Unsafe.As<nuint, uint>(ref MemoryMarshal.GetReference(span)), span.Length) : (ulong)Sum<nuint, UnsignedIntegerOverflowTracker<nuint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{nuint})"/>
    [CLSCompliant(false)]
    public static ulong LongSum(this scoped ReadOnlySpan<nuint> span) => Unsafe.SizeOf<nuint>() == sizeof(uint) ? LongSum<uint, ulong, ulong, UInt64UInt32VectorAddition, VectorSummation<ulong>>(ref Unsafe.As<nuint, uint>(ref MemoryMarshal.GetReference(span)), span.Length) : (ulong)Sum<nuint, UnsignedIntegerOverflowTracker<nuint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="float"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="float"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    public static double LongSum(this scoped Span<float> span) => LongSum<float, double, double, DoubleSingleVectorAddition, VectorSummation<double>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{float})"/>
    public static double LongSum(this scoped ReadOnlySpan<float> span) => LongSum<float, double, double, DoubleSingleVectorAddition, VectorSummation<double>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    public static double LongSum(this scoped Span<double> span) => Sum<double, NullOverflowTracker<double>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{double})"/>
    public static double LongSum(this scoped ReadOnlySpan<double> span) => Sum<double, NullOverflowTracker<double>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="decimal"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="decimal"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="LongSum(Span{long})"/>
    public static decimal LongSum(this scoped Span<decimal> span) => Sum(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum(Span{decimal})"/>
    public static decimal LongSum(this scoped ReadOnlySpan<decimal> span) => Sum(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of generic values.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source span.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="span">A span of generic values to calculate the sum of.</param>
    /// <inheritdoc cref="LongSum(Span{long})"/>
    public static TResult LongSum<TSource, TResult>(this scoped Span<TSource> span) where TSource : INumberBase<TSource> where TResult : INumberBase<TResult> => LongSum<TSource, TResult>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="LongSum{TSource, TResult}(Span{TSource})"/>
    public static TResult LongSum<TSource, TResult>(this scoped ReadOnlySpan<TSource> span) where TSource : INumberBase<TSource> where TResult : INumberBase<TResult> => LongSum<TSource, TResult>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Determines if the specified accumulator type can be safely used
    /// to process the data of the given length.
    /// </summary>
    /// <typeparam name="TSource">The type of the values.</typeparam>
    /// <typeparam name="TAccumulator">The type used for intermediate accumulation.</typeparam>
    /// <param name="length">The length of the data to be accumulated.</param>
    /// <returns>
    /// <c>true</c> if the specified accumulator type can be safely used; otherwise, <c>false</c>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSafeAccumulator<TSource, TAccumulator>(int length)
        where TSource : struct, IBinaryNumber<TSource>, IMinMaxValue<TSource>
        where TAccumulator : struct, IBinaryNumber<TAccumulator>, IMinMaxValue<TAccumulator>
    {
        int vectorCount;
        if (!Vector128.IsHardwareAccelerated || length < Vector128<TSource>.Count)
        {
            vectorCount = 1;
        }
        else if (!Vector256.IsHardwareAccelerated || length < Vector256<TSource>.Count)
        {
            vectorCount = Vector128<TAccumulator>.Count;
        }
        else if (!Vector512.IsHardwareAccelerated || length < Vector512<TSource>.Count)
        {
            vectorCount = Vector256<TAccumulator>.Count;
        }
        else
        {
            vectorCount = Vector512<TAccumulator>.Count;
        }

        TAccumulator perLaneLength = TAccumulator.CreateChecked(length / vectorCount);
        TAccumulator maxPerLaneLength = TAccumulator.MaxValue / TAccumulator.CreateChecked(TSource.MaxValue);
        return perLaneLength <= maxPerLaneLength;
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of two vectors.
    /// </summary>
    /// <typeparam name="TLeft">The element type of the left operand vector.</typeparam>
    /// <typeparam name="TRight">The element type of the right operand vector.</typeparam>
    /// <typeparam name="TResult">The element type of the result vector.</typeparam>
    private interface IVectorAddition<TLeft, TRight, TResult>
    {
        /// <summary>
        /// Adds two vectors to compute their sum.
        /// </summary>
        /// <param name="left">The vector to add with <paramref name="right"/>.</param>
        /// <param name="right">The vector to add with <paramref name="left"/>.</param>
        /// <returns>The sum of <paramref name="left"/> and <paramref name="right"/>.</returns>
        static abstract Vector128<TResult> Add(Vector128<TLeft> left, Vector128<TRight> right);

        /// <inheritdoc cref="Add(Vector128{TLeft}, Vector128{TRight})"/>
        static abstract Vector256<TResult> Add(Vector256<TLeft> left, Vector256<TRight> right);

        /// <inheritdoc cref="Add(Vector128{TLeft}, Vector128{TRight})"/>
        static abstract Vector512<TResult> Add(Vector512<TLeft> left, Vector512<TRight> right);
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of all elements in a vector.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source vector.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the result vector.</typeparam>
    private interface IVectorSummation<TSource, TResult>
    {
        /// <summary>
        /// Computes the sum of all elements in a vector.
        /// </summary>
        /// <param name="vector">The vector whose elements will be summed.</param>
        /// <returns>The sum of all elements in vector.</returns>
        static abstract TResult Sum(Vector128<TSource> vector);

        /// <inheritdoc cref="Sum(Vector128{TSource})"/>
        static abstract TResult Sum(Vector256<TSource> vector);

        /// <inheritdoc cref="Sum(Vector128{TSource})"/>
        static abstract TResult Sum(Vector512<TSource> vector);
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of two vectors.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the vector.</typeparam>
    private readonly struct VectorAddition<T> : IVectorAddition<T, T, T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> Add(Vector128<T> left, Vector128<T> right) => left + right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<T> Add(Vector256<T> left, Vector256<T> right) => left + right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<T> Add(Vector512<T> left, Vector512<T> right) => left + right;
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of all elements in a vector.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the vector.</typeparam>
    private readonly struct VectorSummation<T> : IVectorSummation<T, T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Sum(Vector128<T> vector) => Vector128.Sum(vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Sum(Vector256<T> vector) => Vector256.Sum(vector);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Sum(Vector512<T> vector) => Vector512.Sum(vector);
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="ushort"/> and
    /// <see cref="byte"/> vectors.
    /// </summary>
    private readonly struct UInt16ByteVectorAddition : IVectorAddition<ushort, byte, ushort>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<ushort> Add(Vector128<ushort> left, Vector128<byte> right)
            => left + Vector128.WidenLower(right) + Vector128.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<ushort> Add(Vector256<ushort> left, Vector256<byte> right)
            => left + Vector256.WidenLower(right) + Vector256.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<ushort> Add(Vector512<ushort> left, Vector512<byte> right)
            => left + Vector512.WidenLower(right) + Vector512.WidenUpper(right);
    }

    /// <summary>
    /// Defines a mechanism for computing <see cref="ulong"/> sum of
    /// <see cref="ushort"/> elements in a vector.
    /// </summary>
    private readonly struct UInt16UInt64VectorSummation : IVectorSummation<ushort, ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Sum(Vector128<ushort> vector)
            => Vector128.Sum(Vector128.WidenLower(vector) + Vector128.WidenUpper(vector));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Sum(Vector256<ushort> vector)
            => Vector256.Sum(Vector256.WidenLower(vector) + Vector256.WidenUpper(vector));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Sum(Vector512<ushort> vector)
            => Vector512.Sum(Vector512.WidenLower(vector) + Vector512.WidenUpper(vector));
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="uint"/> and
    /// <see cref="byte"/> vectors.
    /// </summary>
    private readonly struct UInt32ByteVectorAddition : IVectorAddition<uint, byte, uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<uint> Add(Vector128<uint> left, Vector128<byte> right)
        {
            Vector128<ushort> rightUInt16 = Vector128.WidenLower(right) + Vector128.WidenUpper(right);
            return left + Vector128.WidenLower(rightUInt16) + Vector128.WidenUpper(rightUInt16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<uint> Add(Vector256<uint> left, Vector256<byte> right)
        {
            Vector256<ushort> rightUInt16 = Vector256.WidenLower(right) + Vector256.WidenUpper(right);
            return left + Vector256.WidenLower(rightUInt16) + Vector256.WidenUpper(rightUInt16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<uint> Add(Vector512<uint> left, Vector512<byte> right)
        {
            Vector512<ushort> rightUInt16 = Vector512.WidenLower(right) + Vector512.WidenUpper(right);
            return left + Vector512.WidenLower(rightUInt16) + Vector512.WidenUpper(rightUInt16);
        }
    }

    /// <summary>
    /// Defines a mechanism for computing <see cref="ulong"/> sum of
    /// <see cref="uint"/> elements in a vector.
    /// </summary>
    private readonly struct UInt32UInt64VectorSummation : IVectorSummation<uint, ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Sum(Vector128<uint> vector)
            => Vector128.Sum(Vector128.WidenLower(vector) + Vector128.WidenUpper(vector));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Sum(Vector256<uint> vector)
            => Vector256.Sum(Vector256.WidenLower(vector) + Vector256.WidenUpper(vector));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Sum(Vector512<uint> vector)
            => Vector512.Sum(Vector512.WidenLower(vector) + Vector512.WidenUpper(vector));
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="ulong"/> and
    /// <see cref="byte"/> vectors.
    /// </summary>
    private readonly struct UInt64ByteVectorAddition : IVectorAddition<ulong, byte, ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<ulong> Add(Vector128<ulong> left, Vector128<byte> right)
        {
            Vector128<ushort> rightUInt16 = Vector128.WidenLower(right) + Vector128.WidenUpper(right);
            Vector128<uint> rightUInt32 = Vector128.WidenLower(rightUInt16) + Vector128.WidenUpper(rightUInt16);
            return left + Vector128.WidenLower(rightUInt32) + Vector128.WidenUpper(rightUInt32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<ulong> Add(Vector256<ulong> left, Vector256<byte> right)
        {
            Vector256<ushort> rightUInt16 = Vector256.WidenLower(right) + Vector256.WidenUpper(right);
            Vector256<uint> rightUInt32 = Vector256.WidenLower(rightUInt16) + Vector256.WidenUpper(rightUInt16);
            return left + Vector256.WidenLower(rightUInt32) + Vector256.WidenUpper(rightUInt32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<ulong> Add(Vector512<ulong> left, Vector512<byte> right)
        {
            Vector512<ushort> rightUInt16 = Vector512.WidenLower(right) + Vector512.WidenUpper(right);
            Vector512<uint> rightUInt32 = Vector512.WidenLower(rightUInt16) + Vector512.WidenUpper(rightUInt16);
            return left + Vector512.WidenLower(rightUInt32) + Vector512.WidenUpper(rightUInt32);
        }
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="short"/> and
    /// <see cref="sbyte"/> vectors.
    /// </summary>
    private readonly struct Int16SByteVectorAddition : IVectorAddition<short, sbyte, short>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<short> Add(Vector128<short> left, Vector128<sbyte> right)
            => left + Vector128.WidenLower(right) + Vector128.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<short> Add(Vector256<short> left, Vector256<sbyte> right)
            => left + Vector256.WidenLower(right) + Vector256.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<short> Add(Vector512<short> left, Vector512<sbyte> right)
            => left + Vector512.WidenLower(right) + Vector512.WidenUpper(right);
    }

    /// <summary>
    /// Defines a mechanism for computing <see cref="long"/> sum of
    /// <see cref="short"/> elements in a vector.
    /// </summary>
    private readonly struct Int16Int64VectorSummation : IVectorSummation<short, long>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Sum(Vector128<short> vector)
            => Vector128.Sum(Vector128.WidenLower(vector) + Vector128.WidenUpper(vector));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Sum(Vector256<short> vector)
            => Vector256.Sum(Vector256.WidenLower(vector) + Vector256.WidenUpper(vector));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Sum(Vector512<short> vector)
            => Vector512.Sum(Vector512.WidenLower(vector) + Vector512.WidenUpper(vector));
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="int"/> and
    /// <see cref="sbyte"/> vectors.
    /// </summary>
    private readonly struct Int32SByteVectorAddition : IVectorAddition<int, sbyte, int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<int> Add(Vector128<int> left, Vector128<sbyte> right)
        {
            Vector128<short> rightInt16 = Vector128.WidenLower(right) + Vector128.WidenUpper(right);
            return left + Vector128.WidenLower(rightInt16) + Vector128.WidenUpper(rightInt16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<int> Add(Vector256<int> left, Vector256<sbyte> right)
        {
            Vector256<short> rightInt16 = Vector256.WidenLower(right) + Vector256.WidenUpper(right);
            return left + Vector256.WidenLower(rightInt16) + Vector256.WidenUpper(rightInt16);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<int> Add(Vector512<int> left, Vector512<sbyte> right)
        {
            Vector512<short> rightInt16 = Vector512.WidenLower(right) + Vector512.WidenUpper(right);
            return left + Vector512.WidenLower(rightInt16) + Vector512.WidenUpper(rightInt16);
        }
    }

    /// <summary>
    /// Defines a mechanism for computing <see cref="long"/> sum of
    /// <see cref="int"/> elements in a vector.
    /// </summary>
    private readonly struct Int32Int64VectorSummation : IVectorSummation<int, long>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Sum(Vector128<int> vector)
            => Vector128.Sum(Vector128.WidenLower(vector) + Vector128.WidenUpper(vector));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Sum(Vector256<int> vector)
            => Vector256.Sum(Vector256.WidenLower(vector) + Vector256.WidenUpper(vector));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Sum(Vector512<int> vector)
            => Vector512.Sum(Vector512.WidenLower(vector) + Vector512.WidenUpper(vector));
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="long"/> and
    /// <see cref="sbyte"/> vectors.
    /// </summary>
    private readonly struct Int64SByteVectorAddition : IVectorAddition<long, sbyte, long>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<long> Add(Vector128<long> left, Vector128<sbyte> right)
        {
            Vector128<short> rightInt16 = Vector128.WidenLower(right) + Vector128.WidenUpper(right);
            Vector128<int> rightInt32 = Vector128.WidenLower(rightInt16) + Vector128.WidenUpper(rightInt16);
            return left + Vector128.WidenLower(rightInt32) + Vector128.WidenUpper(rightInt32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<long> Add(Vector256<long> left, Vector256<sbyte> right)
        {
            Vector256<short> rightInt16 = Vector256.WidenLower(right) + Vector256.WidenUpper(right);
            Vector256<int> rightInt32 = Vector256.WidenLower(rightInt16) + Vector256.WidenUpper(rightInt16);
            return left + Vector256.WidenLower(rightInt32) + Vector256.WidenUpper(rightInt32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<long> Add(Vector512<long> left, Vector512<sbyte> right)
        {
            Vector512<short> rightInt16 = Vector512.WidenLower(right) + Vector512.WidenUpper(right);
            Vector512<int> rightInt32 = Vector512.WidenLower(rightInt16) + Vector512.WidenUpper(rightInt16);
            return left + Vector512.WidenLower(rightInt32) + Vector512.WidenUpper(rightInt32);
        }
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="int"/> and
    /// <see cref="short"/> vectors.
    /// </summary>
    private readonly struct Int32Int16VectorAddition : IVectorAddition<int, short, int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<int> Add(Vector128<int> left, Vector128<short> right)
            => left + Vector128.WidenLower(right) + Vector128.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<int> Add(Vector256<int> left, Vector256<short> right)
            => left + Vector256.WidenLower(right) + Vector256.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<int> Add(Vector512<int> left, Vector512<short> right)
            => left + Vector512.WidenLower(right) + Vector512.WidenUpper(right);
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="long"/> and
    /// <see cref="short"/> vectors.
    /// </summary>
    private readonly struct Int64Int16VectorAddition : IVectorAddition<long, short, long>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<long> Add(Vector128<long> left, Vector128<short> right)
        {
            Vector128<int> rightInt32 = Vector128.WidenLower(right) + Vector128.WidenUpper(right);
            return left + Vector128.WidenLower(rightInt32) + Vector128.WidenUpper(rightInt32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<long> Add(Vector256<long> left, Vector256<short> right)
        {
            Vector256<int> rightInt32 = Vector256.WidenLower(right) + Vector256.WidenUpper(right);
            return left + Vector256.WidenLower(rightInt32) + Vector256.WidenUpper(rightInt32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<long> Add(Vector512<long> left, Vector512<short> right)
        {
            Vector512<int> rightInt32 = Vector512.WidenLower(right) + Vector512.WidenUpper(right);
            return left + Vector512.WidenLower(rightInt32) + Vector512.WidenUpper(rightInt32);
        }
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="uint"/> and
    /// <see cref="ushort"/> vectors.
    /// </summary>
    private readonly struct UInt32UInt16VectorAddition : IVectorAddition<uint, ushort, uint>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<uint> Add(Vector128<uint> left, Vector128<ushort> right)
            => left + Vector128.WidenLower(right) + Vector128.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<uint> Add(Vector256<uint> left, Vector256<ushort> right)
            => left + Vector256.WidenLower(right) + Vector256.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<uint> Add(Vector512<uint> left, Vector512<ushort> right)
            => left + Vector512.WidenLower(right) + Vector512.WidenUpper(right);
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="ulong"/> and
    /// <see cref="ushort"/> vectors.
    /// </summary>
    private readonly struct UInt64UInt16VectorAddition : IVectorAddition<ulong, ushort, ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<ulong> Add(Vector128<ulong> left, Vector128<ushort> right)
        {
            Vector128<uint> rightUInt32 = Vector128.WidenLower(right) + Vector128.WidenUpper(right);
            return left + (Vector128.WidenLower(rightUInt32) + Vector128.WidenUpper(rightUInt32));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<ulong> Add(Vector256<ulong> left, Vector256<ushort> right)
        {
            Vector256<uint> rightUInt32 = Vector256.WidenLower(right) + Vector256.WidenUpper(right);
            return left + (Vector256.WidenLower(rightUInt32) + Vector256.WidenUpper(rightUInt32));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<ulong> Add(Vector512<ulong> left, Vector512<ushort> right)
        {
            Vector512<uint> rightUInt32 = Vector512.WidenLower(right) + Vector512.WidenUpper(right);
            return left + (Vector512.WidenLower(rightUInt32) + Vector512.WidenUpper(rightUInt32));
        }
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="long"/> and
    /// <see cref="int"/> vectors.
    /// </summary>
    private readonly struct Int64Int32VectorAddition : IVectorAddition<long, int, long>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<long> Add(Vector128<long> left, Vector128<int> right)
            => left + Vector128.WidenLower(right) + Vector128.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<long> Add(Vector256<long> left, Vector256<int> right)
            => left + Vector256.WidenLower(right) + Vector256.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<long> Add(Vector512<long> left, Vector512<int> right)
            => left + Vector512.WidenLower(right) + Vector512.WidenUpper(right);
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="ulong"/> and
    /// <see cref="uint"/> vectors.
    /// </summary>
    private readonly struct UInt64UInt32VectorAddition : IVectorAddition<ulong, uint, ulong>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<ulong> Add(Vector128<ulong> left, Vector128<uint> right)
            => left + (Vector128.WidenLower(right) + Vector128.WidenUpper(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<ulong> Add(Vector256<ulong> left, Vector256<uint> right)
            => left + (Vector256.WidenLower(right) + Vector256.WidenUpper(right));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<ulong> Add(Vector512<ulong> left, Vector512<uint> right)
            => left + (Vector512.WidenLower(right) + Vector512.WidenUpper(right));
    }

    /// <summary>
    /// Defines a mechanism for computing the sum of <see cref="double"/> and
    /// <see cref="float"/> vectors.
    /// </summary>
    private readonly struct DoubleSingleVectorAddition : IVectorAddition<double, float, double>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<double> Add(Vector128<double> left, Vector128<float> right)
            => left + Vector128.WidenLower(right) + Vector128.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<double> Add(Vector256<double> left, Vector256<float> right)
            => left + Vector256.WidenLower(right) + Vector256.WidenUpper(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<double> Add(Vector512<double> left, Vector512<float> right)
            => left + Vector512.WidenLower(right) + Vector512.WidenUpper(right);
    }
}
