using Spanned.Collections;

namespace Spanned;

public static partial class Spans
{
    /// <summary>
    /// Removes all elements that match the conditions defined by the specified predicate
    /// from the span.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="span">The span to remove the elements from.</param>
    /// <param name="predicate">
    /// The predicate that defines the conditions of the elements to remove.
    /// </param>
    /// <returns>
    /// The number of elements that were removed from the span.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
    internal static int RemoveWhere<T>(this scoped Span<T> span, Predicate<T> predicate)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(predicate);

        int freeIndex = 0;

        // Find the first item which needs to be removed.
        while (freeIndex < span.Length && !predicate(span[freeIndex]))
            freeIndex++;

        if (freeIndex >= span.Length)
            return 0;

        int current = freeIndex + 1;
        while (current < span.Length)
        {
            // Find the first item which needs to be kept.
            while (current < span.Length && predicate(span[current]))
                current++;

            if (current < span.Length)
            {
                // Overwrite the item we need to remove with the item that needs to be kept.
                span[freeIndex] = span[current];

                freeIndex++;
                current++;
            }
            else
            {
                break;
            }
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            // Clear the elements so that the GC can reclaim the references.
            span.Slice(freeIndex, span.Length - freeIndex).Clear();
        }

        return span.Length - freeIndex;
    }

    /// <summary>
    /// Removes elements from the span at the specified indices.
    /// </summary>
    /// <typeparam name="T">The type of elements in the span.</typeparam>
    /// <param name="span">The span to remove the elements from.</param>
    /// <param name="indices">
    /// A <see cref="ValueBitArray"/> representing the indices of elements to remove.
    /// <para/>
    /// If an index is set to <c>true</c>, the corresponding element will be removed.
    /// </param>
    /// <returns>
    /// The number of elements that were removed from the span.
    /// </returns>
    internal static int RemoveWhere<T>(this scoped Span<T> span, scoped ValueBitArray indices)
    {
        int freeIndex = 0;

        // Find the first item which needs to be removed.
        while (freeIndex < span.Length && !indices[freeIndex])
            freeIndex++;

        if (freeIndex >= span.Length)
            return 0;

        int current = freeIndex + 1;
        while (current < span.Length)
        {
            // Find the first item which needs to be kept.
            while (current < span.Length && indices[current])
                current++;

            if (current < span.Length)
            {
                // Overwrite the item we need to remove with the item that needs to be kept.
                span[freeIndex] = span[current];

                freeIndex++;
                current++;
            }
            else
            {
                break;
            }
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            // Clear the elements so that the GC can reclaim the references.
            span.Slice(freeIndex, span.Length - freeIndex).Clear();
        }

        return span.Length - freeIndex;
    }
}
