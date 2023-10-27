namespace Spanned.Helpers;

/// <summary>
/// Provides constants related to operations on strings and char buffers.
/// </summary>
internal static class StringHelper
{
    /// <summary>
    /// The maximum size for stack-allocated character buffers.
    /// </summary>
    public const int StackallocCharBufferSizeLimit = 256;

    /// <summary>
    /// The maximum length of a string representation of a <see cref="byte"/> value.
    /// </summary>
    /// <remarks>
    /// <c>byte.MaxValue.ToString().Length</c>
    /// </remarks>
    public const int MaxByteStringLength = 3;

    /// <summary>
    /// The maximum length of a string representation of a <see cref="sbyte"/> value.
    /// </summary>
    /// <remarks>
    /// <c>sbyte.MinValue.ToString().Length</c>
    /// </remarks>
    public const int MaxSbyteStringLength = 4;

    /// <summary>
    /// The maximum length of a string representation of a <see cref="short"/> value.
    /// </summary>
    /// <remarks>
    /// <c>short.MinValue.ToString().Length</c>
    /// </remarks>
    public const int MaxInt16StringLength = 6;

    /// <summary>
    /// The maximum length of a string representation of a <see cref="ushort"/> value.
    /// </summary>
    /// <remarks>
    /// <c>ushort.MaxValue.ToString().Length</c>
    /// </remarks>
    public const int MaxUInt16StringLength = 5;

    /// <summary>
    /// The maximum length of a string representation of an <see cref="int"/> value.
    /// </summary>
    /// <remarks>
    /// <c>int.MinValue.ToString().Length</c>
    /// </remarks>
    public const int MaxInt32StringLength = 11;

    /// <summary>
    /// The maximum length of a string representation of a <see cref="uint"/> value.
    /// </summary>
    /// <remarks>
    /// <c>ushort.MaxValue.ToString().Length</c>
    /// </remarks>
    public const int MaxUInt32StringLength = 10;

    /// <summary>
    /// The maximum length of a string representation of a <see cref="long"/> value.
    /// </summary>
    /// <remarks>
    /// <c>long.MinValue.ToString().Length</c>
    /// </remarks>
    public const int MaxInt64StringLength = 20;

    /// <summary>
    /// The maximum length of a string representation of a <see cref="ulong"/> value.
    /// </summary>
    /// <remarks>
    /// <c>ulong.MaxValue.ToString().Length</c>
    /// </remarks>
    public const int MaxUInt64StringLength = 20;

    /// <summary>
    /// The maximum length of a string representation of a <see cref="float"/> value.
    /// </summary>
    /// <remarks>
    /// <c>(-?\d{1,9}\.E[+-]\d{2}) -> 15</c>
    /// <para/>
    /// But you never know with floating point numbers, so
    /// let's take something a little big bigger to make sure.
    /// </remarks>
    public const int MaxSingleStringLength = 32;

    /// <summary>
    /// The maximum length of a string representation of a <see cref="double"/> value.
    /// </summary>
    /// <remarks>
    /// <c>(-?\d{1,17}\.E[+-]\d{2,3}) -> 24</c>
    /// <para/>
    /// But you never know with floating point numbers, so
    /// let's take something a little big bigger to make sure.
    /// </remarks>
    public const int MaxDoubleStringLength = 64;

    /// <summary>
    /// The maximum length of a string representation of a <see cref="decimal"/> value.
    /// </summary>
    /// <remarks>
    /// <c>decimal.MinValue.ToString().Length + 1 (decimal point)</c>
    /// </remarks>
    public const int MaxDecimalStringLength = 31;
}
