namespace Spanned.Collections.Generic;

/// <summary>
/// Represents a collection of keys and values.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
[DebuggerTypeProxy(typeof(ValueDictionaryDebugView<,>))]
[DebuggerDisplay("Count = {Count}")]
public ref struct ValueDictionary<TKey, TValue> where TKey : notnull
{
    /// <summary>
    /// The buffer that is rented from an <see cref="ArrayPool{T}"/>, if one is used.
    /// </summary>
    private KeyValuePair<TKey, TValue>[]? _rentedBuffer;

    /// <summary>
    /// The item buffer used by this instance.
    /// </summary>
    private Span<KeyValuePair<TKey, TValue>> _buffer;

    /// <summary>
    /// The number of elements contained in the buffer.
    /// </summary>
    private int _count;

    /// <summary>
    /// The <see cref="IEqualityComparer{T}"/> object that is used to
    /// determine equality for the keys in the dictionary.
    /// </summary>
    private readonly IEqualityComparer<TKey>? _comparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueDictionary{TKey, TValue}"/> struct that
    /// is empty and uses the specified equality comparer for the keys.
    /// </summary>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys in the dictionary, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the dictionary type.
    /// </param>
    public ValueDictionary(IEqualityComparer<TKey>? comparer)
    {
        _rentedBuffer = null;
        _buffer = default;
        _count = 0;

        if (typeof(TKey).IsValueType)
        {
            // Do not initialize `EqualityComparer<T>.Default` for value types,
            // if possible.
            if (comparer is not null && comparer == EqualityComparer<TKey>.Default)
                comparer = null;

            _comparer = comparer;
        }
        else
        {
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
        }
    }

    /// <inheritdoc cref="ValueDictionary(Span{KeyValuePair{TKey, TValue}}, IEqualityComparer{TKey})"/>
    public ValueDictionary(Span<KeyValuePair<TKey, TValue>> buffer)
    {
        _rentedBuffer = null;
        _buffer = buffer;
        _count = 0;
        _comparer = typeof(TKey).IsValueType ? null : EqualityComparer<TKey>.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueDictionary{TKey, TValue}"/> struct using the provided
    /// initial buffer.
    /// </summary>
    /// <param name="buffer">The initial item buffer for this instance.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys in the dictionary, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the dictionary type.
    /// </param>
    public ValueDictionary(Span<KeyValuePair<TKey, TValue>> buffer, IEqualityComparer<TKey>? comparer)
        : this(comparer)
    {
        _buffer = buffer;
    }

    /// <inheritdoc cref="ValueDictionary(int, IEqualityComparer{TKey})"/>
    public ValueDictionary(int capacity)
    {
        _rentedBuffer = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _count = 0;
        _comparer = typeof(TKey).IsValueType ? null : EqualityComparer<TKey>.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueDictionary{TKey, TValue}"/> struct that
    /// is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new dictionary can initially store.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys in the dictionary, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the dictionary type.
    /// </param>
    public ValueDictionary(int capacity, IEqualityComparer<TKey>? comparer)
        : this(comparer)
    {
        _rentedBuffer = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
    }

    /// <inheritdoc cref="ValueDictionary(ReadOnlySpan{KeyValuePair{TKey, TValue}}, IEqualityComparer{TKey})"/>
    public ValueDictionary(scoped ReadOnlySpan<KeyValuePair<TKey, TValue>> collection)
        : this(collection, collection.Length, comparer: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueDictionary{TKey, TValue}"/> struct that
    /// contains elements copied from the specified collection and has sufficient capacity
    /// to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new dictionary.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys in the dictionary, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the dictionary type.
    /// </param>
    public ValueDictionary(scoped ReadOnlySpan<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer)
        : this(collection, collection.Length, comparer)
    {
    }

    /// <inheritdoc cref="ValueDictionary(ReadOnlySpan{KeyValuePair{TKey, TValue}}, int, IEqualityComparer{TKey})"/>
    public ValueDictionary(scoped ReadOnlySpan<KeyValuePair<TKey, TValue>> collection, int capacity)
        : this(collection, capacity, comparer: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueDictionary{TKey, TValue}"/> struct that
    /// contains elements copied from the specified collection and has the specified capacity.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new dictionary.</param>
    /// <param name="capacity">The number of elements that the new dictionary can initially store.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys in the dictionary, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the dictionary type.
    /// </param>
    public ValueDictionary(scoped ReadOnlySpan<KeyValuePair<TKey, TValue>> collection, int capacity, IEqualityComparer<TKey>? comparer)
        : this(comparer)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(capacity, collection.Length);

        _rentedBuffer = capacity == 0 ? null : ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;

        foreach (KeyValuePair<TKey, TValue> entry in collection)
            Add(entry.Key, entry.Value, ValueDictionaryAddType.ThrowOnExisting);
    }

    /// <inheritdoc cref="ValueDictionary(IEnumerable{KeyValuePair{TKey, TValue}}, IEqualityComparer{TKey})"/>
    public ValueDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        : this(collection, comparer: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueDictionary{TKey, TValue}"/> struct that
    /// contains elements copied from the specified collection and has sufficient capacity
    /// to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new dictionary.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys in the dictionary, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the dictionary type.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public ValueDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer)
        : this(comparer)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        _buffer = _rentedBuffer = default;
        _count = 0;

        foreach (KeyValuePair<TKey, TValue> entry in collection)
            Add(entry.Key, entry.Value, ValueDictionaryAddType.ThrowOnExisting);
    }

    /// <summary>
    /// Implicitly converts a <see cref="ValueDictionary{TKey, TValue}"/> to a span.
    /// </summary>
    /// <param name="dictionary">The dictionary to be converted.</param>
    /// <returns>A span covering the content of the <see cref="ValueDictionary{TKey, TValue}"/>.</returns>
    public static implicit operator ReadOnlySpan<KeyValuePair<TKey, TValue>>(ValueDictionary<TKey, TValue> dictionary)
        => dictionary._buffer.Slice(0, dictionary._count);

    /// <summary>
    /// Returns a span that represents the content of the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <remarks>
    /// In the case of direct modification, the uniqueness of the elements is no longer guaranteed.
    /// </remarks>
    /// <returns>A span covering the content of the <see cref="ValueDictionary{TKey, TValue}"/>.</returns>
    public readonly Span<KeyValuePair<TKey, TValue>> AsSpan()
        => _buffer.Slice(0, _count);

    /// <summary>
    /// Returns a span that represents a segment of the dictionary starting from the specified index.
    /// </summary>
    /// <remarks>
    /// In the case of direct modification, the uniqueness of the elements is no longer guaranteed.
    /// </remarks>
    /// <param name="start">The start index of the segment.</param>
    /// <returns>A span covering the segment of the dictionary.</returns>
    public readonly Span<KeyValuePair<TKey, TValue>> AsSpan(int start) => AsSpan(start, _count - start);

    /// <summary>
    /// Returns a span that represents a segment of the dictionary with the specified start index and length.
    /// </summary>
    /// <remarks>
    /// In the case of direct modification, the uniqueness of the elements is no longer guaranteed.
    /// </remarks>
    /// <param name="start">The start index of the segment.</param>
    /// <param name="length">The length of the segment.</param>
    /// <returns>A span covering the segment of the dictionary.</returns>
    public readonly Span<KeyValuePair<TKey, TValue>> AsSpan(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        return _buffer.Slice(start, length);
    }

    /// <summary>
    /// Returns a span that represents the remaining unused segment of the dictionary.
    /// </summary>
    /// <remarks>
    /// In the case of direct modification, the uniqueness of the elements is no longer guaranteed.
    /// </remarks>
    /// <returns>A span covering the unused segment of the dictionary.</returns>
    public readonly Span<KeyValuePair<TKey, TValue>> AsRemainingSpan() => _buffer.Slice(_count);

    /// <summary>
    /// Returns a span that represents the entire capacity of the dictionary,
    /// including used and unused segments.
    /// </summary>
    /// <remarks>
    /// In the case of direct modification, the uniqueness of the elements is no longer guaranteed.
    /// </remarks>
    /// <returns>A span covering the entire buffer of the dictionary.</returns>
    public readonly Span<KeyValuePair<TKey, TValue>> AsCapacitySpan() => _buffer;

    /// <summary>
    /// The number of key/value pairs contained in the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _count;
        set
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCount(value, _buffer.Length);

            _count = value;
        }
    }

    /// <summary>
    /// The total number of key/value pairs the internal data structure can hold
    /// without resizing.
    /// </summary>
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _buffer.Length;
        set
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(value, _count);

            if (value != _buffer.Length)
            {
                ResizeBuffer(value);
            }
        }
    }

    /// <summary>
    /// The <see cref="IEqualityComparer{T}"/> object that is used to
    /// determine equality for the keys in the dictionary.
    /// </summary>
    public readonly IEqualityComparer<TKey> Comparer => _comparer ?? EqualityComparer<TKey>.Default;

    /// <summary>
    /// A collection containing the keys in the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    public readonly KeyCollection Keys => new(this);

    /// <summary>
    /// A collection containing the values in the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    public readonly ValueCollection Values => new(this);

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <remarks>
    /// If the specified key is not found,
    /// a get operation throws a <see cref="KeyNotFoundException"/>, and
    /// a set operation creates a new element with the specified key.
    /// </remarks>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    /// <exception cref="KeyNotFoundException">
    /// The property is retrieved and key does not exist in the collection.
    /// </exception>
    public TValue this[TKey key]
    {
        readonly get
        {
            if (IndexOfKey(key, out TValue value) >= 0)
                return value;

            ThrowHelper.ThrowKeyNotFoundException(key);
            return default;
        }
        set => Add(key, value, ValueDictionaryAddType.OverwriteExisting);
    }

    /// <summary>
    /// Returns the items contained in this instance for debugging purposes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal readonly KeyValuePair<TKey, TValue>[] DebuggerItems
    {
        get
        {
            // `.AsSpan().ToArray()` can break the debugger.
            // Therefore, prefer working with the underlying array directly when possible.
            if (_rentedBuffer is null)
                return AsSpan().ToArray();

            KeyValuePair<TKey, TValue>[] items = new KeyValuePair<TKey, TValue>[_count];
            Array.Copy(_rentedBuffer, items, items.Length);
            return items;
        }
    }

    /// <summary>
    /// Ensures that the dictionary can hold up to a specified number of entries without
    /// any further expansion of its backing storage.
    /// </summary>
    /// <param name="capacity">The number of entries.</param>
    /// <returns>The current capacity of the <see cref="ValueDictionary{TKey, TValue}"/>.</returns>
    public int EnsureCapacity(int capacity)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(capacity);

        if ((uint)capacity > (uint)_buffer.Length)
            Grow(capacity - _count);

        return _buffer.Length;
    }

    /// <summary>
    /// Provides a reference to the first entry of this instance's buffer, which can be used for pinning.
    /// </summary>
    /// <returns>A reference to the first entry of the buffer.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ref KeyValuePair<TKey, TValue> GetPinnableReference()
        => ref MemoryMarshal.GetReference(_buffer);

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified key,
    /// if the key is found; otherwise, the default value for the type of the value parameter.
    /// </param>
    /// <returns>
    /// <c>true</c> if the <see cref="ValueDictionary{TKey, TValue}"/> contains an element with
    /// the specified key; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public readonly bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        int i = IndexOfKey(key, out value);
        return i >= 0;
    }

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">
    /// The value of the element to add.
    /// <para/>
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </exception>
    public void Add(TKey key, TValue value) => Add(key, value, ValueDictionaryAddType.ThrowOnExisting);

    /// <summary>
    /// Attempts to add the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">
    /// The value of the element to add.
    /// <para/>
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <returns>
    /// <c>true</c> if the key/value pair was added to the dictionary successfully;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public bool TryAdd(TKey key, TValue value) => Add(key, value, ValueDictionaryAddType.SkipExisting);

    /// <summary>
    /// Adds the specified key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">
    /// The value of the element to add.
    /// <para/>
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <param name="addType">
    /// The type of action to take if the key already exists in the dictionary.
    /// </param>
    /// <returns>
    /// <c>true</c> if the key/value pair was added to the dictionary successfully;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the <see cref="ValueDictionary{TKey, TValue}"/>
    /// and the specified <paramref name="addType"/> is set to <see cref="ValueDictionaryAddType.ThrowOnExisting"/>.
    /// </exception>
    private bool Add(TKey key, TValue value, ValueDictionaryAddType addType)
    {
        if (key is null)
            ThrowHelper.ThrowArgumentNullException(nameof(key));

        int index = IndexOfKey(key, out _);
        if (index >= 0)
        {
            switch (addType)
            {
                case ValueDictionaryAddType.ThrowOnExisting:
                    ThrowHelper.ThrowArgumentException_DuplicateKey(key);
                    return false;

                case ValueDictionaryAddType.OverwriteExisting:
                    _buffer[index] = new(key, value);
                    return true;

                case ValueDictionaryAddType.SkipExisting:
                    return false;
            }
        }

        if ((uint)_count >= (uint)_buffer.Length)
            Grow(1);

        _buffer[_count] = new(key, value);
        _count++;
        return true;
    }

    /// <summary>
    /// Removes the value with the specified key from the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>
    /// <c>true</c> if the element is successfully found and removed;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public bool Remove(TKey key) => Remove(key, out _);

    /// <summary>
    /// Removes the value with the specified key from the <see cref="ValueDictionary{TKey, TValue}"/>,
    /// and copies the element to the value parameter.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <param name="value">The removed element.</param>
    /// <returns>
    /// <c>true</c> if the element is successfully found and removed;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        int index = IndexOfKey(key, out value);
        if (index < 0)
            return false;

        // Clear the element so that the GC can reclaim its reference.
        _buffer[index] = default!;

        _buffer.Slice(index + 1).CopyTo(_buffer.Slice(index));
        _count--;

        return true;
    }

    /// <summary>
    /// Removes the entry at the specified index of the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the entry to remove.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(index, _count);

        // Clear the element so that the GC can reclaim its reference.
        _buffer[index] = default!;

        _buffer.Slice(index + 1).CopyTo(_buffer.Slice(index));
        _count--;
    }

    /// <summary>
    /// Removes a range of entries from the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="start">The zero-based starting index of the range of entries to remove.</param>
    /// <param name="length">The number of entries to remove.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void RemoveRange(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        // Clear the elements so that the GC can reclaim the references.
        AsSpan(start, length).Clear();

        _buffer.Slice(start + length).CopyTo(_buffer.Slice(start));
        _count -= length;
    }

    /// <summary>
    /// Determines whether the <see cref="ValueDictionary{TKey, TValue}"/> contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="ValueDictionary{TKey, TValue}"/>.</param>
    /// <returns>
    /// <c>true</c> if the <see cref="ValueDictionary{TKey, TValue}"/> contains an element with the specified key;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    public readonly bool ContainsKey(TKey key) => IndexOfKey(key, out _) >= 0;

    /// <summary>
    /// Determines whether the <see cref="ValueDictionary{TKey, TValue}"/> contains the specified value.
    /// </summary>
    /// <param name="value">
    /// The value to locate in the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// <para/>
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <returns>
    /// <c>true</c> if the <see cref="ValueDictionary{TKey, TValue}"/> contains an element with the specified value;
    /// otherwise, <c>false</c>.
    /// </returns>
    public readonly bool ContainsValue(TValue value) => IndexOfValue(value, out _) >= 0;

    /// <summary>
    /// Removes all keys and values from the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    public void Clear()
    {
        // Clear the elements so that the GC can reclaim the references.
        AsSpan().Clear();

        _count = 0;
    }

    /// <summary>
    /// Searches for the specified key and returns the zero-based index of its occurrence
    /// within the entire <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified key, if the key is found;
    /// otherwise, the default value for the type of the <typeparamref name="TValue"/> parameter.
    /// </param>
    /// <returns>
    /// The zero-based index of the occurrence of the specified key within the entire dictionary,
    /// if found; otherwise, -1.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
    internal readonly int IndexOfKey(TKey key, out TValue value)
    {
        if (key is null)
        {
            ThrowHelper.ThrowArgumentNullException(nameof(key));
        }

        Span<KeyValuePair<TKey, TValue>> entries = AsSpan();
        IEqualityComparer<TKey>? comparer = _comparer;

        if (typeof(TKey).IsValueType && comparer is null)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                KeyValuePair<TKey, TValue> pair = entries[i];
                if (EqualityComparer<TKey>.Default.Equals(key, pair.Key))
                {
                    value = pair.Value;
                    return i;
                }
            }
        }
        else
        {
            Debug.Assert(comparer is not null);

            for (int i = 0; i < entries.Length; i++)
            {
                KeyValuePair<TKey, TValue> pair = entries[i];
                if (comparer!.Equals(key, pair.Key))
                {
                    value = pair.Value;
                    return i;
                }
            }
        }

        value = default!;
        return -1;
    }

    /// <summary>
    /// Searches for the specified value and returns the zero-based index of the first occurrence
    /// within the entire <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="value">The value to locate in the dictionary.</param>
    /// <param name="key">
    /// When this method returns, contains the key associated with the specified value, if the value is found;
    /// otherwise, the default value for the type of the <typeparamref name="TKey"/> parameter.
    /// </param>
    /// <returns>
    /// The zero-based index of the first occurrence of the specified value within the entire dictionary,
    /// if found; otherwise, -1.
    /// </returns>
    internal readonly int IndexOfValue(TValue value, out TKey key)
    {
        Span<KeyValuePair<TKey, TValue>> entries = AsSpan();

        for (int i = 0; i < entries.Length; i++)
        {
            KeyValuePair<TKey, TValue> pair = entries[i];
            if (EqualityComparer<TValue>.Default.Equals(value, pair.Value))
            {
                key = pair.Key;
                return i;
            }
        }

        key = default!;
        return -1;
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="length">The number of elements in the range.</param>
    /// <returns>
    /// A shallow copy of a range of elements in the source <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ValueDictionary<TKey, TValue> Slice(int start, int length)
    {
        // This method is needed for the slicing syntax ([i..n]) to work.

        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        ValueDictionary<TKey, TValue> dictionary = new(length, _comparer) { _count = length };
        AsSpan(start, length).CopyTo(dictionary._buffer);
        return dictionary;
    }

    /// <summary>
    /// Copies the elements from this <see cref="ValueDictionary{TKey, TValue}"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The number of elements in the source <see cref="ValueDictionary{TKey, TValue}"/> is greater
    /// than the number of elements that the destination can contain.
    /// </exception>
    public readonly void CopyTo(scoped Span<KeyValuePair<TKey, TValue>> destination)
        => AsSpan().CopyTo(destination);

    /// <inheritdoc cref="CopyTo(Span{KeyValuePair{TKey, TValue}})"/>
    /// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
    public readonly void CopyTo(KeyValuePair<TKey, TValue>[] array) => CopyTo(0, array, destinationStart: 0, _count);

    /// <inheritdoc cref="CopyTo(KeyValuePair{TKey, TValue}[])"/>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    public readonly void CopyTo(KeyValuePair<TKey, TValue>[] destination, int destinationStart) => CopyTo(0, destination, destinationStart, _count);

    /// <inheritdoc cref="CopyTo(Span{KeyValuePair{TKey, TValue}})"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueDictionary{TKey, TValue}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </param>
    /// <param name="length">
    /// The number of elements to copy.
    /// </param>
    public readonly void CopyTo(int start, scoped Span<KeyValuePair<TKey, TValue>> destination, int length)
        => AsSpan(start, length).CopyTo(destination);

    /// <inheritdoc cref="CopyTo(KeyValuePair{TKey, TValue}[], int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueDictionary{TKey, TValue}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    public readonly void CopyTo(int start, KeyValuePair<TKey, TValue>[] destination, int destinationStart, int length)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        CopyTo(start, destination.AsSpan(destinationStart), length);
    }

    /// <summary>
    /// Attempts to copy the elements from this <see cref="ValueDictionary{TKey, TValue}"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </param>
    /// <param name="written">
    /// The number of elements copied to the destination.
    /// </param>
    /// <returns>
    /// <c>true</c> if the operation was successful; otherwise, <c>false</c>.
    /// </returns>
    public readonly bool TryCopyTo(scoped Span<KeyValuePair<TKey, TValue>> destination, out int written)
    {
        if (AsSpan().TryCopyTo(destination))
        {
            written = _count;
            return true;
        }
        else
        {
            written = 0;
            return false;
        }
    }

    /// <inheritdoc cref="TryCopyTo(Span{KeyValuePair{TKey, TValue}}, out int)"/>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public readonly bool TryCopyTo(KeyValuePair<TKey, TValue>[] destination, out int written)
        => TryCopyTo(0, destination, destinationStart: 0, _count, out written);

    /// <inheritdoc cref="TryCopyTo(KeyValuePair{TKey, TValue}[], out int)"/>
    /// <param name="destination">The destination of the elements copied from <see cref="ValueDictionary{TKey, TValue}"/>.</param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="written">The number of elements copied to the destination.</param>
    public readonly bool TryCopyTo(KeyValuePair<TKey, TValue>[] destination, int destinationStart, out int written)
        => TryCopyTo(0, destination, destinationStart, _count, out written);

    /// <inheritdoc cref="TryCopyTo(Span{KeyValuePair{TKey, TValue}}, out int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueDictionary{TKey, TValue}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">The destination of the elements copied from <see cref="ValueDictionary{TKey, TValue}"/>.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <param name="written">The number of elements copied to the destination.</param>
    public readonly bool TryCopyTo(int start, scoped Span<KeyValuePair<TKey, TValue>> destination, int length, out int written)
    {
        if (AsSpan(start, length).TryCopyTo(destination))
        {
            written = length;
            return true;
        }
        else
        {
            written = 0;
            return false;
        }
    }

    /// <inheritdoc cref="TryCopyTo(KeyValuePair{TKey, TValue}[], int, out int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueDictionary{TKey, TValue}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <param name="written">The number of elements copied to the array.</param>
    public readonly bool TryCopyTo(int start, KeyValuePair<TKey, TValue>[] destination, int destinationStart, int length, out int written)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        return TryCopyTo(start, destination.AsSpan(destinationStart), length, out written);
    }

    /// <summary>
    /// Copies the elements of the <see cref="ValueDictionary{TKey, TValue}"/> to a new array.
    /// </summary>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </returns>
    public readonly KeyValuePair<TKey, TValue>[] ToArray() => AsSpan().ToArray();

    /// <summary>
    /// Copies the elements of the <see cref="ValueDictionary{TKey, TValue}"/> to a new array.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this instance, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </returns>
    public KeyValuePair<TKey, TValue>[] ToArray(bool dispose)
    {
        KeyValuePair<TKey, TValue>[] array = AsSpan().ToArray();

        if (dispose)
        {
            Dispose();
        }

        return array;
    }

    /// <summary>
    /// Copies the elements of the <see cref="ValueDictionary{TKey, TValue}"/> to a new <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Dictionary{TKey, TValue}"/> containing copies of the elements of the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </returns>
    public readonly Dictionary<TKey, TValue> ToDictionary()
    {
        Dictionary<TKey, TValue> dictionary = new(_count, _comparer);

        for (int i = 0; i < _count; i++)
        {
            KeyValuePair<TKey, TValue> pair = _buffer[i];
            dictionary[pair.Key] = pair.Value;
        }

        return dictionary;
    }

    /// <summary>
    /// Copies the elements of the <see cref="ValueDictionary{TKey, TValue}"/> to a new <see cref="Dictionary{TKey, TValue}"/>.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this instance, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>
    /// A <see cref="Dictionary{TKey, TValue}"/> containing copies of the elements of the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </returns>
    public Dictionary<TKey, TValue> ToDictionary(bool dispose)
    {
        Dictionary<TKey, TValue> dictionary = ToDictionary();

        if (dispose)
        {
            Dispose();
        }

        return dictionary;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Span{T}.Enumerator"/> for the <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<KeyValuePair<TKey, TValue>>.Enumerator GetEnumerator()
        => AsSpan().GetEnumerator();

    /// <inheritdoc/>
    [Obsolete("Equals(object) on ValueDictionary will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly bool Equals(object? obj)
        => ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct();

    /// <inheritdoc/>
    [Obsolete("GetHashCode() on ValueDictionary will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly int GetHashCode()
        => ThrowHelper.ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct();

    /// <summary>
    /// Removes all entries from this instance and releases the buffer used by it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        // Clear the elements so that the GC can reclaim the references.
        AsSpan().Clear();

        KeyValuePair<TKey, TValue>[]? oldRentedBuffer = _rentedBuffer;

        _buffer = _rentedBuffer = default;
        _count = 0;

        if (oldRentedBuffer is not null)
        {
            ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(oldRentedBuffer);
        }
    }

    /// <summary>
    /// Sets the capacity of this instance to the actual number of entries in
    /// the <see cref="ValueDictionary{TKey, TValue}"/>, if that number is less than
    /// a threshold value.
    /// </summary>
    public void TrimExcess()
    {
        if (CollectionHelper.ShouldTrim(_count, _buffer.Length))
        {
            Capacity = _count;
        }
    }

    /// <summary>
    /// Sets the capacity of this dictionary to hold up a specified number of entries
    /// without any further expansion of its backing storage.
    /// </summary>
    /// <param name="capacity">The new capacity.</param>
    public void TrimExcess(int capacity)
    {
        if (capacity < _buffer.Length)
        {
            Capacity = capacity;
        }
    }

    /// <summary>
    /// Grows the buffer used by this instance to ensure it can accommodate more items.
    /// </summary>
    /// <param name="minimumGrowLength">
    /// The minimum number of additional items the buffer should be able to accommodate
    /// after growth.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int minimumGrowLength)
    {
        int newCapacity = CollectionHelper.CalculateNewCapacity(_count, _buffer.Length, minimumGrowLength);
        ResizeBuffer(newCapacity);
    }

    /// <summary>
    /// Resizes the buffer used by this instance to a specific capacity.
    /// </summary>
    /// <param name="capacity">The desired capacity for the buffer.</param>
    private void ResizeBuffer(int capacity)
    {
        Debug.Assert(capacity >= 0);
        Debug.Assert(capacity >= _count);
        Debug.Assert(capacity <= CollectionHelper.MaxCapacity);

        // Let the `Rent` method throw an exception if the capacity is invalid.
        KeyValuePair<TKey, TValue>[] newRentedBuffer = ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Rent(capacity);
        KeyValuePair<TKey, TValue>[]? oldRentedBuffer = _rentedBuffer;
        AsSpan().CopyTo(newRentedBuffer);

        // Clear the elements so that the GC can reclaim the references.
        AsSpan().Clear();

        _buffer = _rentedBuffer = newRentedBuffer;
        if (oldRentedBuffer is not null)
        {
            ArrayPool<KeyValuePair<TKey, TValue>>.Shared.Return(oldRentedBuffer);
        }
    }

    /// <summary>
    /// Represents the collection of keys in a <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    [DebuggerTypeProxy(typeof(ValueDictionaryKeyCollectionDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public readonly ref struct KeyCollection
    {
        /// <summary>
        /// The <see cref="ValueDictionary{TKey, TValue}"/> whose keys are reflected
        /// by this instance.
        /// </summary>
        private readonly ValueDictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyCollection"/> struct
        /// that reflects the keys in the specified <see cref="ValueDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="dictionary">
        /// The <see cref="ValueDictionary{TKey, TValue}"/> whose keys are reflected in the new
        /// <see cref="KeyCollection"/>.
        /// </param>
        public KeyCollection(scoped in ValueDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// The number of elements contained in the <see cref="KeyCollection"/>.
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Returns the items contained in this instance for debugging purposes.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal readonly TKey[] DebuggerItems => Array.ConvertAll(_dictionary.DebuggerItems, static x => x.Key);

        /// <summary>
        /// Determines whether the <see cref="KeyCollection"/> contains a specific key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="KeyCollection"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="key"/> is found in the <see cref="KeyCollection"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TKey key) => _dictionary.ContainsKey(key);

        /// <summary>
        /// Copies the <see cref="KeyCollection"/> elements to an existing one-dimensional array,
        /// starting at the specified array index.
        /// </summary>
        /// <param name="array">
        /// The destination of the elements copied from the <see cref="KeyCollection"/>.
        /// </param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="KeyCollection"/> is greater
        /// than the number of elements that the destination can contain.
        /// </exception>
        public void CopyTo(TKey[] array, int index)
        {
            ThrowHelper.ThrowArgumentNullException_IfNull(array);

            CopyTo(array.AsSpan(index));
        }

        /// <summary>
        /// Copies the <see cref="KeyCollection"/> elements to the specified destination span.
        /// </summary>
        /// <param name="destination">
        /// The destination of the elements copied from the <see cref="KeyCollection"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="KeyCollection"/> is greater
        /// than the number of elements that the destination can contain.
        /// </exception>
        public void CopyTo(scoped Span<TKey> destination)
        {
            Span<KeyValuePair<TKey, TValue>> entries = _dictionary.AsSpan();
            if (destination.Length < entries.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            for (int i = 0; i < entries.Length; i++)
                destination[i] = entries[i].Key;
        }

        /// <inheritdoc/>
        [Obsolete("Equals(object) on KeyCollection will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly bool Equals(object? obj)
            => ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct();

        /// <inheritdoc/>
        [Obsolete("GetHashCode() on KeyCollection will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly int GetHashCode()
            => ThrowHelper.ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct();

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="KeyCollection"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Enumerator"/> for the <see cref="KeyCollection"/>.
        /// </returns>
        public Enumerator GetEnumerator() => new(_dictionary.AsSpan());

        /// <summary>
        /// Enumerates the elements of a <see cref="KeyCollection"/>.
        /// </summary>
        public ref struct Enumerator
        {
            /// <summary>
            /// The span being enumerated.
            /// </summary>
            private readonly Span<KeyValuePair<TKey, TValue>> _span;

            /// <summary>
            /// The next index to yield.
            /// </summary>
            private int _index;

            /// <summary>
            /// Initialize the enumerator.
            /// </summary>
            /// <param name="span">The span to enumerate.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Span<KeyValuePair<TKey, TValue>> span)
            {
                _span = span;
                _index = -1;
            }

            /// <summary>
            /// Advances the position of the current enumerator.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the position of the current enumerator was advanced;
            /// otherwise, <c>false</c>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int index = _index + 1;
                if (index < _span.Length)
                {
                    _index = index;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// The element at the current position of the enumerator.
            /// </summary>
            public readonly TKey Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _span[_index].Key;
            }
        }
    }

    /// <summary>
    /// Represents the collection of values in a <see cref="ValueDictionary{TKey, TValue}"/>.
    /// </summary>
    [DebuggerTypeProxy(typeof(ValueDictionaryValueCollectionDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]
    public readonly ref struct ValueCollection
    {
        /// <summary>
        /// The <see cref="ValueDictionary{TKey, TValue}"/> whose values are reflected
        /// by this instance.
        /// </summary>
        private readonly ValueDictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueCollection"/> struct
        /// that reflects the values in the specified <see cref="ValueDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="dictionary">
        /// The <see cref="ValueDictionary{TKey, TValue}"/> whose values are reflected in the new
        /// <see cref="ValueCollection"/>.
        /// </param>
        public ValueCollection(scoped in ValueDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <summary>
        /// The number of elements contained in the <see cref="ValueCollection"/>.
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Returns the items contained in this instance for debugging purposes.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal readonly TValue[] DebuggerItems => Array.ConvertAll(_dictionary.DebuggerItems, static x => x.Value);

        /// <summary>
        /// Determines whether the <see cref="ValueCollection"/> contains a specific value.
        /// </summary>
        /// <param name="value">The value to locate in the <see cref="ValueCollection"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="value"/> is found in the <see cref="ValueCollection"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TValue value) => _dictionary.ContainsValue(value);

        /// <summary>
        /// Copies the <see cref="ValueCollection"/> elements to an existing one-dimensional array,
        /// starting at the specified array index.
        /// </summary>
        /// <param name="array">
        /// The destination of the elements copied from the <see cref="ValueCollection"/>.
        /// </param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="ValueCollection"/> is greater
        /// than the number of elements that the destination can contain.
        /// </exception>
        public void CopyTo(TValue[] array, int index)
        {
            ThrowHelper.ThrowArgumentNullException_IfNull(array);

            CopyTo(array.AsSpan(index));
        }

        /// <summary>
        /// Copies the <see cref="ValueCollection"/> elements to the specified destination span.
        /// </summary>
        /// <param name="destination">
        /// The destination of the elements copied from the <see cref="ValueCollection"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="ValueCollection"/> is greater
        /// than the number of elements that the destination can contain.
        /// </exception>
        public void CopyTo(scoped Span<TValue> destination)
        {
            Span<KeyValuePair<TKey, TValue>> entries = _dictionary.AsSpan();
            if (destination.Length < entries.Length)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            for (int i = 0; i < entries.Length; i++)
                destination[i] = entries[i].Value;
        }

        /// <inheritdoc/>
        [Obsolete("Equals(object) on ValueCollection will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly bool Equals(object? obj)
            => ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct();

        /// <inheritdoc/>
        [Obsolete("GetHashCode() on ValueCollection will always throw an exception.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly int GetHashCode()
            => ThrowHelper.ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct();

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ValueCollection"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Enumerator"/> for the <see cref="ValueCollection"/>.
        /// </returns>
        public Enumerator GetEnumerator() => new(_dictionary.AsSpan());

        /// <summary>
        /// Enumerates the elements of a <see cref="ValueCollection"/>.
        /// </summary>
        public ref struct Enumerator
        {
            /// <summary>
            /// The span being enumerated.
            /// </summary>
            private readonly Span<KeyValuePair<TKey, TValue>> _span;

            /// <summary>
            /// The next index to yield.
            /// </summary>
            private int _index;

            /// <summary>
            /// Initialize the enumerator.
            /// </summary>
            /// <param name="span">The span to enumerate.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Span<KeyValuePair<TKey, TValue>> span)
            {
                _span = span;
                _index = -1;
            }

            /// <summary>
            /// Advances the position of the current enumerator.
            /// </summary>
            /// <returns>
            /// <c>true</c> if the position of the current enumerator was advanced;
            /// otherwise, <c>false</c>.
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int index = _index + 1;
                if (index < _span.Length)
                {
                    _index = index;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// The element at the current position of the enumerator.
            /// </summary>
            public readonly TValue Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _span[_index].Value;
            }
        }
    }
}

/// <summary>
/// Controls behavior of the insertion into a <see cref="ValueDictionary{TKey, TValue}"/>.
/// </summary>
internal enum ValueDictionaryAddType : byte
{
    /// <summary>
    /// If an existing entry with the same key is encountered, an exception should be thrown.
    /// </summary>
    ThrowOnExisting = 0,

    /// <summary>
    /// If an existing entry with the same key is encountered, it should be overwritten.
    /// </summary>
    OverwriteExisting = 1,

    /// <summary>
    /// If an existing entry with the same key is encountered, it should be skipped.
    /// </summary>
    SkipExisting = 2,
}
