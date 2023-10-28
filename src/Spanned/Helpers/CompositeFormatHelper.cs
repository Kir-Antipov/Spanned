using System.Text;

namespace Spanned.Helpers;

/// <summary>
/// Provides utility functions for operations related to the <see cref="CompositeFormat"/> class.
/// </summary>
internal static class CompositeFormatHelper
{
    /// <summary>
    /// Returns the sum of the lengths of all of the literals in the segments.
    /// </summary>
    /// <param name="format">The <see cref="CompositeFormat"/> instance.</param>
    /// <returns>
    /// The sum of the lengths of all of the literals in the segments.
    /// </returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_literalLength")]
    public static extern ref int LiteralLength(this CompositeFormat format);

    /// <summary>
    /// Returns the number of segments in that represent format holes.
    /// </summary>
    /// <param name="format">The <see cref="CompositeFormat"/> instance.</param>
    /// <returns>
    /// The number of segments in that represent format holes.
    /// </returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_formattedCount")]
    public static extern ref int FormattedCount(this CompositeFormat format);

    /// <summary>
    /// Returns the parsed segments that make up the composite format string.
    /// </summary>
    /// <remarks>
    /// Every segment represents either a literal or a format hole, based on whether Literal
    /// is non-null or ArgIndex is non-negative.
    /// </remarks>
    /// <param name="format">The <see cref="CompositeFormat"/> instance.</param>
    /// <returns>
    /// The parsed segments that make up the composite format string.
    /// </returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_segments")]
    public static extern ref (string? Literal, int ArgIndex, int Alignment, string? Format)[] Segments(this CompositeFormat format);

    /// <summary>
    /// Throws an exception if the specified number of arguments is fewer than the number required.
    /// </summary>
    /// <param name="format">The <see cref="CompositeFormat"/> instance.</param>
    /// <param name="numArgs">The number of arguments provided by the caller.</param>
    /// <exception cref="FormatException">An insufficient number of arguments were provided.</exception>
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = nameof(ValidateNumberOfArgs))]
    public static extern void ValidateNumberOfArgs(this CompositeFormat format, int numArgs);
}
