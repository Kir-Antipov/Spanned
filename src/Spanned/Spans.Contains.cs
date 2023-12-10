namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Indicates whether a specified value is found in a span.
    /// </summary>
    /// <typeparam name="T">The type of the span and value.</typeparam>
    /// <param name="span">The span to search.</param>
    /// <param name="value">The value to search for.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the span, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the span type.
    /// </param>
    /// <returns><c>true</c> if found; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains<T>(this scoped Span<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
        if (comparer is null || comparer == EqualityComparer<T>.Default)
        {
            if (value is IEquatable<T>)
                return IndexOfEquatable(ref MemoryMarshal.GetReference(span), span.Length, value) >= 0;
        }

        return IndexOf(ref MemoryMarshal.GetReference(span), span.Length, value, comparer) >= 0;
    }

    /// <inheritdoc cref="Contains{T}(Span{T}, T, IEqualityComparer{T}?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains<T>(this scoped ReadOnlySpan<T> span, T value, IEqualityComparer<T>? comparer = null)
    {
        if (comparer is null || comparer == EqualityComparer<T>.Default)
        {
            if (value is IEquatable<T>)
                return IndexOfEquatable(ref MemoryMarshal.GetReference(span), span.Length, value) >= 0;
        }

        return IndexOf(ref MemoryMarshal.GetReference(span), span.Length, value, comparer) >= 0;
    }
}
