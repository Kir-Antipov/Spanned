namespace Spanned.Collections.Generic;

/// <summary>
/// Provides a debug view for <see cref="ValueDictionary{TKey, TValue}"/>.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
internal sealed class ValueDictionaryDebugView<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// The items represented in the debug view.
    /// </summary>
    private readonly KeyValuePair<TKey, TValue>[] _items;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueDictionaryDebugView{TKey, TValue}"/> class
    /// using a <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="dictionary">
    /// The <see cref="ValueDictionary{TKey, TValue}"/> to be represented in the debug view.
    /// </param>
    public ValueDictionaryDebugView(ValueDictionary<TKey, TValue> dictionary)
    {
        _items = dictionary.DebuggerItems;
    }

    /// <summary>
    /// The items represented in the debug view.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public KeyValuePair<TKey, TValue>[] Items => _items;
}

/// <summary>
/// Provides a debug view for <see cref="ValueDictionary{TKey, TValue}.KeyCollection"/>.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
internal sealed class ValueDictionaryKeyCollectionDebugView<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// The items represented in the debug view.
    /// </summary>
    private readonly TKey[] _items;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueDictionaryKeyCollectionDebugView{TKey, TValue}"/> class
    /// using a <see cref="ValueDictionary{TKey, TValue}.KeyCollection"/>.
    /// </summary>
    /// <param name="keyCollection">
    /// The <see cref="ValueDictionary{TKey, TValue}.KeyCollection"/> to be represented in the debug view.
    /// </param>
    public ValueDictionaryKeyCollectionDebugView(ValueDictionary<TKey, TValue>.KeyCollection keyCollection)
    {
        _items = keyCollection.DebuggerItems;
    }

    /// <summary>
    /// The items represented in the debug view.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public TKey[] Items => _items;
}

/// <summary>
/// Provides a debug view for <see cref="ValueDictionary{TKey, TValue}.ValueCollection"/>.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
internal sealed class ValueDictionaryValueCollectionDebugView<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// The items represented in the debug view.
    /// </summary>
    private readonly TValue[] _items;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueDictionaryValueCollectionDebugView{TKey, TValue}"/> class
    /// using a <see cref="ValueDictionary{TKey, TValue}.ValueCollection"/>.
    /// </summary>
    /// <param name="valueCollection">
    /// The <see cref="ValueDictionary{TKey, TValue}.ValueCollection"/> to be represented in the debug view.
    /// </param>
    public ValueDictionaryValueCollectionDebugView(ValueDictionary<TKey, TValue>.ValueCollection valueCollection)
    {
        _items = valueCollection.DebuggerItems;
    }

    /// <summary>
    /// The items represented in the debug view.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public TValue[] Items => _items;
}
