namespace Spanned;

/// <summary>
/// Represents the owner of a block of memory who is responsible for disposing of
/// the underlying memory appropriately.
/// </summary>
/// <typeparam name="T">The type of elements to store in a span.</typeparam>
public readonly ref struct SpanOwner<T>
{
    /// <summary>
    /// If present, the array from which the span was derived.
    /// </summary>
    /// <remarks>
    /// This is used primarily to ensure the array can be returned to the pool upon disposal.
    /// </remarks>
    private readonly T[]? _array;

    /// <summary>
    /// The span representing the current instance.
    /// </summary>
    private readonly Span<T> _span;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpanOwner{T}"/> struct using a span.
    /// </summary>
    /// <param name="span">The span to be represented by this instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SpanOwner(Span<T> span)
    {
        _span = span;
        _array = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpanOwner{T}"/> struct using a span
    /// and an array it was derived from.
    /// </summary>
    /// <param name="span">The span to be represented by this instance.</param>
    /// <param name="array">The array from which the span was derived.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal SpanOwner(Span<T> span, T[] array)
    {
        _span = span;
        _array = array;
    }

    /// <summary>
    /// Determines whether stack memory allocation is sufficient
    /// for the given array length.
    /// </summary>
    /// <param name="length">The desired length of the array.</param>
    /// <returns>
    /// <c>true</c> if stack memory allocation is sufficient for the given length;
    /// <c>false</c> if it is recommended to rent an array from the array pool.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool MayStackalloc(int length)
    {
        const int StackallocByteBufferSizeLimit = StringHelper.StackallocCharBufferSizeLimit * sizeof(char);

        return length <= StackallocByteBufferSizeLimit / Unsafe.SizeOf<T>();
    }

    /// <summary>
    /// Determines whether it is advisable to rent an array from the pool
    /// instead of using stack allocation.
    /// </summary>
    /// <param name="length">The desired length of the array.</param>
    /// <returns>
    /// <c>true</c> if it is recommended to rent an array from the array pool;
    /// <c>false</c> if stack memory allocation is sufficient for the given length.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ShouldRent(int length)
    {
        const int StackallocByteBufferSizeLimit = StringHelper.StackallocCharBufferSizeLimit * sizeof(char);

        return length > StackallocByteBufferSizeLimit / Unsafe.SizeOf<T>();
    }

    /// <summary>
    /// Rents an array from the array pool and wraps it in a <see cref="SpanOwner{T}"/>.
    /// </summary>
    /// <param name="length">The desired length of the array.</param>
    /// <returns>A <see cref="SpanOwner{T}"/> representing the rented array.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SpanOwner<T> Rent(int length)
    {
        T[] array = ArrayPool<T>.Shared.Rent(length);
        Span<T> span = array.AsSpan(0, length);
        return new(span, array);
    }

    /// <summary>
    /// Returns a <see cref="Span{T}"/> belonging to the provided <see cref="SpanOwner{T}"/>.
    /// </summary>
    /// <param name="spanOwner">The <see cref="SpanOwner{T}"/> whose <see cref="Span{T}"/> needs to be returned.</param>
    /// <returns>A <see cref="Span{T}"/> belonging to the provided <see cref="SpanOwner{T}"/>.</returns>
    public static explicit operator Span<T>(SpanOwner<T> spanOwner) => spanOwner._span;

    /// <summary>
    /// Wraps a <see cref="Span{T}"/> with a <see cref="SpanOwner{T}"/>.
    /// </summary>
    /// <param name="span">The <see cref="Span{T}"/> to be wrapped.</param>
    /// <returns>A <see cref="SpanOwner{T}"/> that represents the given span.</returns>
    public static implicit operator SpanOwner<T>(Span<T> span) => new(span);

    /// <summary>
    /// The span belonging to this owner.
    /// </summary>
    public Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _span;
    }

    /// <summary>
    /// Disposes the underlying memory reserved for this owner.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_array is not null)
        {
            // While it's possible for a user to call `Dispose` again, or
            // to continue using the array after it has been returned to the pool,
            // I don't want to add guards against this malicious behavior, since
            // it would worsen the chances of the logic of this struct being inlined,
            // and calls to this instance being completely eradicated from the JIT Asm.
            //
            // Even if such checks were added, they still wouldn't guard
            // against cases when the user already has a Span instance wrapping the
            // pooled array.
            //
            // The cons of such guards are tremendous, while the pros are questionable at best.
            // Therefore, let's leave it up to the user to behave.
            ArrayPool<T>.Shared.Return(_array);
        }
    }

    /// <inheritdoc cref="ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct"/>
    [Obsolete("Equals(object) on SpanOwner will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly bool Equals(object? obj)
        => ThrowHelper.ThrowNotSupportedException_CannotCallEqualsOnRefStruct();

    /// <inheritdoc cref="ThrowHelper.ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct"/>
    [Obsolete("GetHashCode() on SpanOwner will always throw an exception.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly int GetHashCode()
        => ThrowHelper.ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct();
}
