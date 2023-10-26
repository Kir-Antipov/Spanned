namespace Spanned.Helpers;

/// <summary>
/// Provides helper methods to throw specific exceptions.
/// </summary>
internal static class ThrowHelper
{
    /// <summary>
    /// Throws an <see cref="OverflowException"/>.
    /// </summary>
    /// <exception cref="OverflowException"/>
    [DoesNotReturn]
    public static void ThrowOverflowException()
        => throw new OverflowException();

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [DoesNotReturn]
    public static void ThrowArgumentOutOfRangeException()
        => throw new ArgumentOutOfRangeException();

    /// <summary>
    /// Throws a <see cref="FormatException"/> indicating that the input string
    /// was not in a correct format.
    /// </summary>
    /// <exception cref="FormatException"/>
    [DoesNotReturn]
    public static void ThrowFormatException_InvalidString()
        => throw new FormatException("Input string was not in a correct format.");

    /// <summary>
    /// Throws a <see cref="FormatException"/> indicating that the provided
    /// index was out of the range of acceptable values.
    /// </summary>
    /// <exception cref="FormatException"/>
    [DoesNotReturn]
    public static void ThrowFormatException_IndexOutOfRange()
        => throw new FormatException("Index (zero based) must be greater than or equal to zero and less than the size of the argument list.");

    /// <summary>
    /// Throws a <see cref="FormatException"/> indicating that a format item ends prematurely.
    /// </summary>
    /// <exception cref="FormatException"/>
    [DoesNotReturn]
    public static void ThrowFormatException_ExpectedClosingBrace()
        => throw new FormatException("Format item ends prematurely.");

    /// <summary>
    /// Throws a <see cref="FormatException"/> indicating that an unexpected closing brace
    /// was encountered without a corresponding opening brace.
    /// </summary>
    /// <exception cref="FormatException"/>
    [DoesNotReturn]
    public static void ThrowFormatException_UnexpectedClosingBrace()
        => throw new FormatException("Unexpected closing brace without a corresponding opening brace.");

    /// <summary>
    /// Throws a <see cref="FormatException"/> indicating that an ASCII digit was expected.
    /// </summary>
    /// <exception cref="FormatException"/>
    [DoesNotReturn]
    public static void ThrowFormatException_ExpectedAsciiDigit()
        => throw new FormatException("Expected an ASCII digit.");

    /// <summary>
    /// <c>Equals(object)</c> is not supported as ref structs cannot be boxed.
    /// </summary>
    /// <returns>Calls to the <c>Equals(object)</c> method are not supported.</returns>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    [DoesNotReturn]
    public static bool ThrowNotSupportedException_CannotCallEqualsOnRefStruct()
        => throw new NotSupportedException("Equals(object) on ref structs is not supported.");

    /// <summary>
    /// <c>GetHashCode()</c> is not supported as ref structs cannot be boxed.
    /// </summary>
    /// <returns>Calls to the <c>GetHashCode()</c> method are not supported.</returns>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    [DoesNotReturn]
    public static int ThrowNotSupportedException_CannotCallGetHashCodeOnRefStruct()
        => throw new NotSupportedException("GetHashCode() on ref structs is not supported.");

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> indicating that a sequence contains no elements.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    [DoesNotReturn]
    public static void ThrowInvalidOperationException_NoElements()
        => throw new InvalidOperationException("Sequence contains no elements.");

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> indicating that the stack is empty.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    [DoesNotReturn]
    public static void ThrowInvalidOperationException_EmptyStack()
        => throw new InvalidOperationException("The stack is empty.");

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> indicating that the queue is empty.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    [DoesNotReturn]
    public static void ThrowInvalidOperationException_EmptyQueue()
        => throw new InvalidOperationException("The queue is empty.");

    /// <summary>
    /// Throws a <see cref="KeyNotFoundException"/> indicating that the given key was not present in the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the key.</typeparam>
    /// <param name="key">The key that was not found.</param>
    /// <exception cref="KeyNotFoundException"/>
    [DoesNotReturn]
    public static void ThrowKeyNotFoundException<T>(T key)
        => throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> indicating that an item with the same key has already been added to the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the key.</typeparam>
    /// <param name="key">The key that is duplicated.</param>
    /// <exception cref="ArgumentException"/>
    [DoesNotReturn]
    public static void ThrowArgumentException_DuplicateKey<T>(T key)
        => throw new ArgumentException($"An item with the same key has already been added. Key: {key}");

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is <c>null</c>.
    /// </summary>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <c>null</c>.</exception>
    public static void ThrowArgumentNullException_IfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
            ThrowArgumentNullException(paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is <c>null</c>.
    /// </summary>
    /// <param name="argument">The pointer argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <c>null</c>.</exception>
    public static unsafe void ThrowArgumentNullException_IfNull([NotNull] void* argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
            ThrowArgumentNullException(paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    [DoesNotReturn]
    public static void ThrowArgumentNullException(string? paramName) =>
        throw new ArgumentNullException(paramName);

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the capacity value is invalid.
    /// </summary>
    /// <param name="capacity">The number of elements that the collection can store.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The specified capacity value is less than zero.
    /// -or-
    /// The specified capacity value is greater than the maximum allowed capacity.
    /// </exception>
    public static void ThrowArgumentOutOfRangeException_IfInvalidCapacity(int capacity)
    {
        // While the method description states that it will throw if capacity
        // exceeds MaxCapacity, it's better to leave this validation to
        // the methods that actually consume the provided value, e.g.,
        // `ArrayPool.Rent`.
        //
        // Therefore, this method only checks if the capacity value is negative.
        if (capacity < 0)
            ThrowArgumentOutOfRangeException_NegativeCapacity();
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the capacity value is invalid.
    /// </summary>
    /// <param name="capacity">The number of elements that the collection can store.</param>
    /// <param name="count">The number of elements that the collection actually stores.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The specified capacity value is less than zero.
    /// -or-
    /// The specified capacity value is less than the actual number of elements in the collection.
    /// -or-
    /// The specified capacity value is greater than the maximum allowed capacity.
    /// </exception>
    public static void ThrowArgumentOutOfRangeException_IfInvalidCapacity(int capacity, int count)
    {
        // While the method description states that it will throw if capacity
        // exceeds MaxCapacity, it's better to leave this validation to
        // the methods that actually consume the provided value, e.g.,
        // `ArrayPool.Rent`.
        //
        // Therefore, this method only checks if the capacity value is less than the count value.
        if (capacity < count)
            ThrowArgumentOutOfRangeException_InsufficientCapacity();
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> indicating that the capacity is negative.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [DoesNotReturn]
    public static void ThrowArgumentOutOfRangeException_NegativeCapacity()
        => throw new ArgumentOutOfRangeException("capacity", "Capacity cannot be less than zero.");

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> indicating that the capacity is insufficient.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    [DoesNotReturn]
    public static void ThrowArgumentOutOfRangeException_InsufficientCapacity()
        => throw new ArgumentOutOfRangeException("capacity", "Capacity cannot be less than the current size.");

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the count value is invalid.
    /// </summary>
    /// <param name="count">The number of elements that the collection actually stores.</param>
    /// <param name="capacity">The number of elements that the collection can store.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The specified count value is less than zero.
    /// -or-
    /// The specified count value is greater than the number of elements the collection can store.
    /// </exception>
    public static void ThrowArgumentOutOfRangeException_IfInvalidCount(int count, int capacity)
    {
        if ((uint)count > (uint)capacity)
            ThrowArgumentOutOfRangeException();
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified range is invalid.
    /// </summary>
    /// <param name="start">The start index of the segment.</param>
    /// <param name="count">The number of elements that the collection actually stores.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The start index of the segment is less than zero.
    /// -or-
    /// The start index of the segment is greater than the actual number of elements in the collection.
    /// </exception>
    public static void ThrowArgumentOutOfRangeException_IfInvalidRange(int start, int count)
    {
        if ((uint)start > (uint)count)
            ThrowArgumentOutOfRangeException();
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified range is invalid.
    /// </summary>
    /// <param name="start">The start index of the segment.</param>
    /// <param name="length">The length of the segment.</param>
    /// <param name="count">The number of elements that the collection actually stores.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The start index of the segment is less than zero.
    /// -or-
    /// The length of the segment is less than zero.
    /// -or-
    /// The specified segment exceeds the actual number of elements in the collection.
    /// </exception>
    public static void ThrowArgumentOutOfRangeException_IfInvalidRange(int start, int length, int count)
    {
        if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)count)
            ThrowArgumentOutOfRangeException();
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified index is invalid.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <param name="count">The number of elements that the collection actually stores.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The index is less than zero.
    /// -or-
    /// The index exceeds the actual number of elements in the collection.
    /// </exception>
    public static void ThrowArgumentOutOfRangeException_IfInvalidIndex(int index, int count)
    {
        if ((uint)index >= (uint)count)
            ThrowArgumentOutOfRangeException();
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified length is invalid.
    /// </summary>
    /// <param name="length">The length of a collection.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The length is less than zero.
    /// </exception>
    public static void ThrowArgumentOutOfRangeException_IfInvalidLength(int length)
    {
        if (length < 0)
            ThrowArgumentOutOfRangeException();
    }
}
