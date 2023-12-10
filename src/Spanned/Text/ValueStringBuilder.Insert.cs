namespace Spanned.Text;

public ref partial struct ValueStringBuilder
{
    /// <summary>
    /// Inserts the string representation of a specified Unicode character into this
    /// instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, char value)
    {
        if ((uint)index > (uint)_length)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        if ((ulong)(uint)_length + 1ul > (ulong)(uint)_buffer.Length)
        {
            Grow(1);
        }

        if (index != _length)
        {
            int remaining = _length - index;
            _buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + 1));
        }

        _buffer[index] = value;
        _length += 1;
    }

    /// <summary>
    /// Inserts a specified number of copies of the string representation of a Unicode
    /// character into this instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="repeatCount">The number of times to insert the value.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatCount"/> is less than zero.</exception>
    public void Insert(int index, char value, int repeatCount)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidLength(repeatCount);
        if ((uint)index > (uint)_length)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        if ((ulong)(uint)_length + (ulong)(uint)repeatCount > (ulong)(uint)_buffer.Length)
        {
            Grow(repeatCount);
        }

        if (index != _length)
        {
            int remaining = _length - index;
            _buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + repeatCount));
        }

        _buffer.Slice(index, repeatCount).Fill(value);
        _length += repeatCount;
    }

    /// <summary>
    /// Inserts a string into this instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, string? value)
    {
        if (value is null)
            return;

        Insert(index, value.AsSpan());
    }

    /// <summary>
    /// Inserts a string into this instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void InsertString(int index, string value)
        => Insert(index, value.AsSpan());

    /// <summary>
    /// Inserts a copy of a specified substring into this instance at
    /// the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="startIndex">The starting index within value.</param>
    /// <param name="count">The number of characters to insert.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <c>null</c>, and
    /// <paramref name="startIndex"/> and <paramref name="count"/> are not zero.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero or greater than the length of this instance.
    /// -or-
    /// <paramref name="startIndex"/> is less than zero.
    /// -or-
    /// <paramref name="count"/> is less than zero.
    /// -or-
    /// <paramref name="startIndex"/> plus <paramref name="count"/> is not a position
    /// within <paramref name="value"/>.
    /// </exception>
    public void Insert(int index, string? value, int startIndex, int count)
    {
        if (startIndex is not 0 && count is not 0)
        {
            ThrowHelper.ThrowArgumentNullException_IfNull(value);
        }

        ReadOnlySpan<char> valueSpan = value.AsSpan(startIndex, count);
        Insert(index, valueSpan);
    }

    /// <summary>
    /// Inserts one or more copies of a specified string into this instance at the specified
    /// character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="repeatCount">The number of times to insert value.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatCount"/> is less than zero.</exception>
    public void Insert(int index, string? value, int repeatCount)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidLength(repeatCount);
        if ((uint)index > (uint)_length)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        if (value is null)
            return;

        int count = value.Length * repeatCount;
        if ((ulong)(uint)_length + (ulong)(uint)count > (ulong)(uint)_buffer.Length)
        {
            Grow(count);
        }

        if (index != _length)
        {
            int remaining = _length - index;
            _buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + count));
        }

        ReadOnlySpan<char> valueSpan = value.AsSpan();
        for (int i = 0; i < repeatCount; i++)
        {
            valueSpan.CopyTo(_buffer.Slice(index + i * value.Length));
        }

        _length += count;
    }

    /// <summary>
    /// Inserts the string representation of a specified string builder into this instance
    /// at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, scoped in ValueStringBuilder value)
        => Insert(index, value, 0, value._length);

    /// <summary>
    /// Inserts a copy of a substring within a specified string builder into this instance
    /// at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="startIndex">The starting position of the substring within value.</param>
    /// <param name="count">The number of characters in value to append.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero or greater than the length of this instance.
    /// -or-
    /// <paramref name="startIndex"/> is less than zero.
    /// -or-
    /// <paramref name="count"/> is less than zero.
    /// -or-
    /// <paramref name="startIndex"/> plus <paramref name="count"/> is not a position
    /// within <paramref name="value"/>.
    /// </exception>
    public void Insert(int index, scoped in ValueStringBuilder value, int startIndex, int count)
    {
        ReadOnlySpan<char> valueSpan = value.AsSpan(startIndex, count);
        Insert(index, valueSpan);
    }

    /// <summary>
    /// Appends an array of Unicode characters starting at a specified address into
    /// this instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="valueCount">The number of characters in the array.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is a null pointer.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero or greater than the length of this instance.
    /// -or-
    /// <paramref name="valueCount"/> is negative.
    /// </exception>
    [CLSCompliant(false)]
    public unsafe void Insert(int index, char* value, int valueCount)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(value);

        ReadOnlySpan<char> valueSpan = new(value, valueCount);
        Insert(index, valueSpan);
    }

    /// <summary>
    /// Inserts the sequence of characters into this instance at the specified character
    /// position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, scoped ReadOnlySpan<char> value)
    {
        if ((uint)index > (uint)_length)
            ThrowHelper.ThrowArgumentOutOfRangeException();

        int count = value.Length;

        if ((ulong)(uint)_length + (ulong)(uint)count > (ulong)(uint)_buffer.Length)
        {
            Grow(count);
        }

        if (index != _length)
        {
            int remaining = _length - index;
            _buffer.Slice(index, remaining).CopyTo(_buffer.Slice(index + count));
        }

        value.CopyTo(_buffer.Slice(index));
        _length += count;
    }

    /// <summary>
    /// Inserts the string representation of a specified array of Unicode characters
    /// into this instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, char[]? value)
    {
        if (value is null)
            return;

        Insert(index, value.AsSpan());
    }

    /// <summary>
    /// Inserts the string representation of a specified array segment of Unicode characters
    /// into this instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="startIndex">The starting index within value.</param>
    /// <param name="count">The number of characters to insert.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <c>null</c>, and
    /// <paramref name="startIndex"/> and <paramref name="count"/> are not zero.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero or greater than the length of this instance.
    /// -or-
    /// <paramref name="startIndex"/> is less than zero.
    /// -or-
    /// <paramref name="count"/> is less than zero.
    /// -or-
    /// <paramref name="startIndex"/> plus <paramref name="count"/> is not a position
    /// within <paramref name="value"/>.
    /// </exception>
    public void Insert(int index, char[]? value, int startIndex, int count)
    {
        if (startIndex is not 0 && count is not 0)
        {
            ThrowHelper.ThrowArgumentNullException_IfNull(value);
        }

        ReadOnlySpan<char> valueSpan = value.AsSpan(startIndex, count);
        Insert(index, valueSpan);
    }

    /// <summary>
    /// Inserts the string representation of a specified read-only character memory region
    /// into this instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, ReadOnlyMemory<char> value)
        => Insert(index, value.Span);

    /// <summary>
    /// Inserts the string representation of an object into this instance at the specified
    /// character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, object? value)
    {
        if (value is not null)
        {
            Insert(index, value.ToString());
        }
    }

    /// <summary>
    /// Inserts the string representation of a Boolean value into this instance at the
    /// specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, bool value)
    {
        // `ToString()` on booleans does not allocate,
        // since it always returns one of 2 values.
        Insert(index, value.ToString().AsSpan());
    }

    /// <summary>
    /// Inserts the string representation of a specified 8-bit unsigned integer into
    /// this instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, byte value) => Insert(index, value.ToString());

    /// <summary>
    /// Inserts the string representation of a specified 8-bit signed integer into this
    /// instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    [CLSCompliant(false)]
    public void Insert(int index, sbyte value) => Insert(index, value.ToString());

    /// <summary>
    /// Inserts the string representation of a specified 16-bit signed integer into this
    /// instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, short value) => Insert(index, value.ToString());

    /// <summary>
    /// Inserts the string representation of a 16-bit unsigned integer into this instance
    /// at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    [CLSCompliant(false)]
    public void Insert(int index, ushort value) => Insert(index, value.ToString());

    /// <summary>
    /// Inserts the string representation of a specified 32-bit signed integer into this
    /// instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, int value) => Insert(index, value.ToString());

    /// <summary>
    /// Inserts the string representation of a 32-bit unsigned integer into this instance
    /// at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    [CLSCompliant(false)]
    public void Insert(int index, uint value) => Insert(index, value.ToString());

    /// <summary>
    /// Inserts the string representation of a 64-bit signed integer into this instance
    /// at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, long value) => Insert(index, value.ToString());

    /// <summary>
    /// Inserts the string representation of a 64-bit unsigned integer into this instance
    /// at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    [CLSCompliant(false)]
    public void Insert(int index, ulong value) => Insert(index, value.ToString());

    /// <summary>
    /// Inserts the string representation of a single-precision floating point number
    /// into this instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, float value) => Insert(index, value.ToString());

    /// <summary>
    /// Inserts the string representation of a double-precision floating-point number
    /// into this instance at the specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, double value) => Insert(index, value.ToString());

    /// <summary>
    /// Inserts the string representation of a decimal number into this instance at the
    /// specified character position.
    /// </summary>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    public void Insert(int index, decimal value) => Insert(index, value.ToString());

    /// <summary>
    /// Inserts a formatted representation of the specified value into this instance
    /// at the specified character position.
    /// </summary>
    /// <typeparam name="T">The type of the value to insert.</typeparam>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="format">The format string to be used.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    public void Insert<T>(int index, T? value, string? format = null, IFormatProvider? provider = null)
    {
        // If there's a custom formatter, let it deal with the formatting.
        if (provider?.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter customFormatter)
        {
            Insert(index, customFormatter.Format(format, value, provider));
            return;
        }

        if (value is IFormattable)
        {
            Insert(index, ((IFormattable)value).ToString(format, provider));
        }
        else if (value is not null)
        {
            Insert(index, value.ToString());
        }
    }

    /// <summary>
    /// Formats the specified value into a temporary buffer and inserts it into this instance
    /// at the specified character position.
    /// </summary>
    /// <typeparam name="T">The type of the value to format.</typeparam>
    /// <param name="index">The position in this instance where insertion begins.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="format">The format string to be used.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <remarks>
    /// This method does not support custom formatters.
    /// </remarks>
    private void FormatAndInsert<T>(int index, T? value, string? format, IFormatProvider? provider)
    {
        if (value is IFormattable)
        {
            Insert(index, ((IFormattable)value).ToString(format, provider));
        }
        else if (value is not null)
        {
            Insert(index, value.ToString());
        }
    }
}
