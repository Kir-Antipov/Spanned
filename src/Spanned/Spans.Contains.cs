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
                return MemoryExtensions.IndexOf(UnsafeCast<T, byte>(span), (byte)(object)value!) >= 0;

            if (typeof(T) == typeof(sbyte))
                return MemoryExtensions.IndexOf(UnsafeCast<T, sbyte>(span), (sbyte)(object)value!) >= 0;

            if (typeof(T) == typeof(short))
                return MemoryExtensions.IndexOf(UnsafeCast<T, short>(span), (short)(object)value!) >= 0;

            if (typeof(T) == typeof(ushort))
                return MemoryExtensions.IndexOf(UnsafeCast<T, ushort>(span), (ushort)(object)value!) >= 0;

            if (typeof(T) == typeof(int))
                return MemoryExtensions.IndexOf(UnsafeCast<T, int>(span), (int)(object)value!) >= 0;

            if (typeof(T) == typeof(uint))
                return MemoryExtensions.IndexOf(UnsafeCast<T, uint>(span), (uint)(object)value!) >= 0;

            if (typeof(T) == typeof(long))
                return MemoryExtensions.IndexOf(UnsafeCast<T, long>(span), (long)(object)value!) >= 0;

            if (typeof(T) == typeof(ulong))
                return MemoryExtensions.IndexOf(UnsafeCast<T, ulong>(span), (ulong)(object)value!) >= 0;

            if (typeof(T) == typeof(float))
                return MemoryExtensions.IndexOf(UnsafeCast<T, float>(span), (float)(object)value!) >= 0;

            if (typeof(T) == typeof(double))
                return MemoryExtensions.IndexOf(UnsafeCast<T, double>(span), (double)(object)value!) >= 0;

            if (typeof(T) == typeof(char))
                return MemoryExtensions.IndexOf(UnsafeCast<T, char>(span), (char)(object)value!) >= 0;

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
                return MemoryExtensions.IndexOf(UnsafeCast<T, byte>(span), (byte)(object)value!) >= 0;

            if (typeof(T) == typeof(sbyte))
                return MemoryExtensions.IndexOf(UnsafeCast<T, sbyte>(span), (sbyte)(object)value!) >= 0;

            if (typeof(T) == typeof(short))
                return MemoryExtensions.IndexOf(UnsafeCast<T, short>(span), (short)(object)value!) >= 0;

            if (typeof(T) == typeof(ushort))
                return MemoryExtensions.IndexOf(UnsafeCast<T, ushort>(span), (ushort)(object)value!) >= 0;

            if (typeof(T) == typeof(int))
                return MemoryExtensions.IndexOf(UnsafeCast<T, int>(span), (int)(object)value!) >= 0;

            if (typeof(T) == typeof(uint))
                return MemoryExtensions.IndexOf(UnsafeCast<T, uint>(span), (uint)(object)value!) >= 0;

            if (typeof(T) == typeof(long))
                return MemoryExtensions.IndexOf(UnsafeCast<T, long>(span), (long)(object)value!) >= 0;

            if (typeof(T) == typeof(ulong))
                return MemoryExtensions.IndexOf(UnsafeCast<T, ulong>(span), (ulong)(object)value!) >= 0;

            if (typeof(T) == typeof(float))
                return MemoryExtensions.IndexOf(UnsafeCast<T, float>(span), (float)(object)value!) >= 0;

            if (typeof(T) == typeof(double))
                return MemoryExtensions.IndexOf(UnsafeCast<T, double>(span), (double)(object)value!) >= 0;

            if (typeof(T) == typeof(char))
                return MemoryExtensions.IndexOf(UnsafeCast<T, char>(span), (char)(object)value!) >= 0;

            if (value is IEquatable<T>)
                return IndexOfEquatable(ref MemoryMarshal.GetReference(span), span.Length, value) >= 0;
        }

        return IndexOf(ref MemoryMarshal.GetReference(span), span.Length, value, comparer) >= 0;
    }
}
