#pragma warning disable IDE0060 // The "unused" parameters are required by the compiler.

using System.Globalization;

namespace Spanned.Text;

public ref partial struct ValueStringBuilder
{
    /// <inheritdoc cref="Append(IFormatProvider?, ref AppendInterpolatedStringHandler)"/>
    public void Append([InterpolatedStringHandlerArgument("")] ref AppendInterpolatedStringHandler handler)
    {
        // Sadly, it's not possible to pass the builder to a handler instance by its reference.
        // So, its value will be **copied**, causing de-syncs in cases when any of the operations
        // on handler causes a copied version of the builder to grow its internal buffer.
        // To prevent de-syncs, just copy all the updated fields back to this instance when the handler is done.
        this = handler._builder;
    }

    /// <summary>
    /// Appends the specified interpolated string to this instance.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="handler">The interpolated string to append.</param>
    public void Append(IFormatProvider? provider, [InterpolatedStringHandlerArgument("", nameof(provider))] ref AppendInterpolatedStringHandler handler)
    {
        // Sadly, it's not possible to pass the builder to a handler instance by its reference.
        // So, its value will be **copied**, causing de-syncs in cases when any of the operations
        // on handler causes a copied version of the builder to grow its internal buffer.
        // To prevent de-syncs, just copy all the updated fields back to this instance when the handler is done.
        this = handler._builder;
    }

    /// <inheritdoc cref="AppendLine(IFormatProvider?, ref AppendInterpolatedStringHandler)"/>
    public void AppendLine([InterpolatedStringHandlerArgument("")] ref AppendInterpolatedStringHandler handler)
    {
        // Sadly, it's not possible to pass the builder to a handler instance by its reference.
        // So, its value will be **copied**, causing de-syncs in cases when any of the operations
        // on handler causes a copied version of the builder to grow its internal buffer.
        // To prevent de-syncs, just copy all the updated fields back to this instance when the handler is done.
        this = handler._builder;

        Append(Environment.NewLine);
    }

    /// <summary>
    /// Appends the specified interpolated string followed by the default line terminator to the end of the current <see cref="ValueStringBuilder"/> instance.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="handler">The interpolated string to append.</param>
    public void AppendLine(IFormatProvider? provider, [InterpolatedStringHandlerArgument("", nameof(provider))] ref AppendInterpolatedStringHandler handler)
    {
        // Sadly, it's not possible to pass the builder to a handler instance by its reference.
        // So, its value will be **copied**, causing de-syncs in cases when any of the operations
        // on handler causes a copied version of the builder to grow its internal buffer.
        // To prevent de-syncs, just copy all the updated fields back to this instance when the handler is done.
        this = handler._builder;

        Append(Environment.NewLine);
    }

    /// <summary>
    /// A handler used by the language compiler to append interpolated strings to <see cref="ValueStringBuilder"/> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [InterpolatedStringHandler]
    public ref struct AppendInterpolatedStringHandler
    {
        /// <summary>
        /// The associated <see cref="ValueStringBuilder"/> to which to append.
        /// </summary>
        internal ValueStringBuilder _builder;

        /// <summary>
        /// The custom format provider used for the <see cref="IFormattable.ToString"/> calls.
        /// </summary>
        private readonly IFormatProvider? _provider;

        /// <summary>
        /// Indicates whether <see cref="_provider"/> provides an <see cref="ICustomFormatter"/>.
        /// </summary>
        /// <remarks>
        /// Custom formatters are very rare. Therefore, it doesn't make sense to create a reference type field
        /// for that in a ref struct, which should be as compact as possible.
        /// </remarks>
        private readonly bool _hasCustomFormatter;

        /// <inheritdoc cref="AppendInterpolatedStringHandler(int, int, ValueStringBuilder, IFormatProvider?)"/>
        public AppendInterpolatedStringHandler(int literalLength, int formattedCount, ValueStringBuilder builder)
        {
            _builder = builder;
            _provider = null;
            _hasCustomFormatter = false;
        }

        /// <summary>
        /// Initializes a new <see cref="AppendInterpolatedStringHandler"/> instance.
        /// </summary>
        /// <param name="literalLength">The number of constant characters outside of interpolation expressions in the interpolated string.</param>
        /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
        /// <param name="builder">The associated <see cref="ValueStringBuilder"/> to which to append.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <remarks>
        /// This is constructor is intended to be called only by the compiler-generated code.
        /// Therefore, arguments are not validated.
        /// </remarks>
        public AppendInterpolatedStringHandler(int literalLength, int formattedCount, ValueStringBuilder builder, IFormatProvider? provider)
        {
            _builder = builder;
            _provider = provider;
            _hasCustomFormatter = provider is not (null or CultureInfo) && provider.GetFormat(typeof(ICustomFormatter)) is not null;
        }

        /// <summary>
        /// Appends a literal string to the handler.
        /// </summary>
        /// <param name="value">The literal string value to append.</param>
        public void AppendLiteral(string value) => _builder.Append(value);

        /// <inheritdoc cref="AppendFormatted{T}(T, int, string?)"/>
        public void AppendFormatted<T>(T value)
        {
            // While any sane person would just delegate the call to another overload with a `null` format,
            // explicitly passing default as the format to TryFormat can improve code quality in certain cases when TryFormat is inlined.
            // For instance, with Int32, it enables the JIT to eliminate code in the inlined method based on a length check on the format.
            //
            // Thus, duplicating the entire method yields significant performance benefits in this scenario,
            // which outweigh the concerns regarding code maintainability.

            // If there's a custom formatter, let it deal with the formatting
            if (_hasCustomFormatter)
            {
                AppendCustomFormatted(value, alignment: 0, format: null);
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
                    if (EnumHelper.TryFormatUnconstrained(value, _builder.AsRemainingSpan(), out int charsWritten))
                    {
                        _builder._length += charsWritten;
                    }
                    else
                    {
                        FormatAndAppend(value, alignment: 0, format: null);
                    }
                }
                else if (value is ISpanFormattable)
                {
                    Span<char> destination = _builder.AsRemainingSpan();
                    if (((ISpanFormattable)value).TryFormat(destination, out int charsWritten, default, _provider))
                    {
                        // Protect against faulty ISpanFormattable implementations.
                        // We don't want to destabilize a structure that might operate on data allocated on the stack.
                        if ((uint)charsWritten > (uint)destination.Length)
                        {
                            ThrowHelper.ThrowFormatException_InvalidString();
                        }

                        _builder._length += charsWritten;
                    }
                    else
                    {
                        // ValueStringBuilder doesn't have enough space for the current value.
                        // Therefore, we first format it separately into a temporary buffer, and then append it to the builder.
                        FormatAndAppend(value, 0, format: null);
                    }
                }
                else
                {
                    _builder.Append(((IFormattable)value).ToString(format: null, _provider));
                }
            }
            else if (value is not null)
            {
                _builder.Append(value.ToString());
            }
        }

        /// <inheritdoc cref="AppendFormatted{T}(T, int, string?)"/>
        public void AppendFormatted<T>(T value, string? format)
        {
            // If there's a custom formatter, let it deal with the formatting.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatted(value, alignment: 0, format);
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
                    if (EnumHelper.TryFormatUnconstrained(value, _builder.AsRemainingSpan(), out int charsWritten, format))
                    {
                        _builder._length += charsWritten;
                    }
                    else
                    {
                        FormatAndAppend(value, alignment: 0, format);
                    }
                }
                else if (value is ISpanFormattable)
                {
                    Span<char> destination = _builder.AsRemainingSpan();
                    if (((ISpanFormattable)value).TryFormat(destination, out int charsWritten, format, _provider))
                    {
                        // Protect against faulty ISpanFormattable implementations.
                        // We don't want to destabilize a structure that might operate on data allocated on the stack.
                        if ((uint)charsWritten > (uint)destination.Length)
                        {
                            ThrowHelper.ThrowFormatException_InvalidString();
                        }

                        _builder._length += charsWritten;
                    }
                    else
                    {
                        // ValueStringBuilder doesn't have enough space for the current value.
                        // Therefore, we first format it separately into a temporary buffer, and then append it to the builder.
                        FormatAndAppend(value, 0, format);
                    }
                }
                else
                {
                    _builder.Append(((IFormattable)value).ToString(format, _provider));
                }
            }
            else if (value is not null)
            {
                _builder.Append(value.ToString());
            }
        }

        /// <inheritdoc cref="AppendFormatted{T}(T, int, string?)"/>
        public void AppendFormatted<T>(T value, int alignment)
        {
            // This overload is here just for the disambiguation purposes.
            AppendFormatted(value, alignment, format: null);
        }

        /// <summary>
        /// Appends a formatted representation of the specified value to the handler.
        /// </summary>
        /// <typeparam name="T">The type of the value to format.</typeparam>
        /// <param name="value">The value to be formatted and appended.</param>
        /// <param name="alignment">
        /// The integer value that represents the total width of the formatted string.
        /// Positive integers right-align the value and negative integers left-align it.
        /// </param>
        /// <param name="format">The format string to be used.</param>
        public void AppendFormatted<T>(T value, int alignment, string? format)
        {
            if (alignment == 0)
            {
                AppendFormatted(value, format);
            }
            else if (alignment < 0)
            {
                int start = _builder._length;
                AppendFormatted(value, format);
                int padding = -alignment - (_builder._length - start);
                if (padding > 0)
                {
                    _builder.Append(' ', padding);
                }
            }
            else
            {
                // Since the value is right-aligned, it's necessary to format it into a temporary buffer first.
                // After that, copy the formatted value into the handler, ensuring proper alignment.

                // Note: `FormatAndAppend` doesn't support custom formatting.
                // We need to call `AppendCustomFormatted` explicitly.
                if (_hasCustomFormatter)
                {
                    AppendCustomFormatted(value, alignment, format);
                }
                else
                {
                    FormatAndAppend(value, alignment, format);
                }
            }
        }

        /// <summary>
        /// Formats the specified value into a temporary buffer and appends it to the handler.
        /// </summary>
        /// <remarks>
        /// This method does not support custom formatters.
        /// </remarks>
        /// <inheritdoc cref="AppendFormatted{T}(T, int, string?)"/>
        [SkipLocalsInit] // 256 * sizeof(char) == 0.5 KiB, that's a lot to init for nothing.
        private void FormatAndAppend<T>(T value, int alignment, string? format)
        {
            Span<char> chars = stackalloc char[StringHelper.StackallocCharBufferSizeLimit];
            int charsWritten = 0;

            if (value is IFormattable)
            {
                if (typeof(T).IsEnum)
                {
                    if (EnumHelper.TryFormatUnconstrained(value, chars, out charsWritten, format))
                    {
                        AppendFormatted(chars.Slice(0, charsWritten), alignment);
                        return;
                    }
                }
                else if (value is ISpanFormattable)
                {
                    if (((ISpanFormattable)value).TryFormat(chars, out charsWritten, format, _provider))
                    {
                        AppendFormatted(chars.Slice(0, charsWritten), alignment);
                        return;
                    }
                }

                ReadOnlySpan<char> formattedValue = ((IFormattable)value).ToString(format, _provider).AsSpan();
                AppendFormatted(formattedValue, alignment);
            }
            else
            {
                ReadOnlySpan<char> stringifiedValue = value?.ToString();
                AppendFormatted(stringifiedValue, alignment);
            }
        }

        /// <inheritdoc cref="AppendFormatted{T}(T, int, string?)"/>
        public void AppendFormatted(scoped ReadOnlySpan<char> value) => _builder.Append(value);

        /// <inheritdoc cref="AppendFormatted{T}(T, int, string?)"/>
        public void AppendFormatted(scoped ReadOnlySpan<char> value, int alignment = 0, string? format = null)
        {
            // The format is meaningless for spans and nobody should really specify it.
            // If somebody does specify it, well, we just ignore it.
            _ = format;

            if (alignment == 0)
            {
                _builder.Append(value);
                return;
            }

            bool leftAlign = false;
            if (alignment < 0)
            {
                leftAlign = true;
                alignment = -alignment;
            }

            int padding = alignment - value.Length;
            if (padding <= 0)
            {
                _builder.Append(value);
            }
            else if (leftAlign)
            {
                _builder.Append(value);
                _builder.Append(' ', padding);
            }
            else
            {
                _builder.Append(' ', padding);
                _builder.Append(value);
            }
        }

        /// <inheritdoc cref="AppendFormatted{T}(T, int, string?)"/>
        public void AppendFormatted(string? value)
        {
            // If there's a custom formatter, let it deal with the formatting.
            if (_hasCustomFormatter)
            {
                AppendCustomFormatted(value, alignment: 0, format: null);
                return;
            }

            _builder.Append(value);
        }

        /// <inheritdoc cref="AppendFormatted{T}(T, int, string?)"/>
        public void AppendFormatted(string? value, int alignment = 0, string? format = null)
        {
            // The format is meaningless for strings and nobody should really specify it. This overload exists mainly
            // to disambiguate between ReadOnlySpan<char> and object overloads, especially in cases where someone
            // does specify a format, as a string is implicitly convertible to both.
            // We'll just delegate to the T-based implementation to deal with it.
            AppendFormatted<string?>(value, alignment, format);
        }

        /// <inheritdoc cref="AppendFormatted{T}(T, int, string?)"/>
        public void AppendFormatted(object? value, int alignment = 0, string? format = null)
        {
            // This overload is expected to be used very rarely, and only in the following scenarios:
            // a) When something strongly typed as an object is formatted with both an alignment and a format.
            // b) When the compiler is unable to determine the target type for T.
            // This overload is here primarily to ensure that cases from (b) can compile.
            // We'll just delegate to the T-based implementation to deal with it.
            AppendFormatted<object?>(value, alignment, format);
        }

        /// <summary>
        /// Formats the specified value using a custom formatter and appends it to the handler.
        /// </summary>
        /// <inheritdoc cref="AppendFormatted{T}(T, int, string?)"/>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AppendCustomFormatted<T>(T value, int alignment, string? format)
        {
            Debug.Assert(_hasCustomFormatter);
            Debug.Assert(_provider is not null);

            ICustomFormatter? formatter = (ICustomFormatter?)_provider.GetFormat(typeof(ICustomFormatter));
            Debug.Assert(formatter is not null, "Where did the custom formatter go?");

            string? formattedValue = formatter?.Format(format, value, _provider);
            AppendFormatted(formattedValue.AsSpan(), alignment);
        }
    }
}

#pragma warning restore IDE0060 // The "unused" parameters are required by the compiler.
