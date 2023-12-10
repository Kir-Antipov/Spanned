namespace Spanned.Text;

public ref partial struct ValueStringBuilder
{
    /// <summary>
    /// Appends the string representation of the specified <see cref="char"/> to this instance.
    /// </summary>
    /// <param name="value">The UTF-16-encoded code unit to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char value)
    {
        int length = _length;
        Span<char> buffer = _buffer;

        if ((uint)length < (uint)buffer.Length)
        {
            buffer[length] = value;
            _length = length + 1;
        }
        else
        {
            GrowAndAppend(value);
        }
    }

    /// <summary>
    /// Grows the internal buffer by 1 and appends the string representation of
    /// the specified <see cref="char"/> to this instance.
    /// </summary>
    /// <param name="value">The UTF-16-encoded code unit to append.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char value)
    {
        Grow(1);

        _buffer[_length] = value;
        _length++;
    }

    /// <summary>
    /// Appends a specified number of copies of the string representation of a Unicode
    /// character to this instance.
    /// </summary>
    /// <param name="value">The character to append.</param>
    /// <param name="repeatCount">The number of times to append value.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatCount"/> is less than zero.</exception>
    public void Append(char value, int repeatCount)
    {
        ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidLength(repeatCount);

        if ((uint)_length + (uint)repeatCount > (uint)_buffer.Length)
        {
            Grow(repeatCount);
        }

        _buffer.Slice(_length, repeatCount).Fill(value);
        _length += repeatCount;
    }

    /// <summary>
    /// Appends a copy of the specified string to this instance.
    /// </summary>
    /// <param name="value">The string to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string? value)
    {
        if (value is null)
            return;

        int length = _length;
        Span<char> buffer = _buffer;

        // Optimize a very common case, such as appending single characters
        // (e.g., separators, percent symbols, etc.) when there is enough space in the buffer.
        if (value.Length is 1 && (uint)length < (uint)buffer.Length)
        {
            buffer[length] = value[0];
            _length = length + 1;
        }
        else
        {
            AppendString(value);
        }
    }

    /// <summary>
    /// Appends a copy of the specified string to this instance.
    /// </summary>
    /// <param name="value">The string to append.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AppendString(string value)
    {
        int length = _length;
        if ((uint)length + (uint)value.Length > (uint)_buffer.Length)
        {
            Grow(value.Length);
        }

        value.AsSpan().CopyTo(_buffer.Slice(length));
        _length = length + value.Length;
    }

    /// <summary>
    /// Appends a copy of a specified substring to this instance.
    /// </summary>
    /// <param name="value">The string that contains the substring to append.</param>
    /// <param name="start">The starting position of the substring within value.</param>
    /// <param name="length">The number of characters in value to append.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is null, and <paramref name="start"/> and <paramref name="length"/> are not zero.
    /// </exception>
    public void Append(string? value, int start, int length)
    {
        if (start is not 0 && length is not 0)
        {
            ThrowHelper.ThrowArgumentNullException_IfNull(value);
        }

        ReadOnlySpan<char> valueSpan = value.AsSpan(start, length);
        Append(valueSpan);
    }

    /// <summary>
    /// Appends the string representation of a specified string builder to this instance.
    /// </summary>
    /// <param name="value">The string builder to append.</param>
    public void Append(scoped in ValueStringBuilder value)
        => Append(value, 0, value._length);

    /// <summary>
    /// Appends a copy of a substring within a specified string builder to this instance.
    /// </summary>
    /// <param name="value">The string builder that contains the substring to append.</param>
    /// <param name="start">The starting position of the substring within value.</param>
    /// <param name="length">The number of characters in value to append.</param>
    public void Append(scoped in ValueStringBuilder value, int start, int length)
    {
        ReadOnlySpan<char> valueSpan = value.AsSpan(start, length);
        Append(valueSpan);
    }

    /// <summary>
    /// Appends an array of Unicode characters starting at a specified address to this
    /// instance.
    /// </summary>
    /// <param name="value">A pointer to an array of characters.</param>
    /// <param name="valueLength">The number of characters in the array.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is a null pointer.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="valueLength"/> is negative.</exception>
    [CLSCompliant(false)]
    public unsafe void Append(char* value, int valueLength)
    {
        if (valueLength > 0)
        {
            ThrowHelper.ThrowArgumentNullException_IfNull(value);
        }

        ReadOnlySpan<char> valueSpan = new(value, valueLength);
        Append(valueSpan);
    }

    /// <summary>
    /// Appends an array of Unicode characters starting at a specified address to this
    /// instance.
    /// </summary>
    /// <param name="value">A reference to an array of characters.</param>
    /// <param name="valueCount">The number of characters in the array.</param>
    private unsafe void Append(in char value, int valueCount)
    {
        int length = _length;
        Span<char> buffer = _buffer;

        // Optimize a very common case, such as appending single characters
        // (e.g., separators, percent symbols, etc.) when there is enough space in the buffer.
        if (valueCount is 1 && (uint)length < (uint)buffer.Length)
        {
            buffer[length] = value;
            _length = length + 1;
            return;
        }

        if ((ulong)(uint)length + (ulong)(uint)valueCount > (ulong)(uint)buffer.Length)
        {
            Grow(valueCount);
        }

        Span<char> target = _buffer.Slice(length);
        for (int i = 0; i < valueCount; i++)
            target[i] = Unsafe.Add(ref Unsafe.AsRef(in value), i);

        _length = length + valueCount;
    }

    /// <summary>
    /// Appends the string representation of a specified read-only character span to
    /// this instance.
    /// </summary>
    /// <param name="value">The read-only character span to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(scoped ReadOnlySpan<char> value)
    {
        if ((uint)_length + (uint)value.Length > (uint)_buffer.Length)
        {
            Grow(value.Length);
        }

        value.CopyTo(_buffer.Slice(_length));
        _length += value.Length;
    }

    /// <summary>
    /// Appends the string representation of the Unicode characters in a specified array
    /// to this instance.
    /// </summary>
    /// <param name="value">The array of characters to append.</param>
    public void Append(char[]? value)
    {
        if (value is null)
            return;

        if ((uint)_length + (uint)value.Length > (uint)_buffer.Length)
        {
            Grow(value.Length);
        }

        value.CopyTo(_buffer.Slice(_length));
        _length += value.Length;
    }

    /// <summary>
    /// Appends the string representation of a specified array segment of Unicode characters
    /// to this instance.
    /// </summary>
    /// <param name="value">A character array.</param>
    /// <param name="start">The starting position in value.</param>
    /// <param name="length">The number of characters to append.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is null, and <paramref name="start"/> and <paramref name="length"/> are not zero.
    /// </exception>
    public void Append(char[]? value, int start, int length)
    {
        if (start is not 0 && length is not 0)
        {
            ThrowHelper.ThrowArgumentNullException_IfNull(value);
        }

        ReadOnlySpan<char> valueSpan = value.AsSpan(start, length);
        Append(valueSpan);
    }

    /// <summary>
    /// Appends the string representation of a specified read-only character memory region
    /// to this instance.
    /// </summary>
    /// <param name="value">The read-only character memory region to append.</param>
    public void Append(ReadOnlyMemory<char> value) => Append(value.Span);

    /// <summary>
    /// Appends the string representation of a specified object to this instance.
    /// </summary>
    /// <param name="value">The object to append.</param>
    public void Append(object? value)
    {
        if (value is not null)
        {
            Append(value.ToString());
        }
    }

    /// <summary>
    /// Appends the string representation of a specified <see cref="bool"/> value to this instance.
    /// </summary>
    /// <param name="value">The <see cref="bool"/> value to append.</param>
    public void Append(bool value)
    {
        // `ToString()` on booleans does not allocate,
        // since it always returns one of 2 values.
        Append(value.ToString().AsSpan());
    }

    /// <summary>
    /// Appends the string representation of a specified 8-bit unsigned integer to this
    /// instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public void Append(byte value) => Append(value.ToString());

    /// <summary>
    /// Appends the string representation of a specified 8-bit signed integer to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [CLSCompliant(false)]
    public void Append(sbyte value) => Append(value.ToString());

    /// <summary>
    /// Appends the string representation of a specified 16-bit signed integer to this
    /// instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public void Append(short value) => Append(value.ToString());

    /// <summary>
    /// Appends the string representation of a specified 16-bit unsigned integer to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [CLSCompliant(false)]
    public void Append(ushort value) => Append(value.ToString());

    /// <summary>
    /// Appends the string representation of a specified 32-bit signed integer to this
    /// instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public void Append(int value) => Append(value.ToString());

    /// <summary>
    /// Appends the string representation of a specified 32-bit unsigned integer to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [CLSCompliant(false)]
    public void Append(uint value) => Append(value.ToString());

    /// <summary>
    /// Appends the string representation of a specified 64-bit signed integer to this
    /// instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public void Append(long value) => Append(value.ToString());

    /// <summary>
    /// Appends the string representation of a specified 64-bit unsigned integer to this
    /// </summary>
    /// <param name="value">The value to append.</param>
    [CLSCompliant(false)]
    public void Append(ulong value) => Append(value.ToString());

    /// <summary>
    /// Appends the string representation of a specified single-precision floating-point
    /// number to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public void Append(float value) => Append(value.ToString());

    /// <summary>
    /// Appends the string representation of a specified double-precision floating-point
    /// number to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public void Append(double value) => Append(value.ToString());

    /// <summary>
    /// Appends the string representation of a specified decimal number to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public void Append(decimal value) => Append(value.ToString());

    /// <summary>
    /// Appends a formatted representation of the specified value to this instance.
    /// </summary>
    /// <typeparam name="T">The type of the value to append.</typeparam>
    /// <param name="value">The value to be formatted and appended.</param>
    /// <param name="format">The format string to be used.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    public void Append<T>(T? value, string? format = null, IFormatProvider? provider = null)
    {
        // If there's a custom formatter, let it deal with the formatting.
        if (provider?.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter customFormatter)
        {
            Append(customFormatter.Format(format, value, provider));
            return;
        }

        // First, check for `IFormattable`, even though `ISpanFormattable` is preferred. The latter
        // requires the former anyway.
        //
        // For value types, it won't matter since the type checks become JIT-time constants.
        //
        // For reference types, it's more probable they implement `IFormattable` over `ISpanFormattable`.
        // So, if they don't implement either, we save an interface check over first checking for `ISpanFormattable`
        // and then for `IFormattable`, and if they only implement `IFormattable`, we come out even.
        // Only if they implement both we end up paying for an extra interface check.
        if (value is IFormattable)
        {
            Append(((IFormattable)value).ToString(format, provider));
        }
        else if (value is not null)
        {
            Append(value.ToString());
        }
    }

    /// <summary>
    /// Formats the specified value into a temporary buffer and appends it to this instance.
    /// </summary>
    /// <typeparam name="T">The type of the value to format.</typeparam>
    /// <param name="value">The value to be formatted and appended.</param>
    /// <param name="format">The format string to be used.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <remarks>
    /// This method does not support custom formatters.
    /// </remarks>
    private void FormatAndAppend<T>(T? value, string? format, IFormatProvider? provider)
    {
        if (value is IFormattable)
        {
            Append(((IFormattable)value).ToString(format, provider));
        }
        else if (value is not null)
        {
            Append(value.ToString());
        }
    }
}
