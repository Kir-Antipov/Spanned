namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// A vectorized solution to fill a memory block with sequential values,
    /// starting at the specified <paramref name="offset"/> and incrementing
    /// by the specified <paramref name="step"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the memory block.</typeparam>
    /// <param name="searchSpace">The reference to the start of the memory block.</param>
    /// <param name="length">The length of the memory block.</param>
    /// <param name="offset">The starting value for the sequential filling.</param>
    /// <param name="step">The increment value for the sequential filling.</param>
    /// <exception cref="NotSupportedException"><typeparamref name="T"/> is not supported.</exception>
    private static void FillSequential<T>(ref T searchSpace, int length, T offset, T step)
        where T : struct, IBinaryNumber<T>
    {
        Debug.Assert(Vector<T>.IsSupported);

        if (length == 0)
            return;

        nuint end = (nuint)length;

        if (!Vector128.IsHardwareAccelerated || length < Vector128<T>.Count)
        {
            T previous = offset;
            searchSpace = previous;

            for (nuint i = 1; i < end; i++)
            {
                previous += step;
                Unsafe.Add(ref searchSpace, i) = previous;
            }
        }
        else if (!Vector256.IsHardwareAccelerated || length < Vector256<T>.Count)
        {
            Vector128<T> value = (T.IsNegative(step) ? Sequence128<T>.Negative : Sequence128<T>.Positive) * T.Abs(step) + Vector128.Create(offset);
            Vector128<T> shift = Sequence128<T>.Shift * step;

            nuint i = 0;
            nuint lastVectorStart = end - (nuint)Vector128<T>.Count;
            do
            {
                value.StoreUnsafe(ref searchSpace, i);
                value += shift;
                i += (nuint)Vector128<T>.Count;
            }
            while (i <= lastVectorStart);

            for (int j = 0; i < end; i++, j++)
                Unsafe.Add(ref searchSpace, i) = value[j];
        }
        else if (!Vector512.IsHardwareAccelerated || length < Vector512<T>.Count)
        {
            Vector256<T> value = (T.IsNegative(step) ? Sequence256<T>.Negative : Sequence256<T>.Positive) * T.Abs(step) + Vector256.Create(offset);
            Vector256<T> shift = Sequence256<T>.Shift * step;

            nuint i = 0;
            nuint lastVectorStart = end - (nuint)Vector256<T>.Count;
            do
            {
                value.StoreUnsafe(ref searchSpace, i);
                value += shift;
                i += (nuint)Vector256<T>.Count;
            }
            while (i <= lastVectorStart);

            for (int j = 0; i < end; i++, j++)
                Unsafe.Add(ref searchSpace, i) = value[j];
        }
        else
        {
            Vector512<T> value = (T.IsNegative(step) ? Sequence512<T>.Negative : Sequence512<T>.Positive) * T.Abs(step) + Vector512.Create(offset);
            Vector512<T> shift = Sequence512<T>.Shift * step;

            nuint i = 0;
            nuint lastVectorStart = end - (nuint)Vector512<T>.Count;
            do
            {
                value.StoreUnsafe(ref searchSpace, i);
                value += shift;
                i += (nuint)Vector512<T>.Count;
            }
            while (i <= lastVectorStart);

            for (int j = 0; i < end; i++, j++)
                Unsafe.Add(ref searchSpace, i) = value[j];
        }
    }

    /// <summary>
    /// Fills a generic span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the <paramref name="span"/>.</typeparam>
    /// <param name="span">The span to fill with sequential values.</param>
    /// <param name="offset">The starting value for the sequential filling.</param>
    /// <param name="step">The increment value for the sequential filling.</param>
    public static void FillSequential<T>(this scoped Span<T> span, T offset, T step)
        where T : IAdditionOperators<T, T, T>
    {
        if (typeof(T) == typeof(byte))
        {
            FillSequential(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(span)), span.Length, (byte)(object)offset, (byte)(object)step);
            return;
        }

        if (typeof(T) == typeof(sbyte))
        {
            FillSequential(ref Unsafe.As<T, sbyte>(ref MemoryMarshal.GetReference(span)), span.Length, (sbyte)(object)offset, (sbyte)(object)step);
            return;
        }

        if (typeof(T) == typeof(short))
        {
            FillSequential(ref Unsafe.As<T, short>(ref MemoryMarshal.GetReference(span)), span.Length, (short)(object)offset, (short)(object)step);
            return;
        }

        if (typeof(T) == typeof(ushort))
        {
            FillSequential(ref Unsafe.As<T, ushort>(ref MemoryMarshal.GetReference(span)), span.Length, (ushort)(object)offset, (ushort)(object)step);
            return;
        }

        if (typeof(T) == typeof(int))
        {
            FillSequential(ref Unsafe.As<T, int>(ref MemoryMarshal.GetReference(span)), span.Length, (int)(object)offset, (int)(object)step);
            return;
        }

        if (typeof(T) == typeof(uint))
        {
            FillSequential(ref Unsafe.As<T, uint>(ref MemoryMarshal.GetReference(span)), span.Length, (uint)(object)offset, (uint)(object)step);
            return;
        }

        if (typeof(T) == typeof(long))
        {
            FillSequential(ref Unsafe.As<T, long>(ref MemoryMarshal.GetReference(span)), span.Length, (long)(object)offset, (long)(object)step);
            return;
        }

        if (typeof(T) == typeof(ulong))
        {
            FillSequential(ref Unsafe.As<T, ulong>(ref MemoryMarshal.GetReference(span)), span.Length, (ulong)(object)offset, (ulong)(object)step);
            return;
        }

        if (typeof(T) == typeof(nint))
        {
            FillSequential(ref Unsafe.As<T, nint>(ref MemoryMarshal.GetReference(span)), span.Length, (nint)(object)offset, (nint)(object)step);
            return;
        }

        if (typeof(T) == typeof(nuint))
        {
            FillSequential(ref Unsafe.As<T, nuint>(ref MemoryMarshal.GetReference(span)), span.Length, (nuint)(object)offset, (nuint)(object)step);
            return;
        }

        if (typeof(T) == typeof(float))
        {
            FillSequential(ref Unsafe.As<T, float>(ref MemoryMarshal.GetReference(span)), span.Length, (float)(object)offset, (float)(object)step);
            return;
        }

        if (typeof(T) == typeof(double))
        {
            FillSequential(ref Unsafe.As<T, double>(ref MemoryMarshal.GetReference(span)), span.Length, (double)(object)offset, (double)(object)step);
            return;
        }

        if (span.IsEmpty)
            return;

        T previous = offset;
        span[0] = previous;

        for (int i = 1; i < span.Length; i++)
        {
            previous += step;
            span[i] = previous;
        }
    }

    /// <summary>
    /// Provides precomputed sequence vectors for <typeparamref name="T"/> using <see cref="Vector128{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type for which sequences are generated.</typeparam>
    private static class Sequence128<T> where T : struct, IBinaryNumber<T>
    {
        /// <summary>
        /// A sequence of positive values starting from zero.
        /// </summary>
        public static readonly Vector128<T> Positive;

        /// <summary>
        /// A sequence of negative values starting from zero.
        /// </summary>
        public static readonly Vector128<T> Negative;

        /// <summary>
        /// A value used for shifting a sequence vector.
        /// </summary>
        public static readonly Vector128<T> Shift;

        /// <summary>
        /// Statically initializes the <see cref="Sequence128{T}"/> class.
        /// </summary>
        static Sequence128()
        {
            Span<T> values = new T[Vector128<T>.Count];
            values[0] = T.Zero;

            for (int i = 1; i < values.Length; ++i)
            {
                values[i] = values[i - 1] + T.One;
            }
            Positive = Vector128.Create<T>(values);

            for (int i = 1; i < values.Length; ++i)
            {
                values[i] = values[i - 1] - T.One;
            }
            Negative = Vector128.Create<T>(values);

            Shift = Vector128<T>.One * T.CreateChecked(Vector128<T>.Count);
        }
    }

    /// <summary>
    /// Provides precomputed sequence vectors for <typeparamref name="T"/> using <see cref="Vector256{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type for which sequences are generated.</typeparam>
    private static class Sequence256<T> where T : struct, IBinaryNumber<T>
    {
        /// <summary>
        /// A sequence of positive values starting from zero.
        /// </summary>
        public static Vector256<T> Positive;

        /// <summary>
        /// A sequence of negative values starting from zero.
        /// </summary>
        public static Vector256<T> Negative;

        /// <summary>
        /// A value used for shifting a sequence vector.
        /// </summary>
        public static Vector256<T> Shift;

        /// <summary>
        /// Statically initializes the <see cref="Sequence256{T}"/> class.
        /// </summary>
        static Sequence256()
        {
            Span<T> values = new T[Vector256<T>.Count];
            values[0] = T.Zero;

            for (int i = 1; i < values.Length; ++i)
            {
                values[i] = values[i - 1] + T.One;
            }
            Positive = Vector256.Create<T>(values);

            for (int i = 1; i < values.Length; ++i)
            {
                values[i] = values[i - 1] - T.One;
            }
            Negative = Vector256.Create<T>(values);

            Shift = Vector256<T>.One * T.CreateChecked(Vector256<T>.Count);
        }
    }

    /// <summary>
    /// Provides precomputed sequence vectors for <typeparamref name="T"/> using <see cref="Vector512{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type for which sequences are generated.</typeparam>
    private static class Sequence512<T> where T : struct, IBinaryNumber<T>
    {
        /// <summary>
        /// A sequence of positive values starting from zero.
        /// </summary>
        public static Vector512<T> Positive;

        /// <summary>
        /// A sequence of negative values starting from zero.
        /// </summary>
        public static Vector512<T> Negative;

        /// <summary>
        /// A value used for shifting a sequence vector.
        /// </summary>
        public static Vector512<T> Shift;

        /// <summary>
        /// Statically initializes the <see cref="Sequence512{T}"/> class.
        /// </summary>
        static Sequence512()
        {
            Span<T> values = new T[Vector512<T>.Count];
            values[0] = T.Zero;

            for (int i = 1; i < values.Length; ++i)
            {
                values[i] = values[i - 1] + T.One;
            }
            Positive = Vector512.Create<T>(values);

            for (int i = 1; i < values.Length; ++i)
            {
                values[i] = values[i - 1] - T.One;
            }
            Negative = Vector512.Create<T>(values);

            Shift = Vector512<T>.One * T.CreateChecked(Vector512<T>.Count);
        }
    }

    /// <summary>
    /// Fills a <see cref="byte"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<byte> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, (byte)0, (byte)1);

    /// <summary>
    /// Fills a <see cref="byte"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    /// <param name="offset">The starting value for the sequential filling.</param>
    /// <param name="step">The increment value for the sequential filling.</param>
    public static void FillSequential(this scoped Span<byte> span, byte offset = 0, byte step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="sbyte"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<sbyte> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, (sbyte)0, (sbyte)1);

    /// <summary>
    /// Fills a <see cref="sbyte"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<sbyte> span, sbyte offset = 0, sbyte step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="short"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<short> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, (short)0, (short)1);

    /// <summary>
    /// Fills a <see cref="short"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<short> span, short offset = 0, short step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="ushort"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<ushort> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, (ushort)0, (ushort)1);

    /// <summary>
    /// Fills a <see cref="ushort"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<ushort> span, ushort offset = 0, ushort step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="int"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<int> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, 0, 1);

    /// <summary>
    /// Fills a <see cref="int"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<int> span, int offset = 0, int step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="uint"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<uint> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, 0u, 1u);

    /// <summary>
    /// Fills a <see cref="uint"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<uint> span, uint offset = 0, uint step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="long"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<long> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, 0L, 1L);

    /// <summary>
    /// Fills a <see cref="long"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<long> span, long offset = 0, long step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="ulong"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<ulong> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, 0ul, 1ul);

    /// <summary>
    /// Fills a <see cref="ulong"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<ulong> span, ulong offset = 0, ulong step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="nint"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<nint> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, (nint)0, (nint)1);

    /// <summary>
    /// Fills a <see cref="nint"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<nint> span, nint offset = 0, nint step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="nuint"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<nuint> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, (nuint)0, (nuint)1);

    /// <summary>
    /// Fills a <see cref="nuint"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    [CLSCompliant(false)]
    public static void FillSequential(this scoped Span<nuint> span, nuint offset = 0, nuint step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="float"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<float> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, 0f, 1f);

    /// <summary>
    /// Fills a <see cref="float"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<float> span, float offset = 0, float step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="double"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<double> span) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, 0d, 1d);

    /// <summary>
    /// Fills a <see cref="double"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<double> span, double offset = 0, double step = 1) => FillSequential(ref MemoryMarshal.GetReference(span), span.Length, offset, step);

    /// <summary>
    /// Fills a <see cref="decimal"/> span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential(this scoped Span<decimal> span) => FillSequential<decimal>(span, 0m, 1m);

    /// <summary>
    /// Fills a <see cref="decimal"/> span with sequential values, starting at the specified
    /// <paramref name="offset"/> and incrementing by the specified <paramref name="step"/>.
    /// </summary>
    /// <inheritdoc cref="FillSequential(Span{byte}, byte, byte)"/>
    public static void FillSequential(this scoped Span<decimal> span, decimal offset = 0, decimal step = 1) => FillSequential<decimal>(span, offset, step);

    /// <summary>
    /// Fills a generic span with sequential values, starting at zero and incrementing by one.
    /// </summary>
    /// <typeparam name="T">The type of elements in the <paramref name="span"/>.</typeparam>
    /// <param name="span">The span to fill with sequential values.</param>
    public static void FillSequential<T>(this scoped Span<T> span) where T : INumberBase<T> => FillSequential(span, T.Zero, T.One);
}
