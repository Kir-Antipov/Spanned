namespace Spanned.Text;

/// <summary>
/// Represents a mutable string-like value type for efficient string operations.
/// </summary>
[DebuggerTypeProxy(typeof(ValueStringBuilderDebugView))]
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)}}}")]
public ref partial struct ValueStringBuilder
{
    /// <summary>
    /// The buffer that is rented from an <see cref="ArrayPool{T}"/>, if one is used.
    /// </summary>
    private char[]? _rentedBuffer;

    /// <summary>
    /// The character buffer used by this instance.
    /// </summary>
    private Span<char> _buffer;

    /// <summary>
    /// The current length of the content of this instance.
    /// </summary>
    private int _length;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStringBuilder"/> struct using the provided
    /// initial buffer.
    /// </summary>
    /// <param name="buffer">The initial character buffer for this instance.</param>
    public ValueStringBuilder(Span<char> buffer)
    {
        _buffer = buffer;
        _length = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStringBuilder"/> struct using the specified
    /// capacity.
    /// </summary>
    /// <param name="capacity">The suggested starting size of this instance.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(int)"/>
    public ValueStringBuilder(int capacity)
    {
        _rentedBuffer = ArrayPool<char>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _length = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStringBuilder"/> struct with a given value.
    /// </summary>
    /// <param name="value">The initial content for this instance.</param>
    public ValueStringBuilder(scoped ReadOnlySpan<char> value)
        : this(value, value.Length)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStringBuilder"/> struct with a given value and capacity.
    /// </summary>
    /// <param name="value">The initial content for this instance.</param>
    /// <param name="capacity">The suggested starting size of this instance.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(int, int)"/>
    public ValueStringBuilder(scoped ReadOnlySpan<char> value, int capacity)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(capacity, value.Length);

        _rentedBuffer = capacity == 0 ? null : ArrayPool<char>.Shared.Rent(capacity);
        _buffer = _rentedBuffer;
        _length = value.Length;

        value.CopyTo(_buffer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueStringBuilder"/> struct with the
    /// provided substring and capacity.
    /// </summary>
    /// <param name="value">The string from which a substring will be used.</param>
    /// <param name="startIndex">The starting index of the substring.</param>
    /// <param name="length">The length of the substring.</param>
    /// <param name="capacity">The suggested starting size of this instance.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="capacity"/> is less than zero.
    /// -or-
    /// <paramref name="capacity"/> is less than the length of <paramref name="value"/>.
    /// -or-
    /// <paramref name="startIndex"/> is less than zero.
    /// -or-
    /// <paramref name="length"/> is less than zero.
    /// -or-
    /// <paramref name="startIndex"/> plus <paramref name="length"/> is not a position
    /// within <paramref name="value"/>.
    /// </exception>
    public ValueStringBuilder(string? value, int startIndex, int length, int capacity)
        : this(value.AsSpan(startIndex, length), capacity)
    {
    }

    /// <summary>
    /// Implicitly converts a <see cref="ValueStringBuilder"/> to a span.
    /// </summary>
    /// <param name="sb">The string builder to be converted.</param>
    /// <returns>A span covering the content of the <see cref="ValueStringBuilder"/>.</returns>
    public static implicit operator ReadOnlySpan<char>(ValueStringBuilder sb)
        => MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(sb._buffer), sb._length);

    /// <summary>
    /// Returns a span that represents the content of the <see cref="ValueStringBuilder"/>.
    /// </summary>
    /// <returns>A span covering the content of the <see cref="ValueStringBuilder"/>.</returns>
    public readonly Span<char> AsSpan() => MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(_buffer), _length);

    /// <summary>
    /// Returns a span that represents the content of the <see cref="ValueStringBuilder"/>,
    /// optionally ensuring a null terminator after its end.
    /// </summary>
    /// <param name="ensureNullTerminator">
    /// Indicates whether to ensure a null terminator after the end of the content.
    /// </param>
    /// <returns>A span covering the content of the <see cref="ValueStringBuilder"/>.</returns>
    public Span<char> AsSpan(bool ensureNullTerminator)
    {
        if (ensureNullTerminator)
            EnsureNullTerminator();

        return MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(_buffer), _length);
    }

    /// <summary>
    /// Returns a span that represents a segment of the content starting from the specified index.
    /// </summary>
    /// <param name="start">The start index of the segment.</param>
    /// <returns>A span covering the segment of the content.</returns>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int)"/>
    public readonly Span<char> AsSpan(int start) => AsSpan(start, _length - start);

    /// <summary>
    /// Returns a span that represents a segment of the content with the specified start index and length.
    /// </summary>
    /// <param name="start">The start index of the segment.</param>
    /// <param name="length">The length of the segment.</param>
    /// <returns>A span covering the segment of the content.</returns>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly Span<char> AsSpan(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _length);

        // Skip additional bound checks.
        return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), start), length);
    }

    /// <summary>
    /// Returns a span that represents the remaining unused segment of the buffer.
    /// </summary>
    /// <returns>A span covering the unused segment of the buffer.</returns>
    public readonly Span<char> AsRemainingSpan() => _buffer.Slice(_length);

    /// <summary>
    /// Returns a span that represents the entire capacity of this instance,
    /// including used and unused segments.
    /// </summary>
    /// <returns>A span covering the entire buffer of this instance.</returns>
    public readonly Span<char> AsCapacitySpan() => _buffer;

    /// <summary>
    /// The length of this instance.
    /// </summary>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCount(int, int)"/>
    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _length;
        set
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCount(value, _buffer.Length);

            _length = value;
        }
    }

    /// <summary>
    /// The maximum number of characters that can be contained in the memory
    /// allocated by the current instance.
    /// </summary>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(int, int)"/>
    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _buffer.Length;
        set
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(value, _length);

            if (value != _buffer.Length)
            {
                ResizeBuffer(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the character at the specified character position in this instance.
    /// </summary>
    /// <param name="index">The position of the character.</param>
    /// <returns>The Unicode character at position index.</returns>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    public readonly ref char this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((uint)index >= (uint)_length)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            // Skip additional bound checks.
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), (nint)(uint)index);
        }
    }

    /// <summary>
    /// The string representation of the current content for debugging purposes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly string DebuggerDisplay
    {
        get
        {
            // `.AsSpan().ToString()` can break the debugger.
            // Therefore, prefer working with the underlying array directly when possible.
            if (_rentedBuffer is null)
                return AsSpan().ToString();

            return new(_rentedBuffer, 0, _length);
        }
    }

    /// <summary>
    /// Ensures that there's a null terminator after the end of the content.
    /// </summary>
    public void EnsureNullTerminator()
    {
        EnsureCapacity(_length + 1);
        _buffer[_length] = '\0';
    }

    /// <summary>
    /// Ensures that the capacity of this instance is at least the specified value.
    /// </summary>
    /// <param name="capacity">The minimum capacity to ensure.</param>
    /// <returns>The new capacity of this instance.</returns>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(int)"/>
    public int EnsureCapacity(int capacity)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidCapacity(capacity);

        if ((uint)capacity > (uint)_buffer.Length)
            Grow(capacity - _length);

        return _buffer.Length;
    }

    /// <summary>
    /// Provides a reference to the first character of this instance's buffer, which can be used for pinning.
    /// </summary>
    /// <returns>A reference to the first character of the buffer.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly ref char GetPinnableReference()
        => ref MemoryMarshal.GetReference(_buffer);

    /// <summary>
    /// Provides a reference to the first character of this instance's buffer, which can be used for pinning,
    /// optionally ensuring a null terminator after the end of the content
    /// </summary>
    /// <param name="ensureNullTerminator">
    /// Indicates whether to ensure a null terminator after the end of the content.
    /// </param>
    /// <returns>A reference to the first character of the buffer.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ref char GetPinnableReference(bool ensureNullTerminator)
    {
        if (ensureNullTerminator)
            EnsureNullTerminator();

        return ref MemoryMarshal.GetReference(_buffer);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the characters this instance contains.
    /// </summary>
    /// <returns>The enumerator for this instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<char>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();

    /// <summary>
    /// Reserves a span of characters at the end of the current content and returns it for direct modification.
    /// </summary>
    /// <param name="length">The length of the span to reserve.</param>
    /// <returns>A span that represents the reserved space.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> AppendSpan(int length)
    {
        int previousLength = _length;
        Span<char> buffer = _buffer;

        // Helps with removing bound checks during inlining.
        // The exactly same check as can be seen in the `Span.Slice()` implementation.
        if ((ulong)(uint)previousLength + (ulong)(uint)length <= (ulong)(uint)buffer.Length)
        {
            _length = previousLength + length;
            return buffer.Slice(previousLength, length);
        }
        else
        {
            return GrowAndAppendSpan(length);
        }
    }

    /// <summary>
    /// Grows the internal buffer, reserves a span of characters at the end of the current content,
    /// and returns it for direct modification.
    /// </summary>
    /// <param name="length">The length of the span to reserve.</param>
    /// <returns>A span that represents the reserved space.</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private Span<char> GrowAndAppendSpan(int length)
    {
        int previousLength = _length;
        Grow(previousLength + length - _buffer.Length);
        _length += length;
        return _buffer.Slice(previousLength, length);
    }

    /// <summary>
    /// Appends the default line terminator to the end of this instance.
    /// </summary>
    public void AppendLine() => Append(Environment.NewLine);

    /// <summary>
    /// Appends a copy of the specified string followed by the default line terminator
    /// to the end of this instance.
    /// </summary>
    /// <param name="value">The string to append.</param>
    public void AppendLine(string? value)
    {
        Append(value);
        Append(Environment.NewLine);
    }

    /// <summary>
    /// Appends the string representation of a specified read-only character span
    /// followed by the default line terminator to the end of this instance.
    /// </summary>
    /// <param name="value">The string to append.</param>
    public void AppendLine(scoped ReadOnlySpan<char> value)
    {
        Append(value);
        Append(Environment.NewLine);
    }

    /// <summary>
    /// Removes the specified range of characters from this instance.
    /// </summary>
    /// <param name="start">The zero-based position in this instance where removal begins.</param>
    /// <param name="length">The number of characters to remove.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public void Remove(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _length);

        _buffer.Slice(start + length).CopyTo(_buffer.Slice(start));
        _length -= length;
    }

    /// <summary>
    /// Replaces all occurrences of a specified character in this instance with another
    /// specified character.
    /// </summary>
    /// <param name="oldChar">The character to replace.</param>
    /// <param name="newChar">The character that replaces oldChar.</param>
    public readonly void Replace(char oldChar, char newChar)
        => Replace(oldChar, newChar, 0, _length);

    /// <summary>
    /// Replaces, within a substring of this instance, all occurrences of a specified
    /// character with another specified character.
    /// </summary>
    /// <param name="oldChar">The character to replace.</param>
    /// <param name="newChar">The character that replaces oldChar.</param>
    /// <param name="start">The position in this instance where the substring begins.</param>
    /// <param name="length">The length of the substring.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly void Replace(char oldChar, char newChar, int start, int length)
    {
        Span<char> chars = AsSpan(start, length);

        int i = chars.IndexOf(oldChar);
        while (i >= 0)
        {
            chars[i] = newChar;
            chars = chars.Slice(i + 1);
            i = chars.IndexOf(oldChar);
        }
    }

    /// <summary>
    /// Replaces all occurrences of a specified string in this instance with another
    /// specified string.
    /// </summary>
    /// <param name="oldValue">The string to replace.</param>
    /// <param name="newValue">The string that replaces <paramref name="oldValue"/>, or <c>null</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="oldValue"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The length of <paramref name="oldValue"/> is zero.</exception>
    public void Replace(string oldValue, string? newValue)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(oldValue);

        Replace(oldValue.AsSpan(), newValue.AsSpan(), 0, _length);
    }

    /// <summary>
    /// Replaces, within a substring of this instance, all occurrences of a specified
    /// string with another specified string.
    /// </summary>
    /// <param name="oldValue">The string to replace.</param>
    /// <param name="newValue">The string that replaces <paramref name="oldValue"/>, or <c>null</c>.</param>
    /// <param name="start">The position in this instance where the substring begins.</param>
    /// <param name="length">The length of the substring.</param>
    /// <exception cref="ArgumentNullException"><paramref name="oldValue"/> is null.</exception>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public void Replace(string oldValue, string? newValue, int start, int length)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(oldValue);

        Replace(oldValue.AsSpan(), newValue.AsSpan(), start, length);
    }

    /// <summary>
    /// Replaces all occurrences of a specified character sequence in this instance
    /// with another specified character sequence.
    /// </summary>
    /// <param name="oldValue">The character sequence to replace.</param>
    /// <param name="newValue">The character sequence that replaces <paramref name="oldValue"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The length of <paramref name="oldValue"/> is zero.</exception>
    public void Replace(scoped ReadOnlySpan<char> oldValue, scoped ReadOnlySpan<char> newValue)
        => Replace(oldValue, newValue, 0, _length);

    /// <summary>
    /// Replaces, within a substring of this instance, all occurrences of a specified
    /// character sequence with another specified character sequence.
    /// </summary>
    /// <param name="oldValue">The character sequence to replace.</param>
    /// <param name="newValue">The character sequence that replaces <paramref name="oldValue"/>.</param>
    /// <param name="start">The position in this instance where the substring begins.</param>
    /// <param name="length">The length of the substring.</param>
    /// <exception cref="ArgumentOutOfRangeException">The length of <paramref name="oldValue"/> is zero.</exception>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public void Replace(scoped ReadOnlySpan<char> oldValue, scoped ReadOnlySpan<char> newValue, int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _length);
        if (oldValue.IsEmpty)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        int oldValueLength = oldValue.Length;
        int newValueLength = newValue.Length;
        int shift = newValueLength - oldValueLength;

        // Technically, we could use just one loop (namely, the one from the `shift > 0` case), but
        // it requires too much additional operations compared to the best scenario case (`shift == 0`),
        // so its better to keep 3 versions of the code, slightly optimized for their niche scenarios.
        if (shift < 0)
        {
            // When newValue's length is less than oldValue's length, we don't
            // need to enlarge the buffer, but we need to move existing data around.
            // However, we need to shift the existing data to the left every time we
            // encounter a new oldValue entry.
            Span<char> buffer = _buffer;
            int nextIndex = buffer.Slice(start, length).IndexOf(oldValue);
            while (nextIndex >= 0)
            {
                int currentIndex = start + nextIndex;

                buffer.Slice(currentIndex + oldValueLength).CopyTo(buffer.Slice(currentIndex + newValueLength));
                newValue.CopyTo(buffer.Slice(currentIndex));

                _length += shift;
                start = currentIndex + newValueLength;
                length -= nextIndex + newValueLength - shift;

                nextIndex = buffer.Slice(start, length).IndexOf(oldValue);
            }
        }
        else if (shift == 0)
        {
            // When olValue and newValue have the same length, there's no need
            // to change the length of the builder, or to enlarge the internal buffer.
            // All we need to do is to find all entries of oldValue and overwrite them
            // with newValue.
            Span<char> buffer = _buffer;
            int nextIndex = buffer.Slice(start, length).IndexOf(oldValue);
            while (nextIndex >= 0)
            {
                int currentIndex = start + nextIndex;

                newValue.CopyTo(buffer.Slice(currentIndex));

                start += nextIndex + newValueLength;
                length -= nextIndex + newValueLength;

                nextIndex = buffer.Slice(start, length).IndexOf(oldValue);
            }
        }
        else
        {
            // The worst case scenario: newValue's length is greater than oldValue's length.
            // We need to keep ensuring that there's enough place in the buffer for the insertion, and
            // we need to shift the existing data to the right every time we encounter a new oldValue entry.
            int nextIndex = _buffer.Slice(start, length).IndexOf(oldValue);
            while (nextIndex >= 0)
            {
                EnsureCapacity(_length + shift);

                int currentIndex = start + nextIndex;
                int remaining = _length - currentIndex;

                _buffer.Slice(currentIndex + oldValueLength, remaining).CopyTo(_buffer.Slice(currentIndex + newValueLength));
                newValue.CopyTo(_buffer.Slice(currentIndex));

                _length += shift;
                start = currentIndex + newValueLength;
                length -= nextIndex + newValueLength - shift;

                nextIndex = _buffer.Slice(start, length).IndexOf(oldValue);
            }
        }
    }

    /// <summary>
    /// Removes all characters from this instance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _length = 0;
    }

    /// <summary>
    /// Copies the elements from this <see cref="ValueStringBuilder"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStringBuilder"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// The number of elements in the source <see cref="ValueStringBuilder"/> is greater
    /// than the number of elements that the destination can contain.
    /// </exception>
    public readonly void CopyTo(scoped Span<char> destination) => AsSpan().CopyTo(destination);

    /// <inheritdoc cref="CopyTo(Span{char})"/>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public readonly void CopyTo(char[] destination) => CopyTo(0, destination, destinationStart: 0, _length);

    /// <inheritdoc cref="CopyTo(char[])"/>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStringBuilder"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    public readonly void CopyTo(char[] destination, int destinationStart) => CopyTo(0, destination, destinationStart, _length);

    /// <inheritdoc cref="CopyTo(Span{char})"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueStringBuilder"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStringBuilder"/>.
    /// </param>
    /// <param name="length">
    /// The number of elements to copy.
    /// </param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly void CopyTo(int start, scoped Span<char> destination, int length) => AsSpan(start, length).CopyTo(destination);

    /// <inheritdoc cref="CopyTo(char[], int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueStringBuilder"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStringBuilder"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly void CopyTo(int start, char[] destination, int destinationStart, int length)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        AsSpan(start, length).CopyTo(destination.AsSpan(destinationStart, length));
    }

    /// <summary>
    /// Attempts to copy the elements from this <see cref="ValueStringBuilder"/> to the provided destination.
    /// </summary>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStringBuilder"/>.
    /// </param>
    /// <param name="written">
    /// The number of elements copied to the destination.
    /// </param>
    /// <returns>
    /// <c>true</c> if the operation was successful; otherwise, <c>false</c>.
    /// </returns>
    public readonly bool TryCopyTo(scoped Span<char> destination, out int written)
    {
        if (AsSpan().TryCopyTo(destination))
        {
            written = _length;
            return true;
        }
        else
        {
            written = 0;
            return false;
        }
    }

    /// <inheritdoc cref="TryCopyTo(Span{char}, out int)"/>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
    public readonly bool TryCopyTo(char[] destination, out int written)
        => TryCopyTo(0, destination, destinationStart: 0, _length, out written);

    /// <inheritdoc cref="TryCopyTo(char[], out int)"/>
    /// <param name="destination">The destination of the elements copied from <see cref="ValueStringBuilder"/>.</param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="written">The number of elements copied to the destination.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(int, int)"/>
    public readonly bool TryCopyTo(char[] destination, int destinationStart, out int written)
        => TryCopyTo(0, destination, destinationStart, _length, out written);

    /// <inheritdoc cref="TryCopyTo(Span{char}, out int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueStringBuilder"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">The destination of the elements copied from <see cref="ValueStringBuilder"/>.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <param name="written">The number of elements copied to the destination.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly bool TryCopyTo(int start, scoped Span<char> destination, int length, out int written)
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

    /// <inheritdoc cref="TryCopyTo(char[], int, out int)"/>
    /// <param name="start">
    /// The zero-based starting position in the <see cref="ValueStringBuilder"/> from where elements will be copied.
    /// </param>
    /// <param name="destination">
    /// The destination of the elements copied from <see cref="ValueStringBuilder"/>.
    /// </param>
    /// <param name="destinationStart">The zero-based index in the destination at which copying begins.</param>
    /// <param name="length">The number of elements to copy.</param>
    /// <param name="written">The number of elements copied to the array.</param>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    public readonly bool TryCopyTo(int start, char[] destination, int destinationStart, int length, out int written)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(destination);

        return TryCopyTo(start, destination.AsSpan(destinationStart, length), length, out written);
    }

    /// <summary>
    /// Returns a string representation of a segment of the content of this instance.
    /// </summary>
    /// <param name="start">The starting index of the segment in this instance.</param>
    /// <param name="length">The length of the segment.</param>
    /// <returns>The string representation of the specified segment.</returns>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString(int start, int length)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _length);

        // Skip bound checks.
        Span<char> chars = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), start), length);

        return chars.ToString();
    }

    /// <summary>
    /// Returns a string representation of a segment of the content of this instance.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this builder, rendering it unusable.
    /// </summary>
    /// <param name="start">The starting index of the segment in this instance.</param>
    /// <param name="length">The length of the segment.</param>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>The string representation of the specified segment.</returns>
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(int start, int length, bool dispose)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(start, length, _length);

        // Skip bound checks.
        Span<char> chars = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(_buffer), start), length);
        string s = chars.ToString();

        if (dispose)
        {
            if (_rentedBuffer is not null)
            {
                ArrayPool<char>.Shared.Return(_rentedBuffer);
            }

            this = default;
        }

        return s;
    }

    /// <summary>
    /// Returns a string representation of this instance.
    /// </summary>
    /// <returns>The string representation of this instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString()
    {
        // Skip additional bound checks.
        Span<char> chars = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(_buffer), _length);

        return chars.ToString();
    }

    /// <summary>
    /// Returns a string representation of this instance.
    /// <para/>
    /// If the <paramref name="dispose"/> flag is set to <c>true</c>, it subsequently
    /// releases the internal resources of this builder, rendering it unusable.
    /// </summary>
    /// <param name="dispose">A flag indicating whether to dispose of internal resources.</param>
    /// <returns>The string representation of this instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(bool dispose)
    {
        // Skip additional bound checks.
        Span<char> chars = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(_buffer), _length);
        string s = chars.ToString();

        if (dispose)
        {
            if (_rentedBuffer is not null)
            {
                ArrayPool<char>.Shared.Return(_rentedBuffer);
            }

            this = default;
        }

        return s;
    }

    /// <summary>
    /// Returns a value indicating whether the characters in this instance are equal
    /// to the characters in a specified read-only character span.
    /// </summary>
    /// <param name="span">The character span to compare with the current instance.</param>
    /// <returns><c>true</c> if the characters in this instance and span are the same; otherwise, <c>false</c>.</returns>
    public readonly bool Equals(scoped ReadOnlySpan<char> span) => AsSpan().SequenceEqual(span);

    /// <summary>
    /// Returns a value indicating whether the characters in this instance are equal
    /// to the characters in a specified string builder.
    /// </summary>
    /// <param name="sb">The string builder to compare this instance to.</param>
    /// <returns><c>true</c> if the characters in this instance and another string builder are the same; otherwise, <c>false</c>.</returns>
    public readonly bool Equals(in ValueStringBuilder sb) => AsSpan().SequenceEqual(sb.AsSpan());

    /// <inheritdoc cref="ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct"/>
    [Obsolete("Equals(object) on ValueStringBuilder will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly bool Equals(object? obj)
        => ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct();

    /// <inheritdoc cref="ThrowHelper.ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct"/>
    [Obsolete("GetHashCode() on ValueStringBuilder will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly int GetHashCode()
        => ThrowHelper.ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct();

    /// <summary>
    /// Removes all characters from this instance and releases the buffer used by it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_rentedBuffer is not null)
        {
            ArrayPool<char>.Shared.Return(_rentedBuffer);
        }

        this = default;
    }

    /// <summary>
    /// Sets the capacity to the actual number of characters in this instance,
    /// if that number is less than a threshold value.
    /// </summary>
    public void TrimExcess()
    {
        if (CollectionHelper.ShouldTrim(_length, _buffer.Length))
        {
            Capacity = _length;
        }
    }

    /// <summary>
    /// Sets the capacity of this instance to hold up a specified number of characters
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
    /// Grows the buffer used by this instance to ensure it can accommodate more characters.
    /// </summary>
    /// <param name="minimumGrowLength">
    /// The minimum number of additional characters the buffer should be able to accommodate
    /// after growth.
    /// </param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int minimumGrowLength)
    {
        int newCapacity = CollectionHelper.CalculateNewCapacity(_length, _buffer.Length, minimumGrowLength);
        ResizeBuffer(newCapacity);
    }

    /// <summary>
    /// Resizes the buffer used by this instance to a specific capacity.
    /// </summary>
    /// <param name="capacity">The desired capacity for the buffer.</param>
    private void ResizeBuffer(int capacity)
    {
        Debug.Assert(capacity >= 0);
        Debug.Assert(capacity >= _length);
        Debug.Assert(capacity <= CollectionHelper.MaxCapacity);

        // Let the `Rent` method throw an exception if the capacity is invalid.
        char[] newRentedBuffer = ArrayPool<char>.Shared.Rent(capacity);
        char[]? oldRentedBuffer = _rentedBuffer;

        AsSpan().CopyTo(newRentedBuffer);
        _buffer = _rentedBuffer = newRentedBuffer;

        if (oldRentedBuffer is not null)
        {
            ArrayPool<char>.Shared.Return(oldRentedBuffer);
        }
    }
}
