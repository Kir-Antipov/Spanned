namespace Spanned.Collections.Generic;

/// <summary>
/// Provides a debug view for value collections.
/// </summary>
/// <typeparam name="T">The type of elements in the value collection.</typeparam>
internal sealed class ValueCollectionDebugView<T>
{
    /// <summary>
    /// The items represented in the debug view.
    /// </summary>
    private readonly T[] _items;

    /// <summary>
    /// The items represented in the debug view.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => _items;
}
