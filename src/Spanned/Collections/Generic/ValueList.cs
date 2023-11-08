using System.Collections.ObjectModel;

namespace Spanned.Collections.Generic;

/// <summary>
/// Represents a strongly typed list of objects that can be accessed by index.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
[DebuggerTypeProxy(typeof(ValueCollectionDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
public ref struct ValueList<T>
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
    /// Initializes a new instance of the <see cref="ValueList{T}"/> struct using the provided
    /// initial buffer.
    /// </summary>
    /// <param name="buffer">The initial item buffer for this instance.</param>
    public ValueList(Span<T> buffer)
    {
        _buffer = buffer;
        _count = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueList{T}"/> struct that
    /// is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(int)"/>
    public ValueList(int capacity)
    {
        _rentedBuffer = ArrayPool<T>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _count = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueList{T}"/> struct that
    /// contains elements copied from the specified collection and has sufficient capacity
    /// to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    public ValueList(scoped ReadOnlySpan<T> collection)
        : this(collection, collection.Length)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueList{T}"/> struct that
    /// contains elements copied from the specified collection and has the specified capacity.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(int, int)"/>
    public ValueList(scoped ReadOnlySpan<T> collection, int capacity)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(capacity, collection.Length);

        _rentedBuffer = capacity == 0 ? null : ArrayPool<T>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _count = collection.Length;

        collection.CopyTo(_buffer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueList{T}"/> struct that
    /// contains elements copied from the specified collection and has sufficient capacity
    /// to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public ValueList(IEnumerable<T> collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        if (collection is ICollection<T> c)
        {
            int capacity = c.Count;
            _rentedBuffer = capacity == 0 ? null : ArrayPool<T>.Shared.Rent(capacity);
            _buffer = _rentedBuffer;
            _count = capacity;

            if (_rentedBuffer is not null)
            {
                c.CopyTo(_rentedBuffer, 0);
            }
        }
        else
        {
            _buffer = _rentedBuffer = default;
            _count = 0;

            foreach (T item in collection)
            {
                Add(item);
            }
        }
    }

    /// <summary>
    /// Implicitly converts a <see cref="ValueList{T}"/> to a span.
    /// </summary>
    /// <param name="list">The list to be converted.</param>
    /// <returns>A span covering the content of the <see cref="ValueList{T}"/>.</returns>
    public static implicit operator ReadOnlySpan<T>(ValueList<T> list)
        => MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(list._buffer), list._count);

    /// <summary>
    /// Returns a span that represents the content of the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <returns>A span covering the content of the <see cref="ValueList{T}"/>.</returns>
    public readonly Span<T> AsSpan() => MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(_buffer), _count);

    /// <summary>
    /// Returns a span that represents a segment of the list starting from the specified index.
    /// </summary>
    /// <param name="start">The start index of the segment.</param>
    /// <returns>A span covering the segment of the list.</returns>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int)"/>
    public readonly Span<T> AsSpan(int start) => AsSpan(start, _count - start);

    /// <summary>
    /// Returns a span that represents a segment of the list with the specified start index and length.
    /// </summary>
    /// <param name="start">The start index of the segment.</param>
    /// <param name="length">The length of the segment.</param>
    /// <returns>A span covering the segment of the list.</returns>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly Span<T> AsSpan(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        // Skip additional bound checks.
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), start), length);
    }

    /// <summary>
    /// Returns a span that represents the remaining unused segment of the list.
    /// </summary>
    /// <returns>A span covering the unused segment of the list.</returns>
    public readonly Span<T> AsRemainingSpan() => _buffer.Slice(_count);

    /// <summary>
    /// Returns a span that represents the entire capacity of the list,
    /// including used and unused segments.
    /// </summary>
    /// <returns>A span covering the entire buffer of the list.</returns>
    public readonly Span<T> AsCapacitySpan() => _buffer;

    /// <summary>
    /// The number of elements contained in the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCount(int, int)"/>
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
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(int, int)"/>
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
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
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
    /// <returns>The new capacity of this list.</returns>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(int)"/>
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
    /// Adds an object to the end of the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to be added to the end of the <see cref="ValueList{T}"/>.
    /// <para/>
    /// The value can be <c>null</c> for reference types.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        int count = _count;
        Span<T> buffer = _buffer;

        if ((uint)count < (uint)buffer.Length)
        {
            buffer[count] = item;
            _count = count + 1;
        }
        else
        {
            GrowAndAdd(item);
        }
    }

    /// <summary>
    /// Grows the internal buffer by 1 and adds an object to the end of the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to be added to the end of the <see cref="ValueList{T}"/>.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAdd(T item)
    {
        Grow(1);

        _buffer[_count] = item;
        _count++;
    }

    /// <summary>
    /// Reserves a span of items at the end of the current list and
    /// returns it for direct modification.
    /// </summary>
    /// <param name="length">The length of the span to reserve.</param>
    /// <returns>A span that represents the reserved space.</returns>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidLength(int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AddSpan(int length)
    {
        // Do not validate `length`.
        // Let the slow path deal with it.

        int previousCount = _count;
        Span<T> buffer = _buffer;

        // Helps with removing bound checks during inlining.
        // The exactly same check as can be seen in the `Span.Slice()` implementation.
        if ((ulong)(uint)previousCount + (ulong)(uint)length <= (ulong)(uint)buffer.Length)
        {
            _count = previousCount + length;
            return buffer.Slice(previousCount, length);
        }
        else
        {
            return GrowAndAddSpan(length);
        }
    }

    /// <summary>
    /// Grows the internal buffer, reserves a span of items at the end of the current list,
    /// and returns it for direct modification.
    /// </summary>
    /// <param name="length">The length of the span to reserve.</param>
    /// <returns>A span that represents the reserved space.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private Span<T> GrowAndAddSpan(int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidLength(length);

        Grow(_count + length - _buffer.Length);

        int index = _count;
        _count += length;

        return _buffer.Slice(index, length);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="collection">
    /// The collection whose elements should be added to the end of the <see cref="ValueList{T}"/>.
    /// <para/>
    /// The collection can contain elements that are <c>null</c>, if type <typeparamref name="T"/>
    /// is a reference type.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange(scoped ReadOnlySpan<T> collection)
    {
        if ((ulong)(uint)_count + (ulong)(uint)collection.Length > (ulong)(uint)_buffer.Length)
        {
            Grow(collection.Length);
        }

        collection.CopyTo(_buffer.Slice(_count));
        _count += collection.Length;
    }

    /// <inheritdoc cref="AddRange(IEnumerable{T})"/>
    public void AddRange(T[] collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        AddRange(collection.AsSpan());
    }

    /// <inheritdoc cref="AddRange(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public void AddRange(IEnumerable<T> collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        if (collection.TryGetSpan(out ReadOnlySpan<T> span))
        {
            AddRange(span);
            return;
        }

        if (collection is ICollection<T> c)
        {
            int count = c.Count;
            if ((ulong)(uint)_count + (ulong)(uint)count > (ulong)(uint)_buffer.Length)
            {
                Grow(count);
            }

            if (_rentedBuffer is not null)
            {
                c.CopyTo(_rentedBuffer, _count);
                _count += count;
                return;
            }
        }

        foreach (T item in collection)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Inserts an element into the <see cref="ValueList{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">
    /// The object to insert.
    /// <para/>
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    public void Insert(int index, T item)
    {
        if ((uint)index > (uint)_count)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        if ((ulong)(uint)_count + 1ul > (ulong)(uint)_buffer.Length)
        {
            Grow(1);
        }

        if (index != _count)
        {
            int remaining = _count - index;
            _buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + 1));
        }

        _buffer[index] = item;
        _count += 1;
    }

    /// <summary>
    /// Inserts the elements of a collection into the <see cref="ValueList{T}"/>
    /// at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
    /// <param name="collection">
    /// The collection whose elements should be inserted into the <see cref="ValueList{T}"/>.
    /// /// <para/>
    /// The collection can contain elements that are <c>null</c>, if type
    /// <typeparamref name="T"/> is a reference type.
    /// </param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    public void InsertRange(int index, scoped ReadOnlySpan<T> collection)
    {
        if ((uint)index > (uint)_count)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        int count = collection.Length;

        if ((ulong)(uint)_count + (ulong)(uint)count > (ulong)(uint)_buffer.Length)
        {
            Grow(count);
        }

        if (index != _count)
        {
            int remaining = _count - index;
            _buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + count));
        }

        collection.CopyTo(_buffer.Slice(index));
        _count += count;
    }

    /// <inheritdoc cref="InsertRange(int, IEnumerable{T})"/>
    public void InsertRange(int index, T[] collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        InsertRange(index, collection.AsSpan());
    }

    /// <inheritdoc cref="InsertRange(int, ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public void InsertRange(int index, IEnumerable<T> collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);
        if ((uint)index > (uint)_count)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        if (collection.TryGetSpan(out ReadOnlySpan<T> span))
        {
            InsertRange(index, span);
            return;
        }

        if (collection is ICollection<T> c)
        {
            int count = c.Count;
            if ((ulong)(uint)_count + (ulong)(uint)count > (ulong)(uint)_buffer.Length)
            {
                Grow(count);
            }

            if (_rentedBuffer is not null)
            {
                if (index != _count)
                {
                    int remaining = _count - index;
                    _buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + count));
                }

                c.CopyTo(_rentedBuffer, index);
                _count += count;
                return;
            }
        }

        foreach (T item in collection)
        {
            Insert(index++, item);
        }
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to remove from the <see cref="ValueList{T}"/>.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <returns>
    /// <c>true</c> if item is successfully removed; otherwise, <c>false</c>.
    /// This method also returns <c>false</c> if item was not found in the <see cref="ValueList{T}"/>.
    /// </returns>
    public bool Remove(T item)
    {
        int index = AsSpan().IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes all the elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="predicate">
    /// The delegate that defines the conditions of the elements to remove.
    /// </param>
    /// <returns>
    /// The number of elements removed from the <see cref="ValueList{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
    public int RemoveAll(Predicate<T> predicate)
    {
        int removed = AsSpan().RemoveWhere(predicate);
        _count -= removed;

        return removed;
    }

    /// <summary>
    /// Removes the element at the specified index of the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(index, _count);

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            // Clear the element so that the GC can reclaim its reference.
            _buffer[index] = default!;
        }

        _buffer.Slice(index + 1, _count - index - 1).CopyTo(_buffer.Slice(index));
        _count--;
    }

    /// <summary>
    /// Removes a range of elements from the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="start">The zero-based starting index of the range of elements to remove.</param>
    /// <param name="length">The number of elements to remove.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public void RemoveRange(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            // Clear the elements so that the GC can reclaim the references.
            AsSpan(start, length).Clear();
        }

        _buffer.Slice(start + length).CopyTo(_buffer.Slice(start));
        _count -= length;
    }

    /// <summary>
    /// Determines whether an element is in the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueList{T}"/>.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <returns>
    /// <c>true</c> if item is found in the <see cref="ValueList{T}"/>; otherwise, <c>false</c>.
    /// </returns>
    public readonly bool Contains(T item) => AsSpan().Contains(item);

    /// <summary>
    /// Removes all elements from the <see cref="ValueList{T}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            // Clear the elements so that the GC can reclaim the references.
            AsSpan().Clear();
        }

        _count = 0;
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the first
    /// occurrence within the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueList{T}"/>.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <returns>
    /// The zero-based index of the first occurrence of item within the <see cref="ValueList{T}"/>,
    /// if found; otherwise, <c>-1</c>.
    /// </returns>
    public readonly int IndexOf(T item) => AsSpan().IndexOf(item);

    /// <inheritdoc cref="IndexOf(T)"/>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueList{T}"/>.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <param name="index">
    /// The zero-based starting index of the search.
    /// 0 (zero) is valid in an empty list.
    /// </param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    public readonly int IndexOf(T item, int index) => IndexOf(item, index, _count - index);

    /// <inheritdoc cref="IndexOf(T)"/>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueList{T}"/>.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <param name="start">
    /// The zero-based starting index of the search.
    /// 0 (zero) is valid in an empty list.
    /// </param>
    /// <param name="length">The number of elements in the section to search.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly int IndexOf(T item, int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        int itemIndex = AsSpan(start, length).IndexOf(item);
        return itemIndex + (itemIndex < 0 ? 0 : start);
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the last
    /// occurrence within the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueList{T}"/>.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <returns>
    /// The zero-based index of the last occurrence of item within the the <see cref="ValueList{T}"/>,
    /// if found; otherwise, <c>-1</c>.
    /// </returns>
    public readonly int LastIndexOf(T item) => AsSpan().LastIndexOf(item);

    /// <inheritdoc cref="LastIndexOf(T)"/>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueList{T}"/>.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <param name="index">The zero-based starting index of the backward search.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    public readonly int LastIndexOf(T item, int index)
    {
        // Don't use (uint) here.
        // The original code from .NET Framework was garbage:
        //  - It throws when `start` exceeds the number of elements in the list.
        //  - But it returns `-1` if `start` is negative and the list is empty. :clown_emoji:
        if (index >= _count)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        return LastIndexOf(item, index, index + 1);
    }

    /// <inheritdoc cref="LastIndexOf(T)"/>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueList{T}"/>.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <param name="start">The zero-based starting index of the backward search.</param>
    /// <param name="length">The number of elements in the section to search.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly int LastIndexOf(T item, int start, int length)
    {
        // Very f-ing funny.
        // `LastIndexOf` behaves differently compared to `FindLastIndex`
        // when the list is empty.
        if (_count == 0)
        {
            return -1;
        }

        if ((uint)start >= (uint)_count || length < 0 || start - length + 1 < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        int endIndex = start - length + 1;
        int result = _buffer.Slice(endIndex, length).LastIndexOf(item);

        return result + (result < 0 ? 0 : endIndex);
    }

    /// <inheritdoc cref="BinarySearch(T, IComparer{T})"/>
    public readonly int BinarySearch(T item)
        => AsSpan().BinarySearch(item, Comparer<T>.Default);

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of its
    /// occurrence within the sorted <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to locate.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <param name="comparer">
    /// The <see cref="IComparer{T}"/> implementation to use when comparing elements.
    /// -or-
    /// <c>null</c> to use the default comparer <see cref="Comparer{T}.Default"/>.
    /// </param>
    /// <returns>
    /// The zero-based index of item in the sorted <see cref="ValueList{T}"/>, if item is found;
    /// otherwise, a negative number that is the bitwise complement of the index of the next
    /// element that is larger than item or, if there is no larger element,
    /// the bitwise complement of <see cref="Count"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// A custom comparer is not provided, and the default <see cref="Comparer{T}.Default"/>
    /// cannot find an implementation of the <see cref="IComparable{T}"/> generic interface or
    /// the <see cref="IComparable"/> interface for type <typeparamref name="T"/>.
    /// </exception>
    public readonly int BinarySearch(T item, IComparer<T>? comparer)
        => AsSpan().BinarySearch(item, comparer ?? Comparer<T>.Default);

    /// <inheritdoc cref="BinarySearch(T, IComparer{T})"/>
    /// <param name="start">The zero-based starting index of the range to search.</param>
    /// <param name="length">The length of the range to search.</param>
    /// <param name="item">
    /// The object to locate.
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <param name="comparer">
    /// The <see cref="IComparer{T}"/> implementation to use when comparing elements.
    /// -or-
    /// <c>null</c> to use the default comparer <see cref="Comparer{T}.Default"/>.
    /// </param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly int BinarySearch(int start, int length, T item, IComparer<T>? comparer)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        int itemIndex = _buffer.Slice(start, length).BinarySearch(item, comparer ?? Comparer<T>.Default);
        return itemIndex + (itemIndex < 0 ? -start : start);
    }

    /// <summary>
    /// Determines whether the <see cref="ValueList{T}"/> contains elements that
    /// match the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="predicate">
    /// The delegate that defines the conditions of the elements to search for.
    /// </param>
    /// <returns>
    /// <c>true</c> if the <see cref="ValueList{T}"/> contains one or more elements that
    /// match the conditions defined by the specified predicate; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public readonly bool Exists(Predicate<T> predicate) => FindIndex(predicate) >= 0;

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate,
    /// and returns the first occurrence within the entire <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="predicate">
    /// The delegate that defines the conditions of the elements to search for.
    /// </param>
    /// <returns>
    /// The first element that matches the conditions defined by the specified predicate,
    /// if found; otherwise, the default value for type <typeparamref name="T"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public readonly T? Find(Predicate<T> predicate)
    {
        int index = FindIndex(0, _count, predicate);
        return index >= 0 ? _buffer[index] : default;
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified
    /// predicate, and returns the zero-based index of the first occurrence within the
    /// <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="predicate">
    /// The delegate that defines the conditions of the elements to search for.
    /// </param>
    /// <returns>
    /// The zero-based index of the first occurrence of an element that matches the conditions
    /// defined by match, if found; otherwise, <c>-1</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public readonly int FindIndex(Predicate<T> predicate) => FindIndex(0, _count, predicate);

    /// <inheritdoc cref="FindIndex(Predicate{T})"/>
    /// <param name="start">The zero-based starting index of the search.</param>
    /// <param name="predicate">
    /// The delegate that defines the conditions of the elements to search for.
    /// </param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    public readonly int FindIndex(int start, Predicate<T> predicate)
        => FindIndex(start, _count - start, predicate);

    /// <inheritdoc cref="FindIndex(Predicate{T})"/>
    /// <param name="start">The zero-based starting index of the search.</param>
    /// <param name="length">The number of elements in the section to search.</param>
    /// <param name="predicate">
    /// The delegate that defines the conditions of the elements to search for.
    /// </param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly int FindIndex(int start, int length, Predicate<T> predicate)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);
        ThrowHelper.ThrowArgumentNullException_IfNull(predicate);

        int end = start + length;
        for (int i = start; i < end; i++)
        {
            if (predicate(_buffer[i]))
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate,
    /// and returns the last occurrence within the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="predicate">
    /// The delegate that defines the conditions of the elements to search for.
    /// </param>
    /// <returns>
    /// The last element that matches the conditions defined by the specified predicate,
    /// if found; otherwise, the default value for type <typeparamref name="T"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public readonly T? FindLast(Predicate<T> predicate)
    {
        int index = FindLastIndex(_count - 1, _count, predicate);
        return index >= 0 ? _buffer[index] : default;
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified
    /// predicate, and returns the zero-based index of the last occurrence within the
    /// <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="predicate">
    /// The delegate that defines the conditions of the elements to search for.
    /// </param>
    /// <returns>
    /// The zero-based index of the last occurrence of an element that matches the conditions
    /// defined by match, if found; otherwise, <c>-1</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public readonly int FindLastIndex(Predicate<T> predicate)
        => FindLastIndex(_count - 1, _count, predicate);

    /// <inheritdoc cref="FindIndex(Predicate{T})"/>
    /// <param name="start">The zero-based starting index of the backward search.</param>
    /// <param name="predicate">
    /// The delegate that defines the conditions of the elements to search for.
    /// </param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    public readonly int FindLastIndex(int start, Predicate<T> predicate)
    {
        // Additional useless null-check to satisfy unit tests.
        ThrowHelper.ThrowArgumentNullException_IfNull(predicate);

        // Don't use (uint) here.
        // The original code from .NET Framework was garbage:
        //  - It throws when `start` exceeds the number of elements in the list.
        //  - But it returns `-1` if `start` is negative and the list is empty. :clown_emoji:
        if (start >= _count)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        return FindLastIndex(start, start + 1, predicate);
    }

    /// <inheritdoc cref="FindIndex(Predicate{T})"/>
    /// <param name="start">The zero-based starting index of the backward search.</param>
    /// <param name="length">The number of elements in the section to search.</param>
    /// <param name="predicate">
    /// The delegate that defines the conditions of the elements to search for.
    /// </param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly int FindLastIndex(int start, int length, Predicate<T> predicate)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(predicate);

        // Very f-ing funny.
        // `FindLastIndex` behaves differently compared to `LastIndexOf`
        // when the list is empty.
        if (_count == 0 && start is -1 or 0 && length == 0)
        {
            return -1;
        }

        if ((uint)start >= (uint)_count || length < 0 || start - length + 1 < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException();
        }

        int endIndex = start - length + 1;
        for (int i = start; i >= endIndex; i--)
        {
            if (predicate(_buffer[i]))
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Retrieves all the elements that match the conditions defined by the specified
    /// predicate.
    /// </summary>
    /// <param name="predicate">
    /// The delegate that defines the conditions of the elements to search for.
    /// </param>
    /// <returns>
    /// A <see cref="ValueList{T}"/> containing all the elements that match the
    /// conditions defined by the specified predicate, if found; otherwise, an empty
    /// <see cref="ValueList{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public readonly ValueList<T> FindAll(Predicate<T> predicate)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(predicate);

        ValueList<T> list = new(_count);

        for (int i = 0; i < _count; i++)
        {
            if (predicate(_buffer[i]))
                list[list._count++] = _buffer[i];
        }

        return list;
    }

    /// <summary>
    /// Determines whether every element in the <see cref="ValueList{T}"/> matches
    /// the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="predicate">
    /// The delegate that defines the conditions to check against the elements.
    /// </param>
    /// <returns>
    /// <c>true</c> if every element in the <see cref="ValueList{T}"/> matches the conditions
    /// defined by the specified predicate; otherwise, <c>false</c>.
    /// If the list has no elements, the return value is <c>true</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is <c>null</c>.</exception>
    public readonly bool TrueForAll(Predicate<T> predicate)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(predicate);

        for (int i = 0; i < _count; i++)
        {
            if (!predicate(_buffer[i]))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Performs the specified action on each element of the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="action">
    /// The delegate to perform on each element of the <see cref="ValueList{T}"/>.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public readonly void ForEach(Action<T> action)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(action);

        for (int i = 0; i < _count; i++)
        {
            action(_buffer[i]);
        }
    }

    /// <summary>
    /// Sorts the elements in the entire <see cref="ValueList{T}"/> using the default comparer.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// One or more elements in the list do not implement the <see cref="IComparable{T}"/> interface.
    /// </exception>
    public readonly void Sort() => AsSpan().Sort();

    /// <summary>
    /// Sorts the elements in the entire <see cref="ValueList{T}"/> using the specified comparer.
    /// </summary>
    /// <param name="comparer">
    /// The <see cref="IComparer{T}"/> implementation to use when comparing elements, or <c>null</c>
    /// to use the default comparer.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// <paramref name="comparer"/> is <c>null</c>, and one or more elements in the list
    /// do not implement the <see cref="IComparable{T}"/> interface.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The implementation of comparer caused an error during the sort.
    /// </exception>
    public readonly void Sort(IComparer<T>? comparer) => AsSpan().Sort(comparer);

    /// <summary>
    /// Sorts the elements in the entire <see cref="ValueList{T}"/> using the
    /// specified <see cref="Comparison{T}"/>.
    /// </summary>
    /// <param name="comparison">
    /// The <see cref="Comparison{T}"/> to use when comparing elements.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="comparison"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// The implementation of comparison caused an error during the sort.
    /// </exception>
    public readonly void Sort(Comparison<T> comparison) => AsSpan().Sort(comparison);

    /// <inheritdoc cref="Sort(IComparer{T})"/>
    /// <param name="start">The zero-based starting index of the range to sort.</param>
    /// <param name="length">The length of the range to sort.</param>
    /// <param name="comparer">
    /// The <see cref="IComparer{T}"/> implementation to use when comparing elements, or <c>null</c>
    /// to use the default comparer.
    /// </param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly void Sort(int start, int length, IComparer<T>? comparer)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        _buffer.Slice(start, length).Sort(comparer);
    }

    /// <summary>
    /// Reverses the order of the elements in the entire <see cref="ValueList{T}"/>.
    /// </summary>
    public readonly void Reverse() => AsSpan().Reverse();

    /// <inheritdoc cref="Reverse()"/>
    /// <param name="start">The zero-based starting index of the range to reverse.</param>
    /// <param name="length">The number of elements in the range to reverse.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly void Reverse(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        _buffer.Slice(start, length).Reverse();
    }

    /// <summary>
    /// Converts the elements in the current <see cref="ValueList{T}"/> to another
    /// type, and returns a list containing the converted elements.
    /// </summary>
    /// <typeparam name="TOutput">The type of the elements of the target list.</typeparam>
    /// <param name="converter">
    /// A that converts each element from one type to another type.
    /// </param>
    /// <returns>
    /// A <see cref="ValueList{T}"/> of the target type containing the converted
    /// elements from the current <see cref="ValueList{T}"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="converter"/> is <c>null</c>.
    /// </exception>
    public readonly ValueList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
    {
        ValueList<TOutput> list = new(_count) { _count = _count };

        Span<T> buffer = _buffer;
        Span<TOutput> destination = list._buffer;
        for (int i = 0; i < _count; i++)
        {
            destination[i] = converter(buffer[i]);
        }

        return list;
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="ValueList{T}"/>.
    /// </summary>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="length">The number of elements in the range.</param>
    /// <returns>
    /// A shallow copy of a range of elements in the source <see cref="ValueList{T}"/>.
    /// </returns>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly ValueList<T> GetRange(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        ValueList<T> list = new(length) { _count = length };
        _buffer.Slice(start, length).CopyTo(list._buffer);
        return list;
    }

    /// <inheritdoc cref="GetRange(int, int)"/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ValueList<T> Slice(int index, int count)
    {
        // This method is needed for the slicing syntax ([i..n]) to work.
        // `GetRange` was introduced long before the introduction of actual ranges, so
        // we now have to deal with two identical methods in the List class.
        return GetRange(index, count);
    }

    /// <summary>
    /// Copies the elements from this <see cref="ValueList{T}"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueList{T}"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The number of elements in the source <see cref="ValueList{T}"/> is greater
    /// than the number of elements that the destination can contain.
    /// </exception>
    public readonly void CopyTo(scoped Span<T> destination) => AsSpan().CopyTo(destination);

    /// <inheritdoc cref="CopyTo(Span{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public readonly void CopyTo(T[] destination) => CopyTo(0, destination, destinationStart: 0, _count);

    /// <inheritdoc cref="CopyTo(T[])"/>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueList{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    public readonly void CopyTo(T[] destination, int destinationStart) => CopyTo(0, destination, destinationStart, _count);

    /// <inheritdoc cref="CopyTo(Span{T})"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueList{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueList{T}"/>.
    /// </param>
    /// <param name="length">
    /// The number of elements to copy.
    /// </param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly void CopyTo(int start, scoped Span<T> destination, int length) => AsSpan(start, length).CopyTo(destination);

    /// <inheritdoc cref="CopyTo(T[], int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueList{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueList{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly void CopyTo(int start, T[] destination, int destinationStart, int length)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        CopyTo(start, destination.AsSpan(destinationStart), length);
    }

    /// <summary>
    /// Attempts to copy the elements from this <see cref="ValueList{T}"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueList{T}"/>.
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
    /// <param name="destination">The destination of the elements copied from <see cref="ValueList{T}"/>.</param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="written">The number of elements copied to the destination.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    public readonly bool TryCopyTo(T[] destination, int destinationStart, out int written)
        => TryCopyTo(0, destination, destinationStart, _count, out written);

    /// <inheritdoc cref="TryCopyTo(Span{T}, out int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueList{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">The destination of the elements copied from <see cref="ValueList{T}"/>.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <param name="written">The number of elements copied to the destination.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
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
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueList{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueList{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <param name="written">The number of elements copied to the array.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly
     bool TryCopyTo(int start, T[] destination, int destinationStart, int length, out int written)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        return TryCopyTo(start, destination.AsSpan(destinationStart), length, out written);
    }

    /// <summary>
    /// Copies the elements of the <see cref="ValueList{T}"/> to a new array.
    /// </summary>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ValueList{T}"/>.
    /// </returns>
    public readonly T[] ToArray() => AsSpan().ToArray();

    /// <summary>
    /// Copies the elements of the <see cref="ValueList{T}"/> to a new array.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this instance, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ValueList{T}"/>.
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
    /// Copies the elements of the <see cref="ValueList{T}"/> to a new <see cref="List{T}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="List{T}"/> containing copies of the elements of the <see cref="ValueList{T}"/>.
    /// </returns>
    public readonly List<T> ToList() => AsSpan().ToList();

    /// <summary>
    /// Copies the elements of the <see cref="ValueList{T}"/> to a new <see cref="List{T}"/>.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this instance, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>
    /// A <see cref="List{T}"/> containing copies of the elements of the <see cref="ValueList{T}"/>.
    /// </returns>
    public List<T> ToList(bool dispose)
    {
        List<T> list = AsSpan().ToList();

        if (dispose)
        {
            Dispose();
        }

        return list;
    }

    /// <summary>
    /// Copies the elements of the <see cref="ValueList{T}"/> to a new <see cref="List{T}"/>,
    /// and wraps it into a <see cref="ReadOnlyCollection{T}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="ReadOnlyCollection{T}"/> containing copies of the elements of the <see cref="ValueList{T}"/>.
    /// </returns>
    public readonly ReadOnlyCollection<T> ToReadOnlyCollection() => AsSpan().ToList().AsReadOnly();

    /// <summary>
    /// Copies the elements of the <see cref="ValueList{T}"/> to a new <see cref="List{T}"/>,
    /// and wraps it into a <see cref="ReadOnlyCollection{T}"/>.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this instance, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>
    /// A <see cref="ReadOnlyCollection{T}"/> containing copies of the elements of the <see cref="ValueList{T}"/>.
    /// </returns>
    public ReadOnlyCollection<T> ToReadOnlyCollection(bool dispose) => ToList(dispose).AsReadOnly();

    /// <inheritdoc cref="ToReadOnlyCollection()"/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ReadOnlyCollection<T> AsReadOnly()
    {
        // The name of this method may suggest that it returns a wrapper over this instance.
        // This assumption is as far from reality as theoretically possible.
        // Hence, we retain the method to match the one existing in the original List,
        // but hide it from IntelliSense.
        return ToReadOnlyCollection();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ValueList{T}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Span{T}.Enumerator"/> for the <see cref="ValueList{T}"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();

    /// <inheritdoc cref="ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct"/>
    [Obsolete("Equals(object) on ValueList will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly bool Equals(object? obj)
        => ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct();

    /// <inheritdoc cref="ThrowHelper.ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct"/>
    [Obsolete("GetHashCode() on ValueList will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly int GetHashCode()
        => ThrowHelper.ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct();

    /// <summary>
    /// Removes all items from this instance and releases the buffer used by it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            // Clear the elements so that the GC can reclaim the references.
            AsSpan().Clear();
        }

        if (_rentedBuffer is not null)
        {
            ArrayPool<T>.Shared.Return(_rentedBuffer);
        }

        this = default;
    }

    /// <summary>
    /// Sets the capacity to the actual number of elements in the <see cref="ValueList{T}"/>,
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
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(int, int)"/>
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

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            // Clear the elements so that the GC can reclaim the references.
            AsSpan().Clear();
        }

        _buffer = _rentedBuffer = newRentedBuffer;
        if (oldRentedBuffer is not null)
        {
            ArrayPool<T>.Shared.Return(oldRentedBuffer);
        }
    }
}
