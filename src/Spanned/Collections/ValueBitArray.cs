namespace Spanned.Collections;

/// <summary>
/// Represents an array of bits.
/// </summary>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
internal readonly ref struct ValueBitArray
{
    /// <summary>
    /// The number of bits in an <see cref="int"/>.
    /// </summary>
    private const int BitsPerInt32 = sizeof(int) * 8;

    /// <summary>
    /// The reference to the start of the <see cref="ValueBitArray"/>.
    /// </summary>
    private readonly Span<int> _ints;

    /// <summary>
    /// The number of bits contained in the <see cref="ValueBitArray"/>.
    /// </summary>
    private readonly int _length;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueBitArray"/> struct.
    /// </summary>
    /// <param name="bytes">The span of bytes to create a bit array from.</param>
    public ValueBitArray(Span<byte> bytes)
        : this(MemoryMarshal.Cast<byte, int>(bytes))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueBitArray"/> struct.
    /// </summary>
    /// <param name="ints">The span of <see cref="int"/>s to create a bit array from.</param>
    public ValueBitArray(Span<int> ints)
    {
        _ints = ints;
        _length = checked(ints.Length * BitsPerInt32);
    }

    /// <summary>
    /// Implicitly converts a span of bytes to a <see cref="ValueBitArray"/>.
    /// </summary>
    /// <param name="bytes">The span of bytes to convert.</param>
    public static implicit operator ValueBitArray(Span<byte> bytes) => new(bytes);

    /// <summary>
    /// Implicitly converts a span of bytes to a <see cref="ValueBitArray"/>.
    /// </summary>
    /// <param name="ints">The span of <see cref="int"/>s to convert.</param>
    public static implicit operator ValueBitArray(Span<int> ints) => new(ints);

    /// <summary>
    /// Returns the number of bytes required to represent the specified number of bits.
    /// </summary>
    /// <param name="bitCount">The number of bits to represent.</param>
    /// <returns>The number of bytes required.</returns>
    public static int GetByteCount(int bitCount)
        => GetInt32Count(bitCount) * sizeof(int);

    /// <summary>
    /// Returns the number of <see cref="int"/>s required to represent the specified number of bits.
    /// </summary>
    /// <param name="bitCount">The number of bits to represent.</param>
    /// <returns>The number of <see cref="int"/>s required.</returns>
    public static int GetInt32Count(int bitCount)
        => bitCount > 0 ? ((bitCount - 1) / BitsPerInt32 + 1) : 0;

    /// <summary>
    /// The string representation of the current instance for debugging purposes.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString();

    /// <summary>
    /// The number of bits contained in the <see cref="ValueBitArray"/>.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Gets or sets the value of the bit at the specified index.
    /// </summary>
    /// <param name="index">The index of the bit to access.</param>
    public bool this[int index]
    {
        set
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_IfInvalidIndex(index, _length);

            uint intIndex = (uint)index / BitsPerInt32;
            uint bitIndex = (uint)index % BitsPerInt32;
            int bitMask = 1 << (int)bitIndex;

            ref int intValue = ref Unsafe.Add(ref MemoryMarshal.GetReference(_ints), (nint)intIndex);
            intValue = value ? (intValue | bitMask) : (intValue & ~bitMask);
        }

        get
        {
            if ((uint)index >= (uint)_length)
                return false;

            uint intIndex = (uint)index / BitsPerInt32;
            uint bitIndex = (uint)index % BitsPerInt32;
            int bitMask = 1 << (int)bitIndex;

            return (Unsafe.Add(ref MemoryMarshal.GetReference(_ints), (nint)intIndex) & bitMask) != 0;
        }
    }

    /// <summary>
    /// Clears all the bits in the bit array.
    /// </summary>
    public void Clear() => _ints.Clear();

    /// <summary>
    /// Converts this instance to a boolean array.
    /// </summary>
    /// <returns>An array of boolean values representing the bits in the <see cref="ValueBitArray"/>.</returns>
    public bool[] ToArray()
    {
        bool[] array = new bool[_length];
        for (int i = 0; i < _length; i++)
            array[i] = this[i];

        return array;
    }

    /// <summary>
    /// Converts the bit array to its binary string representation.
    /// </summary>
    /// <returns>A string representing the bits in the bit array as '0's and '1's.</returns>
    public override string ToString()
    {
        Span<char> chars = _length <= StringHelper.StackallocCharBufferSizeLimit
            ? stackalloc char[_length]
            : new char[_length];

        for (int i = 0; i < _length; i++)
            chars[i] = this[i] ? '1' : '0';

        return chars.ToString();
    }

    /// <summary>
    /// Returns an enumerator to iterate over the bits in the <see cref="ValueBitArray"/>.
    /// </summary>
    /// <returns>
    /// An enumerator to iterate over the bits in the <see cref="ValueBitArray"/>.
    /// </returns>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>
    /// Enumerates the bits of a <see cref="ValueBitArray"/>.
    /// </summary>
    public ref struct Enumerator
    {
        /// <summary>
        /// The <see cref="ValueBitArray"/> to enumerate.
        /// </summary>
        private readonly ValueBitArray _bitArray;

        /// <summary>
        /// The next index to yield.
        /// </summary>
        private int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumerator"/> struct.
        /// </summary>
        /// <param name="bitArray">The <see cref="ValueBitArray"/> to enumerate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ValueBitArray bitArray)
        {
            _bitArray = bitArray;
            _index = -1;
        }

        /// <summary>
        /// Advances the enumerator to the next bit of the <see cref="ValueBitArray"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the operation was successful; otherwise, <c>false</c>;
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            int index = _index + 1;
            if (index < _bitArray._length)
            {
                _index = index;
                return true;
            }

            return false;
        }

        /// <summary>
        /// The bit at the current position of the enumerator.
        /// </summary>
        public readonly bool Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _bitArray[_index];
        }
    }
}
