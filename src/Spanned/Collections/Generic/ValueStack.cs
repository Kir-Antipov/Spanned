namespace Spanned.Collections.Generic;

/// <summary>
/// Represents a variable size last-in-first-out (LIFO) value type collection of
/// instances of the same specified type.
/// </summary>
/// <typeparam name="T">The type of elements in the stack.</typeparam>
[DebuggerTypeProxy(typeof(ValueCollectionDebugView<>))]
[DebuggerDisplay("Count = {Count}")]
public ref struct ValueStack<T>
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
    /// Initializes a new instance of the <see cref="ValueStack{T}"/> struct using the provided
    /// initial buffer.
    /// </summary>
    /// <param name="buffer">The initial item buffer for this instance.</param>
    public ValueStack(Span<T> buffer)
    {
        _buffer = buffer;
        _count = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStack{T}"/> struct that
    /// is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The number of elements that the new stack can initially store.</param>
    public ValueStack(int capacity)
    {
        _rentedBuffer = ArrayPool<T>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _count = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStack{T}"/> struct that
    /// contains elements copied from the specified collection and has sufficient capacity
    /// to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new stack.</param>
    public ValueStack(scoped ReadOnlySpan<T> collection)
        : this(collection, collection.Length)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStack{T}"/> struct that
    /// contains elements copied from the specified collection and has the specified capacity.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new stack.</param>
    /// <param name="capacity">The number of elements that the new stack can initially store.</param>
    public ValueStack(scoped ReadOnlySpan<T> collection, int capacity)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(capacity, collection.Length);

        _rentedBuffer = capacity == 0 ? null : ArrayPool<T>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _count = collection.Length;

        Span<T> destination = _buffer.Slice(_buffer.Length - collection.Length);
        collection.CopyTo(destination);
        destination.Reverse();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStack{T}"/> struct that
    /// contains elements copied from the specified collection and has sufficient capacity
    /// to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new stack.</param>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public ValueStack(IEnumerable<T> collection)
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
                int index = _buffer.Length - _count;
                Span<T> destination = _buffer.Slice(index);
                c.CopyTo(_rentedBuffer, index);
                destination.Reverse();
            }
        }
        else
        {
            _buffer = _rentedBuffer = default;
            _count = 0;

            foreach (T item in collection)
            {
                Push(item);
            }
        }
    }

    /// <summary>
    /// Implicitly converts a <see cref="ValueStack{T}"/> to a span.
    /// </summary>
    /// <param name="stack">The stack to be converted.</param>
    /// <returns>A span covering the content of the <see cref="ValueStack{T}"/>.</returns>
    public static implicit operator ReadOnlySpan<T>(ValueStack<T> stack)
        => stack._buffer.Slice(stack._buffer.Length - stack._count, stack._count);

    /// <summary>
    /// Returns a span that represents the content of the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <remarks>
    /// The elements in the provided span follow the expected for a stack order (i.e., reverse order).
    /// </remarks>
    /// <returns>A span covering the content of the <see cref="ValueStack{T}"/>.</returns>
    public readonly Span<T> AsSpan()
        => _buffer.Slice(_buffer.Length - _count, _count);

    /// <summary>
    /// Returns a span that represents a segment of the stack starting from the specified index.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="start">The start index of the segment.</param>
    /// <returns>A span covering the segment of the stack.</returns>
    public readonly Span<T> AsSpan(int start) => AsSpan(start, _count - start);

    /// <summary>
    /// Returns a span that represents a segment of the stack with the specified start index and length.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index 0 returns
    /// the topmost element, and index (Count - 1) returns the bottommost element.
    /// </remarks>
    /// <param name="start">The start index of the segment.</param>
    /// <param name="length">The length of the segment.</param>
    /// <returns>A span covering the segment of the stack.</returns>
    public readonly Span<T> AsSpan(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        return _buffer.Slice(_buffer.Length - _count + start, length);
    }

    /// <summary>
    /// Returns a span that represents the remaining unused segment of the stack.
    /// </summary>
    /// <remarks>
    /// Note that if you wish to modify the remaining segment directly, you need to insert elements
    /// from its back, or, alternatively, you could insert items sequentially from the start of the span
    /// and then call <see cref="MemoryExtensions.Reverse{T}(Span{T})"/> on it.
    /// <para/>
    /// By building the stack this way, we can ensure that <see cref="AsSpan()"/> returns a span that
    /// contains elements in the expected for a stack order (i.e., in reverse order).
    /// </remarks>
    /// <returns>A span covering the unused segment of the stack.</returns>
    public readonly Span<T> AsRemainingSpan() => _buffer.Slice(0, _buffer.Length - _count);

    /// <summary>
    /// Returns a span that represents the entire capacity of the stack,
    /// including used and unused segments.
    /// </summary>
    /// <returns>A span covering the entire buffer of the stack.</returns>
    /// <inheritdoc cref="AsRemainingSpan()"/>
    public readonly Span<T> AsCapacitySpan() => _buffer;

    /// <summary>
    /// The number of elements contained in the <see cref="ValueStack{T}"/>.
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
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    public readonly ref T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_count)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            // Skip additional bound checks.
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), _buffer.Length - _count + index);
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
            Array.Copy(_rentedBuffer, _rentedBuffer.Length - _count, items, 0, items.Length);
            return items;
        }
    }

    /// <summary>
    /// Ensures that the capacity of this instance is at least the specified value.
    /// </summary>
    /// <returns>The new capacity of this stack.</returns>
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
    public readonly ref T GetPinnableReference()
        => ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), _buffer.Length - _count);

    /// <summary>
    /// Inserts an object at the top of the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to push onto the <see cref="ValueStack{T}"/>.
    /// <para/>
    /// The value can be <c>null</c> for reference types.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(T item)
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int index = buffer.Length - count - 1;

        if ((uint)index < (uint)buffer.Length)
        {
            buffer[index] = item;
            _count = count + 1;
        }
        else
        {
            GrowAndPush(item);
        }
    }

    /// <summary>
    /// Grows the internal buffer by 1 and inserts an object at the top of the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to push onto the <see cref="ValueStack{T}"/>.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndPush(T item)
    {
        Grow(1);

        _buffer[_buffer.Length - _count - 1] = item;
        _count++;
    }

    /// <summary>
    /// Reserves a span of items at the top of the current stack and
    /// returns it for direct modification.
    /// </summary>
    /// <remarks>
    /// Note that you need to insert elements into the provided span from its back, or, alternatively,
    /// you could insert items sequentially from the start of the span and then call
    /// <see cref="MemoryExtensions.Reverse{T}(Span{T})"/> on it.
    /// <para/>
    /// By building the stack this way, we can ensure that <see cref="AsSpan()"/> returns a span that
    /// contains elements in the expected for a stack order (i.e., in reverse order).
    /// </remarks>
    /// <param name="length">The length of the span to reserve.</param>
    /// <returns>A span that represents the reserved space.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> PushSpan(int length)
    {
        // Do not validate `length`.
        // Let the slow path deal with it.

        int count = _count;
        Span<T> buffer = _buffer;
        int index = buffer.Length - count - length;

        // Helps with removing bound checks during inlining.
        // The exactly same check as can be seen in the `Span.Slice()` implementation.
        if ((ulong)(uint)index + (ulong)(uint)length <= (ulong)(uint)buffer.Length)
        {
            _count = count + length;
            return buffer.Slice(index, length);
        }
        else
        {
            return GrowAndPushSpan(length);
        }
    }

    /// <summary>
    /// Grows the internal buffer, reserves a span of items at the top of the current stack,
    /// and returns it for direct modification.
    /// </summary>
    /// <param name="length">The length of the span to reserve.</param>
    /// <returns>A span that represents the reserved space.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private Span<T> GrowAndPushSpan(int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidLength(length);

        Grow(_count + length - _buffer.Length);

        int index = _buffer.Length - _count - length;
        _count += length;

        return _buffer.Slice(index, length);
    }

    /// <summary>
    /// Sequentially inserts the elements of the specified collection at the top of the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <param name="collection">
    /// The collection whose elements should be inserted at the top of the <see cref="ValueStack{T}"/>.
    /// <para/>
    /// The collection can contain elements that are <c>null</c>, if type <typeparamref name="T"/>
    /// is a reference type.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushRange(scoped ReadOnlySpan<T> collection)
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int index = buffer.Length - count - collection.Length;

        if ((ulong)(uint)_count + (ulong)(uint)collection.Length > (ulong)(uint)_buffer.Length)
        {
            Grow(collection.Length);
        }

        Span<T> slice = buffer.Slice(index, collection.Length);
        collection.CopyTo(slice);
        slice.Reverse();

        _count = count + collection.Length;
    }

    /// <inheritdoc cref="PushRange(IEnumerable{T})"/>
    public void PushRange(T[] collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        PushRange(collection.AsSpan());
    }

    /// <inheritdoc cref="PushRange(ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public void PushRange(IEnumerable<T> collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        if (collection.TryGetSpan(out ReadOnlySpan<T> span))
        {
            PushRange(span);
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
                int index = _buffer.Length - _count - count;
                Span<T> slice = _buffer.Slice(index, count);
                c.CopyTo(_rentedBuffer, index);
                slice.Reverse();
                _count += count;

                return;
            }
        }

        foreach (T item in collection)
        {
            Push(item);
        }
    }

    /// <summary>
    /// Inserts an object at the specified index of the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="index">The zero-based index of the stack to insert the element at.</param>
    /// <param name="item">
    /// The object to push onto the <see cref="ValueStack{T}"/>.
    /// <para/>
    /// The value can be <c>null</c> for reference types.
    /// </param>
    public void Push(int index, T item)
    {
        if ((uint)index > (uint)_count)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        if ((ulong)(uint)_count + 1ul > (ulong)(uint)_buffer.Length)
        {
            Grow(1);
        }

        int oldBufferStart = _buffer.Length - _count;
        int newBufferStart = oldBufferStart - 1;
        if (index != 0)
        {
            _buffer.Slice(oldBufferStart, index).CopyTo(_buffer.Slice(newBufferStart));
        }

        _buffer[newBufferStart + index] = item;
        _count += 1;
    }

    /// <summary>
    /// Sequentially inserts the elements of the specified collection at the specified index
    /// of the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="index">The zero-based index of the stack to insert the elements at.</param>
    /// <param name="collection">
    /// The collection whose elements should be inserted at the specified index
    /// of the <see cref="ValueStack{T}"/>.
    /// <para/>
    /// The collection can contain elements that are <c>null</c>, if type <typeparamref name="T"/>
    /// is a reference type.
    /// </param>
    public void PushRange(int index, scoped ReadOnlySpan<T> collection)
    {
        if ((uint)index > (uint)_count)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        int count = collection.Length;

        if ((ulong)(uint)_count + (ulong)(uint)count > (ulong)(uint)_buffer.Length)
        {
            Grow(count);
        }

        int oldBufferStart = _buffer.Length - _count;
        int newBufferStart = oldBufferStart - collection.Length;
        if (index != 0)
        {
            _buffer.Slice(oldBufferStart, index).CopyTo(_buffer.Slice(newBufferStart));
        }

        Span<T> destination = _buffer.Slice(newBufferStart + index, collection.Length);
        collection.CopyTo(destination);
        destination.Reverse();

        _count += count;
    }

    /// <inheritdoc cref="PushRange(int, IEnumerable{T})"/>
    public void PushRange(int index, T[] collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);

        PushRange(index, collection.AsSpan());
    }

    /// <inheritdoc cref="PushRange(int, ReadOnlySpan{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
    public void PushRange(int index, IEnumerable<T> collection)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(collection);
        if ((uint)index > (uint)_count)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        if (collection.TryGetSpan(out ReadOnlySpan<T> span))
        {
            PushRange(index, span);
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
                int oldBufferStart = _buffer.Length - _count;
                int newBufferStart = oldBufferStart - count;
                if (index != 0)
                {
                    _buffer.Slice(oldBufferStart, index).CopyTo(_buffer.Slice(newBufferStart));
                }

                Span<T> destination = _buffer.Slice(newBufferStart + index, count);
                c.CopyTo(_rentedBuffer, newBufferStart + index);
                destination.Reverse();

                _count += count;
                return;
            }
        }

        foreach (T item in collection)
        {
            Push(index, item);
        }
    }

    /// <summary>
    /// Returns the object at the top of the <see cref="ValueStack{T}"/> without
    /// removing it.
    /// </summary>
    /// <returns>
    /// The object at the top of the <see cref="ValueStack{T}"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="ValueStack{T}"/> is empty.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T Peek()
    {
        Span<T> buffer = _buffer;
        int index = buffer.Length - _count;

        if ((uint)index >= (uint)buffer.Length)
        {
            ThrowHelper.ThrowInvalidOperationException_EmptyStack();
        }

        return buffer[index];
    }

    /// <summary>
    /// Returns a value that indicates whether there is an object at the top of
    /// the <see cref="ValueStack{T}"/>, and if one is present, copies it to
    /// the result parameter.
    /// </summary>
    /// <remarks>
    /// The object is not removed from the <see cref="ValueStack{T}"/>.
    /// </remarks>
    /// <param name="result">
    /// If present, the object at the top of the <see cref="ValueStack{T}"/>;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if there is an object at the top of the <see cref="ValueStack{T}"/>;
    /// <c>false</c> if the <see cref="ValueStack{T}"/> is empty.
    /// </returns>
    public readonly bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        Span<T> buffer = _buffer;
        int index = buffer.Length - _count;

        if ((uint)index >= (uint)buffer.Length)
        {
            result = default;
            return false;
        }

        result = buffer[index];
        return true;
    }

    /// <summary>
    /// Returns the object at the specified position of the <see cref="ValueStack{T}"/>
    /// without removing it.
    /// </summary>
    /// <remarks>
    /// The object is not removed from the <see cref="ValueStack{T}"/>.
    /// <para/>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="index">The zero-based index of the element to peek at.</param>
    /// <returns>
    /// The object at the specified position of the <see cref="ValueStack{T}"/>.
    /// </returns>
    /// <returns>The element at the specified index.</returns>
    public readonly T Peek(int index) => this[index];

    /// <summary>
    /// Returns a value that indicates whether there is an object at the specified
    /// position of the <see cref="ValueStack{T}"/>, and if one is present, copies
    /// it to the result parameter.
    /// </summary>
    /// <remarks>
    /// The object is not removed from the <see cref="ValueStack{T}"/>.
    /// <para/>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="index">The zero-based index of the element to peek at.</param>
    /// <param name="result">
    /// If present, the object at the specified position of the <see cref="ValueStack{T}"/>;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if there is an object at the specified position of the <see cref="ValueStack{T}"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public readonly bool TryPeek(int index, [MaybeNullWhen(false)] out T result)
    {
        if ((uint)index < (uint)_count)
        {
            result = _buffer[_buffer.Length - _count + index];
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Copies the elements starting from the specified position (inclusive) of the <see cref="ValueStack{T}"/>
    /// into the provided <paramref name="result"/> span.
    /// </summary>
    /// <remarks>
    /// The elements are not removed from the <see cref="ValueStack{T}"/>.
    /// <para/>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
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
    /// <see cref="ValueStack{T}"/> into the provided <paramref name="result"/> span.
    /// </summary>
    /// <remarks>
    /// The elements are not removed from the <see cref="ValueStack{T}"/>.
    /// <para/>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="start">The zero-based index of the starting element to copy.</param>
    /// <param name="result">A span to which the elements are copied.</param>
    /// <returns>
    /// <c>true</c> if the elements are successfully copied; otherwise, <c>false</c>.
    /// </returns>
    public readonly bool TryPeek(int start, scoped Span<T> result)
    {
        Span<T> buffer = _buffer;
        start += buffer.Length - _count;

        if ((ulong)(uint)start + (ulong)(uint)result.Length > (ulong)(uint)buffer.Length)
            return false;

        buffer.Slice(start, result.Length).CopyTo(result);
        return true;
    }

    /// <summary>
    /// Removes and returns the object at the top of the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <returns>
    /// The object removed from the top of the <see cref="ValueStack{T}"/>.
    /// </returns>
    public T Pop()
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int index = buffer.Length - count;

        if ((uint)index >= (uint)buffer.Length)
        {
            ThrowHelper.ThrowInvalidOperationException_EmptyStack();
        }


        T item = buffer[index];

        // Clear the element so that the GC can reclaim its reference.
        buffer[index] = default!;

        _count = count - 1;

        return item;
    }

    /// <summary>
    /// Returns a value that indicates whether there is an object at the top of
    /// the <see cref="ValueStack{T}"/>, and if one is present, copies it to
    /// the result parameter, and removes it from the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <param name="result">
    /// If present, the object at the top of the <see cref="ValueStack{T}"/>;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if there is an object at the top of the <see cref="ValueStack{T}"/>;
    /// <c>false</c> if the <see cref="ValueStack{T}"/> is empty.
    /// </returns>
    public bool TryPop([MaybeNullWhen(false)] out T result)
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int index = buffer.Length - count;

        if ((uint)index >= (uint)buffer.Length)
        {
            result = default;
            return false;
        }

        result = buffer[index];

        // Clear the element so that the GC can reclaim its reference.
        buffer[index] = default!;

        _count = count - 1;

        return true;
    }

    /// <summary>
    /// Removes and returns the object at the specified position of the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="index">The zero-based index of the element to pop.</param>
    /// <returns>
    /// The object removed from the specified position of the <see cref="ValueStack{T}"/>.
    /// </returns>
    public T Pop(int index)
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int bufferStart = buffer.Length - count;
        int itemIndex = bufferStart + index;

        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(itemIndex, buffer.Length);

        T item = buffer[itemIndex];

        // Clear the element so that the GC can reclaim its reference.
        buffer[itemIndex] = default!;

        buffer.Slice(bufferStart, index).CopyTo(buffer.Slice(bufferStart + 1));
        _count = count - 1;

        return item;
    }

    /// <summary>
    /// Returns a value that indicates whether there is an object at the specified position
    /// of the <see cref="ValueStack{T}"/>, and if one is present, copies it to
    /// the result parameter, and removes it from the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="index">The zero-based index of the element to pop.</param>
    /// <param name="result">
    /// If present, the object at the specified position of the <see cref="ValueStack{T}"/>;
    /// otherwise, the default value of <typeparamref name="T"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if there is an object at the specified position of the <see cref="ValueStack{T}"/>;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool TryPop(int index, [MaybeNullWhen(false)] out T result)
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int bufferStart = buffer.Length - count;
        int itemIndex = bufferStart + index;

        if ((uint)itemIndex >= (uint)buffer.Length)
        {
            result = default;
            return false;
        }


        result = buffer[itemIndex];

        // Clear the element so that the GC can reclaim its reference.
        buffer[itemIndex] = default!;

        buffer.Slice(bufferStart, index).CopyTo(buffer.Slice(bufferStart + 1));
        _count = count - 1;

        return true;
    }

    /// <summary>
    /// Removes the elements starting from the specified position (inclusive) of the
    /// <see cref="ValueStack{T}"/> and copies them into the provided <paramref name="result"/> span.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="start">The zero-based index of the starting element to pop.</param>
    /// <param name="result">A span to which the elements are copied.</param>
    public void Pop(int start, scoped Span<T> result)
    {
        if (!TryPop(start, result))
            ThrowHelper.ThrowArgumentOutOfRangeException();
    }

    /// <summary>
    /// Attempts to remove the elements starting from the specified position (inclusive) of the
    /// <see cref="ValueStack{T}"/> and copy them into the provided <paramref name="result"/> span.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="start">The zero-based index of the starting element to pop.</param>
    /// <param name="result">A span to which the elements are copied.</param>
    /// <returns>
    /// <c>true</c> if the elements are successfully popped; otherwise, <c>false</c>.
    /// </returns>
    public bool TryPop(int start, scoped Span<T> result)
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int bufferStart = buffer.Length - count;
        int segmentStart = bufferStart + start;

        if ((ulong)(uint)segmentStart + (ulong)(uint)result.Length > (ulong)(uint)buffer.Length)
            return false;


        buffer.Slice(segmentStart, result.Length).CopyTo(result);

        // Clear the elements so that the GC can reclaim the references.
        _buffer.Slice(segmentStart, result.Length).Clear();

        buffer.Slice(bufferStart, start).CopyTo(buffer.Slice(bufferStart + result.Length));
        _count = count - result.Length;

        return true;
    }

    /// <summary>
    /// Removes the specified number of elements starting from the specified position (inclusive)
    /// of the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="start">The zero-based index of the starting element to pop.</param>
    /// <param name="length">The number of elements to pop.</param>
    public void Pop(int start, int length)
    {
        if (!TryPop(start, length))
            ThrowHelper.ThrowArgumentOutOfRangeException();
    }

    /// <summary>
    /// Attempts to remove the specified number of elements starting from the specified position (inclusive)
    /// of the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="start">The zero-based index of the starting element to pop.</param>
    /// <param name="length">The number of elements to pop.</param>
    /// <returns>
    /// <c>true</c> if the elements are successfully popped; otherwise, <c>false</c>.
    /// </returns>
    public bool TryPop(int start, int length)
    {
        Span<T> buffer = _buffer;
        int count = _count;
        int bufferStart = buffer.Length - count;
        int segmentStart = bufferStart + start;

        if ((ulong)(uint)segmentStart + (ulong)(uint)length > (ulong)(uint)buffer.Length)
            return false;

        // Clear the elements so that the GC can reclaim the references.
        _buffer.Slice(segmentStart, length).Clear();

        buffer.Slice(bufferStart, start).CopyTo(buffer.Slice(bufferStart + length));
        _count = count - length;

        return true;
    }

    /// <summary>
    /// Determines whether an element is in the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="ValueStack{T}"/>.
    /// <para/>
    /// The value can be <c>null</c> for reference types.
    /// </param>
    /// <returns>
    /// <c>true</c> if item is found in the <see cref="ValueStack{T}"/>; otherwise, <c>false</c>.
    /// </returns>
    public readonly bool Contains(T item) => AsSpan().Contains(item);

    /// <summary>
    /// Removes all objects from the <see cref="ValueStack{T}"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        // Clear the elements so that the GC can reclaim the references.
        _buffer.Slice(_buffer.Length - _count, _count).Clear();

        _count = 0;
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <remarks>
    /// Indexing is performed from the top of the stack to its bottom, where index <c>0</c> returns
    /// the topmost element, and index <c>Count - 1</c> returns the bottommost element.
    /// </remarks>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="length">The number of elements in the range.</param>
    /// <returns>
    /// A shallow copy of a range of elements in the source <see cref="ValueStack{T}"/>.
    /// </returns>
    public readonly ValueStack<T> Slice(int start, int length)
    {
        // This method is needed for the slicing syntax ([i..n]) to work.

        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _count);

        ValueStack<T> stack = new(length) { _count = length };

        Span<T> from = _buffer.Slice(_buffer.Length - _count + start, length);
        Span<T> to = stack._buffer.Slice(stack._buffer.Length - length, length);
        from.CopyTo(to);

        return stack;
    }

    /// <summary>
    /// Copies the elements from this <see cref="ValueStack{T}"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStack{T}"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The number of elements in the source <see cref="ValueStack{T}"/> is greater
    /// than the number of elements that the destination can contain.
    /// </exception>
    public readonly void CopyTo(scoped Span<T> destination) => AsSpan().CopyTo(destination);

    /// <inheritdoc cref="CopyTo(Span{T})"/>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public readonly void CopyTo(T[] destination) => CopyTo(0, destination, destinationStart: 0, _count);

    /// <inheritdoc cref="CopyTo(T[])"/>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStack{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    public readonly void CopyTo(T[] destination, int destinationStart) => CopyTo(0, destination, destinationStart, _count);

    /// <inheritdoc cref="CopyTo(Span{T})"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueStack{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStack{T}"/>.
    /// </param>
    /// <param name="length">
    /// The number of elements to copy.
    /// </param>
    public readonly void CopyTo(int start, scoped Span<T> destination, int length) => AsSpan(start, length).CopyTo(destination);

    /// <inheritdoc cref="CopyTo(T[], int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueStack{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStack{T}"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    public readonly void CopyTo(int start, T[] destination, int destinationStart, int length)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        CopyTo(start, destination.AsSpan(destinationStart), length);
    }

    /// <summary>
    /// Attempts to copy the elements from this <see cref="ValueStack{T}"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStack{T}"/>.
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
    /// <param name="destination">The destination of the elements copied from <see cref="ValueStack{T}"/>.</param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="written">The number of elements copied to the destination.</param>
    public readonly bool TryCopyTo(T[] destination, int destinationStart, out int written)
        => TryCopyTo(0, destination, destinationStart, _count, out written);

    /// <inheritdoc cref="TryCopyTo(Span{T}, out int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueStack{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">The destination of the elements copied from <see cref="ValueStack{T}"/>.</param>
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
    /// The zero-based starting position in the <see cref="ValueStack{T}"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStack{T}"/>.
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
    /// Copies the elements of the <see cref="ValueStack{T}"/> to a new array.
    /// </summary>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ValueStack{T}"/>.
    /// </returns>
    public readonly T[] ToArray() => AsSpan().ToArray();

    /// <summary>
    /// Copies the elements of the <see cref="ValueStack{T}"/> to a new array.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this instance, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ValueStack{T}"/>.
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
    /// Copies the elements of the <see cref="ValueStack{T}"/> to a new <see cref="Stack{T}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Stack{T}"/> containing copies of the elements of the <see cref="ValueStack{T}"/>.
    /// </returns>
    public readonly Stack<T> ToStack()
    {
        Stack<T> stack = new(_count);

        int lastIndex = _buffer.Length - _count;
        for (int i = _buffer.Length - 1; i >= lastIndex; i--)
            stack.Push(_buffer[i]);

        return stack;
    }

    /// <summary>
    /// Copies the elements of the <see cref="ValueStack{T}"/> to a new <see cref="Stack{T}"/>.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this instance, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>
    /// A <see cref="Stack{T}"/> containing copies of the elements of the <see cref="ValueStack{T}"/>.
    /// </returns>
    public Stack<T> ToStack(bool dispose)
    {
        Stack<T> stack = ToStack();

        if (dispose)
        {
            Dispose();
        }

        return stack;
    }

    /// <summary>
    /// Returns an enumerator for the <see cref="ValueStack{T}"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="Span{T}.Enumerator"/> for the <see cref="ValueStack{T}"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();

    /// <inheritdoc/>
    [Obsolete("Equals(object) on ValueStack will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly bool Equals(object? obj)
        => ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct();

    /// <inheritdoc/>
    [Obsolete("GetHashCode() on ValueStack will always throw an exception.")]
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
    /// Sets the capacity to the actual number of elements in the <see cref="ValueStack{T}"/>,
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

        Span<T> from = _buffer.Slice(_buffer.Length - _count, _count);
        Span<T> to = newRentedBuffer.AsSpan(newRentedBuffer.Length - _count, _count);
        from.CopyTo(to);

        // Clear the elements so that the GC can reclaim the references.
        from.Clear();

        _buffer = _rentedBuffer = newRentedBuffer;
        if (oldRentedBuffer is not null)
        {
            ArrayPool<T>.Shared.Return(oldRentedBuffer);
        }
    }
}
