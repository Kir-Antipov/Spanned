namespace Spanned.Text;

public ref partial struct ValueStringBuilder
{
    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of a corresponding argument in a parameter array.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is null.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> is invalid.
    /// -or-
    /// The index of a format item is less than 0 (zero), or
    /// greater than or equal to the length of the <paramref name="args"/> array.
    /// </exception>
    public void AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(args);

        AppendFormatCore(provider: null, format, (ReadOnlySpan<object?>)args);
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of a corresponding argument in a parameter array.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> is invalid.
    /// -or-
    /// The index of a format item is less than 0 (zero), or
    /// greater than or equal to the length of the <paramref name="args"/> array.
    /// </exception>
    public void AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, scoped ReadOnlySpan<object?> args)
        => AppendFormatCore(provider: null, format, args);

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of a corresponding argument in a parameter array using
    /// a specified format provider.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is null.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> is invalid.
    /// -or-
    /// The index of a format item is less than 0 (zero), or
    /// greater than or equal to the length of the <paramref name="args"/> array.
    /// </exception>
    public void AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, params object?[] args)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(args);

        AppendFormatCore(provider, format, (ReadOnlySpan<object?>)args);
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of a corresponding argument in a parameter array using
    /// a specified format provider.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> is invalid.
    /// -or-
    /// The index of a format item is less than 0 (zero), or
    /// greater than or equal to the length of the <paramref name="args"/> array.
    /// </exception>
    public void AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, scoped ReadOnlySpan<object?> args)
        => AppendFormatCore(provider, format, args);

    #region dotnet/runtime/src/libraries/System.Private.CoreLib/src/System/Text/StringBuilder.cs#AppendFormatHelper
    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.

    private void AppendFormatCore(IFormatProvider? provider, string format, scoped ReadOnlySpan<object?> args)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(format);

        // Undocumented exclusive limits on the range for Argument Hole Index and Argument Hole Alignment.
        const int IndexLimit = 1_000_000; // Note:            0 <= ArgIndex < IndexLimit
        const int WidthLimit = 1_000_000; // Note:  -WidthLimit <  ArgAlign < WidthLimit

        // Query the provider (if one was supplied) for an ICustomFormatter. If there is one,
        // it needs to be used to transform all arguments.
        ICustomFormatter? cf = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));

        // Repeatedly find the next hole and process it.
        int pos = 0;
        char ch;
        while (true)
        {
            // Skip until either the end of the input or the first unescaped opening brace, whichever comes first.
            // Along the way we need to also unescape escaped closing braces.
            while (true)
            {
                // Find the next brace. If there isn't one, the remainder of the input is text to be appended, and we're done.
                if ((uint)pos >= (uint)format.Length)
                {
                    return;
                }

                ReadOnlySpan<char> remainder = format.AsSpan(pos);
                int countUntilNextBrace = remainder.IndexOfAny('{', '}');
                if (countUntilNextBrace < 0)
                {
                    Append(remainder);
                    return;
                }

                // Append the text until the brace.
                Append(remainder.Slice(0, countUntilNextBrace));
                pos += countUntilNextBrace;

                // Get the brace. It must be followed by another character, either a copy of itself in the case of being
                // escaped, or an arbitrary character that's part of the hole in the case of an opening brace.
                char brace = format[pos];
                ch = MoveNext(format, ref pos);
                if (brace == ch)
                {
                    Append(ch);
                    pos++;
                    continue;
                }

                // This wasn't an escape, so it must be an opening brace.
                if (brace != '{')
                {
                    ThrowHelper.ThrowFormatException_UnexpectedClosingBrace();
                }

                // Proceed to parse the hole.
                break;
            }

            // We're now positioned just after the opening brace of an argument hole, which consists of
            // an opening brace, an index, an optional width preceded by a comma, and an optional format
            // preceded by a colon, with arbitrary amounts of spaces throughout.
            int width = 0;
            bool leftJustify = false;
            ReadOnlySpan<char> itemFormatSpan = default; // used if itemFormat is null

            // First up is the index parameter, which is of the form:
            //     at least on digit
            //     optional any number of spaces
            // We've already read the first digit into ch.
            Debug.Assert(format[pos - 1] == '{');
            Debug.Assert(ch != '{');
            int index = ch - '0';
            if ((uint)index >= 10u)
            {
                ThrowHelper.ThrowFormatException_ExpectedAsciiDigit();
            }

            // Common case is a single digit index followed by a closing brace. If it's not a closing brace,
            // proceed to finish parsing the full hole format.
            ch = MoveNext(format, ref pos);
            if (ch != '}')
            {
                // Continue consuming optional additional digits.
                while (IsAsciiDigit(ch) && index < IndexLimit)
                {
                    index = index * 10 + ch - '0';
                    ch = MoveNext(format, ref pos);
                }

                // Consume optional whitespace.
                while (ch == ' ')
                {
                    ch = MoveNext(format, ref pos);
                }

                // Parse the optional alignment, which is of the form:
                //     comma
                //     optional any number of spaces
                //     optional -
                //     at least one digit
                //     optional any number of spaces
                if (ch == ',')
                {
                    // Consume optional whitespace.
                    do
                    {
                        ch = MoveNext(format, ref pos);
                    }
                    while (ch == ' ');

                    // Consume an optional minus sign indicating left alignment.
                    if (ch == '-')
                    {
                        leftJustify = true;
                        ch = MoveNext(format, ref pos);
                    }

                    // Parse alignment digits. The read character must be a digit.
                    width = ch - '0';
                    if ((uint)width >= 10u)
                    {
                        ThrowHelper.ThrowFormatException_ExpectedAsciiDigit();
                    }
                    ch = MoveNext(format, ref pos);
                    while (IsAsciiDigit(ch) && width < WidthLimit)
                    {
                        width = width * 10 + ch - '0';
                        ch = MoveNext(format, ref pos);
                    }

                    // Consume optional whitespace
                    while (ch == ' ')
                    {
                        ch = MoveNext(format, ref pos);
                    }
                }

                // The next character needs to either be a closing brace for the end of the hole,
                // or a colon indicating the start of the format.
                if (ch != '}')
                {
                    if (ch != ':')
                    {
                        // Unexpected character
                        ThrowHelper.ThrowFormatException_ExpectedClosingBrace();
                    }

                    // Search for the closing brace; everything in between is the format,
                    // but opening braces aren't allowed.
                    int startingPos = pos;
                    while (true)
                    {
                        ch = MoveNext(format, ref pos);

                        if (ch == '}')
                        {
                            // Argument hole closed
                            break;
                        }

                        if (ch == '{')
                        {
                            // Braces inside the argument hole are not supported
                            ThrowHelper.ThrowFormatException_ExpectedClosingBrace();
                        }
                    }

                    startingPos++;
                    itemFormatSpan = format.AsSpan(startingPos, pos - startingPos);
                }
            }

            // Construct the output for this arg hole.
            Debug.Assert(format[pos] == '}');
            pos++;
            string? s = null;
            string? itemFormat = null;

            if ((uint)index >= (uint)args.Length)
            {
                ThrowHelper.ThrowFormatException_IndexOutOfRange();
            }
            object? arg = args[index];

            if (cf != null)
            {
                if (!itemFormatSpan.IsEmpty)
                {
                    itemFormat = itemFormatSpan.ToString();
                }

                s = cf.Format(itemFormat, arg, provider);
            }

            if (s == null)
            {
                Span<char> destination = _buffer.Slice(_length);

                if (arg is IFormattable formattableArg)
                {
                    if (itemFormatSpan.Length != 0)
                    {
                        itemFormat ??= itemFormatSpan.ToString();
                    }
                    s = formattableArg.ToString(itemFormat, provider);
                }
                else
                {
                    s = arg?.ToString();
                }

                s ??= string.Empty;
            }

            // Append it to the final output of the Format String.
            if (width <= s.Length)
            {
                Append(s);
            }
            else if (leftJustify)
            {
                Append(s);
                Append(' ', width - s.Length);
            }
            else
            {
                Append(' ', width - s.Length);
                Append(s);
            }

            // Continue parsing the rest of the format string.
        }

        static bool IsAsciiDigit(char c) => (uint)(c - '0') <= 9u;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static char MoveNext(string format, ref int pos)
        {
            pos++;
            if ((uint)pos >= (uint)format.Length)
            {
                ThrowHelper.ThrowFormatException_ExpectedClosingBrace();
            }
            return format[pos];
        }
    }
    #endregion
}
