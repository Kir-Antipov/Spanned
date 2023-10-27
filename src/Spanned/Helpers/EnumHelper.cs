namespace Spanned.Helpers;

/// <summary>
/// Provides utility functions for operations related to enums.
/// </summary>
internal static class EnumHelper
{
    /// <inheritdoc cref="Enum.TryFormat{TEnum}(TEnum, Span{char}, out int, ReadOnlySpan{char})"/>
    /// <remarks>
    /// This is an unconstrained version of the <see cref="Enum.TryFormat{TEnum}(TEnum, Span{char}, out int, ReadOnlySpan{char})"/> method.
    /// </remarks>
    public static bool TryFormatUnconstrained<TEnum>(TEnum value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default)
    {
#if NET9_0_OR_GREATER
        return __Invoke(null, value, destination, out charsWritten, format);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = nameof(TryFormatUnconstrained))]
        static extern bool __Invoke(Enum? targetType, TEnum value, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format);
#else
        if (value is null)
        {
            charsWritten = 0;
            return false;
        }

        return ((ISpanFormattable)value).TryFormat(destination, out charsWritten, format, provider: null);
#endif
    }
}
