using System.Text;
using Spanned.Collections.Generic;

namespace Spanned.Text;

public ref partial struct ValueStringBuilder
{
    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.
    /// Each format item is replaced by the string representation of any of the arguments using a specified format provider.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="CompositeFormat"/>.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    public void AppendFormat<TArg0>(IFormatProvider? provider, CompositeFormat format, TArg0 arg0)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(format);

        format.ValidateNumberOfArgs(1);
        AppendFormatCore(provider, format, arg0, 0, 0, default);
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.
    /// Each format item is replaced by the string representation of any of the arguments using a specified format provider.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <typeparam name="TArg1">The type of the second object to format.</typeparam>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="CompositeFormat"/>.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    public void AppendFormat<TArg0, TArg1>(IFormatProvider? provider, CompositeFormat format, TArg0 arg0, TArg1 arg1)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(format);

        format.ValidateNumberOfArgs(2);
        AppendFormatCore(provider, format, arg0, arg1, 0, default);
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.
    /// Each format item is replaced by the string representation of any of the arguments using a specified format provider.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <typeparam name="TArg1">The type of the second object to format.</typeparam>
    /// <typeparam name="TArg2">The type of the third object to format.</typeparam>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="CompositeFormat"/>.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <param name="arg2">The third object to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    public void AppendFormat<TArg0, TArg1, TArg2>(IFormatProvider? provider, CompositeFormat format, TArg0 arg0, TArg1 arg1, TArg2 arg2)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(format);

        format.ValidateNumberOfArgs(3);
        AppendFormatCore(provider, format, arg0, arg1, arg2, default);
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of any of the arguments using a specified format provider.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="CompositeFormat"/>.</param>
    /// <param name="args">A span of objects to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is null.</exception>
    /// <exception cref="FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    public void AppendFormat(IFormatProvider? provider, CompositeFormat format, params object?[] args)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(args);

        AppendFormat(provider, format, (ReadOnlySpan<object?>)args);
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of any of the arguments using a specified format provider.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="CompositeFormat"/>.</param>
    /// <param name="args">A span of objects to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">The index of a format item is greater than or equal to the number of supplied arguments.</exception>
    public void AppendFormat(IFormatProvider? provider, CompositeFormat format, scoped ReadOnlySpan<object?> args)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(format);

        format.ValidateNumberOfArgs(args.Length);
        switch (args.Length)
        {
            case 0:
                AppendFormatCore(provider, format, (object?)null, (object?)null, (object?)null, args);
                return;

            case 1:
                AppendFormatCore(provider, format, args[0], (object?)null, (object?)null, args);
                return;

            case 2:
                AppendFormatCore(provider, format, args[0], args[1], (object?)null, args);
                return;

            default:
                AppendFormatCore(provider, format, args[0], args[1], args[2], args);
                return;
        }
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of any of the arguments using a specified format provider.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <typeparam name="TArg1">The type of the second object to format.</typeparam>
    /// <typeparam name="TArg2">The type of the third object to format.</typeparam>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="CompositeFormat"/>.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <param name="arg2">The third object to format.</param>
    /// <param name="args">A span of objects to format.</param>
    private void AppendFormatCore<TArg0, TArg1, TArg2>(IFormatProvider? provider, CompositeFormat format, TArg0 arg0, TArg1 arg1, TArg2 arg2, scoped ReadOnlySpan<object?> args)
    {
        AppendInterpolatedStringHandler handler = new(format.LiteralLength(), format.FormattedCount(), this, provider);

        foreach ((string? literal, int argIndex, int argAlignment, string? argFormat) in format.Segments())
        {
            if (literal is not null)
            {
                handler.AppendLiteral(literal);
                continue;
            }

            switch (argIndex)
            {
                case 0:
                    handler.AppendFormatted(arg0, argAlignment, argFormat);
                    break;

                case 1:
                    handler.AppendFormatted(arg1, argAlignment, argFormat);
                    break;

                case 2:
                    handler.AppendFormatted(arg2, argAlignment, argFormat);
                    break;

                default:
                    handler.AppendFormatted(args[argIndex], argAlignment, argFormat);
                    break;
            }
        }

        this = handler._builder;
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of the provided argument.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg0">The object to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> is invalid.
    /// -or-
    /// The index of a format item is less than 0 (zero), or
    /// greater than or equal to 1 (three).
    /// </exception>
    public void AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0)
    {
        InlineArray1<object?> args = new(arg0);
        AppendFormatCore(provider: null, format, args);
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of either of two arguments.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> is invalid.
    /// -or-
    /// The index of a format item is less than 0 (zero), or
    /// greater than or equal to 2 (three).
    /// </exception>
    public void AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1)
    {
        InlineArray2<object?> args = new(arg0, arg1);
        AppendFormatCore(provider: null, format, args);
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of either of three arguments.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <param name="arg2">The third object to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> is invalid.
    /// -or-
    /// The index of a format item is less than 0 (zero), or
    /// greater than or equal to 3 (three).
    /// </exception>
    public void AppendFormat([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1, object? arg2)
    {
        InlineArray3<object?> args = new(arg0, arg1, arg2);
        AppendFormatCore(provider: null, format, args);
    }

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
    /// the string representation of the given arguments using a specified format
    /// provider.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg0">The object to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> is invalid.
    /// -or-
    /// The index of a format item is less than 0 (zero), or
    /// greater than or equal to 1 (three).
    /// </exception>
    public void AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0)
    {
        InlineArray1<object?> args = new(arg0);
        AppendFormatCore(provider, format, args);
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of either of two arguments using a specified format
    /// provider.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> is invalid.
    /// -or-
    /// The index of a format item is less than 0 (zero), or
    /// greater than or equal to 2 (three).
    /// </exception>
    public void AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1)
    {
        InlineArray2<object?> args = new(arg0, arg1);
        AppendFormatCore(provider, format, args);
    }

    /// <summary>
    /// Appends the string returned by processing a composite format string, which contains
    /// zero or more format items, to this instance. Each format item is replaced by
    /// the string representation of either of three arguments using a specified format
    /// provider.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <param name="arg2">The third object to format.</param>
    /// <exception cref="ArgumentNullException"><paramref name="format"/> is null.</exception>
    /// <exception cref="FormatException">
    /// <paramref name="format"/> is invalid.
    /// -or-
    /// The index of a format item is less than 0 (zero), or
    /// greater than or equal to 3 (three).
    /// </exception>
    public void AppendFormat(IFormatProvider? provider, [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format, object? arg0, object? arg1, object? arg2)
    {
        InlineArray3<object?> args = new(arg0, arg1, arg2);
        AppendFormatCore(provider, format, args);
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
                while (char.IsAsciiDigit(ch) && index < IndexLimit)
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
                    while (char.IsAsciiDigit(ch) && width < WidthLimit)
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
                    itemFormat = new string(itemFormatSpan);
                }

                s = cf.Format(itemFormat, arg, provider);
            }

            if (s == null)
            {
                // If arg is ISpanFormattable and the beginning doesn't need padding,
                // try formatting it into the remaining current chunk.
                Span<char> destination = _buffer.Slice(_length);
                if ((leftJustify || width == 0) &&
                    arg is ISpanFormattable spanFormattableArg &&
                    spanFormattableArg.TryFormat(destination, out int charsWritten, itemFormatSpan, provider))
                {
                    if ((uint)charsWritten > (uint)destination.Length)
                    {
                        // Untrusted ISpanFormattable implementations might return an erroneous charsWritten value,
                        // and m_ChunkLength might end up being used in Unsafe code, so fail if we get back an
                        // out-of-range charsWritten value.
                        ThrowHelper.ThrowFormatException_InvalidString();
                    }

                    _length += charsWritten;

                    // Pad the end, if needed.
                    if (leftJustify && width > charsWritten)
                    {
                        Append(' ', width - charsWritten);
                    }

                    // Continue to parse other characters.
                    continue;
                }

                // Otherwise, fallback to trying IFormattable or calling ToString.
                if (arg is IFormattable formattableArg)
                {
                    if (itemFormatSpan.Length != 0)
                    {
                        itemFormat ??= new string(itemFormatSpan);
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
