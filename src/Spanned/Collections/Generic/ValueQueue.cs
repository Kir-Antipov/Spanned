namespace Spanned.Collections.Generic;

/// <summary>
/// Represents a first-in, first-out collection of objects.
/// </summary>
/// <typeparam name="T">The type of elements in the queue.</typeparam>
[DebuggerTypeProxy(typeof(ValueCollectionDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
public ref struct ValueQueue<T>
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
    /// The index from which to dequeue if the queue isn't empty.
    /// </summary>
    private int _head;

    /// <summary>
    /// The number of elements contained in the buffer.
    /// </summary>
    private int _count;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueQueue{T}"/> struct using the provided
    /// initial buffer.
    /// </summary>
    /// <param name="buffer">The initial item buffer for this instance.</param>
    public ValueQueue(Span<T> buffer)
    {
        _rentedBuffer = null;
        _buffer = buffer;
        _head = 0;
        _count = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueQueue{T}"/> struct that
    /// is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new queue can initially store.</param>
    public ValueQueue(int capacity)
    {
        _rentedBuffer = ArrayPool<T>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _head = 0;
        _count = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueQueue{T}"/> struct that
    /// contains elements copied from the specified collection and has sufficient capacity
    /// to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new queue.</param>
    public ValueQueue(scoped ReadOnlySpan<T> collection)
        : this(collection, collection.Length)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueQueue{T}"/> struct that
    /// contains elements copied from the specified collection and has the specified capacity.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new queue.</param>
    /// <param name="capacity">The number of elements that the new queue can initially store.</param>
    public ValueQueue(scoped ReadOnlySpan<T> collection, int capacity)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(capacity, collection.Length);

        _rentedBuffer = capacity == 0 ? null : ArrayPool<T>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _head = 0;
        _count = collection.Length;

        collection.CopyTo(_buffer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueQueue{T}"/> struct that
    /// contains elements copied from the specified collection and has sufficient capacity
    /// to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new queue.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public ValueQueue(IEnumerable<T> collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        if (collection is ICollection<T> c)
        {
            int capacity = c.Count;
            _rentedBuffer = capacity == 0 ? null : ArrayPool<T>.Shared.Rent(capacity);
            _buffer = _rentedBuffer;
            _head = 0;
            _count = capacity;

            if (_rentedBuffer is not null)
            {
                c.CopyTo(_rentedBuffer, 0);
            }
        }
        else
        {
            _rentedBuffer = null;
            _buffer = default;
            _head = 0;
            _count = 0;

            foreach (T item in collection)
            {
                Enqueue(item);
            }
        }
    }

    /// <summary>
    /// Implicitly converts a <see cref="ValueQueue{T}"/> to a span.
    /// </summary>
    /// <param name="queue">The queue to be converted.</param>
    /// <returns>A span covering the content of the <see cref="ValueQueue{T}"/>.</returns>
    public static implicit operator ReadOnlySpan<T>(ValueQueue<T> queue)
        => queue._buffer.Slice(queue._head, queue._count);

    /// <summary>
    /// Returns a span that represents the content of the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <returns>A span covering the content of the <see cref="ValueQueue{T}"/>.</returns>
    public readonly Span<T> AsSpan() => _buffer.Slice(_head, _count);

    /// <summary>
    /// Returns a span that represents a segment of the queue starting from the specified index.
    /// </summary>
    /// <param name="start">The start index of the segment.</param>
    /// <returns>A span covering the segment of the queue.</returns>
    public readonly Span<T> AsSpan(int start) => AsSpan(start, _count - start);

    /// <summary>
    /// Returns a span that represents a segment of the queue with the specified start index and length.
    /// </summary>
    /// <param name="start">The start index of the segment.</param>
    /// <param name="length">The length of the segment.</param>
    /// <returns>A span covering the segment of the queue.</returns>
    public readonly Span<T> AsSpan(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        return _buffer.Slice(_head + start, length);
    }

    /// <summary>
    /// Returns a span that represents the remaining unused segment of the queue.
    /// </summary>
    /// <returns>A span covering the unused segment of the queue.</returns>
    public Span<T> AsRemainingSpan()
    {
        if (_head != 0)
            Compact();

        return _buffer.Slice(_count);
    }

    /// <summary>
    /// Returns a span that represents the entire capacity of the queue,
    /// including used and unused segments.
    /// </summary>
    /// <returns>A span covering the entire buffer of the queue.</returns>
    public Span<T> AsCapacitySpan()
    {
        if (_head != 0)
            Compact();

        return _buffer;
    }

    /// <summary>
    /// The number of elements contained in the <see cref="ValueQueue{T}"/>.
    /// </summary>
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _count;
        set
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCount(value, _buffer.Length);

            if (_head != 0)
                Compact();

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
    /// Gets or sets the element at the specified index.
    /// </summary>
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
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), _head + index);
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
            Array.Copy(_rentedBuffer, _head, items, 0, items.Length);
            return items;
        }
    }

    /// <summary>
    /// Ensures that the capacity of this instance is at least the specified value.
    /// </summary>
    /// <returns>The new capacity of this queue.</returns>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    public int EnsureCapacity(int capacity)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(capacity);

        if ((uint)capacity > (uint)_buffer.Length)
            GrowOrCompact(capacity - _count);

        return _buffer.Length;
    }

    /// <summary>
    /// Provides a reference to the first item of this instance's buffer, which can be used for pinning.
    /// </summary>
    /// <returns>A reference to the first item of the buffer.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ref T GetPinnableReference() => ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), _head);

    /// <summary>
    /// Adds an object to the end of the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to be added to the end of the <see cref="ValueQueue{T}"/>.
    /// <para/>
    /// The value can be <c>null</c> for reference types.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(T item)
    {
        int count = _count;
        int index = _head + count;
        Span<T> buffer = _buffer;

        if ((uint)index < (uint)buffer.Length)
        {
            buffer[index] = item;
            _count = count + 1;
        }
        else
        {
            GrowOrCompactAndEnqueue(item);
        }
    }

    /// <summary>
    /// Grows the internal buffer or compacts it and adds an object to the end of the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to be added to the end of the <see cref="ValueQueue{T}"/>.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowOrCompactAndEnqueue(T item)
    {
        GrowOrCompact(1);

        _buffer[_count] = item;
        _count++;
    }

    /// <summary>
    /// Reserves a span of items at the end of the current queue and
    /// returns it for direct modification.
    /// </summary>
    /// <param name="length">The length of the span to reserve.</param>
    /// <returns>A span that represents the reserved space.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> EnqueueSpan(int length)
    {
        // Do not validate `length`.
        // Let the slow path deal with it.

        int previousCount = _count;
        int start = _head + previousCount;
        Span<T> buffer = _buffer;

        // Helps with removing bound checks during inlining.
        // The exactly same check as can be seen in the `Span.Slice()` implementation.
        if ((ulong)(uint)start + (ulong)(uint)length <= (ulong)(uint)buffer.Length)
        {
            _count = previousCount + length;
            return buffer.Slice(start, length);
        }
        else
        {
            return GrowOrCompactAndEnqueueSpan(length);
        }
    }

    /// <summary>
    /// Grows the internal buffer, reserves a span of items at the end of the current queue,
    /// and returns it for direct modification.
    /// </summary>
    /// <param name="length">The length of the span to reserve.</param>
    /// <returns>A span that represents the reserved space.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private Span<T> GrowOrCompactAndEnqueueSpan(int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidLength(length);

        GrowOrCompact(length);

        int index = _count;
        _count += length;

        return _buffer.Slice(index, length);
    }

    /// <summary>
    /// Adds the elements of the specified collection to the end of the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="collection">
    /// The collection whose elements should be added to the end of the <see cref="ValueQueue{T}"/>.
    /// <para/>
    /// The collection can contain elements that are <c>null</c>, if type <typeparamref name="T"/>
    /// is a reference type.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnqueueRange(scoped ReadOnlySpan<T> collection)
    {
        if ((ulong)(uint)_head + (ulong)(uint)_count + (ulong)(uint)collection.Length > (ulong)(uint)_buffer.Length)
        {
            GrowOrCompact(collection.Length);
        }

        collection.CopyTo(_buffer.Slice(_head + _count));
        _count += collection.Length;
    }

    /// <inheritdoc cref="EnqueueRange(IEnumerable{T})"/>
    public void EnqueueRange(T[] collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        EnqueueRange(collection.AsSpan());
    }

    /// <inheritdoc cref="EnqueueRange(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public void EnqueueRange(IEnumerable<T> collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        if (collection.TryGetSpan(out ReadOnlySpan<T> span))
        {
            EnqueueRange(span);
            return;
        }

        if (collection is ICollection<T> c)
        {
            int count = c.Count;
            if ((ulong)(uint)_head + (ulong)(uint)_count + (ulong)(uint)count > (ulong)(uint)_buffer.Length)
            {
                GrowOrCompact(count);
            }

            if (_rentedBuffer is not null)
            {
                c.CopyTo(_rentedBuffer, _head + _count);
                _count += count;
                return;
            }
        }

        foreach (T item in collection)
        {
            Enqueue(item);
        }
    }

    /// <summary>
    /// Inserts an object at the specified index of the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the queue to insert the element at.</param>
    /// <param name="item">The object to be added to the <see cref="ValueQueue{T}"/>.</param>
    public void Enqueue(int index, T item)
    {
        if ((uint)index > (uint)_count)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        if ((ulong)(uint)_head + (ulong)(uint)_count + 1ul > (ulong)(uint)_buffer.Length)
        {
            GrowOrCompact(1);
        }

        if (index != _count)
        {
            int remaining = _count - index;
            _buffer.Slice(_head + index, remaining).CopyTo(_buffer.Slice(_head + index + 1));
        }

        _buffer[_head + index] = item;
        _count += 1;
    }

    /// <summary>
    /// Sequentially inserts the elements of the specified collection at the specified index
    /// of the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the queue to insert the elements at.</param>
    /// <param name="collection">
    /// The collection whose elements should be inserted at the specified index
    /// of the <see cref="ValueQueue{T}"/>.
    /// <para/>
    /// The collection can contain elements that are <c>null</c>, if type <typeparamref name="T"/>
    /// is a reference type.
    /// </param>
    public void EnqueueRange(int index, scoped ReadOnlySpan<T> collection)
    {
        if ((uint)index > (uint)_count)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        if ((ulong)(uint)_head + (ulong)(uint)_count + (ulong)(uint)collection.Length > (ulong)(uint)_buffer.Length)
        {
            GrowOrCompact(collection.Length);
        }

        if (index != _count)
        {
            int remaining = _count - index;
            _buffer.Slice(_head + index, remaining).CopyTo(_buffer.Slice(_head + index + collection.Length));
        }

        collection.CopyTo(_buffer.Slice(_head + index));
        _count += collection.Length;
    }

    /// <inheritdoc cref="EnqueueRange(int, IEnumerable{T})"/>
    public void EnqueueRange(int index, T[] collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        EnqueueRange(index, collection.AsSpan());
    }

    /// <inheritdoc cref="EnqueueRange(int, ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public void EnqueueRange(int index, IEnumerable<T> collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);
        if ((uint)index > (uint)_count)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        if (collection.TryGetSpan(out ReadOnlySpan<T> span))
        {
            EnqueueRange(index, span);
            return;
        }

        if (collection is ICollection<T> c)
        {
            int count = c.Count;
            if ((ulong)(uint)_head + (ulong)(uint)_count + (ulong)(uint)count > (ulong)(uint)_buffer.Length)
            {
                GrowOrCompact(count);
            }

            if (_rentedBuffer is not null)
            {
                if (index != _count)
                {
                    int remaining = _count - index;
                    _buffer.Slice(_head + index, remaining).CopyTo(_buffer.Slice(_head + index + count));
                }

                c.CopyTo(_rentedBuffer, _head + index);
                _count += count;
                return;
            }
        }

        foreach (T item in collection)
        {
            Enqueue(index++, item);
        }
    }

    /// <summary>
    /// Returns the object at the beginning of the <see cref="ValueQueue{T}"/>
    /// without removing it.
    /// </summary>
    /// <returns>
    /// The object at the beginning of the <see cref="ValueQueue{T}"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T Peek()
    {
        if (_count <= 0)
        {
            ThrowHelper.ThrowInvalidOperationException_EmptyQueue();
        }

        return Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), _head);
    }

    /// <summary>
    /// Returns a value that indicates whether there is an object at the beginning of
    /// the <see cref="ValueQueue{T}"/>, and if one is present, copies it to
    /// the result parameter.
    /// </summary>
    /// <remarks>
    /// The object is not removed from the <see cref="ValueQueue{T}"/>.
    /// </remarks>
    /// <param name="result">
    /// If present, the object at the beginning of the <see cref="ValueQueue{T}"/>;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if there is an object at the beginning of the <see cref="ValueQueue{T}"/>;
    /// <c>false</c> if the <see cref="ValueQueue{T}"/> is empty.
    /// </returns>
    public readonly bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        if (_count <= 0)
        {
            result = default;
            return false;
        }

        result = Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), _head);
        return true;
    }

    /// <summary>
    /// Returns the object at the specified position of the <see cref="ValueQueue{T}"/>
    /// without removing it.
    /// </summary>
    /// <param name="index">The zero-based index of the element to peek at.</param>
    /// <returns>
    /// The object at the specified position of the <see cref="ValueQueue{T}"/>.
    /// </returns>
    /// <returns>The element at the specified index.</returns>
    public readonly T Peek(int index) => this[index];

    /// <summary>
    /// Returns a value that indicates whether there is an object at the specified
    /// position of the <see cref="ValueQueue{T}"/>, and if one is present, copies
    /// it to the result parameter.
    /// </summary>
    /// <param name="index">The zero-based index of the element to peek at.</param>
    /// <param name="result">
    /// If present, the object at the specified position of the <see cref="ValueQueue{T}"/>;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if there is an object at the specified position of the <see cref="ValueQueue{T}"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public readonly bool TryPeek(int index, [MaybeNullWhen(false)] out T result)
    {
        if ((uint)index < (uint)_count)
        {
            result = _buffer[_head + index];
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Copies the elements starting from the specified position (inclusive) of the <see cref="ValueQueue{T}"/>
    /// into the provided <paramref name="result"/> span.
    /// </summary>
    /// <remarks>
    /// The elements are not removed from the <see cref="ValueQueue{T}"/>.
    /// </remarks>
    /// <param name="start">The zero-based index of the starting element to copy.</param>
    /// <param name="result">A span to which the elements are copied.</param>
    public readonly void Peek(int start, scoped Span<T> result)
    {
        if (!TryPeek(start, result))
            ThrowHelper.ThrowArgumentOutOfRangeException();
    }

    /// <summary>
    /// Attempts to copy the elements starting from the specified position (inclusive) of the
    /// <see cref="ValueQueue{T}"/> into the provided <paramref name="result"/> span.
    /// </summary>
    /// <remarks>
    /// The elements are not removed from the <see cref="ValueQueue{T}"/>.
    /// </remarks>
    /// <param name="start">The zero-based index of the starting element to copy.</param>
    /// <param name="result">A span to which the elements are copied.</param>
    /// <returns>
    /// <c>true</c> if the elements are successfully copied; otherwise, <c>false</c>.
    /// </returns>
    public readonly bool TryPeek(int start, scoped Span<T> result)
    {
        Span<T> buffer = _buffer;
        start += _head;

        if ((ulong)(uint)start + (ulong)(uint)result.Length > (ulong)(uint)buffer.Length)
            return false;

        buffer.Slice(start, result.Length).CopyTo(result);
        return true;
    }

    /// <summary>
    /// Removes and returns the object at the beginning of the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <returns>
    /// The object removed from the beginning of the <see cref="ValueQueue{T}"/>.
    /// </returns>
    public T Dequeue()
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int head = _head;

        if (count <= 0)
        {
            ThrowHelper.ThrowInvalidOperationException_EmptyQueue();
        }

        _count = count - 1;
        _head = head + 1;
        T item = Unsafe.Add(ref MemoryMarshal.GetReference(buffer), head);

        // Clear the element so that the GC can reclaim its reference.
        Unsafe.Add(ref MemoryMarshal.GetReference(buffer), head) = default!;

        return item;
    }

    /// <summary>
    /// Returns a value that indicates whether there is an object at the beginning of
    /// the <see cref="ValueQueue{T}"/>, and if one is present, copies it to
    /// the result parameter, and removes it from the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="result">
    /// If present, the object at the beginning of the <see cref="ValueQueue{T}"/>;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if there is an object at the beginning of the <see cref="ValueQueue{T}"/>;
    /// <c>false</c> if the <see cref="ValueQueue{T}"/> is empty.
    /// </returns>
    public bool TryDequeue([MaybeNullWhen(false)] out T result)
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int head = _head;

        if (count <= 0)
        {
            result = default;
            return false;
        }

        _count = count - 1;
        _head = head + 1;
        result = Unsafe.Add(ref MemoryMarshal.GetReference(buffer), head);

        // Clear the element so that the GC can reclaim its reference.
        Unsafe.Add(ref MemoryMarshal.GetReference(buffer), head) = default!;

        return true;
    }

    /// <summary>
    /// Removes and returns the object at the specified position of the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to dequeue.</param>
    /// <returns>
    /// The object removed from the specified position of the <see cref="ValueQueue{T}"/>.
    /// </returns>
    public T Dequeue(int index)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(index, _count);

        int itemIndex = _head + index;
        T item = _buffer[itemIndex];

        // Clear the element so that the GC can reclaim its reference.
        _buffer[itemIndex] = default!;

        _buffer.Slice(itemIndex + 1, _count - index - 1).CopyTo(_buffer.Slice(itemIndex));
        _count--;

        return item;
    }

    /// <summary>
    /// Returns a value that indicates whether there is an object at the specified position
    /// of the <see cref="ValueQueue{T}"/>, and if one is present, copies it to
    /// the result parameter, and removes it from the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to dequeue.</param>
    /// <param name="result">
    /// If present, the object at the specified position of the <see cref="ValueQueue{T}"/>;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if there is an object at the specified position of the <see cref="ValueQueue{T}"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool TryDequeue(int index, [MaybeNullWhen(false)] out T result)
    {
        Span<T> buffer = _buffer;
        int itemIndex = _head + index;

        if ((uint)index >= (uint)_count)
        {
            result = default;
            return false;
        }

        result = buffer[itemIndex];

        // Clear the element so that the GC can reclaim its reference.
        buffer[itemIndex] = default!;

        buffer.Slice(itemIndex + 1, _count - index - 1).CopyTo(buffer.Slice(itemIndex));
        _count--;

        return true;
    }

    /// <summary>
    /// Removes the elements starting from the specified position (inclusive) of the
    /// <see cref="ValueQueue{T}"/> and copies them into the provided <paramref name="result"/> span.
    /// </summary>
    /// <param name="start">The zero-based index of the starting element to dequeue.</param>
    /// <param name="result">A span to which the elements are copied.</param>
    public void Dequeue(int start, scoped Span<T> result)
    {
        if (!TryDequeue(start, result))
            ThrowHelper.ThrowArgumentOutOfRangeException();
    }

    /// <summary>
    /// Attempts to remove the elements starting from the specified position (inclusive) of the
    /// <see cref="ValueQueue{T}"/> and copy them into the provided <paramref name="result"/> span.
    /// </summary>
    /// <param name="start">The zero-based index of the starting element to dequeue.</param>
    /// <param name="result">A span to which the elements are copied.</param>
    /// <returns>
    /// <c>true</c> if the elements are successfully dequeued; otherwise, <c>false</c>.
    /// </returns>
    public bool TryDequeue(int start, scoped Span<T> result)
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int head = _head;
        int segmentStart = head + start;

        if ((ulong)(uint)start + (ulong)(uint)result.Length > (ulong)(uint)count)
            return false;

        buffer.Slice(segmentStart, result.Length).CopyTo(result);

        // Clear the elements so that the GC can reclaim the references.
        _buffer.Slice(segmentStart, result.Length).Clear();

        buffer.Slice(segmentStart + result.Length, _count - start - result.Length).CopyTo(buffer.Slice(segmentStart));
        _count = count - result.Length;

        return true;
    }

    /// <summary>
    /// Removes the specified number of elements starting from the specified position (inclusive)
    /// of the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="start">The zero-based index of the starting element to dequeue.</param>
    /// <param name="length">The number of elements to dequeue.</param>
    public void Dequeue(int start, int length)
    {
        if (!TryDequeue(start, length))
            ThrowHelper.ThrowArgumentOutOfRangeException();
    }

    /// <summary>
    /// Attempts to remove the specified number of elements starting from the specified position (inclusive)
    /// of the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="start">The zero-based index of the starting element to dequeue.</param>
    /// <param name="length">The number of elements to dequeue.</param>
    /// <returns>
    /// <c>true</c> if the elements are successfully dequeued; otherwise, <c>false</c>.
    /// </returns>
    public bool TryDequeue(int start, int length)
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int head = _head;
        int segmentStart = head + start;

        if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)_count)
            return false;

        // Clear the elements so that the GC can reclaim the references.
        _buffer.Slice(segmentStart, length).Clear();

        buffer.Slice(segmentStart + length, _count - start - length).CopyTo(buffer.Slice(segmentStart));
        _count = count - length;

        return true;
    }

    /// <summary>
    /// Determines whether an element is in the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueQueue{T}"/>.
    /// <para/>
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <returns>
    /// <c>true</c> if item is found in the <see cref="ValueQueue{T}"/>; otherwise, <c>false</c>.
    /// </returns>
    public readonly bool Contains(T item) => AsSpan().Contains(item);

    /// <summary>
    /// Removes all objects from the <see cref="ValueQueue{T}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        // Clear the elements so that the GC can reclaim the references.
        AsSpan().Clear();

        _count = 0;
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="length">The number of elements in the range.</param>
    /// <returns>
    /// A shallow copy of a range of elements in the source <see cref="ValueQueue{T}"/>.
    /// </returns>
    public readonly ValueQueue<T> Slice(int start, int length)
    {
        // This method is needed for the slicing syntax ([i..n]) to work.

        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        ValueQueue<T> queue = new(length) { _count = length };
        AsSpan(start, length).CopyTo(queue._buffer);
        return queue;
    }

    /// <summary>
    /// Copies the elements from this <see cref="ValueQueue{T}"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueQueue{T}"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The number of elements in the source <see cref="ValueQueue{T}"/> is greater
    /// than the number of elements that the destination can contain.
    /// </exception>
    public readonly void CopyTo(scoped Span<T> destination) => AsSpan().CopyTo(destination);

    /// <inheritdoc cref="CopyTo(Span{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public readonly void CopyTo(T[] destination) => CopyTo(0, destination, destinationStart: 0, _count);

    /// <inheritdoc cref="CopyTo(T[])"/>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueQueue{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    public readonly void CopyTo(T[] destination, int destinationStart) => CopyTo(0, destination, destinationStart, _count);

    /// <inheritdoc cref="CopyTo(Span{T})"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueQueue{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueQueue{T}"/>.
    /// </param>
    /// <param name="length">
    /// The number of elements to copy.
    /// </param>
    public readonly void CopyTo(int start, scoped Span<T> destination, int length) => AsSpan(start, length).CopyTo(destination);

    /// <inheritdoc cref="CopyTo(T[], int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueQueue{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueQueue{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    public readonly void CopyTo(int start, T[] destination, int destinationStart, int length)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        CopyTo(start, destination.AsSpan(destinationStart), length);
    }

    /// <summary>
    /// Attempts to copy the elements from this <see cref="ValueQueue{T}"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueQueue{T}"/>.
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
    /// <param name="destination">The destination of the elements copied from <see cref="ValueQueue{T}"/>.</param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="written">The number of elements copied to the destination.</param>
    public readonly bool TryCopyTo(T[] destination, int destinationStart, out int written)
        => TryCopyTo(0, destination, destinationStart, _count, out written);

    /// <inheritdoc cref="TryCopyTo(Span{T}, out int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueQueue{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">The destination of the elements copied from <see cref="ValueQueue{T}"/>.</param>
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
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueQueue{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueQueue{T}"/>.
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
    /// Copies the elements of the <see cref="ValueQueue{T}"/> to a new array, and subsequently
    /// releases the internal resources of this queue, rendering it unusable.
    /// </summary>
    /// <remarks>
    /// After this method is called, the queue will be in a disposed state and should not be used further.
    /// </remarks>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ValueQueue{T}"/>.
    /// </returns>
    public readonly T[] ToArray() => AsSpan().ToArray();

    /// <summary>
    /// Copies the elements of the <see cref="ValueQueue{T}"/> to a new array.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this instance, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ValueQueue{T}"/>.
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
    /// Copies the elements of the <see cref="ValueQueue{T}"/> to a new <see cref="Queue{T}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Stack{T}"/> containing copies of the elements of the <see cref="ValueQueue{T}"/>.
    /// </returns>
    public readonly Queue<T> ToQueue()
    {
        Queue<T> queue = new(_count);

        int lastIndex = _head + _count;
        for (int i = _head; i < lastIndex; i++)
            queue.Enqueue(_buffer[i]);

        return queue;
    }

    /// <summary>
    /// Copies the elements of the <see cref="ValueQueue{T}"/> to a new <see cref="Queue{T}"/>.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this instance, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>
    /// A <see cref="Stack{T}"/> containing copies of the elements of the <see cref="ValueQueue{T}"/>.
    /// </returns>
    public Queue<T> ToQueue(bool dispose)
    {
        Queue<T> queue = ToQueue();

        if (dispose)
        {
            Dispose();
        }

        return queue;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ValueQueue{T}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Span{T}.Enumerator"/> for the <see cref="ValueQueue{T}"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();

    /// <inheritdoc/>
    [Obsolete("Equals(object) on ValueQueue will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly bool Equals(object? obj)
        => ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct();

    /// <inheritdoc/>
    [Obsolete("GetHashCode() on ValueQueue will always throw an exception.")]
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

        this = default;
    }

    /// <summary>
    /// Sets the capacity to the actual number of elements in the <see cref="ValueQueue{T}"/>,
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
    /// Moves the head position to <c>0</c>.
    /// </summary>
    private void Compact()
    {
        Debug.Assert(_head != 0);

        AsSpan().CopyTo(_buffer);
        _head = 0;
    }

    /// <summary>
    /// Grows the buffer used by this instance to ensure it can accommodate more items.
    /// </summary>
    /// <param name="minimumGrowLength">
    /// The minimum number of additional items the buffer should be able to accommodate
    /// after growth.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowOrCompact(int minimumGrowLength)
    {
        if (minimumGrowLength <= _buffer.Length - _count)
        {
            Compact();
        }
        else
        {
            int newCapacity = CollectionHelper.CalculateNewCapacity(_count, _buffer.Length, minimumGrowLength);
            ResizeBuffer(newCapacity);
        }
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
        _head = 0;

        if (oldRentedBuffer is not null)
        {
            ArrayPool<T>.Shared.Return(oldRentedBuffer);
        }
    }
}
