namespace Spanned.Helpers;

/// <summary>
/// Provides utility functions for operations related to collections.
/// </summary>
internal static class CollectionHelper
{
    /// <summary>
    /// The maximum allowed capacity for a collection.
    /// </summary>
    /// <remarks>
    /// Represents the same value as <see cref="Array.MaxLength"/>.
    /// </remarks>
    public const int MaxCapacity = 0x7FFFFFC7;

    /// <summary>
    /// The growth factor used during buffer resizing.
    /// </summary>
    public const int GrowthFactor = 2;

    /// <summary>
    /// The threshold used to determine if trimming is necessary.
    /// </summary>
    public const float TrimThreshold = 0.8f;

    /// <summary>
    /// Determines whether trimming should be performed based on the provided
    /// number of elements in the buffer and its actual capacity.
    /// </summary>
    /// <param name="length">The number of elements in the buffer.</param>
    /// <param name="capacity">The current capacity of the buffer.</param>
    /// <returns><c>true</c> if trimming is recommended; otherwise, <c>false</c>.</returns>
    public static bool ShouldTrim(int length, int capacity)
        => length < (int)(capacity * TrimThreshold);

    /// <summary>
    /// Calculates the new capacity for a buffer based on its current length, capacity, and a minimum growth requirement.
    /// </summary>
    /// <param name="length">The number of elements in the buffer.</param>
    /// <param name="capacity">The current capacity of the buffer.</param>
    /// <param name="minimumGrowLength">The minimum number of elements by which the buffer should grow.</param>
    /// <returns>The new capacity for the buffer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalculateNewCapacity(int length, int capacity, int minimumGrowLength)
    {
        Debug.Assert(minimumGrowLength > 0);
        Debug.Assert(length + minimumGrowLength > capacity);

        // Determine the new capacity for the buffer:
        // - Ensure a minimum growth by adding the `minimumGrowLength` to the current length.
        // - Opt for a more aggressive growth by multiplying the current capacity by the `GrowthFactor`.
        // - Select the larger of the two, but ensure it doesn't exceed the maximum allowed capacity.
        return (int)Math.Max(
            (uint)(length + minimumGrowLength),
            Math.Min((uint)capacity * GrowthFactor, MaxCapacity));
    }
}
