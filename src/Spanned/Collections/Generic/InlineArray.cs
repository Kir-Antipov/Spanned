namespace Spanned.Collections.Generic;

#pragma warning disable CA1823, CS0169, IDE0044, IDE0051 // `_element0` is used by the compiler.

/// <summary>
/// Represents an inline array with one element.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
[StructLayout(LayoutKind.Sequential)] // Like it's gonna help...
internal struct InlineArray1<T>
{
    /// <summary>
    /// The reference to the first element.
    /// </summary>
    private T _element0;

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineArray1{T}"/> struct with the specified elements.
    /// </summary>
    /// <inheritdoc cref="InlineArray3{T}(T, T, T)"/>
    public InlineArray1(T arg0)
    {
        _element0 = arg0;
    }

    /// <summary>
    /// Implicitly converts an <see cref="InlineArray1{T}"/> to a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="inlineArray">The <see cref="InlineArray1{T}"/> to convert.</param>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the elements of the <see cref="InlineArray1{T}"/>.</returns>
    public static implicit operator ReadOnlySpan<T>(in InlineArray1<T> inlineArray)
        => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in inlineArray._element0), 1);
}

/// <summary>
/// Represents an inline array with two elements.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
[StructLayout(LayoutKind.Sequential)] // Like it's gonna help...
internal struct InlineArray2<T>
{
    /// <summary>
    /// The reference to the first element.
    /// </summary>
    private T _element0;

    /// <summary>
    /// The reference to the second element.
    /// </summary>
    private T _element1;

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineArray2{T}"/> struct with the specified elements.
    /// </summary>
    /// <inheritdoc cref="InlineArray3{T}(T, T, T)"/>
    public InlineArray2(T arg0, T arg1)
    {
        _element0 = arg0;
        _element1 = arg1;
    }

    /// <summary>
    /// Implicitly converts an <see cref="InlineArray2{T}"/> to a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="inlineArray">The <see cref="InlineArray2{T}"/> to convert.</param>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the elements of the <see cref="InlineArray2{T}"/>.</returns>
    public static implicit operator ReadOnlySpan<T>(in InlineArray2<T> inlineArray)
        => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in inlineArray._element0), 2);
}

/// <summary>
/// Represents an inline array with three elements.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
[StructLayout(LayoutKind.Sequential)] // Like it's gonna help...
internal struct InlineArray3<T>
{
    /// <summary>
    /// The reference to the first element.
    /// </summary>
    private T _element0;

    /// <summary>
    /// The reference to the second element.
    /// </summary>
    private T _element1;

    /// <summary>
    /// The reference to the third element.
    /// </summary>
    private T _element2;

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineArray3{T}"/> struct with the specified elements.
    /// </summary>
    /// <param name="arg0">The first element.</param>
    /// <param name="arg1">The second element.</param>
    /// <param name="arg2">The third element.</param>
    public InlineArray3(T arg0, T arg1, T arg2)
    {
        _element0 = arg0;
        _element1 = arg1;
        _element2 = arg2;
    }

    /// <summary>
    /// Implicitly converts an <see cref="InlineArray3{T}"/> to a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="inlineArray">The <see cref="InlineArray3{T}"/> to convert.</param>
    /// <returns>A <see cref="ReadOnlySpan{T}"/> representing the elements of the <see cref="InlineArray3{T}"/>.</returns>
    public static implicit operator ReadOnlySpan<T>(in InlineArray3<T> inlineArray)
        => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in inlineArray._element0), 3);
}

#pragma warning restore CA1823, CS0169, IDE0044, IDE0051 // `_element0` is used by the compiler.
