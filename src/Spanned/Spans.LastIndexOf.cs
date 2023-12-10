namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Searches for the specified value and returns the index of its last occurrence.
    /// </summary>
    /// <typeparam name="T">The type of the span and value.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the span, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the span type.
    /// </param>
    /// <returns>The index of the last occurrence of the value in the span. If not found, returns -1.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastIndexOf<T>(this scoped Span<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
        if (comparer is null || comparer == EqualityComparer<T>.Default)
        {
            if (value is IEquatable<T>)
                return LastIndexOfEquatable(ref MemoryMarshal.GetReference(span), span.Length, value);
        }

        return LastIndexOf(ref MemoryMarshal.GetReference(span), span.Length, value, comparer);
    }

    /// <inheritdoc cref="LastIndexOf{T}(Span{T}, T, IEqualityComparer{T}?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LastIndexOf<T>(this scoped ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
        if (comparer is null || comparer == EqualityComparer<T>.Default)
        {
            if (value is IEquatable<T>)
                return LastIndexOfEquatable(ref MemoryMarshal.GetReference(span), span.Length, value);
        }

        return LastIndexOf(ref MemoryMarshal.GetReference(span), span.Length, value, comparer);
    }

    /// <summary>
    /// Searches for the specified value using the provided <see cref="IEqualityComparer{T}"/> and returns the index of its last occurrence.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="searchSpace">The reference to the start of the search space.</param>
    /// <param name="length">The length of the search space.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the value type.
    /// </param>
    /// <returns>The index of the last occurrence of the value in the search space. If not found, returns -1.</returns>
    private static int LastIndexOf<T>(ref T searchSpace, int length, T? value, IEqualityComparer<T>? comparer)
    {
        if (typeof(T).IsValueType && (comparer is null || comparer == EqualityComparer<T>.Default))
        {
            for (int i = length; i > 0;)
            {
                i--;

                // Let JIT de-virtualize `Equals` calls for value types.
                if (EqualityComparer<T>.Default.Equals(value!, Unsafe.Add(ref searchSpace, i)))
                    return i;
            }
        }
        else
        {
            comparer ??= EqualityComparer<T>.Default;

            for (int i = length; i > 0;)
            {
                i--;

                if (comparer.Equals(value!, Unsafe.Add(ref searchSpace, i)))
                    return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Searches for the specified value using <see cref="IEquatable{T}.Equals"/> and returns the index of its last occurrence.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="searchSpace">The reference to the start of the search space.</param>
    /// <param name="length">The length of the search space.</param>
    /// <param name="value">The value to search for.</param>
    /// <returns>The index of the last occurrence of the value in the search space. If not found, returns -1.</returns>
    private static int LastIndexOfEquatable<T>(ref T searchSpace, int length, T value)
    {
        if (value is not null)
        {
            // Use IEquatable<T>.Equals on structs/non-null values.
            for (int i = length; i > 0;)
            {
                i--;

                // Let JIT de-virtualize `Equals` calls for value types.
                if (((IEquatable<T>)value).Equals(Unsafe.Add(ref searchSpace, i)))
                    return i;
            }
        }
        else
        {
            // Otherwise, search for null.
            for (int i = length; i > 0;)
            {
                i--;

                if (Unsafe.Add(ref searchSpace, i) is null)
                    return i;
            }
        }

        return -1;
    }
}
