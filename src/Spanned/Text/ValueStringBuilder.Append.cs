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

        value.CopyTo(_buffer.Slice(length));
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
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
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
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
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

        MemoryMarshal.CreateReadOnlySpan(in value, valueCount).CopyTo(_buffer.Slice(length));
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
    /// <inheritdoc cref="ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidRange(int, int, int)"/>
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
        => Append<object?>(value);

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
    [SkipLocalsInit]
    public void Append(byte value)
    {
        // JIT does not inline methods that use stackalloc yet, so
        // we cannot move this logic to a shared location, sadly.
        Span<char> chars = stackalloc char[StringHelper.MaxByteStringLength];

        // Since the `byte` data type is unsigned, its formatting is not influenced by the current culture.
        // The current culture could provide a custom sign, which might be longer than one symbol,
        // but in the case of `byte`, this operation will always succeed.
        value.TryFormat(chars, out int charsWritten, format: default, provider: null);
        Append(chars.Slice(0, charsWritten));
    }

    /// <summary>
    /// Appends the string representation of a specified 8-bit signed integer to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [CLSCompliant(false)]
    [SkipLocalsInit]
    public void Append(sbyte value)
    {
        // JIT does not inline methods that use stackalloc yet, so
        // we cannot move this logic to a shared location, sadly.
        Span<char> chars = stackalloc char[StringHelper.MaxSbyteStringLength];

        if (value.TryFormat(chars, out int charsWritten, format: default, provider: null))
        {
            Append(chars.Slice(0, charsWritten));
        }
        else
        {
            // This path should never be taken. But if it is,
            // it means that there's something wrong with the current culture.
            // We still need to make it work, but we must not optimize this case,
            // since it will actually degrade the overall performance and increase
            // the assembly size for nothing.
            AppendString(value.ToString());
        }
    }

    /// <summary>
    /// Appends the string representation of a specified 16-bit signed integer to this
    /// instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [SkipLocalsInit]
    public void Append(short value)
    {
        // JIT does not inline methods that use stackalloc yet, so
        // we cannot move this logic to a shared location, sadly.
        Span<char> chars = stackalloc char[StringHelper.MaxInt16StringLength];

        if (value.TryFormat(chars, out int charsWritten, format: default, provider: null))
        {
            Append(chars.Slice(0, charsWritten));
        }
        else
        {
            // This path should never be taken. But if it is,
            // it means that there's something wrong with the current culture.
            // We still need to make it work, but we must not optimize this case,
            // since it will actually degrade the overall performance and increase
            // the assembly size for nothing.
            AppendString(value.ToString());
        }
    }

    /// <summary>
    /// Appends the string representation of a specified 16-bit unsigned integer to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [CLSCompliant(false)]
    [SkipLocalsInit]
    public void Append(ushort value)
    {
        // JIT does not inline methods that use stackalloc yet, so
        // we cannot move this logic to a shared location, sadly.
        Span<char> chars = stackalloc char[StringHelper.MaxUInt16StringLength];

        // Since the `ushort` data type is unsigned, its formatting is not influenced by the current culture.
        // The current culture could provide a custom sign, which might be longer than one symbol,
        // but in the case of `ushort`, this operation will always succeed.
        value.TryFormat(chars, out int charsWritten, format: default, provider: null);
        Append(chars.Slice(0, charsWritten));
    }

    /// <summary>
    /// Appends the string representation of a specified 32-bit signed integer to this
    /// instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [SkipLocalsInit]
    public void Append(int value)
    {
        // JIT does not inline methods that use stackalloc yet, so
        // we cannot move this logic to a shared location, sadly.
        Span<char> chars = stackalloc char[StringHelper.MaxInt32StringLength];

        if (value.TryFormat(chars, out int charsWritten, format: default, provider: null))
        {
            Append(chars.Slice(0, charsWritten));
        }
        else
        {
            // This path should never be taken. But if it is,
            // it means that there's something wrong with the current culture.
            // We still need to make it work, but we must not optimize this case,
            // since it will actually degrade the overall performance and increase
            // the assembly size for nothing.
            AppendString(value.ToString());
        }
    }

    /// <summary>
    /// Appends the string representation of a specified 32-bit unsigned integer to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [CLSCompliant(false)]
    [SkipLocalsInit]
    public void Append(uint value)
    {
        // JIT does not inline methods that use stackalloc yet, so
        // we cannot move this logic to a shared location, sadly.
        Span<char> chars = stackalloc char[StringHelper.MaxUInt32StringLength];

        // Since the `uint` data type is unsigned, its formatting is not influenced by the current culture.
        // The current culture could provide a custom sign, which might be longer than one symbol,
        // but in the case of `uint`, this operation will always succeed.
        value.TryFormat(chars, out int charsWritten, format: default, provider: null);
        Append(chars.Slice(0, charsWritten));
    }

    /// <summary>
    /// Appends the string representation of a specified 64-bit signed integer to this
    /// instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [SkipLocalsInit]
    public void Append(long value)
    {
        // JIT does not inline methods that use stackalloc yet, so
        // we cannot move this logic to a shared location, sadly.
        Span<char> chars = stackalloc char[StringHelper.MaxInt64StringLength];

        if (value.TryFormat(chars, out int charsWritten, format: default, provider: null))
        {
            Append(chars.Slice(0, charsWritten));
        }
        else
        {
            // This path should never be taken. But if it is,
            // it means that there's something wrong with the current culture.
            // We still need to make it work, but we must not optimize this case,
            // since it will actually degrade the overall performance and increase
            // the assembly size for nothing.
            AppendString(value.ToString());
        }
    }

    /// <summary>
    /// Appends the string representation of a specified 64-bit unsigned integer to this
    /// </summary>
    /// <param name="value">The value to append.</param>
    [CLSCompliant(false)]
    [SkipLocalsInit]
    public void Append(ulong value)
    {
        // JIT does not inline methods that use stackalloc yet, so
        // we cannot move this logic to a shared location, sadly.
        Span<char> chars = stackalloc char[StringHelper.MaxUInt64StringLength];

        // Since the `ulong` data type is unsigned, its formatting is not influenced by the current culture.
        // The current culture could provide a custom sign, which might be longer than one symbol,
        // but in the case of `ulong`, this operation will always succeed.
        value.TryFormat(chars, out int charsWritten, format: default, provider: null);
        Append(chars.Slice(0, charsWritten));
    }

    /// <summary>
    /// Appends the string representation of a specified single-precision floating-point
    /// number to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [SkipLocalsInit]
    public void Append(float value)
    {
        // JIT does not inline methods that use stackalloc yet, so
        // we cannot move this logic to a shared location, sadly.
        Span<char> chars = stackalloc char[StringHelper.MaxSingleStringLength];

        if (value.TryFormat(chars, out int charsWritten, format: default, provider: null))
        {
            Append(chars.Slice(0, charsWritten));
        }
        else
        {
            // This path should never be taken. But if it is,
            // it means that there's something wrong with the current culture.
            // We still need to make it work, but we must not optimize this case,
            // since it will actually degrade the overall performance and increase
            // the assembly size for nothing.
            AppendString(value.ToString());
        }
    }

    /// <summary>
    /// Appends the string representation of a specified double-precision floating-point
    /// number to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [SkipLocalsInit]
    public void Append(double value)
    {
        // JIT does not inline methods that use stackalloc yet, so
        // we cannot move this logic to a shared location, sadly.
        Span<char> chars = stackalloc char[StringHelper.MaxDoubleStringLength];

        if (value.TryFormat(chars, out int charsWritten, format: default, provider: null))
        {
            Append(chars.Slice(0, charsWritten));
        }
        else
        {
            // This path should never be taken. But if it is,
            // it means that there's something wrong with the current culture.
            // We still need to make it work, but we must not optimize this case,
            // since it will actually degrade the overall performance and increase
            // the assembly size for nothing.
            AppendString(value.ToString());
        }
    }

    /// <summary>
    /// Appends the string representation of a specified decimal number to this instance.
    /// </summary>
    /// <param name="value">The value to append.</param>
    [SkipLocalsInit]
    public void Append(decimal value)
    {
        // JIT does not inline methods that use stackalloc yet, so
        // we cannot move this logic to a shared location, sadly.
        Span<char> chars = stackalloc char[StringHelper.MaxDecimalStringLength];

        if (value.TryFormat(chars, out int charsWritten, format: default, provider: null))
        {
            Append(chars.Slice(0, charsWritten));
        }
        else
        {
            // This path should never be taken. But if it is,
            // it means that there's something wrong with the current culture.
            // We still need to make it work, but we must not optimize this case,
            // since it will actually degrade the overall performance and increase
            // the assembly size for nothing.
            AppendString(value.ToString());
        }
    }

    /// <summary>
    /// Appends the string representation of a specified object to this instance.
    /// </summary>
    /// <typeparam name="T">The type of the value to append.</typeparam>
    /// <param name="value">The object to append.</param>
    public void Append<T>(T? value)
    {
        if (typeof(T).IsEnum)
        {
            // Stephen Toub, https://github.com/dotnet/runtime/issues/57881#issuecomment-903248743:
            // These were added as instance TryFormat methods, implementing the ISpanFormattable interface.
            // Enum could do that as well, but without assistance from the JIT it'll incur some of the same overheads
            // you're seeing in your STR_Parse_text benchmark: those allocations are for boxing the enum
            // in order to call ToString (for the known enum values, ToString itself won't allocate).
            //
            // Stephen Toub, https://github.com/dotnet/runtime/pull/78580#issue-1455927685:
            // And then that Enum.TryFormat is used in multiple interpolated string handlers to avoid boxing the enum;
            // today it'll be boxed as part of using its IFormattable implementation, and with this change,
            // it both won’t be boxed and will call through the optimized generic path. This uses typeof(T).IsEnum,
            // which is now a JIT intrinsic that'll become a const true/false.
            //
            // Basically, we special-case enums, even though they [explicitly] implement ISpanFormattable, to avoid boxing.
            // Sadly, we cannot use `Enum.TryFormat` directly since there's currently no way to "bridge" generic constraints.
            // Thankfully, we now have the `UnsafeAccessorAttribute`, so we can call its unconstrained internal version.
            if (EnumHelper.TryFormatUnconstrained(value, _buffer.Slice(_length), out int charsWritten))
            {
                _length += charsWritten;
            }
            else
            {
                FormatAndAppend(value, format: null, provider: null);
            }
        }
        else if (value is ISpanFormattable)
        {
            Span<char> destination = _buffer.Slice(_length);
            if (((ISpanFormattable)value).TryFormat(destination, out int charsWritten, format: default, provider: null))
            {
                // Protect against faulty ISpanFormattable implementations.
                // We don't want to destabilize a structure that might operate on data allocated on the stack.
                if ((uint)charsWritten > (uint)destination.Length)
                {
                    ThrowHelper.ThrowFormatException_InvalidString();
                }

                _length += charsWritten;
            }
            else
            {
                // ValueStringBuilder doesn't have enough space for the current value.
                // Therefore, we first format it separately into a temporary buffer, and then append it to the builder.
                FormatAndAppend(value, format: null, provider: null);
            }
        }
        else if (value is not null)
        {
            Append(value.ToString());
        }
    }

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
            if (typeof(T).IsEnum)
            {
                // Stephen Toub, https://github.com/dotnet/runtime/issues/57881#issuecomment-903248743:
                // These were added as instance TryFormat methods, implementing the ISpanFormattable interface.
                // Enum could do that as well, but without assistance from the JIT it'll incur some of the same overheads
                // you're seeing in your STR_Parse_text benchmark: those allocations are for boxing the enum
                // in order to call ToString (for the known enum values, ToString itself won't allocate).
                //
                // Stephen Toub, https://github.com/dotnet/runtime/pull/78580#issue-1455927685:
                // And then that Enum.TryFormat is used in multiple interpolated string handlers to avoid boxing the enum;
                // today it'll be boxed as part of using its IFormattable implementation, and with this change,
                // it both won’t be boxed and will call through the optimized generic path. This uses typeof(T).IsEnum,
                // which is now a JIT intrinsic that'll become a const true/false.
                //
                // Basically, we special-case enums, even though they [explicitly] implement ISpanFormattable, to avoid boxing.
                // Sadly, we cannot use `Enum.TryFormat` directly since there's currently no way to "bridge" generic constraints.
                // Thankfully, we now have the `UnsafeAccessorAttribute`, so we can call its unconstrained internal version.
                if (EnumHelper.TryFormatUnconstrained(value, _buffer.Slice(_length), out int charsWritten, format))
                {
                    _length += charsWritten;
                }
                else
                {
                    FormatAndAppend(value, format, provider);
                }
            }
            else if (value is ISpanFormattable)
            {
                Span<char> destination = _buffer.Slice(_length);
                if (((ISpanFormattable)value).TryFormat(destination, out int charsWritten, format, provider))
                {
                    // Protect against faulty ISpanFormattable implementations.
                    // We don't want to destabilize a structure that might operate on data allocated on the stack.
                    if ((uint)charsWritten > (uint)destination.Length)
                    {
                        ThrowHelper.ThrowFormatException_InvalidString();
                    }

                    _length += charsWritten;
                }
                else
                {
                    // ValueStringBuilder doesn't have enough space for the current value.
                    // Therefore, we first format it separately into a temporary buffer, and then append it to the builder.
                    FormatAndAppend(value, format, provider);
                }
            }
            else
            {
                Append(((IFormattable)value).ToString(format, provider));
            }
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
    [SkipLocalsInit] // 256 * sizeof(char) == 0.5 KiB, that's a lot to init for nothing.
    private void FormatAndAppend<T>(T? value, string? format, IFormatProvider? provider)
    {
        Span<char> chars = stackalloc char[StringHelper.StackallocCharBufferSizeLimit];
        int charsWritten = 0;

        if (value is IFormattable)
        {
            if (typeof(T).IsEnum)
            {
                if (EnumHelper.TryFormatUnconstrained(value, chars, out charsWritten, format))
                {
                    Append(chars.Slice(0, charsWritten));
                    return;
                }
            }
            else if (value is ISpanFormattable)
            {
                if (((ISpanFormattable)value).TryFormat(chars, out charsWritten, format, provider))
                {
                    Append(chars.Slice(0, charsWritten));
                    return;
                }
            }

            Append(((IFormattable)value).ToString(format, provider));
        }
        else if (value is not null)
        {
            Append(value.ToString());
        }
    }
}
