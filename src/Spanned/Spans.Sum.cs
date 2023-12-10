namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// A vectorized solution to compute the sum of the values in the specified memory block.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TOverflowTracker">The type for tracking overflow in vectorized arithmetic operations.</typeparam>
    /// <typeparam name="TNumber">The type of the values.</typeparam>
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <returns>The sum of the values in the memory block.</returns>
    /// <exception cref="NotSupportedException"><typeparamref name="T"/> is not supported.</exception>
    /// <exception cref="OverflowException">The addition operation in a checked context resulted in an overflow.</exception>
    private static T Sum<T, TOverflowTracker, TNumber>(ref T searchSpace, int length)
        where T : struct
        where TOverflowTracker : struct, IOverflowTracker<T>
        where TNumber : struct, INumber<T>
    {
        ref T current = ref searchSpace;
        ref T end = ref Unsafe.Add(ref current, length);
        T sum = default;

        // Note, we use `<=` instead of `<`, because in the end we need to
        // manually process every lane of the resulting vector.
        // Therefore, when `length == Vector.Count` a vectorized solution
        // would just end up performing unnecessary operations, becoming
        // slower than the regular loop without those.
        if (Vector.IsHardwareAccelerated && length > Vector<T>.Count)
        {
            ref T lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector<T>.Count);
            Vector<T> overflowTrackerState = Vector<T>.Zero;
            Vector<T> sums = Vector<T>.Zero;

            do
            {
                Vector<T> currentVector = new(MemoryMarshal.CreateSpan(ref current, Vector<T>.Count));
                Vector<T> newSums = sums + currentVector;
                overflowTrackerState = default(TOverflowTracker).UpdateState(overflowTrackerState, sums, currentVector, newSums);

                sums = newSums;
                current = ref Unsafe.Add(ref current, (nint)Vector<T>.Count);
            }
            while (!Unsafe.IsAddressGreaterThan(ref current, ref lastVectorStart));

            if (default(TOverflowTracker).HasOverflow(overflowTrackerState))
                ThrowHelper.ThrowOverflowException();

            if (default(TOverflowTracker).IsSupported)
            {
                for (int i = 0; i < Vector<T>.Count; i++)
                    sum = default(TNumber).AddChecked(sum, sums[i]);
            }
            else
            {
                for (int i = 0; i < Vector<T>.Count; i++)
                    sum = default(TNumber).Add(sum, sums[i]);
            }
        }

        if (default(TOverflowTracker).IsSupported)
        {
            for (; Unsafe.IsAddressLessThan(ref current, ref end); current = ref Unsafe.Add(ref current, (nint)1))
                sum = default(TNumber).AddChecked(sum, current);
        }
        else
        {
            for (; Unsafe.IsAddressLessThan(ref current, ref end); current = ref Unsafe.Add(ref current, (nint)1))
                sum = default(TNumber).Add(sum, current);
        }

        return sum;
    }

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
    /// Defines an interface for tracking overflow in vectorized arithmetic operations on numeric types.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    private interface IOverflowTracker<T>
        where T : struct
    {
        /// <summary>
        /// Gets a value indicating whether overflow tracking is supported for the specified numeric type.
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// Determines whether the overflow has occurred.
        /// </summary>
        /// <param name="state">The overflow tracking state.</param>
        /// <returns>
        /// <c>true</c> if the overflow has occurred; otherwise, <c>false</c>.
        /// </returns>
        bool HasOverflow(Vector<T> state);

        /// <summary>
        /// Updates the overflow tracking state.
        /// </summary>
        /// <param name="state">The current overflow tracking state.</param>
        /// <param name="leftOperand">The left operand of the vector operation.</param>
        /// <param name="rightOperand">The right operand of the vector operation.</param>
        /// <param name="result">The result of the vector operation.</param>
        /// <returns>The updated overflow tracking state.</returns>
        Vector<T> UpdateState(Vector<T> state, Vector<T> leftOperand, Vector<T> rightOperand, Vector<T> result);
    }

    /// <summary>
    /// Represents a stub for an overflow tracker that does not perform any checks.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    private readonly struct NullOverflowTracker<T> : IOverflowTracker<T>
        where T : struct
    {
        public bool IsSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasOverflow(Vector<T> state) => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<T> UpdateState(Vector<T> state, Vector<T> leftOperand, Vector<T> rightOperand, Vector<T> result) => state;
    }

    /// <summary>
    /// Represents an overflow tracker for signed integer types.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <typeparam name="TNumber">The numeric type.</typeparam>
    private readonly struct SignedIntegerOverflowTracker<T, TNumber> : IOverflowTracker<T>
        where T : struct
        where TNumber : struct, INumber<T>
    {
        public bool IsSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasOverflow(Vector<T> state)
            => (state & new Vector<T>(default(TNumber).MinValue)) != Vector<T>.Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<T> UpdateState(Vector<T> state, Vector<T> leftOperand, Vector<T> rightOperand, Vector<T> result)
            => state | ((result ^ leftOperand) & (result ^ rightOperand));
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
    public static sbyte Sum(this scoped Span<sbyte> span) => Sum<sbyte, SignedIntegerOverflowTracker<sbyte, SByteNumber>, SByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{sbyte})"/>
    [CLSCompliant(false)]
    public static sbyte Sum(this scoped ReadOnlySpan<sbyte> span) => Sum<sbyte, SignedIntegerOverflowTracker<sbyte, SByteNumber>, SByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static short Sum(this scoped Span<short> span) => Sum<short, SignedIntegerOverflowTracker<short, Int16Number>, Int16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{short})"/>
    public static short Sum(this scoped ReadOnlySpan<short> span) => Sum<short, SignedIntegerOverflowTracker<short, Int16Number>, Int16Number>(ref MemoryMarshal.GetReference(span), span.Length);

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
    public static int Sum(this scoped Span<int> span) => Sum<int, SignedIntegerOverflowTracker<int, Int32Number>, Int32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{int})"/>
    public static int Sum(this scoped ReadOnlySpan<int> span) => Sum<int, SignedIntegerOverflowTracker<int, Int32Number>, Int32Number>(ref MemoryMarshal.GetReference(span), span.Length);

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
    public static long Sum(this scoped Span<long> span) => Sum<long, SignedIntegerOverflowTracker<long, Int64Number>, Int64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{long})"/>
    public static long Sum(this scoped ReadOnlySpan<long> span) => Sum<long, SignedIntegerOverflowTracker<long, Int64Number>, Int64Number>(ref MemoryMarshal.GetReference(span), span.Length);

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
    public static float Sum(this scoped Span<float> span) => Sum<float, NullOverflowTracker<float>, SingleNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{float})"/>
    public static float Sum(this scoped ReadOnlySpan<float> span) => Sum<float, NullOverflowTracker<float>, SingleNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the sequence.</returns>
    public static double Sum(this scoped Span<double> span) => Sum<double, NullOverflowTracker<double>, DoubleNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{double})"/>
    public static double Sum(this scoped ReadOnlySpan<double> span) => Sum<double, NullOverflowTracker<double>, DoubleNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="decimal"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="decimal"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static decimal Sum(this scoped Span<decimal> span) => Sum<decimal, DecimalNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{decimal})"/>
    public static decimal Sum(this scoped ReadOnlySpan<decimal> span) => Sum<decimal, DecimalNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="byte"/> values.
    /// </summary>
    /// <remarks>
    /// This method does not account for overflow.
    /// Use with caution and ensure that values in the span cannot cause an overflow if it is not desirable.
    /// </remarks>
    /// <param name="span">A span of <see cref="byte"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    public static byte UnsafeSum(this scoped Span<byte> span) => Sum<byte, NullOverflowTracker<byte>, ByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static byte UnsafeSum(this scoped ReadOnlySpan<byte> span) => Sum<byte, NullOverflowTracker<byte>, ByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="sbyte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="sbyte"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static sbyte UnsafeSum(this scoped Span<sbyte> span) => Sum<sbyte, NullOverflowTracker<sbyte>, SByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{sbyte})"/>
    [CLSCompliant(false)]
    public static sbyte UnsafeSum(this scoped ReadOnlySpan<sbyte> span) => Sum<sbyte, NullOverflowTracker<sbyte>, SByteNumber>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static short UnsafeSum(this scoped Span<short> span) => Sum<short, NullOverflowTracker<short>, Int16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{short})"/>
    public static short UnsafeSum(this scoped ReadOnlySpan<short> span) => Sum<short, NullOverflowTracker<short>, Int16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ushort"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ushort"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ushort UnsafeSum(this scoped Span<ushort> span) => Sum<ushort, NullOverflowTracker<ushort>, UInt16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{ushort})"/>
    [CLSCompliant(false)]
    public static ushort UnsafeSum(this scoped ReadOnlySpan<ushort> span) => Sum<ushort, NullOverflowTracker<ushort>, UInt16Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="int"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="int"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static int UnsafeSum(this scoped Span<int> span) => Sum<int, NullOverflowTracker<int>, Int32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{int})"/>
    public static int UnsafeSum(this scoped ReadOnlySpan<int> span) => Sum<int, NullOverflowTracker<int>, Int32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="uint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="uint"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static uint UnsafeSum(this scoped Span<uint> span) => Sum<uint, NullOverflowTracker<uint>, UInt32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{uint})"/>
    [CLSCompliant(false)]
    public static uint UnsafeSum(this scoped ReadOnlySpan<uint> span) => Sum<uint, NullOverflowTracker<uint>, UInt32Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="long"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="long"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static long UnsafeSum(this scoped Span<long> span) => Sum<long, NullOverflowTracker<long>, Int64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{long})"/>
    public static long UnsafeSum(this scoped ReadOnlySpan<long> span) => Sum<long, NullOverflowTracker<long>, Int64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ulong"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ulong"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ulong UnsafeSum(this scoped Span<ulong> span) => Sum<ulong, NullOverflowTracker<ulong>, UInt64Number>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{ulong})"/>
    [CLSCompliant(false)]
    public static ulong UnsafeSum(this scoped ReadOnlySpan<ulong> span) => Sum<ulong, NullOverflowTracker<ulong>, UInt64Number>(ref MemoryMarshal.GetReference(span), span.Length);
}
