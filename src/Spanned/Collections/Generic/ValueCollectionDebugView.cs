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
    /// Initializes a new instance of the <see cref="ValueCollectionDebugView{T}"/> class
    /// using a <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="list">The <see cref="ValueList{T}"/> to be represented in the debug view.</param>
    public ValueCollectionDebugView(ValueList<T> list)
    {
        _items = list.DebuggerItems;
    }

    /// <summary>
    /// The items represented in the debug view.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => _items;
}
