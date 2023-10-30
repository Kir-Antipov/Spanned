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
            if (typeof(T) == typeof(byte))
                return MemoryExtensions.Contains(UnsafeCast<T, byte>(span), (byte)(object)value!);

            if (typeof(T) == typeof(sbyte))
                return MemoryExtensions.Contains(UnsafeCast<T, sbyte>(span), (sbyte)(object)value!);

            if (typeof(T) == typeof(short))
                return MemoryExtensions.Contains(UnsafeCast<T, short>(span), (short)(object)value!);

            if (typeof(T) == typeof(ushort))
                return MemoryExtensions.Contains(UnsafeCast<T, ushort>(span), (ushort)(object)value!);

            if (typeof(T) == typeof(int))
                return MemoryExtensions.Contains(UnsafeCast<T, int>(span), (int)(object)value!);

            if (typeof(T) == typeof(uint))
                return MemoryExtensions.Contains(UnsafeCast<T, uint>(span), (uint)(object)value!);

            if (typeof(T) == typeof(long))
                return MemoryExtensions.Contains(UnsafeCast<T, long>(span), (long)(object)value!);

            if (typeof(T) == typeof(ulong))
                return MemoryExtensions.Contains(UnsafeCast<T, ulong>(span), (ulong)(object)value!);

            if (typeof(T) == typeof(nint))
                return MemoryExtensions.Contains(UnsafeCast<T, nint>(span), (nint)(object)value!);

            if (typeof(T) == typeof(nuint))
                return MemoryExtensions.Contains(UnsafeCast<T, nuint>(span), (nuint)(object)value!);

            if (typeof(T) == typeof(float))
                return MemoryExtensions.Contains(UnsafeCast<T, float>(span), (float)(object)value!);

            if (typeof(T) == typeof(double))
                return MemoryExtensions.Contains(UnsafeCast<T, double>(span), (double)(object)value!);

            if (typeof(T) == typeof(char))
                return MemoryExtensions.Contains(UnsafeCast<T, char>(span), (char)(object)value!);

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
            if (typeof(T) == typeof(byte))
                return MemoryExtensions.Contains(UnsafeCast<T, byte>(span), (byte)(object)value!);

            if (typeof(T) == typeof(sbyte))
                return MemoryExtensions.Contains(UnsafeCast<T, sbyte>(span), (sbyte)(object)value!);

            if (typeof(T) == typeof(short))
                return MemoryExtensions.Contains(UnsafeCast<T, short>(span), (short)(object)value!);

            if (typeof(T) == typeof(ushort))
                return MemoryExtensions.Contains(UnsafeCast<T, ushort>(span), (ushort)(object)value!);

            if (typeof(T) == typeof(int))
                return MemoryExtensions.Contains(UnsafeCast<T, int>(span), (int)(object)value!);

            if (typeof(T) == typeof(uint))
                return MemoryExtensions.Contains(UnsafeCast<T, uint>(span), (uint)(object)value!);

            if (typeof(T) == typeof(long))
                return MemoryExtensions.Contains(UnsafeCast<T, long>(span), (long)(object)value!);

            if (typeof(T) == typeof(ulong))
                return MemoryExtensions.Contains(UnsafeCast<T, ulong>(span), (ulong)(object)value!);

            if (typeof(T) == typeof(nint))
                return MemoryExtensions.Contains(UnsafeCast<T, nint>(span), (nint)(object)value!);

            if (typeof(T) == typeof(nuint))
                return MemoryExtensions.Contains(UnsafeCast<T, nuint>(span), (nuint)(object)value!);

            if (typeof(T) == typeof(float))
                return MemoryExtensions.Contains(UnsafeCast<T, float>(span), (float)(object)value!);

            if (typeof(T) == typeof(double))
                return MemoryExtensions.Contains(UnsafeCast<T, double>(span), (double)(object)value!);

            if (typeof(T) == typeof(char))
                return MemoryExtensions.Contains(UnsafeCast<T, char>(span), (char)(object)value!);

            if (value is IEquatable<T>)
                return IndexOfEquatable(ref MemoryMarshal.GetReference(span), span.Length, value) >= 0;
        }

        return IndexOf(ref MemoryMarshal.GetReference(span), span.Length, value, comparer) >= 0;
    }
}
