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

            if (typeof(T) == typeof(char) && enumerable is string str)
            {
                span = UnsafeCast<char, T>(str);
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<TTo> UnsafeCast<TFrom, TTo>(this scoped Span<TFrom> span)
    {
        // Source: dotnet/runtime/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/MemoryMarshal.cs#Cast

        // Use unsigned integers - unsigned division by constant (especially by power of 2)
        // and checked casts are faster and smaller.
        uint fromSize = (uint)Unsafe.SizeOf<TFrom>();
        uint toSize = (uint)Unsafe.SizeOf<TTo>();
        uint fromLength = (uint)span.Length;
        int toLength;

        if (fromSize == toSize)
        {
            // Special case for same size types - `(ulong)fromLength * (ulong)fromSize / (ulong)toSize`
            // should be optimized to just `length` but the JIT doesn't do that today.
            toLength = (int)fromLength;
        }
        else if (fromSize == 1)
        {
            // Special case for byte sized TFrom - `(ulong)fromLength * (ulong)fromSize / (ulong)toSize`
            // becomes `(ulong)fromLength / (ulong)toSize` but the JIT can't narrow it down to `int`
            // and can't eliminate the checked cast. This also avoids a 32 bit specific issue,
            // the JIT can't eliminate long multiply by 1.
            toLength = (int)(fromLength / toSize);
        }
        else
        {
            // Ensure that casts are done in such a way that the JIT is able to "see"
            // the uint->ulong casts and the multiply together so that on 32 bit targets
            // 32x32to64 multiplication is used.
            ulong toLengthUInt64 = (ulong)fromLength * (ulong)fromSize / (ulong)toSize;
            toLength = checked((int)toLengthUInt64);
        }

        return MemoryMarshal.CreateSpan(ref Unsafe.As<TFrom, TTo>(ref MemoryMarshal.GetReference(span)), toLength);
    }

    /// <summary>
    /// Casts a Span of <typeparamref name="TFrom"/> to a Span of <typeparamref name="TTo"/>.
    /// </summary>
    /// <remarks>
    /// Supported only for platforms that support misaligned memory access or when the memory block is aligned by other means.
    /// </remarks>
    /// <param name="span">The source slice, of type <typeparamref name="TFrom"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<TTo> UnsafeCast<TFrom, TTo>(this scoped ReadOnlySpan<TFrom> span)
    {
        // Source: dotnet/runtime/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/MemoryMarshal.cs#Cast

        // Use unsigned integers - unsigned division by constant (especially by power of 2)
        // and checked casts are faster and smaller.
        uint fromSize = (uint)Unsafe.SizeOf<TFrom>();
        uint toSize = (uint)Unsafe.SizeOf<TTo>();
        uint fromLength = (uint)span.Length;
        int toLength;
        if (fromSize == toSize)
        {
            // Special case for same size types - `(ulong)fromLength * (ulong)fromSize / (ulong)toSize`
            // should be optimized to just `length` but the JIT doesn't do that today.
            toLength = (int)fromLength;
        }
        else if (fromSize == 1)
        {
            // Special case for byte sized TFrom - `(ulong)fromLength * (ulong)fromSize / (ulong)toSize`
            // becomes `(ulong)fromLength / (ulong)toSize` but the JIT can't narrow it down to `int`
            // and can't eliminate the checked cast. This also avoids a 32 bit specific issue,
            // the JIT can't eliminate long multiply by 1.
            toLength = (int)(fromLength / toSize);
        }
        else
        {
            // Ensure that casts are done in such a way that the JIT is able to "see"
            // the uint->ulong casts and the multiply together so that on 32 bit targets
            // 32x32to64 multiplication is used.
            ulong toLengthUInt64 = (ulong)fromLength * (ulong)fromSize / (ulong)toSize;
            toLength = checked((int)toLengthUInt64);
        }

        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<TFrom, TTo>(ref MemoryMarshal.GetReference(span)), toLength);
    }
}
