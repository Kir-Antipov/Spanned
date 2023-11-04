namespace Spanned.Collections.Generic;

#pragma warning disable CA1823, CS0169, IDE0044, IDE0051 // `_element0` is used by the compiler.

/// <summary>
/// Represents an inline array with one element.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
[InlineArray(1)]
internal struct InlineArray1<T>
{
    /// <summary>
    /// The reference to the first element.
    /// </summary>
    private T? _element0;

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineArray1{T}"/> struct with the specified elements.
    /// </summary>
    /// <inheritdoc cref="InlineArray3{T}(T, T, T)"/>
    public InlineArray1(T arg0)
    {
        this[0] = arg0;
    }
}

/// <summary>
/// Represents an inline array with two elements.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
[InlineArray(2)]
internal struct InlineArray2<T>
{
    /// <summary>
    /// The reference to the first element.
    /// </summary>
    private T? _element0;

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineArray2{T}"/> struct with the specified elements.
    /// </summary>
    /// <inheritdoc cref="InlineArray3{T}(T, T, T)"/>
    public InlineArray2(T arg0, T arg1)
    {
        this[0] = arg0;
        this[1] = arg1;
    }
}

/// <summary>
/// Represents an inline array with three elements.
/// </summary>
/// <typeparam name="T">The type of the elements in the array.</typeparam>
[InlineArray(3)]
internal struct InlineArray3<T>
{
    /// <summary>
    /// The reference to the first element.
    /// </summary>
    private T? _element0;

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineArray3{T}"/> struct with the specified elements.
    /// </summary>
    /// <param name="arg0">The first element.</param>
    /// <param name="arg1">The second element.</param>
    /// <param name="arg2">The third element.</param>
    public InlineArray3(T arg0, T arg1, T arg2)
    {
        this[0] = arg0;
        this[1] = arg1;
        this[2] = arg2;
    }
}

#pragma warning restore CA1823, CS0169, IDE0044, IDE0051 // `_element0` is used by the compiler.
