using System.Collections;

namespace Spanned.Collections.Generic;

/// <summary>
/// Exposes the enumerator, which supports a simple iteration over a collection of
/// a specified type.
/// </summary>
/// <typeparam name="T">The type of objects to enumerate.</typeparam>
internal readonly ref struct ValueEnumerable<T>
{
    /// <summary>
    /// The underlying <see cref="IEnumerable{T}"/> if this instance was constructed from one;
    /// otherwise, it is set to <c>null</c>.
    /// </summary>
    internal readonly IEnumerable<T>? _enumerable;

    /// <summary>
    /// The underlying <see cref="ReadOnlySpan{T}"/> if this instance was constructed from one;
    /// otherwise, it is set to an empty <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    internal readonly ReadOnlySpan<T> _span;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueEnumerable{T}"/> struct.
    /// </summary>
    /// <param name="span">
    /// The <see cref="ReadOnlySpan{T}"/> to create the <see cref="ValueEnumerable{T}"/> from.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueEnumerable(ReadOnlySpan<T> span)
    {
        _enumerable = null;
        _span = span;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueEnumerable{T}"/> struct.
    /// </summary>
    /// <param name="array">
    /// The array to create the <see cref="ValueEnumerable{T}"/> from.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueEnumerable(T[]? array)
    {
        _enumerable = null;
        _span = new(array);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueEnumerable{T}"/> struct.
    /// </summary>
    /// <param name="enumerable">
    /// The <see cref="IEnumerable{T}"/> to create the <see cref="ValueEnumerable{T}"/> from.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueEnumerable(IEnumerable<T>? enumerable)
    {
        _enumerable = enumerable;
        _span = default;
    }

    /// <summary>
    /// Implicitly converts a <see cref="Span{T}"/> to a <see cref="ValueEnumerable{T}"/>.
    /// </summary>
    /// <param name="span">The <see cref="Span{T}"/> to convert to a <see cref="ValueEnumerable{T}"/>.</param>
    public static implicit operator ValueEnumerable<T>(Span<T> span) => new(span);

    /// <summary>
    /// Implicitly converts a <see cref="ReadOnlySpan{T}"/> to a <see cref="ValueEnumerable{T}"/>.
    /// </summary>
    /// <param name="span">The <see cref="ReadOnlySpan{T}"/> to convert to a <see cref="ValueEnumerable{T}"/>.</param>
    public static implicit operator ValueEnumerable<T>(ReadOnlySpan<T> span) => new(span);

    /// <summary>
    /// Implicitly converts an array of elements to a <see cref="ValueEnumerable{T}"/>.
    /// </summary>
    /// <param name="array">The array to convert to a <see cref="ValueEnumerable{T}"/>.</param>
    public static implicit operator ValueEnumerable<T>(T[]? array) => new(array);

    /// <summary>
    /// Implicitly converts a list of elements to a <see cref="ValueEnumerable{T}"/>.
    /// </summary>
    /// <param name="list">The list to convert to a <see cref="ValueEnumerable{T}"/>.</param>
    public static implicit operator ValueEnumerable<T>(List<T>? list) => new(list);

    /// <summary>
    /// An empty <see cref="ValueEnumerable{T}"/>.
    /// </summary>
    public static ValueEnumerable<T> Empty => default;

    /// <summary>
    /// Returns an enumerator to iterate over the <see cref="ValueEnumerable"/>.
    /// </summary>
    /// <returns>
    /// An enumerator to iterate over the <see cref="ValueEnumerable"/>.
    /// </returns>
    public Enumerator GetEnumerator() => new(this);

    /// <inheritdoc/>
    [Obsolete("Equals(object) on ValueEnumerable will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly bool Equals(object? obj)
        => ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct();

    /// <inheritdoc/>
    [Obsolete("GetHashCode() on ValueEnumerable will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly int GetHashCode()
        => ThrowHelper.ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct();

    /// <summary>
    /// Enumerates the elements of a <see cref="ValueEnumerable{T}"/>.
    /// </summary>
    public ref struct Enumerator
    {
        /// <summary>
        /// The underlying <see cref="IEnumerator{T}"/>.
        /// </summary>
        private readonly IEnumerator<T>? _enumerableEnumerator;

        /// <summary>
        /// The underlying <see cref="ReadOnlySpan{T}.Enumerator"/>.
        /// </summary>
        private ReadOnlySpan<T>.Enumerator _spanEnumerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumerator"/> struct.
        /// </summary>
        /// <param name="valueEnumerable">
        /// The <see cref="ValueEnumerable{T}"/> to enumerate.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ValueEnumerable<T> valueEnumerable)
        {
            if (valueEnumerable._enumerable is null)
            {
                _enumerableEnumerator = null;
                _spanEnumerator = valueEnumerable._span.GetEnumerator();
            }
            else
            {
                _enumerableEnumerator = valueEnumerable._enumerable.GetEnumerator();
                _spanEnumerator = default;
            }
        }

        /// <summary>
        /// Advances the enumerator to the next element of the <see cref="ValueEnumerable{T}"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the operation was successful; otherwise, <c>false</c>;
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() => _enumerableEnumerator is null ? _spanEnumerator.MoveNext() : _enumerableEnumerator.MoveNext();

        /// <summary>
        /// The element at the current position of the enumerator.
        /// </summary>
        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _enumerableEnumerator is null ? _spanEnumerator.Current : _enumerableEnumerator.Current;
        }

        /// <summary>
        /// Releases resources used by this enumerator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly void Dispose() => _enumerableEnumerator?.Dispose();
    }
}

/// <summary>
/// Provides a set of static (Shared in Visual Basic) methods for querying
/// <see cref="ValueEnumerable{T}"/>.
/// </summary>
internal static class ValueEnumerable
{
    /// <summary>
    /// Returns the input sequence wrapped in a <see cref="ValueEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of the elements of <paramref name="source"/>.
    /// </typeparam>
    /// <param name="source">
    /// The sequence to wrap in a <see cref="ValueEnumerable{T}"/>.
    /// </param>
    /// <returns>
    /// The input sequence wrapped in a <see cref="ValueEnumerable{T}"/>.
    /// </returns>
    public static ValueEnumerable<TSource> AsValueEnumerable<TSource>(this IEnumerable<TSource>? source)
    {
        if (source.TryGetSpan(out ReadOnlySpan<TSource> span))
            return new(span);

        return new(source);
    }

    /// <summary>
    /// Determines whether a sequence contains any elements.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">The <see cref="ValueEnumerable{T}"/> to check for emptiness.</param>
    /// <returns>
    /// <c>true</c> if the source sequence contains any elements; otherwise, <c>false</c>.
    /// </returns>
    internal static bool Any<TSource>(this scoped ValueEnumerable<TSource> source)
    {
        if (source._enumerable is null)
            return source._span.Length > 0;

        return source._enumerable.Any();
    }

    /// <inheritdoc cref="TryGetSpan{T}(ValueEnumerable{T}, out ReadOnlySpan{T}, out IEnumerable{T})"/>
    public static bool TryGetSpan<TSource>(this ValueEnumerable<TSource> source, out ReadOnlySpan<TSource> span)
    {
        span = source._span;
        return source._enumerable is null || source._enumerable.TryGetSpan(out span);
    }

    /// <summary>
    /// Attempts to retrieve an underlying span from a <see cref="ValueEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">The <see cref="ValueEnumerable{T}"/> to obtain a span from.</param>
    /// <param name="span">When this method returns, contains the span that represents the source, if any.</param>
    /// <param name="enumerable">When this method returns, contains the <see cref="IEnumerable{T}"/> that represents the source, if any.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="source"/> is backed by a span; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetSpan<TSource>(this ValueEnumerable<TSource> source, out ReadOnlySpan<TSource> span, [NotNullWhen(false)] out IEnumerable<TSource>? enumerable)
    {
        span = source._span;
        enumerable = source._enumerable;
        return enumerable is null || enumerable.TryGetSpan(out span);
    }

    /// <inheritdoc cref="TryGetEnumerable{T}(ValueEnumerable{T}, out IEnumerable{T}, out ReadOnlySpan{T})"/>
    public static bool TryGetEnumerable<TSource>(this scoped ValueEnumerable<TSource> source, [NotNullWhen(true)] out IEnumerable<TSource>? enumerable)
    {
        enumerable = source._enumerable;
        return enumerable is not null;
    }

    /// <summary>
    /// Attempts to retrieve an underlying <see cref="IEnumerable{T}"/> from a <see cref="ValueEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">The <see cref="ValueEnumerable{T}"/> to obtain an enumerable from.</param>
    /// <param name="enumerable">When this method returns, contains the <see cref="IEnumerable{T}"/> that represents the source, if any.</param>
    /// <param name="span">When this method returns, contains the span that represents the source, if any.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="source"/> is backed by <see cref="IEnumerable{T}"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetEnumerable<TSource>(this ValueEnumerable<TSource> source, [NotNullWhen(true)] out IEnumerable<TSource>? enumerable, out ReadOnlySpan<TSource> span)
    {
        enumerable = source._enumerable;
        span = source._span;
        return enumerable is not null;
    }

    /// <summary>
    /// Attempts to determine the number of elements in a sequence without forcing an
    /// enumeration.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <param name="source">A sequence that contains elements to be counted.</param>
    /// <param name="count">
    /// When this method returns, contains the number of elements in source, or 0 if
    /// the count couldn't be determined without enumeration.
    /// </param>
    /// <returns>
    /// <c>true</c> if the count of source can be determined without enumeration;
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool TryGetNonEnumeratedCount<TSource>(this scoped ValueEnumerable<TSource> source, out int count)
    {
        if (source._enumerable is null)
        {
            count = source._span.Length;
            return true;
        }

        if (source._enumerable is ICollection<TSource> collection)
        {
            count = collection.Count;
            return true;
        }

        if (source._enumerable is ICollection nonGenericCollection)
        {
            count = nonGenericCollection.Count;
            return true;
        }

        count = 0;
        return false;
    }
}
