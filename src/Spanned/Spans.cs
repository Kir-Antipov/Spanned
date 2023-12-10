namespace Spanned;

/// <summary>
/// Provides extension methods for the span-related types, such as <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/>.
/// </summary>
public static partial class Spans
{
    /// <summary>
    /// Attempts to obtain a <see cref="ReadOnlySpan{T}"/> representation of the elements in the enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="enumerable">The enumerable to convert to a span.</param>
    /// <param name="span">
    /// When this method returns, contains the <see cref="ReadOnlySpan{T}"/> representation
    /// of the elements in the enumerable, or an empty span if the conversion is not possible.
    /// </param>
    /// <returns>
    /// <c>true</c> if the conversion was successful;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetSpan<T>(this IEnumerable<T>? enumerable, out ReadOnlySpan<T> span)
    {
        if (enumerable is not null)
        {
            if (enumerable.GetType() == typeof(T[]))
            {
                span = (T[])enumerable;
                return true;
            }
        }

        span = default;
        return false;
    }

    /// <summary>
    /// Copies elements of a <see cref="Span{T}"/> to a new <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="span">The span to convert to a list.</param>
    /// <returns>A new <see cref="List{T}"/> containing the elements from the span.</returns>
    public static List<T> ToList<T>(this scoped Span<T> span)
        => ToList((ReadOnlySpan<T>)span);

    /// <summary>
    /// Copies elements of a <see cref="Span{T}"/> to a new <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="span">The span to convert to a list.</param>
    /// <returns>A new <see cref="List{T}"/> containing the elements from the span.</returns>
    public static List<T> ToList<T>(this scoped ReadOnlySpan<T> span)
    {
        List<T> list = new(span.Length);

        for (int i = 0; i < span.Length; i++)
            list.Add(span[i]);

        return list;
    }

    /// <summary>
    /// Casts a Span of <typeparamref name="TFrom"/> to a Span of <typeparamref name="TTo"/>.
    /// </summary>
    /// <remarks>
    /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
    /// </remarks>
    /// <param name="span">The source slice, of type <typeparamref name="TFrom"/>.</param>
    public static Span<TTo> UnsafeCast<TFrom, TTo>(this Span<TFrom> span)
        where TFrom : struct
        where TTo : struct
        => MemoryMarshal.Cast<TFrom, TTo>(span);

    /// <summary>
    /// Casts a Span of <typeparamref name="TFrom"/> to a Span of <typeparamref name="TTo"/>.
    /// </summary>
    /// <remarks>
    /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
    /// </remarks>
    /// <param name="span">The source slice, of type <typeparamref name="TFrom"/>.</param>
    public static ReadOnlySpan<TTo> UnsafeCast<TFrom, TTo>(this ReadOnlySpan<TFrom> span)
        where TFrom : struct
        where TTo : struct
        => MemoryMarshal.Cast<TFrom, TTo>(span);
}
