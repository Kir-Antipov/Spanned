namespace Spanned.Text;

/// <summary>
/// Provides a debug view of the internal state of a <see cref="ValueStringBuilder"/>.
/// </summary>
internal sealed class ValueStringBuilderDebugView
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStringBuilderDebugView"/> class.
    /// </summary>
    /// <param name="sb">The <see cref="ValueStringBuilder"/> to create a debug view for.</param>
    public ValueStringBuilderDebugView(ValueStringBuilder sb)
    {
        Length = sb.Length;
        Capacity = sb.Capacity;
    }

    /// <summary>
    /// The current length of the <see cref="ValueStringBuilder"/>.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// The capacity of the character buffer of the <see cref="ValueStringBuilder"/>.
    /// </summary>
    public int Capacity { get; }
}
