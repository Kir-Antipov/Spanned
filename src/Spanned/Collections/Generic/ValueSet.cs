namespace Spanned.Collections.Generic;

/// <summary>
/// Represents a set of values.
/// </summary>
/// <typeparam name="T">The type of elements in the hash set.</typeparam>
[DebuggerTypeProxy(typeof(ValueCollectionDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
public ref struct ValueSet<T>
{
    /// <summary>
    /// The buffer that is rented from an <see cref="ArrayPool{T}"/>, if one is used.
    /// </summary>
    private T[]? _rentedBuffer;

    /// <summary>
    /// The item buffer used by this instance.
    /// </summary>
    private Span<T> _buffer;

    /// <summary>
    /// The number of elements contained in the buffer.
    /// </summary>
    private int _count;

    /// <summary>
    /// The <see cref="IEqualityComparer{T}"/> object that is used to
    /// determine equality for the values in the set.
    /// </summary>
    private readonly IEqualityComparer<T>? _comparer;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSet{T}"/> struct that
    /// is empty and uses the specified equality comparer for the set type.
    /// </summary>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the set type.
    /// </param>
    public ValueSet(IEqualityComparer<T>? comparer)
    {
        _buffer = default;
        _count = 0;

        if (typeof(T).IsValueType)
        {
            // Do not initialize `EqualityComparer<T>.Default` for value types,
            // if possible.
            if (comparer is not null && comparer == EqualityComparer<T>.Default)
                comparer = null;

            _comparer = comparer;
        }
        else
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }
    }

    /// <inheritdoc cref="ValueSet(Span{T}, IEqualityComparer{T})"/>
    public ValueSet(Span<T> buffer)
    {
        _buffer = buffer;
        _count = 0;
        _comparer = typeof(T).IsValueType ? null : EqualityComparer<T>.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSet{T}"/> struct using the provided
    /// initial buffer.
    /// </summary>
    /// <param name="buffer">The initial item buffer for this instance.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the set type.
    /// </param>
    public ValueSet(Span<T> buffer, IEqualityComparer<T>? comparer)
        : this(comparer)
    {
        _buffer = buffer;
        _count = 0;
    }

    /// <inheritdoc cref="ValueSet(int, IEqualityComparer{T})"/>
    public ValueSet(int capacity)
    {
        _rentedBuffer = ArrayPool<T>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _count = 0;
        _comparer = typeof(T).IsValueType ? null : EqualityComparer<T>.Default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSet{T}"/> struct that
    /// is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new set can initially store.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the set type.
    /// </param>
    public ValueSet(int capacity, IEqualityComparer<T>? comparer)
        : this(comparer)
    {
        _rentedBuffer = ArrayPool<T>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _count = 0;
    }

    /// <inheritdoc cref="ValueSet(ReadOnlySpan{T}, IEqualityComparer{T})"/>
    public ValueSet(scoped ReadOnlySpan<T> collection)
        : this(collection, collection.Length, comparer: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSet{T}"/> struct that
    /// contains elements copied from the specified collection and has sufficient capacity
    /// to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the set type.
    /// </param>
    public ValueSet(scoped ReadOnlySpan<T> collection, IEqualityComparer<T>? comparer)
        : this(collection, collection.Length, comparer)
    {
    }

    /// <inheritdoc cref="ValueSet(ReadOnlySpan{T}, int, IEqualityComparer{T})"/>
    public ValueSet(scoped ReadOnlySpan<T> collection, int capacity)
        : this(collection, capacity, comparer: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSet{T}"/> struct that
    /// contains elements copied from the specified collection and has the specified capacity.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    /// <param name="capacity">The number of elements that the new set can initially store.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the set type.
    /// </param>
    public ValueSet(scoped ReadOnlySpan<T> collection, int capacity, IEqualityComparer<T>? comparer)
        : this(comparer)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(capacity, collection.Length);

        _rentedBuffer = capacity == 0 ? null : ArrayPool<T>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _count = 0;

        UnionWith(collection);
    }

    /// <inheritdoc cref="ValueSet(IEnumerable{T}, IEqualityComparer{T})"/>
    public ValueSet(IEnumerable<T> collection)
        : this(collection, comparer: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueSet{T}"/> struct that
    /// contains elements copied from the specified collection and has sufficient capacity
    /// to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new set.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the set, or
    /// <c>null</c> to use the default <see cref="EqualityComparer{T}"/> implementation for the set type.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public ValueSet(IEnumerable<T> collection, IEqualityComparer<T>? comparer)
        : this(comparer)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        _buffer = _rentedBuffer = default;
        _count = 0;

        UnionWith(collection);
    }

    /// <summary>
    /// Implicitly converts a <see cref="ValueSet{T}"/> to a span.
    /// </summary>
    /// <param name="set">The set to be converted.</param>
    /// <returns>A span covering the content of the <see cref="ValueSet{T}"/>.</returns>
    public static implicit operator ReadOnlySpan<T>(ValueSet<T> set)
        => set._buffer.Slice(0, set._count);

    /// <summary>
    /// Returns a span that represents the content of the <see cref="ValueSet{T}"/>.
    /// </summary>
    /// <remarks>
    /// In the case of direct modification, the uniqueness of the elements is no longer guaranteed.
    /// </remarks>
    /// <returns>A span covering the content of the <see cref="ValueSet{T}"/>.</returns>
    public readonly Span<T> AsSpan() => _buffer.Slice(0, _count);

    /// <summary>
    /// Returns a span that represents a segment of the set starting from the specified index.
    /// </summary>
    /// <remarks>
    /// In the case of direct modification, the uniqueness of the elements is no longer guaranteed.
    /// </remarks>
    /// <param name="start">The start index of the segment.</param>
    /// <returns>A span covering the segment of the set.</returns>
    public readonly Span<T> AsSpan(int start) => AsSpan(start, _count - start);

    /// <summary>
    /// Returns a span that represents a segment of the set with the specified start index and length.
    /// </summary>
    /// <remarks>
    /// In the case of direct modification, the uniqueness of the elements is no longer guaranteed.
    /// </remarks>
    /// <param name="start">The start index of the segment.</param>
    /// <param name="length">The length of the segment.</param>
    /// <returns>A span covering the segment of the set.</returns>
    public readonly Span<T> AsSpan(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        return _buffer.Slice(start, length);
    }

    /// <summary>
    /// Returns a span that represents the remaining unused segment of the set.
    /// </summary>
    /// <remarks>
    /// In the case of direct modification, the uniqueness of the elements is no longer guaranteed.
    /// </remarks>
    /// <returns>A span covering the unused segment of the set.</returns>
    public readonly Span<T> AsRemainingSpan() => _buffer.Slice(_count);

    /// <summary>
    /// Returns a span that represents the entire capacity of the set,
    /// including used and unused segments.
    /// </summary>
    /// <remarks>
    /// In the case of direct modification, the uniqueness of the elements is no longer guaranteed.
    /// </remarks>
    /// <returns>A span covering the entire buffer of the set.</returns>
    public readonly Span<T> AsCapacitySpan() => _buffer;

    /// <summary>
    /// The number of elements contained in the <see cref="ValueSet{T}"/>.
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
    /// The total number of elements the internal data structure can hold
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
    /// determine equality for the values in the set.
    /// </summary>
    public readonly IEqualityComparer<T> Comparer => _comparer ?? EqualityComparer<T>.Default;

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <remarks>
    /// In the case of direct modification, the uniqueness of the element is no longer guaranteed.
    /// </remarks>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    public readonly ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= (uint)_count)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            // Skip additional bound checks.
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), (nint)(uint)index);
        }
    }

    /// <summary>
    /// Returns the items contained in this instance for debugging purposes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal readonly T[] DebuggerItems
    {
        get
        {
            // `.AsSpan().ToArray()` can break the debugger.
            // Therefore, prefer working with the underlying array directly when possible.
            if (_rentedBuffer is null)
                return AsSpan().ToArray();

            T[] items = new T[_count];
            Array.Copy(_rentedBuffer, items, items.Length);
            return items;
        }
    }

    /// <summary>
    /// Ensures that the capacity of this instance is at least the specified value.
    /// </summary>
    /// <returns>The new capacity of this set.</returns>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    public int EnsureCapacity(int capacity)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(capacity);

        if ((uint)capacity > (uint)_buffer.Length)
            Grow(capacity - _count);

        return _buffer.Length;
    }

    /// <summary>
    /// Provides a reference to the first item of this instance's buffer, which can be used for pinning.
    /// </summary>
    /// <returns>A reference to the first item of the buffer.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ref T GetPinnableReference() => ref MemoryMarshal.GetReference(_buffer);

    /// <summary>
    /// Searches the set for a given value and returns the equal value it finds, if any.
    /// </summary>
    /// <param name="equalValue">The value to search for.</param>
    /// <param name="actualValue">
    /// The value from the set that the search found, or the default value of
    /// <typeparamref name="T"/> when the search yielded no match.
    /// </param>
    /// <returns>A value indicating whether the search was successful.</returns>
    public readonly bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue)
    {
        int index = AsSpan().IndexOf(equalValue, _comparer);
        if (index < 0)
        {
            actualValue = default;
            return false;
        }

        actualValue = _buffer[index];
        return true;
    }

    /// <summary>
    /// Adds the specified element to the set.
    /// </summary>
    /// <param name="item">The element to add to the set.</param>
    /// <returns>
    /// <c>true</c> if the element is added to the set;
    /// <c>false</c> if the element is already present.
    /// </returns>
    public bool Add(T item)
    {
        int count = _count;
        Span<T> buffer = _buffer;

        if (buffer.Slice(0, count).Contains(item, _comparer))
            return false;

        if ((uint)count < (uint)buffer.Length)
        {
            buffer[count] = item;
            _count = count + 1;
        }
        else
        {
            GrowAndAdd(item);
        }

        return true;
    }

    /// <summary>
    /// Adds the specified element to the set and provides its index.
    /// </summary>
    /// <param name="item">The element to add to the set.</param>
    /// <param name="index">
    /// When this method returns, contains the index of the element in the set, whether it was added or already existed.
    /// </param>
    /// <returns>
    /// <c>true</c> if the element is added to the set;
    /// <c>false</c> if the element is already present.
    /// </returns>
    internal bool Add(T item, out int index)
    {
        int count = _count;
        Span<T> buffer = _buffer;

        index = buffer.Slice(0, count).IndexOf(item, _comparer);
        if (index >= 0)
            return false;

        if ((uint)count < (uint)buffer.Length)
        {
            buffer[count] = item;
            _count = count + 1;
        }
        else
        {
            GrowAndAdd(item);
        }

        index = count;
        return true;
    }

    /// <summary>
    /// Grows the internal buffer by 1 and adds an object to the end of the <see cref="ValueSet{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to be added to the end of the <see cref="ValueSet{T}"/>.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAdd(T item)
    {
        Grow(1);

        _buffer[_count] = item;
        _count++;
    }

    /// <summary>
    /// Removes the specified element from the set.
    /// </summary>
    /// <param name="item">The element to remove.</param>
    /// <returns>
    /// <c>true</c> if the element is successfully found and removed; otherwise, <c>false</c>.
    /// </returns>
    public bool Remove(T item)
    {
        int index = AsSpan().IndexOf(item, _comparer);
        if (index < 0)
            return false;

        // Clear the element so that the GC can reclaim its reference.
        _buffer[index] = default!;

        _buffer.Slice(index + 1).CopyTo(_buffer.Slice(index));
        _count--;

        return true;
    }

    /// <summary>
    /// Removes all elements that match the conditions defined by the specified predicate
    /// from the set.
    /// </summary>
    /// <param name="predicate">
    /// The predicate that defines the conditions of the elements to remove.
    /// </param>
    /// <returns>
    /// The number of elements that were removed from the set.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
    public int RemoveWhere(Predicate<T> predicate)
    {
        int removed = AsSpan().RemoveWhere(predicate);
        _count -= removed;

        return removed;
    }

    /// <summary>
    /// Removes elements from the set at the specified indices.
    /// </summary>
    /// <param name="indices">
    /// A <see cref="ValueBitArray"/> representing the indices of elements to remove.
    /// <para/>
    /// If an index is set to <c>true</c>, the corresponding element will be removed.
    /// </param>
    /// <returns>
    /// The number of elements that were removed from the set.
    /// </returns>
    internal int RemoveWhere(scoped ValueBitArray indices)
    {
        int removed = AsSpan().RemoveWhere(indices);
        _count -= removed;

        return removed;
    }

    /// <summary>
    /// Removes the element at the specified index of the <see cref="ValueSet{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
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
    /// Removes a range of elements from the <see cref="ValueSet{T}"/>.
    /// </summary>
    /// <param name="start">The zero-based starting index of the range of elements to remove.</param>
    /// <param name="length">The number of elements to remove.</param>
    public void RemoveRange(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        // Clear the elements so that the GC can reclaim the references.
        AsSpan(start, length).Clear();

        _buffer.Slice(start + length).CopyTo(_buffer.Slice(start));
        _count -= length;
    }

    /// <summary>
    /// Determines whether the set contains the specified element.
    /// </summary>
    /// <param name="item">The element to locate in the set.</param>
    /// <returns>
    /// <c>true</c> if the set contains the specified element; otherwise, <c>false</c>.
    /// </returns>
    public readonly bool Contains(T item) => AsSpan().Contains(item, _comparer);

    /// <summary>
    /// Removes all elements from the set.
    /// </summary>
    public void Clear()
    {
        // Clear the elements so that the GC can reclaim the references.
        AsSpan().Clear();

        _count = 0;
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the first
    /// occurrence within the <see cref="ValueSet{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueSet{T}"/>.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <returns>
    /// The zero-based index of the first occurrence of item within the <see cref="ValueSet{T}"/>,
    /// if found; otherwise, <c>-1</c>.
    /// </returns>
    public readonly int IndexOf(T item) => AsSpan().IndexOf(item, _comparer);

    /// <inheritdoc cref="IndexOf(T)"/>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueSet{T}"/>.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <param name="index">
    /// The zero-based starting index of the search.
    /// 0 (zero) is valid in an empty set.
    /// </param>
    public readonly int IndexOf(T item, int index) => IndexOf(item, index, _count - index);

    /// <inheritdoc cref="IndexOf(T)"/>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueSet{T}"/>.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <param name="start">
    /// The zero-based starting index of the search.
    /// 0 (zero) is valid in an empty set.
    /// </param>
    /// <param name="length">The number of elements in the section to search.</param>
    public readonly int IndexOf(T item, int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        int itemIndex = AsSpan(start, length).IndexOf(item, _comparer);
        return itemIndex + (itemIndex < 0 ? 0 : start);
    }

    /// <summary>
    /// Modifies the current set to contain all elements that are present in itself,
    /// the specified collection, or both.
    /// </summary>
    /// <param name="other">
    /// The collection to compare to the current set.
    /// </param>
    public void UnionWith(scoped ReadOnlySpan<T> other)
    {
        for (int i = 0; i < other.Length; i++)
            Add(other[i]);
    }

    /// <inheritdoc cref="UnionWith(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public void UnionWith(IEnumerable<T> other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        if (other.TryGetSpan(out ReadOnlySpan<T> span))
        {
            for (int i = 0; i < span.Length; i++)
                Add(span[i]);
        }
        else
        {
            foreach (T item in other)
                Add(item);
        }
    }

    /// <inheritdoc cref="UnionWith(ReadOnlySpan{T})"/>
    public void UnionWith(scoped in ValueSet<T> other)
        => UnionWith(other.AsSpan());

    /// <inheritdoc cref="UnionWith(IEnumerable{T})"/>
    public void UnionWith(T[] other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        UnionWith(other.AsSpan());
    }

    /// <inheritdoc cref="UnionWith(ReadOnlySpan{T})"/>
    internal void UnionWith(scoped ValueEnumerable<T> other)
    {
        if (other.TryGetSpan(out ReadOnlySpan<T> span, out IEnumerable<T>? enumerable))
        {
            UnionWith(span);
        }
        else
        {
            UnionWith(enumerable);
        }
    }

    /// <summary>
    /// Modifies the current set to contain only elements that are present in itself and
    /// in the specified collection.
    /// </summary>
    /// <param name="other">
    /// The collection to compare to the current set.
    /// </param>
    public void IntersectWith(scoped ReadOnlySpan<T> other)
        => IntersectWith((ValueEnumerable<T>)other);

    /// <inheritdoc cref="IntersectWith(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public void IntersectWith(IEnumerable<T> other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        if (other is HashSet<T> { Count: > 0 } otherHashSet && Equals(Comparer, otherHashSet.Comparer))
        {
            IntersectWithHashSetWithSameComparer(otherHashSet);
            return;
        }

        IntersectWith(other.AsValueEnumerable());
    }

    /// <inheritdoc cref="IntersectWith(ReadOnlySpan{T})"/>
    public void IntersectWith(scoped in ValueSet<T> other)
    {
        if (_buffer == other._buffer && _count == other._count && _comparer == other._comparer)
            return;

        IntersectWith(other.AsSpan());
    }

    /// <inheritdoc cref="IntersectWith(IEnumerable{T})"/>
    public void IntersectWith(T[] other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        IntersectWith(other.AsSpan());
    }

    /// <inheritdoc cref="IntersectWith(ReadOnlySpan{T})"/>
    private void IntersectWith(scoped ValueEnumerable<T> other)
    {
        if (_count == 0)
            return;

        if (other.TryGetNonEnumeratedCount(out int otherCount) && otherCount == 0)
        {
            Clear();
            return;
        }

        int originalCount = _count;
        Span<T> items = _buffer.Slice(0, originalCount);
        IEqualityComparer<T>? comparer = _comparer;

        int intCount = ValueBitArray.GetInt32Count(originalCount);
        SpanOwner<int> ints = SpanOwner<int>.MayStackalloc(intCount) ? stackalloc int[intCount] : SpanOwner<int>.Rent(intCount);
        ints.Span.Fill(-1); // Set all bits to 1.
        ValueBitArray itemsToRemove = new(ints.Span);

        if (other.TryGetSpan(out ReadOnlySpan<T> span, out IEnumerable<T>? enumerable))
        {
            for (int i = 0; i < span.Length; i++)
            {
                int index = items.IndexOf(span[i], comparer);
                if (index >= 0)
                    itemsToRemove[index] = false;
            }
        }
        else
        {
            foreach (T item in enumerable)
            {
                int index = items.IndexOf(item, comparer);
                if (index >= 0)
                    itemsToRemove[index] = false;
            }
        }

        RemoveWhere(itemsToRemove);

        ints.Dispose();
    }

    /// <inheritdoc cref="IntersectWith(ReadOnlySpan{T})"/>
    private void IntersectWithHashSetWithSameComparer(HashSet<T> other)
    {
        int originalCount = _count;
        Span<T> items = _buffer.Slice(0, originalCount);

        int intCount = ValueBitArray.GetInt32Count(originalCount);
        SpanOwner<int> itemsToRemoveSource = SpanOwner<int>.MayStackalloc(intCount) ? stackalloc int[intCount] : SpanOwner<int>.Rent(intCount);
        itemsToRemoveSource.Span.Fill(-1); // Set all bits to 1.
        ValueBitArray itemsToRemove = itemsToRemoveSource.Span;

        for (int i = 0; i < items.Length; i++)
        {
            if (other.Contains(items[i]))
                itemsToRemove[i] = false;
        }

        RemoveWhere(itemsToRemove);

        itemsToRemoveSource.Dispose();
    }

    /// <summary>
    /// Removes all elements in the specified collection from the current set.
    /// </summary>
    /// <param name="other">
    /// The collection of items to remove from the set.
    /// </param>
    public void ExceptWith(scoped ReadOnlySpan<T> other)
    {
        if (_count == 0)
            return;

        for (int i = 0; i < other.Length; i++)
            Remove(other[i]);
    }

    /// <inheritdoc cref="ExceptWith(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public void ExceptWith(IEnumerable<T> other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        if (_count == 0)
            return;

        foreach (T element in other)
            Remove(element);
    }

    /// <inheritdoc cref="ExceptWith(ReadOnlySpan{T})"/>
    public void ExceptWith(scoped in ValueSet<T> other)
    {
        if (_buffer == other._buffer && _count == other._count && _comparer == other._comparer)
        {
            Clear();
            return;
        }

        ExceptWith(other.AsSpan());
    }

    /// <inheritdoc cref="ExceptWith(IEnumerable{T})"/>
    public void ExceptWith(T[] other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        ExceptWith(other.AsSpan());
    }

    /// <summary>
    /// Modifies the current set to contain only elements that are present either
    /// in itself or in the specified collection, but not both.
    /// </summary>
    /// <param name="other">
    /// The collection to compare to the current set.
    /// </param>
    public void SymmetricExceptWith(scoped ReadOnlySpan<T> other)
        => SymmetricExceptWith((ValueEnumerable<T>)other);

    /// <inheritdoc cref="SymmetricExceptWith(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        if (other is HashSet<T> { Count: > 0 } otherSet && Equals(Comparer, otherSet.Comparer))
        {
            SymmetricExceptWithHashSetWithSameComparer(otherSet);
            return;
        }

        SymmetricExceptWith(other.AsValueEnumerable());
    }

    /// <inheritdoc cref="SymmetricExceptWith(ReadOnlySpan{T})"/>
    public void SymmetricExceptWith(scoped in ValueSet<T> other)
    {
        if (_buffer == other._buffer && _count == other._count && _comparer == other._comparer)
        {
            Clear();
            return;
        }

        if (!Equals(_comparer, other._comparer))
        {
            SymmetricExceptWith(new ValueEnumerable<T>(other));
            return;
        }

        foreach (T item in other)
        {
            if (!Remove(item))
                Add(item);
        }
    }

    /// <inheritdoc cref="SymmetricExceptWith(IEnumerable{T})"/>
    public void SymmetricExceptWith(T[] other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        SymmetricExceptWith(other.AsSpan());
    }

    /// <inheritdoc cref="SymmetricExceptWith(ReadOnlySpan{T})"/>
    private void SymmetricExceptWithHashSetWithSameComparer(HashSet<T> other)
    {
        foreach (T item in other)
        {
            if (!Remove(item))
                Add(item);
        }
    }

    /// <inheritdoc cref="SymmetricExceptWith(ReadOnlySpan{T})"/>
    private void SymmetricExceptWith(scoped ValueEnumerable<T> other)
    {
        if (_count == 0)
        {
            UnionWith(other);
            return;
        }

        int originalCount = _count;
        int intCount = ValueBitArray.GetInt32Count(originalCount);
        scoped SpanOwner<int> itemsToRemoveSource;
        scoped SpanOwner<int> addedItemsSource;
        if (SpanOwner<int>.MayStackalloc(intCount * 2))
        {
            itemsToRemoveSource = stackalloc int[intCount];
            addedItemsSource = stackalloc int[intCount];
        }
        else
        {
            itemsToRemoveSource = SpanOwner<int>.Rent(intCount);
            addedItemsSource = SpanOwner<int>.Rent(intCount);
        }

        ValueBitArray itemsToRemove = itemsToRemoveSource.Span;
        ValueBitArray addedItems = addedItemsSource.Span;

        foreach (T item in other)
        {
            if (Add(item, out int index))
            {
                if (index >= originalCount)
                    continue;

                addedItems[index] = true;
            }
            else if (index < originalCount && !addedItems[index])
            {
                itemsToRemove[index] = true;
            }
        }

        RemoveWhere(itemsToRemove);

        itemsToRemoveSource.Dispose();
        addedItemsSource.Dispose();
    }

    /// <summary>
    /// Determines whether the set is a subset of the specified collection.
    /// </summary>
    /// <param name="other">
    /// The collection to compare to the current set.
    /// </param>
    /// <returns>
    /// <c>true</c> if the set is a subset of <paramref name="other"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public readonly bool IsSubsetOf(scoped ReadOnlySpan<T> other)
    {
        if (_count == 0)
            return true;

        (int found, _) = CompareElements(other);
        return found == _count;
    }

    /// <inheritdoc cref="IsSubsetOf(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public readonly bool IsSubsetOf(IEnumerable<T> other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        if (_count == 0)
            return true;

        if (other is HashSet<T> otherSet && Equals(Comparer, otherSet.Comparer))
        {
            if (_count > otherSet.Count)
                return false;

            return IsSubsetOfHashSetWithSameComparer(otherSet);
        }

        (int found, _) = CompareElements(other.AsValueEnumerable());
        return found == _count;
    }

    /// <inheritdoc cref="IsSubsetOf(ReadOnlySpan{T})"/>
    public readonly bool IsSubsetOf(scoped in ValueSet<T> other)
    {
        if (!Equals(_comparer, other._comparer))
            return IsSubsetOf(other.AsSpan());

        foreach (T item in this)
        {
            if (!other.Contains(item))
                return false;
        }

        return true;
    }

    /// <inheritdoc cref="IsSubsetOf(IEnumerable{T})"/>
    public readonly bool IsSubsetOf(T[] other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        return IsSubsetOf(other.AsSpan());
    }

    /// <inheritdoc cref="IsSubsetOf(ReadOnlySpan{T})"/>
    private readonly bool IsSubsetOfHashSetWithSameComparer(HashSet<T> other)
    {
        foreach (T item in this)
        {
            if (!other.Contains(item))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the set is a proper subset of the specified collection.
    /// </summary>
    /// <param name="other">
    /// The collection to compare to the current set.
    /// </param>
    /// <returns>
    /// <c>true</c> if the set is a proper subset of <paramref name="other"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public readonly bool IsProperSubsetOf(scoped ReadOnlySpan<T> other)
    {
        if (other.IsEmpty)
            return false;

        if (_count == 0)
            return !other.IsEmpty;

        (int found, int notFound) = CompareElements(other);
        return found == _count && notFound != 0;
    }

    /// <inheritdoc cref="IsProperSubsetOf(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public readonly bool IsProperSubsetOf(IEnumerable<T> other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        if (other is ICollection<T> otherCollection)
        {
            if (otherCollection.Count == 0)
                return false;

            if (_count == 0)
                return otherCollection.Count > 0;

            if (other is HashSet<T> otherSet && Equals(Comparer, otherSet.Comparer))
            {
                if (_count >= otherSet.Count)
                    return false;

                return IsSubsetOfHashSetWithSameComparer(otherSet);
            }
        }

        (int found, int notFound) = CompareElements(other.AsValueEnumerable());
        return found == _count && notFound != 0;
    }

    /// <inheritdoc cref="IsProperSubsetOf(ReadOnlySpan{T})"/>
    public readonly bool IsProperSubsetOf(scoped in ValueSet<T> other)
    {
        if (!Equals(_comparer, other._comparer))
            return IsProperSubsetOf(other.AsSpan());

        return _count < other._count && IsSubsetOf(other);
    }

    /// <inheritdoc cref="IsProperSubsetOf(IEnumerable{T})"/>
    public readonly bool IsProperSubsetOf(T[] other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        return IsProperSubsetOf(other.AsSpan());
    }

    /// <summary>
    /// Determines whether the set is a superset of the specified collection.
    /// </summary>
    /// <param name="other">
    /// The collection to compare to the current set.
    /// </param>
    /// <returns>
    /// <c>true</c> if the set is a superset of <paramref name="other"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public readonly bool IsSupersetOf(scoped ReadOnlySpan<T> other)
    {
        Span<T> items = AsSpan();
        IEqualityComparer<T>? comparer = _comparer;

        for (int i = 0; i < other.Length; i++)
        {
            if (!items.Contains(other[i], comparer))
                return false;
        }

        return true;
    }

    /// <inheritdoc cref="IsSupersetOf(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public readonly bool IsSupersetOf(IEnumerable<T> other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        if (other.TryGetSpan(out ReadOnlySpan<T> span))
            return IsSupersetOf(span);

        if (other is ICollection<T> otherCollection)
        {
            if (otherCollection.Count == 0)
                return true;

            if (other is HashSet<T> otherSet && Equals(Comparer, otherSet.Comparer) && _count < otherSet.Count)
                return false;
        }

        foreach (T element in other)
        {
            if (!Contains(element))
                return false;
        }

        return true;
    }

    /// <inheritdoc cref="IsSupersetOf(ReadOnlySpan{T})"/>
    public readonly bool IsSupersetOf(scoped in ValueSet<T> other)
    {
        if (Equals(_comparer, other._comparer) && _count < other._count)
            return false;

        return IsSupersetOf(other.AsSpan());
    }

    /// <inheritdoc cref="IsSupersetOf(IEnumerable{T})"/>
    public readonly bool IsSupersetOf(T[] other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        return IsSupersetOf(other.AsSpan());
    }

    /// <summary>
    /// Determines whether the set is a proper superset of the specified collection.
    /// </summary>
    /// <param name="other">
    /// The collection to compare to the current set.
    /// </param>
    /// <returns>
    /// <c>true</c> if the set is a proper superset of <paramref name="other"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public readonly bool IsProperSupersetOf(scoped ReadOnlySpan<T> other)
    {
        if (_count == 0)
            return false;

        (int found, int notFound) = CompareElements(other, returnIfNotFound: true);
        return found < _count && notFound == 0;
    }

    /// <inheritdoc cref="IsProperSupersetOf(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public readonly bool IsProperSupersetOf(IEnumerable<T> other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        if (_count == 0)
            return false;

        if (other is ICollection<T> otherCollection)
        {
            if (otherCollection.Count == 0)
                return true;

            if (other is HashSet<T> otherSet && Equals(Comparer, otherSet.Comparer))
            {
                if (otherSet.Count >= Count)
                    return false;
            }
        }

        (int found, int notFound) = CompareElements(other.AsValueEnumerable(), returnIfNotFound: true);
        return found < _count && notFound == 0;
    }

    /// <inheritdoc cref="IsProperSupersetOf(ReadOnlySpan{T})"/>
    public readonly bool IsProperSupersetOf(scoped in ValueSet<T> other)
    {
        if (Equals(_comparer, other._comparer) && other._count >= _count)
            return false;

        return IsProperSupersetOf(other.AsSpan());
    }

    /// <inheritdoc cref="IsProperSupersetOf(IEnumerable{T})"/>
    public readonly bool IsProperSupersetOf(T[] other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        return IsProperSupersetOf(other.AsSpan());
    }

    /// <summary>
    /// Determines whether the current set and a specified collection share common elements.
    /// </summary>
    /// <param name="other">
    /// The collection to compare to the current set.
    /// </param>
    /// <returns>
    /// <c>true</c> if the set and <paramref name="other"/> share at least one common element;
    /// otherwise, <c>false</c>.
    /// </returns>
    public readonly bool Overlaps(scoped ReadOnlySpan<T> other)
    {
        if (_count == 0)
            return false;

        Span<T> items = AsSpan();
        IEqualityComparer<T>? comparer = _comparer;
        for (int i = 0; i < other.Length; i++)
        {
            if (items.Contains(other[i], comparer))
                return true;
        }

        return false;
    }

    /// <inheritdoc cref="Overlaps(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public readonly bool Overlaps(IEnumerable<T> other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        if (_count == 0)
            return false;

        if (other is HashSet<T> otherSet && Equals(Comparer, otherSet.Comparer))
            return OverlapsWithHashSetWithSameComparer(otherSet);

        Span<T> items = AsSpan();
        IEqualityComparer<T>? comparer = _comparer;
        foreach (T item in other)
        {
            if (items.Contains(item, comparer))
                return true;
        }

        return false;
    }

    /// <inheritdoc cref="Overlaps(ReadOnlySpan{T})"/>
    public readonly bool Overlaps(scoped in ValueSet<T> other)
        => Overlaps(other.AsSpan());

    /// <inheritdoc cref="Overlaps(IEnumerable{T})"/>
    public readonly bool Overlaps(T[] other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        return Overlaps(other.AsSpan());
    }

    /// <inheritdoc cref="Overlaps(ReadOnlySpan{T})"/>
    private readonly bool OverlapsWithHashSetWithSameComparer(HashSet<T> other)
    {
        foreach (T item in this)
        {
            if (other.Contains(item))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the set and the specified collection contain the same elements.
    /// </summary>
    /// <param name="other">
    /// The collection to compare to the current set.
    /// </param>
    /// <returns>
    /// <c>true</c> if the set is equal to <paramref name="other"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public readonly bool SetEquals(scoped ReadOnlySpan<T> other)
    {
        if (_count == 0 && other.Length > 0)
            return false;

        (int found, int notFound) = CompareElements(other, returnIfNotFound: true);
        return found == _count && notFound == 0;
    }

    /// <inheritdoc cref="SetEquals(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <c>null</c>.</exception>
    public readonly bool SetEquals(IEnumerable<T> other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        if (other is HashSet<T> otherSet && Equals(Comparer, otherSet.Comparer))
        {
            if (_count != otherSet.Count)
                return false;

            return IsSubsetOfHashSetWithSameComparer(otherSet);
        }

        if (_count == 0 && other is ICollection<T> { Count: > 0 })
            return false;

        (int found, int notFound) = CompareElements(other.AsValueEnumerable(), returnIfNotFound: true);
        return found == _count && notFound == 0;
    }

    /// <inheritdoc cref="SetEquals(ReadOnlySpan{T})"/>
    public readonly bool SetEquals(scoped in ValueSet<T> other)
    {
        if (_comparer == other._comparer && _count != other._count)
            return false;

        return SetEquals(other.AsSpan());
    }

    /// <inheritdoc cref="SetEquals(IEnumerable{T})"/>
    public readonly bool SetEquals(T[] other)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(other);

        return SetEquals(other.AsSpan());
    }

    /// <summary>
    /// Compares the elements of the current <see cref="ValueSet{T}"/> with those in another collection,
    /// returning the counts of unique items found/not found in <paramref name="other"/>.
    /// </summary>
    /// <param name="other">
    /// The collection to compare to the current set.
    /// </param>
    /// <param name="returnIfNotFound">
    /// Specifies whether to stop the comparison operation after the first missing element is encountered.
    /// </param>
    /// <returns>
    /// The counts of unique items found/not found in <paramref name="other"/>.
    /// </returns>
    private readonly (int FoundCount, int NotFoundCount) CompareElements(scoped ValueEnumerable<T> other, bool returnIfNotFound = false)
    {
        if (_count == 0)
            return (0, other.Any() ? 1 : 0);

        int originalCount = _count;
        Span<T> items = _buffer.Slice(0, originalCount);
        IEqualityComparer<T>? comparer = _comparer;

        int intCount = ValueBitArray.GetInt32Count(originalCount);
        SpanOwner<int> foundItemsSource = SpanOwner<int>.MayStackalloc(intCount) ? stackalloc int[intCount] : SpanOwner<int>.Rent(intCount);
        ValueBitArray foundItems = foundItemsSource.Span;

        int found = 0;
        int notFound = 0;

        foreach (T item in other)
        {
            int index = items.IndexOf(item, comparer);
            if (index >= 0)
            {
                if (!foundItems[index])
                {
                    foundItems[index] = true;
                    found++;
                }
            }
            else
            {
                notFound++;
                if (returnIfNotFound)
                    break;
            }
        }

        foundItemsSource.Dispose();
        return (found, notFound);
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="ValueSet{T}"/>.
    /// </summary>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="length">The number of elements in the range.</param>
    /// <returns>
    /// A shallow copy of a range of elements in the source <see cref="ValueSet{T}"/>.
    /// </returns>
    public readonly ValueSet<T> Slice(int start, int length)
    {
        // This method is needed for the slicing syntax ([i..n]) to work.
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        ValueSet<T> set = new(length, _comparer) { _count = length };
        _buffer.Slice(start, length).CopyTo(set._buffer);
        return set;
    }

    /// <summary>
    /// Copies the elements from this <see cref="ValueSet{T}"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueSet{T}"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The number of elements in the source <see cref="ValueSet{T}"/> is greater
    /// than the number of elements that the destination can contain.
    /// </exception>
    public readonly void CopyTo(scoped Span<T> destination) => AsSpan().CopyTo(destination);

    /// <inheritdoc cref="CopyTo(Span{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="array"/> is <c>null</c>.</exception>
    public readonly void CopyTo(T[] array) => CopyTo(0, array, destinationStart: 0, _count);

    /// <inheritdoc cref="CopyTo(T[])"/>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueSet{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    public readonly void CopyTo(T[] destination, int destinationStart) => CopyTo(0, destination, destinationStart, _count);

    /// <inheritdoc cref="CopyTo(Span{T})"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueSet{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueSet{T}"/>.
    /// </param>
    /// <param name="length">
    /// The number of elements to copy.
    /// </param>
    public readonly void CopyTo(int start, scoped Span<T> destination, int length) => AsSpan(start, length).CopyTo(destination);

    /// <inheritdoc cref="CopyTo(T[], int)"/>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueSet{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="destinationLength">The length of the destination.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly void CopyTo(T[] destination, int destinationStart, int destinationLength)
    {
        // This method only exists for consistency purposes. It mimics the
        // out of line behavior of the `HashSet.CopyTo(T[], int, int)` method,
        // which doesn't throw an exception if the `length` parameter exceeds
        // the number of elements in the collection.
        // Even .NET has some stupid legacy stuff.

        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        CopyTo(0, destination.AsSpan(destinationStart, destinationLength), _count);
    }

    /// <inheritdoc cref="CopyTo(T[], int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueSet{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueSet{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    public readonly void CopyTo(int start, T[] destination, int destinationStart, int length)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        CopyTo(start, destination.AsSpan(destinationStart), length);
    }

    /// <summary>
    /// Attempts to copy the elements from this <see cref="ValueSet{T}"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueSet{T}"/>.
    /// </param>
    /// <param name="written">
    /// The number of elements copied to the destination.
    /// </param>
    /// <returns>
    /// <c>true</c> if the operation was successful; otherwise, <c>false</c>.
    /// </returns>
    public readonly bool TryCopyTo(scoped Span<T> destination, out int written)
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

    /// <inheritdoc cref="TryCopyTo(Span{T}, out int)"/>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public readonly bool TryCopyTo(T[] destination, out int written)
        => TryCopyTo(0, destination, destinationStart: 0, _count, out written);

    /// <inheritdoc cref="TryCopyTo(T[], out int)"/>
    /// <param name="destination">The destination of the elements copied from <see cref="ValueSet{T}"/>.</param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="written">The number of elements copied to the destination.</param>
    public readonly bool TryCopyTo(T[] destination, int destinationStart, out int written)
        => TryCopyTo(0, destination, destinationStart, _count, out written);

    /// <inheritdoc cref="TryCopyTo(Span{T}, out int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueSet{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">The destination of the elements copied from <see cref="ValueSet{T}"/>.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <param name="written">The number of elements copied to the destination.</param>
    public readonly bool TryCopyTo(int start, scoped Span<T> destination, int length, out int written)
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

    /// <inheritdoc cref="TryCopyTo(T[], int, out int)"/>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueSet{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="destinationLength">The length of the destination.</param>
    /// <param name="written">The number of elements copied to the array.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly bool TryCopyTo(T[] destination, int destinationStart, int destinationLength, out int written)
    {
        // This method only exists for consistency purposes. It mimics the
        // out of line behavior of the `HashSet.CopyTo(T[], int, int)` method,
        // which doesn't throw an exception if the `length` parameter exceeds
        // the number of elements in the collection.
        // Even .NET has some stupid legacy stuff.

        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        return TryCopyTo(0, destination.AsSpan(destinationStart, destinationLength), _count, out written);
    }

    /// <inheritdoc cref="TryCopyTo(T[], int, out int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueSet{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueSet{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <param name="written">The number of elements copied to the array.</param>
    public readonly bool TryCopyTo(int start, T[] destination, int destinationStart, int length, out int written)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        return TryCopyTo(start, destination.AsSpan(destinationStart), length, out written);
    }

    /// <summary>
    /// Copies the elements of the <see cref="ValueSet{T}"/> to a new array.
    /// </summary>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ValueSet{T}"/>.
    /// </returns>
    public readonly T[] ToArray() => AsSpan().ToArray();

    /// <summary>
    /// Copies the elements of the <see cref="ValueSet{T}"/> to a new array.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this instance, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ValueSet{T}"/>.
    /// </returns>
    public T[] ToArray(bool dispose)
    {
        T[] array = AsSpan().ToArray();

        if (dispose)
        {
            Dispose();
        }

        return array;
    }

    /// <summary>
    /// Copies the elements of the <see cref="ValueSet{T}"/> to a new <see cref="HashSet{T}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="HashSet{T}"/> containing copies of the elements of the <see cref="ValueSet{T}"/>.
    /// </returns>
    public readonly HashSet<T> ToHashSet()
    {
        HashSet<T> set = new(_comparer);
        for (int i = 0; i < _count; i++)
            set.Add(_buffer[i]);

        return set;
    }

    /// <summary>
    /// Copies the elements of the <see cref="ValueSet{T}"/> to a new <see cref="HashSet{T}"/>.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this instance, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>
    /// A <see cref="HashSet{T}"/> containing copies of the elements of the <see cref="ValueSet{T}"/>.
    /// </returns>
    public HashSet<T> ToHashSet(bool dispose)
    {
        HashSet<T> set = ToHashSet();

        if (dispose)
        {
            Dispose();
        }

        return set;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ValueSet{T}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Span{T}.Enumerator"/> for the <see cref="ValueSet{T}"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();

    /// <inheritdoc/>
    [Obsolete("Equals(object) on ValueSet will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly bool Equals(object? obj)
        => ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct();

    /// <inheritdoc/>
    [Obsolete("GetHashCode() on ValueSet will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly int GetHashCode()
        => ThrowHelper.ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct();

    /// <summary>
    /// Removes all items from this instance and releases the buffer used by it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        // Clear the elements so that the GC can reclaim the references.
        AsSpan().Clear();

        if (_rentedBuffer is not null)
        {
            ArrayPool<T>.Shared.Return(_rentedBuffer);
        }

        _buffer = _rentedBuffer = default;
        _count = 0;
    }

    /// <summary>
    /// Sets the capacity to the actual number of elements in the <see cref="ValueSet{T}"/>,
    /// if that number is less than a threshold value.
    /// </summary>
    public void TrimExcess()
    {
        if (CollectionHelper.ShouldTrim(_count, _buffer.Length))
        {
            Capacity = _count;
        }
    }

    /// <summary>
    /// Sets the capacity of this instance to hold up a specified number of entries
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
        T[] newRentedBuffer = ArrayPool<T>.Shared.Rent(capacity);
        T[]? oldRentedBuffer = _rentedBuffer;
        AsSpan().CopyTo(newRentedBuffer);

        // Clear the elements so that the GC can reclaim the references.
        AsSpan().Clear();

        _buffer = _rentedBuffer = newRentedBuffer;
        if (oldRentedBuffer is not null)
        {
            ArrayPool<T>.Shared.Return(oldRentedBuffer);
        }
    }
}
