namespace Spanned.Text;

public ref partial struct ValueStringBuilder
{
    /// <summary>
    /// Concatenates and appends the members of a span, using the specified separator
    /// between each member.
    /// </summary>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <param name="separator">
    /// The string to use as a separator.
    /// <paramref name="separator"/> is included in the concatenated and
    /// appended strings only if values has more than one element.
    /// </param>
    /// <param name="values">
    /// A span that contains the objects to concatenate and append to the current
    /// instance of the string builder.
    /// </param>
    public void AppendJoin<T>(string? separator, scoped ReadOnlySpan<T> values)
    {
        separator ??= string.Empty;
        AppendJoinCore(ref MemoryMarshal.GetReference(separator.AsSpan()), separator.Length, values);
    }

    /// <summary>
    /// Concatenates and appends the members of a span, using the specified char
    /// separator between each member.
    /// </summary>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <param name="separator">
    /// The character to use as a separator.
    /// <paramref name="separator"/> is included in the concatenated and
    /// appended strings only if values has more than one element.
    /// </param>
    /// <param name="values">
    /// A span that contains the objects to concatenate and append to the current
    /// instance of the string builder.
    /// </param>
    public void AppendJoin<T>(char separator, scoped ReadOnlySpan<T> values)
    {
        AppendJoinCore(ref separator, 1, values);
    }

    /// <summary>
    /// Concatenates and appends the members of a collection, using the specified separator
    /// between each member.
    /// </summary>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <param name="separator">
    /// The string to use as a separator.
    /// <paramref name="separator"/> is included in the concatenated and
    /// appended strings only if values has more than one element.
    /// </param>
    /// <param name="values">
    /// A collection that contains the objects to concatenate and append to the current
    /// instance of the string builder.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is a <c>null</c>.</exception>
    public void AppendJoin<T>(string? separator, IEnumerable<T> values)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(values);

        separator ??= string.Empty;
        if (values.TryGetSpan(out ReadOnlySpan<T> span))
        {
            AppendJoinCore(ref MemoryMarshal.GetReference(separator.AsSpan()), separator.Length, span);
        }
        else
        {
            AppendJoinCore(ref MemoryMarshal.GetReference(separator.AsSpan()), separator.Length, values);
        }
    }

    /// <summary>
    /// Concatenates and appends the members of a collection, using the specified char
    /// separator between each member.
    /// </summary>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <param name="separator">
    /// The character to use as a separator.
    /// <paramref name="separator"/> is included in the concatenated and
    /// appended strings only if values has more than one element.
    /// </param>
    /// <param name="values">
    /// A collection that contains the objects to concatenate and append to the current
    /// instance of the string builder.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is a <c>null</c>.</exception>
    public void AppendJoin<T>(char separator, IEnumerable<T> values)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(values);

        if (values.TryGetSpan(out ReadOnlySpan<T> span))
        {
            AppendJoinCore(ref separator, 1, span);
        }
        else
        {
            AppendJoinCore(ref separator, 1, values);
        }
    }

    /// <summary>
    /// Concatenates the strings of the provided array, using the specified separator
    /// between each string, then appends the result to the current instance of the string
    /// builder.
    /// </summary>
    /// <param name="separator">
    /// The string to use as a separator.
    /// <paramref name="separator"/> is included in the joined strings
    /// only if values has more than one element.
    /// </param>
    /// <param name="values">
    /// An array that contains the strings to concatenate and append to the current instance
    /// of the string builder.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is a <c>null</c>.</exception>
    public void AppendJoin(string? separator, params string?[] values)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(values);

        separator ??= string.Empty;
        AppendJoinCore(ref MemoryMarshal.GetReference(separator.AsSpan()), separator.Length, (ReadOnlySpan<string?>)values);
    }

    /// <summary>
    /// Concatenates the strings of the provided array, using the specified char separator
    /// between each string, then appends the result to the current instance of the string
    /// builder.
    /// </summary>
    /// <param name="separator">
    /// The character to use as a separator.
    /// <paramref name="separator"/> is included in the joined strings
    /// only if values has more than one element.
    /// </param>
    /// <param name="values">
    /// An array that contains the strings to concatenate and append to the current instance
    /// of the string builder.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is a <c>null</c>.</exception>
    public void AppendJoin(char separator, params string?[] values)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(values);

        AppendJoinCore(ref separator, 1, (ReadOnlySpan<string?>)values);
    }

    /// <summary>
    /// Concatenates the string representations of the elements in the provided array
    /// of objects, using the specified separator between each member, then appends the
    /// result to the current instance of the string builder.
    /// </summary>
    /// <param name="separator">
    /// The string to use as a separator.
    /// <paramref name="separator"/> is included in the joined strings
    /// only if values has more than one element.
    /// </param>
    /// <param name="values">
    /// An array that contains the strings to concatenate and append to the current instance
    /// of the string builder.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is a <c>null</c>.</exception>
    public void AppendJoin(string? separator, params object?[] values)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(values);

        separator ??= string.Empty;
        AppendJoinCore(ref MemoryMarshal.GetReference(separator.AsSpan()), separator.Length, (ReadOnlySpan<object?>)values);
    }

    /// <summary>
    /// Concatenates the string representations of the elements in the provided array
    /// of objects, using the specified char separator between each member, then appends
    /// the result to the current instance of the string builder.
    /// </summary>
    /// <param name="separator">
    /// The character to use as a separator.
    /// <paramref name="separator"/> is included in the joined strings
    /// only if values has more than one element.
    /// </param>
    /// <param name="values">
    /// An array that contains the strings to concatenate and append to the current instance
    /// of the string builder.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="values"/> is a <c>null</c>.</exception>
    public void AppendJoin(char separator, params object?[] values)
    {
        ThrowHelper.ThrowArgumentNullException_IfNull(values);

        AppendJoinCore(ref separator, 1, (ReadOnlySpan<object?>)values);
    }

    /// <summary>
    /// Concatenates and appends the members of a collection, using the specified separator
    /// between each member.
    /// </summary>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <param name="separator">A reference to the separator.</param>
    /// <param name="separatorLength">A length of the separator.</param>
    /// <param name="values">
    /// A collection that contains the objects to concatenate and append to the current
    /// instance of the string builder.
    /// </param>
    private void AppendJoinCore<T>(ref char separator, int separatorLength, IEnumerable<T> values)
    {
        Debug.Assert(!Unsafe.IsNullRef(ref separator));
        Debug.Assert(separatorLength >= 0);
        Debug.Assert(values is not null);

        using IEnumerator<T> en = values!.GetEnumerator();
        if (!en.MoveNext())
        {
            return;
        }

        T value = en.Current;
        if (value is not null)
        {
            Append(value);
        }

        while (en.MoveNext())
        {
            value = en.Current;

            Append(in separator, separatorLength);
            if (value is not null)
            {
                Append(value);
            }
        }
    }

    /// <summary>
    /// Concatenates and appends the members of a span, using the specified separator
    /// between each member.
    /// </summary>
    /// <typeparam name="T">The type of the members of values.</typeparam>
    /// <param name="separator">A reference to the separator.</param>
    /// <param name="separatorLength">A length of the separator.</param>
    /// <param name="values">
    /// A span that contains the objects to concatenate and append to the current
    /// instance of the string builder.
    /// </param>
    private void AppendJoinCore<T>(ref char separator, int separatorLength, scoped ReadOnlySpan<T> values)
    {
        Debug.Assert(!Unsafe.IsNullRef(ref separator));
        Debug.Assert(separatorLength >= 0);

        if (values.Length == 0)
        {
            return;
        }

        if (values[0] is not null)
        {
            Append(values[0]);
        }

        for (int i = 1; i < values.Length; i++)
        {
            Append(in separator, separatorLength);
            if (values[i] is not null)
            {
                Append(values[i]);
            }
        }
    }
}
