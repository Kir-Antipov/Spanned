namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// A vectorized solution to compute the sum of the values in the specified memory block.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <typeparam name="TOverflowTracker">The type for tracking overflow in vectorized arithmetic operations.</typeparam>
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <returns>The sum of the values in the memory block.</returns>
    /// <exception cref="NotSupportedException"><typeparamref name="T"/> is not supported.</exception>
    /// <exception cref="OverflowException">The addition operation in a checked context resulted in an overflow.</exception>
    private static T Sum<T, TOverflowTracker>(ref T searchSpace, int length)
        where T : struct, IBinaryNumber<T>, IMinMaxValue<T>
        where TOverflowTracker : IOverflowTracker<T>
    {
        Debug.Assert(Vector<T>.IsSupported);

        ref T current = ref searchSpace;
        ref T end = ref Unsafe.Add(ref current, length);
        T sum = T.Zero;

        // Note, we use `<=` instead of `<`, because in the end we need to
        // manually process every lane of the resulting vector.
        // Therefore, when `length == Vector.Count` a vectorized solution
        // would just end up performing unnecessary operations, becoming
        // slower than the regular loop without those.
        if (!Vector128.IsHardwareAccelerated || length <= Vector128<T>.Count)
        {
            // The operation cannot be vectorized.
            // Skip to the manual loop.
        }
        else if (!Vector256.IsHardwareAccelerated || length <= Vector256<T>.Count)
        {
            ref T lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector128<T>.Count);
            Vector128<T> overflowTrackerState = Vector128<T>.Zero;
            Vector128<T> sums = Vector128<T>.Zero;

            do
            {
                Vector128<T> currentVector = Vector128.LoadUnsafe(ref current);
                Vector128<T> newSums = sums + currentVector;
                overflowTrackerState = TOverflowTracker.UpdateState(overflowTrackerState, sums, currentVector, newSums);

                sums = newSums;
                current = ref Unsafe.Add(ref current, (nint)Vector128<T>.Count);
            }
            while (!Unsafe.IsAddressGreaterThan(ref current, ref lastVectorStart));

            if (TOverflowTracker.HasOverflow(overflowTrackerState))
                ThrowHelper.ThrowOverflowException();

            if (TOverflowTracker.IsSupported)
            {
                for (int i = 0; i < Vector128<T>.Count; i++)
                    sum = checked(sum + sums[i]);
            }
            else
            {
                sum = Vector128.Sum(sums);
            }
        }
        else if (!Vector512.IsHardwareAccelerated || length <= Vector512<T>.Count)
        {
            ref T lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector256<T>.Count);
            Vector256<T> overflowTrackerState = Vector256<T>.Zero;
            Vector256<T> sums = Vector256<T>.Zero;

            do
            {
                Vector256<T> currentVector = Vector256.LoadUnsafe(ref current);
                Vector256<T> newSums = sums + currentVector;
                overflowTrackerState = TOverflowTracker.UpdateState(overflowTrackerState, sums, currentVector, newSums);

                sums = newSums;
                current = ref Unsafe.Add(ref current, (nint)Vector256<T>.Count);
            }
            while (!Unsafe.IsAddressGreaterThan(ref current, ref lastVectorStart));

            if (TOverflowTracker.HasOverflow(overflowTrackerState))
                ThrowHelper.ThrowOverflowException();

            if (TOverflowTracker.IsSupported)
            {
                for (int i = 0; i < Vector256<T>.Count; i++)
                    sum = checked(sum + sums[i]);
            }
            else
            {
                sum = Vector256.Sum(sums);
            }
        }
        else
        {
            ref T lastVectorStart = ref Unsafe.Add(ref searchSpace, length - Vector512<T>.Count);
            Vector512<T> overflowTrackerState = Vector512<T>.Zero;
            Vector512<T> sums = Vector512<T>.Zero;

            do
            {
                Vector512<T> currentVector = Vector512.LoadUnsafe(ref current);
                Vector512<T> newSums = sums + currentVector;
                overflowTrackerState = TOverflowTracker.UpdateState(overflowTrackerState, sums, currentVector, newSums);

                sums = newSums;
                current = ref Unsafe.Add(ref current, (nint)Vector512<T>.Count);
            }
            while (!Unsafe.IsAddressGreaterThan(ref current, ref lastVectorStart));

            if (TOverflowTracker.HasOverflow(overflowTrackerState))
                ThrowHelper.ThrowOverflowException();

            if (TOverflowTracker.IsSupported)
            {
                for (int i = 0; i < Vector512<T>.Count; i++)
                    sum = checked(sum + sums[i]);
            }
            else
            {
                sum = Vector512.Sum(sums);
            }
        }

        for (; Unsafe.IsAddressLessThan(ref current, ref end); current = ref Unsafe.Add(ref current, (nint)1))
            sum = TOverflowTracker.IsSupported ? checked(sum + current) : (sum + current);

        return sum;
    }

    /// <summary>
    /// Computes the sum of the values in the specified memory block.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <returns>The sum of the values in the memory block.</returns>
    /// <exception cref="OverflowException">The addition operation in a checked context resulted in an overflow.</exception>
    private static T Sum<T>(ref T searchSpace, int length)
        where T : INumberBase<T>
    {
        if (typeof(T) == typeof(byte))
            return (T)(object)Sum<byte, UnsignedIntegerOverflowTracker<byte>>(ref Unsafe.As<T, byte>(ref searchSpace), length);

        if (typeof(T) == typeof(sbyte))
            return (T)(object)Sum<sbyte, SignedIntegerOverflowTracker<sbyte>>(ref Unsafe.As<T, sbyte>(ref searchSpace), length);

        if (typeof(T) == typeof(short))
            return (T)(object)Sum<short, SignedIntegerOverflowTracker<short>>(ref Unsafe.As<T, short>(ref searchSpace), length);

        if (typeof(T) == typeof(ushort))
            return (T)(object)Sum<ushort, UnsignedIntegerOverflowTracker<ushort>>(ref Unsafe.As<T, ushort>(ref searchSpace), length);

        if (typeof(T) == typeof(int))
            return (T)(object)Sum<int, SignedIntegerOverflowTracker<int>>(ref Unsafe.As<T, int>(ref searchSpace), length);

        if (typeof(T) == typeof(uint))
            return (T)(object)Sum<uint, UnsignedIntegerOverflowTracker<uint>>(ref Unsafe.As<T, uint>(ref searchSpace), length);

        if (typeof(T) == typeof(long))
            return (T)(object)Sum<long, SignedIntegerOverflowTracker<long>>(ref Unsafe.As<T, long>(ref searchSpace), length);

        if (typeof(T) == typeof(ulong))
            return (T)(object)Sum<ulong, UnsignedIntegerOverflowTracker<ulong>>(ref Unsafe.As<T, ulong>(ref searchSpace), length);

        if (typeof(T) == typeof(nint))
            return (T)(object)Sum<nint, SignedIntegerOverflowTracker<nint>>(ref Unsafe.As<T, nint>(ref searchSpace), length);

        if (typeof(T) == typeof(nuint))
            return (T)(object)Sum<nuint, UnsignedIntegerOverflowTracker<nuint>>(ref Unsafe.As<T, nuint>(ref searchSpace), length);

        if (typeof(T) == typeof(float))
            return (T)(object)Sum<float, NullOverflowTracker<float>>(ref Unsafe.As<T, float>(ref searchSpace), length);

        if (typeof(T) == typeof(double))
            return (T)(object)Sum<double, NullOverflowTracker<double>>(ref Unsafe.As<T, double>(ref searchSpace), length);

        T sum = T.Zero;
        nint end = (nint)length;

        for (nint i = 0; i < end; i++)
            sum = checked(sum + Unsafe.Add(ref searchSpace, i));

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
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <returns>The sum of the values in the memory block.</returns>
    private static T UnsafeSum<T>(ref T searchSpace, int length)
        where T : INumberBase<T>
    {
        if (typeof(T) == typeof(byte))
            return (T)(object)Sum<byte, NullOverflowTracker<byte>>(ref Unsafe.As<T, byte>(ref searchSpace), length);

        if (typeof(T) == typeof(sbyte))
            return (T)(object)Sum<sbyte, NullOverflowTracker<sbyte>>(ref Unsafe.As<T, sbyte>(ref searchSpace), length);

        if (typeof(T) == typeof(short))
            return (T)(object)Sum<short, NullOverflowTracker<short>>(ref Unsafe.As<T, short>(ref searchSpace), length);

        if (typeof(T) == typeof(ushort))
            return (T)(object)Sum<ushort, NullOverflowTracker<ushort>>(ref Unsafe.As<T, ushort>(ref searchSpace), length);

        if (typeof(T) == typeof(int))
            return (T)(object)Sum<int, NullOverflowTracker<int>>(ref Unsafe.As<T, int>(ref searchSpace), length);

        if (typeof(T) == typeof(uint))
            return (T)(object)Sum<uint, NullOverflowTracker<uint>>(ref Unsafe.As<T, uint>(ref searchSpace), length);

        if (typeof(T) == typeof(long))
            return (T)(object)Sum<long, NullOverflowTracker<long>>(ref Unsafe.As<T, long>(ref searchSpace), length);

        if (typeof(T) == typeof(ulong))
            return (T)(object)Sum<ulong, NullOverflowTracker<ulong>>(ref Unsafe.As<T, ulong>(ref searchSpace), length);

        if (typeof(T) == typeof(nint))
            return (T)(object)Sum<nint, NullOverflowTracker<nint>>(ref Unsafe.As<T, nint>(ref searchSpace), length);

        if (typeof(T) == typeof(nuint))
            return (T)(object)Sum<nuint, NullOverflowTracker<nuint>>(ref Unsafe.As<T, nuint>(ref searchSpace), length);

        if (typeof(T) == typeof(float))
            return (T)(object)Sum<float, NullOverflowTracker<float>>(ref Unsafe.As<T, float>(ref searchSpace), length);

        if (typeof(T) == typeof(double))
            return (T)(object)Sum<double, NullOverflowTracker<double>>(ref Unsafe.As<T, double>(ref searchSpace), length);

        T sum = T.Zero;
        nint end = (nint)length;

        for (nint i = 0; i < end; i++)
            sum += Unsafe.Add(ref searchSpace, i);

        return sum;
    }

    /// <summary>
    /// Defines an interface for tracking overflow in vectorized arithmetic operations on numeric types.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    private interface IOverflowTracker<T>
        where T : struct, INumberBase<T>, IMinMaxValue<T>
    {
        /// <summary>
        /// Gets a value indicating whether overflow tracking is supported for the specified numeric type.
        /// </summary>
        static abstract bool IsSupported { get; }

        /// <summary>
        /// Determines whether the overflow has occurred.
        /// </summary>
        /// <param name="state">The overflow tracking state.</param>
        /// <returns>
        /// <c>true</c> if the overflow has occurred; otherwise, <c>false</c>.
        /// </returns>
        static abstract bool HasOverflow(Vector128<T> state);

        /// <inheritdoc cref="HasOverflow(Vector128{T})"/>
        static abstract bool HasOverflow(Vector256<T> state);

        /// <inheritdoc cref="HasOverflow(Vector128{T})"/>
        static abstract bool HasOverflow(Vector512<T> state);

        /// <summary>
        /// Updates the overflow tracking state.
        /// </summary>
        /// <param name="state">The current overflow tracking state.</param>
        /// <param name="leftOperand">The left operand of the vector operation.</param>
        /// <param name="rightOperand">The right operand of the vector operation.</param>
        /// <param name="result">The result of the vector operation.</param>
        /// <returns>The updated overflow tracking state.</returns>
        static abstract Vector128<T> UpdateState(Vector128<T> state, Vector128<T> leftOperand, Vector128<T> rightOperand, Vector128<T> result);

        /// <inheritdoc cref="UpdateState(Vector128{T}, Vector128{T}, Vector128{T}, Vector128{T})"/>
        static abstract Vector256<T> UpdateState(Vector256<T> state, Vector256<T> leftOperand, Vector256<T> rightOperand, Vector256<T> result);

        /// <inheritdoc cref="UpdateState(Vector128{T}, Vector128{T}, Vector128{T}, Vector128{T})"/>
        static abstract Vector512<T> UpdateState(Vector512<T> state, Vector512<T> leftOperand, Vector512<T> rightOperand, Vector512<T> result);
    }

    /// <summary>
    /// Represents a stub for an overflow tracker that does not perform any checks.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    private readonly struct NullOverflowTracker<T> : IOverflowTracker<T>
        where T : struct, INumberBase<T>, IMinMaxValue<T>
    {
        public static bool IsSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOverflow(Vector128<T> state) => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOverflow(Vector256<T> state) => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOverflow(Vector512<T> state) => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> UpdateState(Vector128<T> state, Vector128<T> leftOperand, Vector128<T> rightOperand, Vector128<T> result) => state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<T> UpdateState(Vector256<T> state, Vector256<T> leftOperand, Vector256<T> rightOperand, Vector256<T> result) => state;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<T> UpdateState(Vector512<T> state, Vector512<T> leftOperand, Vector512<T> rightOperand, Vector512<T> result) => state;
    }

    /// <summary>
    /// Represents an overflow tracker for signed integer types.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    private readonly struct SignedIntegerOverflowTracker<T> : IOverflowTracker<T>
        where T : struct, IBinaryInteger<T>, ISignedNumber<T>, IMinMaxValue<T>
    {
        public static bool IsSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOverflow(Vector128<T> state)
            => (state & Vector128.Create(T.MinValue)) != Vector128<T>.Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOverflow(Vector256<T> state)
            => (state & Vector256.Create(T.MinValue)) != Vector256<T>.Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOverflow(Vector512<T> state)
            => (state & Vector512.Create(T.MinValue)) != Vector512<T>.Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> UpdateState(Vector128<T> state, Vector128<T> leftOperand, Vector128<T> rightOperand, Vector128<T> result)
            => state | ((result ^ leftOperand) & (result ^ rightOperand));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<T> UpdateState(Vector256<T> state, Vector256<T> leftOperand, Vector256<T> rightOperand, Vector256<T> result)
            => state | ((result ^ leftOperand) & (result ^ rightOperand));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<T> UpdateState(Vector512<T> state, Vector512<T> leftOperand, Vector512<T> rightOperand, Vector512<T> result)
            => state | ((result ^ leftOperand) & (result ^ rightOperand));
    }

    /// <summary>
    /// Represents an overflow tracker for unsigned integer types.
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    private readonly struct UnsignedIntegerOverflowTracker<T> : IOverflowTracker<T>
        where T : struct, IBinaryInteger<T>, IUnsignedNumber<T>, IMinMaxValue<T>
    {
        public static bool IsSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOverflow(Vector128<T> state)
            => (state & Vector128.Create((T.MaxValue >> 1) + T.One)) != Vector128<T>.Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOverflow(Vector256<T> state)
            => (state & Vector256.Create((T.MaxValue >> 1) + T.One)) != Vector256<T>.Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasOverflow(Vector512<T> state)
            => (state & Vector512.Create((T.MaxValue >> 1) + T.One)) != Vector512<T>.Zero;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector128<T> UpdateState(Vector128<T> state, Vector128<T> leftOperand, Vector128<T> rightOperand, Vector128<T> result)
            => state | ((leftOperand & rightOperand) | Vector128.AndNot(leftOperand | rightOperand, result));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector256<T> UpdateState(Vector256<T> state, Vector256<T> leftOperand, Vector256<T> rightOperand, Vector256<T> result)
            => state | ((leftOperand & rightOperand) | Vector256.AndNot(leftOperand | rightOperand, result));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector512<T> UpdateState(Vector512<T> state, Vector512<T> leftOperand, Vector512<T> rightOperand, Vector512<T> result)
            => state | ((leftOperand & rightOperand) | Vector512.AndNot(leftOperand | rightOperand, result));
    }

    /// <summary>
    /// Computes the sum of a span of <see cref="byte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="byte"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    /// <exception cref="OverflowException">The addition operation in a checked context resulted in an overflow.</exception>
    public static byte Sum(this scoped Span<byte> span) => Sum<byte, UnsignedIntegerOverflowTracker<byte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static byte Sum(this scoped ReadOnlySpan<byte> span) => Sum<byte, UnsignedIntegerOverflowTracker<byte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="sbyte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="sbyte"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    [CLSCompliant(false)]
    public static sbyte Sum(this scoped Span<sbyte> span) => Sum<sbyte, SignedIntegerOverflowTracker<sbyte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{sbyte})"/>
    [CLSCompliant(false)]
    public static sbyte Sum(this scoped ReadOnlySpan<sbyte> span) => Sum<sbyte, SignedIntegerOverflowTracker<sbyte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static short Sum(this scoped Span<short> span) => Sum<short, SignedIntegerOverflowTracker<short>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{short})"/>
    public static short Sum(this scoped ReadOnlySpan<short> span) => Sum<short, SignedIntegerOverflowTracker<short>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ushort"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ushort"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ushort Sum(this scoped Span<ushort> span) => Sum<ushort, UnsignedIntegerOverflowTracker<ushort>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{ushort})"/>
    [CLSCompliant(false)]
    public static ushort Sum(this scoped ReadOnlySpan<ushort> span) => Sum<ushort, UnsignedIntegerOverflowTracker<ushort>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="int"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="int"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static int Sum(this scoped Span<int> span) => Sum<int, SignedIntegerOverflowTracker<int>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{int})"/>
    public static int Sum(this scoped ReadOnlySpan<int> span) => Sum<int, SignedIntegerOverflowTracker<int>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="uint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="uint"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    [CLSCompliant(false)]
    public static uint Sum(this scoped Span<uint> span) => Sum<uint, UnsignedIntegerOverflowTracker<uint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{uint})"/>
    [CLSCompliant(false)]
    public static uint Sum(this scoped ReadOnlySpan<uint> span) => Sum<uint, UnsignedIntegerOverflowTracker<uint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="long"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="long"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static long Sum(this scoped Span<long> span) => Sum<long, SignedIntegerOverflowTracker<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{long})"/>
    public static long Sum(this scoped ReadOnlySpan<long> span) => Sum<long, SignedIntegerOverflowTracker<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ulong"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ulong"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ulong Sum(this scoped Span<ulong> span) => Sum<ulong, UnsignedIntegerOverflowTracker<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{ulong})"/>
    [CLSCompliant(false)]
    public static ulong Sum(this scoped ReadOnlySpan<ulong> span) => Sum<ulong, UnsignedIntegerOverflowTracker<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="nint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="nint"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static nint Sum(this scoped Span<nint> span) => Sum<nint, SignedIntegerOverflowTracker<nint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{nint})"/>
    public static nint Sum(this scoped ReadOnlySpan<nint> span) => Sum<nint, SignedIntegerOverflowTracker<nint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="nuint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="nuint"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    [CLSCompliant(false)]
    public static nuint Sum(this scoped Span<nuint> span) => Sum<nuint, UnsignedIntegerOverflowTracker<nuint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{nuint})"/>
    [CLSCompliant(false)]
    public static nuint Sum(this scoped ReadOnlySpan<nuint> span) => Sum<nuint, UnsignedIntegerOverflowTracker<nuint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="float"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="float"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the sequence.</returns>
    public static float Sum(this scoped Span<float> span) => Sum<float, NullOverflowTracker<float>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{float})"/>
    public static float Sum(this scoped ReadOnlySpan<float> span) => Sum<float, NullOverflowTracker<float>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="double"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="double"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the sequence.</returns>
    public static double Sum(this scoped Span<double> span) => Sum<double, NullOverflowTracker<double>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{double})"/>
    public static double Sum(this scoped ReadOnlySpan<double> span) => Sum<double, NullOverflowTracker<double>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="decimal"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="decimal"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static decimal Sum(this scoped Span<decimal> span) => Sum(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum(Span{decimal})"/>
    public static decimal Sum(this scoped ReadOnlySpan<decimal> span) => Sum(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of generic values.
    /// </summary>
    /// <typeparam name="T">The type of the span.</typeparam>
    /// <param name="span">A span of generic values to calculate the sum of.</param>
    /// <inheritdoc cref="Sum(Span{byte})"/>
    public static T Sum<T>(this scoped Span<T> span) where T : INumberBase<T> => Sum(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="Sum{T}(Span{T})"/>
    public static T Sum<T>(this scoped ReadOnlySpan<T> span) where T : INumberBase<T> => Sum(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="byte"/> values.
    /// </summary>
    /// <remarks>
    /// This method does not account for overflow.
    /// Use with caution and ensure that values in the span cannot cause an overflow if it is not desirable.
    /// </remarks>
    /// <param name="span">A span of <see cref="byte"/> values to calculate the sum of.</param>
    /// <returns>The sum of the values in the span.</returns>
    public static byte UnsafeSum(this scoped Span<byte> span) => Sum<byte, NullOverflowTracker<byte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static byte UnsafeSum(this scoped ReadOnlySpan<byte> span) => Sum<byte, NullOverflowTracker<byte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="sbyte"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="sbyte"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static sbyte UnsafeSum(this scoped Span<sbyte> span) => Sum<sbyte, NullOverflowTracker<sbyte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{sbyte})"/>
    [CLSCompliant(false)]
    public static sbyte UnsafeSum(this scoped ReadOnlySpan<sbyte> span) => Sum<sbyte, NullOverflowTracker<sbyte>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="short"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="short"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static short UnsafeSum(this scoped Span<short> span) => Sum<short, NullOverflowTracker<short>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{short})"/>
    public static short UnsafeSum(this scoped ReadOnlySpan<short> span) => Sum<short, NullOverflowTracker<short>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ushort"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ushort"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ushort UnsafeSum(this scoped Span<ushort> span) => Sum<ushort, NullOverflowTracker<ushort>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{ushort})"/>
    [CLSCompliant(false)]
    public static ushort UnsafeSum(this scoped ReadOnlySpan<ushort> span) => Sum<ushort, NullOverflowTracker<ushort>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="int"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="int"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static int UnsafeSum(this scoped Span<int> span) => Sum<int, NullOverflowTracker<int>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{int})"/>
    public static int UnsafeSum(this scoped ReadOnlySpan<int> span) => Sum<int, NullOverflowTracker<int>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="uint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="uint"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static uint UnsafeSum(this scoped Span<uint> span) => Sum<uint, NullOverflowTracker<uint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{uint})"/>
    [CLSCompliant(false)]
    public static uint UnsafeSum(this scoped ReadOnlySpan<uint> span) => Sum<uint, NullOverflowTracker<uint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="long"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="long"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static long UnsafeSum(this scoped Span<long> span) => Sum<long, NullOverflowTracker<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{long})"/>
    public static long UnsafeSum(this scoped ReadOnlySpan<long> span) => Sum<long, NullOverflowTracker<long>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="ulong"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="ulong"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static ulong UnsafeSum(this scoped Span<ulong> span) => Sum<ulong, NullOverflowTracker<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{ulong})"/>
    [CLSCompliant(false)]
    public static ulong UnsafeSum(this scoped ReadOnlySpan<ulong> span) => Sum<ulong, NullOverflowTracker<ulong>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="nint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="nint"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static nint UnsafeSum(this scoped Span<nint> span) => Sum<nint, NullOverflowTracker<nint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{nint})"/>
    public static nint UnsafeSum(this scoped ReadOnlySpan<nint> span) => Sum<nint, NullOverflowTracker<nint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of <see cref="nuint"/> values.
    /// </summary>
    /// <param name="span">A span of <see cref="nuint"/> values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    [CLSCompliant(false)]
    public static nuint UnsafeSum(this scoped Span<nuint> span) => Sum<nuint, NullOverflowTracker<nuint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum(Span{nuint})"/>
    [CLSCompliant(false)]
    public static nuint UnsafeSum(this scoped ReadOnlySpan<nuint> span) => Sum<nuint, NullOverflowTracker<nuint>>(ref MemoryMarshal.GetReference(span), span.Length);

    /// <summary>
    /// Computes the sum of a span of generic values.
    /// </summary>
    /// <typeparam name="T">The type of the span.</typeparam>
    /// <param name="span">A span of generic values to calculate the sum of.</param>
    /// <inheritdoc cref="UnsafeSum(Span{byte})"/>
    public static T UnsafeSum<T>(this scoped Span<T> span) where T : INumberBase<T> => UnsafeSum(ref MemoryMarshal.GetReference(span), span.Length);

    /// <inheritdoc cref="UnsafeSum{T}(Span{T})"/>
    public static T UnsafeSum<T>(this scoped ReadOnlySpan<T> span) where T : INumberBase<T> => UnsafeSum(ref MemoryMarshal.GetReference(span), span.Length);
}
