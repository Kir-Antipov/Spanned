namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Searches for the specified value and returns the index of its first occurrence.
    /// </summary>
    /// <typeparam name="T">The type of the span and value.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the span, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the span type.
    /// </param>
    /// <returns>The index of the occurrence of the value in the span. If not found, returns -1.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf<T>(this scoped Span<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
        if (comparer is null || comparer == EqualityComparer<T>.Default)
        {
            if (typeof(T) == typeof(byte))
                return MemoryExtensions.IndexOf(UnsafeCast<T, byte>(span), (byte)(object)value!);

            if (typeof(T) == typeof(sbyte))
                return MemoryExtensions.IndexOf(UnsafeCast<T, sbyte>(span), (sbyte)(object)value!);

            if (typeof(T) == typeof(short))
                return MemoryExtensions.IndexOf(UnsafeCast<T, short>(span), (short)(object)value!);

            if (typeof(T) == typeof(ushort))
                return MemoryExtensions.IndexOf(UnsafeCast<T, ushort>(span), (ushort)(object)value!);

            if (typeof(T) == typeof(int))
                return MemoryExtensions.IndexOf(UnsafeCast<T, int>(span), (int)(object)value!);

            if (typeof(T) == typeof(uint))
                return MemoryExtensions.IndexOf(UnsafeCast<T, uint>(span), (uint)(object)value!);

            if (typeof(T) == typeof(long))
                return MemoryExtensions.IndexOf(UnsafeCast<T, long>(span), (long)(object)value!);

            if (typeof(T) == typeof(ulong))
                return MemoryExtensions.IndexOf(UnsafeCast<T, ulong>(span), (ulong)(object)value!);

            if (typeof(T) == typeof(nint))
                return MemoryExtensions.IndexOf(UnsafeCast<T, nint>(span), (nint)(object)value!);

            if (typeof(T) == typeof(nuint))
                return MemoryExtensions.IndexOf(UnsafeCast<T, nuint>(span), (nuint)(object)value!);

            if (typeof(T) == typeof(float))
                return MemoryExtensions.IndexOf(UnsafeCast<T, float>(span), (float)(object)value!);

            if (typeof(T) == typeof(double))
                return MemoryExtensions.IndexOf(UnsafeCast<T, double>(span), (double)(object)value!);

            if (typeof(T) == typeof(char))
                return MemoryExtensions.IndexOf(UnsafeCast<T, char>(span), (char)(object)value!);

            if (value is IEquatable<T>)
                return IndexOfEquatable(ref MemoryMarshal.GetReference(span), span.Length, value);
        }

        return IndexOf(ref MemoryMarshal.GetReference(span), span.Length, value, comparer);
    }

    /// <inheritdoc cref="IndexOf{T}(Span{T}, T, IEqualityComparer{T}?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOf<T>(this scoped ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
        if (comparer is null || comparer == EqualityComparer<T>.Default)
        {
            if (typeof(T) == typeof(byte))
                return MemoryExtensions.IndexOf(UnsafeCast<T, byte>(span), (byte)(object)value!);

            if (typeof(T) == typeof(sbyte))
                return MemoryExtensions.IndexOf(UnsafeCast<T, sbyte>(span), (sbyte)(object)value!);

            if (typeof(T) == typeof(short))
                return MemoryExtensions.IndexOf(UnsafeCast<T, short>(span), (short)(object)value!);

            if (typeof(T) == typeof(ushort))
                return MemoryExtensions.IndexOf(UnsafeCast<T, ushort>(span), (ushort)(object)value!);

            if (typeof(T) == typeof(int))
                return MemoryExtensions.IndexOf(UnsafeCast<T, int>(span), (int)(object)value!);

            if (typeof(T) == typeof(uint))
                return MemoryExtensions.IndexOf(UnsafeCast<T, uint>(span), (uint)(object)value!);

            if (typeof(T) == typeof(long))
                return MemoryExtensions.IndexOf(UnsafeCast<T, long>(span), (long)(object)value!);

            if (typeof(T) == typeof(ulong))
                return MemoryExtensions.IndexOf(UnsafeCast<T, ulong>(span), (ulong)(object)value!);

            if (typeof(T) == typeof(nint))
                return MemoryExtensions.IndexOf(UnsafeCast<T, nint>(span), (nint)(object)value!);

            if (typeof(T) == typeof(nuint))
                return MemoryExtensions.IndexOf(UnsafeCast<T, nuint>(span), (nuint)(object)value!);

            if (typeof(T) == typeof(float))
                return MemoryExtensions.IndexOf(UnsafeCast<T, float>(span), (float)(object)value!);

            if (typeof(T) == typeof(double))
                return MemoryExtensions.IndexOf(UnsafeCast<T, double>(span), (double)(object)value!);

            if (typeof(T) == typeof(char))
                return MemoryExtensions.IndexOf(UnsafeCast<T, char>(span), (char)(object)value!);

            if (value is IEquatable<T>)
                return IndexOfEquatable(ref MemoryMarshal.GetReference(span), span.Length, value);
        }

        return IndexOf(ref MemoryMarshal.GetReference(span), span.Length, value, comparer);
    }

    /// <summary>
    /// Searches for the specified value using the provided <see cref="IEqualityComparer{T}"/> and returns the index of its first occurrence.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="searchSpace">The reference to the start of the search space.</param>
    /// <param name="length">The length of the search space.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the value type.
    /// </param>
    /// <returns>The index of the occurrence of the value in the search space. If not found, returns -1.</returns>
    private static int IndexOf<T>(ref T searchSpace, int length, T? value, IEqualityComparer<T>? comparer)
    {
        // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations.
        nint end = (nint)length;

        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            for (nint i = 0; i < end; i++)
            {
                // Let JIT de-virtualize `Equals` calls for value types.
                if (EqualityComparer<T>.Default.Equals(value, Unsafe.Add(ref searchSpace, i)))
                    return (int)i;
            }
        }
        else
        {
            comparer ??= EqualityComparer<T>.Default;

            for (nint i = 0; i < end; i++)
            {
                if (comparer.Equals(value, Unsafe.Add(ref searchSpace, i)))
                    return (int)i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Searches for the specified value using <see cref="IEquatable{T}.Equals"/> and returns the index of its first occurrence.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="searchSpace">The reference to the start of the search space.</param>
    /// <param name="length">The length of the search space.</param>
    /// <param name="value">The value to search for.</param>
    /// <returns>The index of the occurrence of the value in the search space. If not found, returns -1.</returns>
    private static int IndexOfEquatable<T>(ref T searchSpace, int length, T value)
    {
        // Use nint for arithmetic to avoid unnecessary 64->32->64 truncations.
        nint end = (nint)length;

        if (value is not null)
        {
            // Use IEquatable<T>.Equals on structs/non-null values.
            for (nint i = 0; i < end; i++)
            {
                // Let JIT de-virtualize `Equals` calls for value types.
                if (((IEquatable<T>)value).Equals(Unsafe.Add(ref searchSpace, i)))
                    return (int)i;
            }
        }
        else
        {
            // Otherwise, search for null.
            for (nint i = 0; i < end; i++)
            {
                if (Unsafe.Add(ref searchSpace, i) is null)
                    return (int)i;
            }
        }

        return -1;
    }
}
