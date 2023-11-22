using System.Numerics;

namespace Spanned.Tests.Spans;

public class SumTests
{
    public static IEnumerable<object[]> Sum_Integer_SumTooLarge_VectorVertical_TestData()
    {
        for (int element = 0; element < 2; element++)
        {
            for (int verticalOffset = 1; verticalOffset < 6; verticalOffset++)
            {
                yield return new object[] { element, verticalOffset };
            }
        }
    }

    [Fact]
    public void Sum_Byte()
    {
        byte[] sourceArray = new byte[] { 1, 2, 3, 4 };

        Assert.Equal(10u, ((Span<byte>)sourceArray).Sum());
        Assert.Equal(10u, ((ReadOnlySpan<byte>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Byte_SumTooLarge_VectorHorizontal_ThrowsOverflowException()
    {
        byte[] sourceArray = new byte[Vector<byte>.Count * 4];
        Array.Fill(sourceArray, (byte)0);

        for (int i = 0; i < Vector<byte>.Count; i++)
        {
            sourceArray[i] = byte.MaxValue - 3;
        }
        for (int i = Vector<byte>.Count; i < sourceArray.Length; i++)
        {
            sourceArray[i] = 1;
        }

        Assert.Throws<OverflowException>(() => ((Span<byte>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<byte>)sourceArray).Sum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void Sum_Byte_SumTooLarge_VectorVertical_ThrowsOverflowException(int element, int verticalOffset)
    {
        byte[] sourceArray = new byte[Vector<byte>.Count * 6];
        Array.Fill(sourceArray, (byte)0);

        sourceArray[element] = byte.MaxValue;
        sourceArray[element + Vector<byte>.Count * verticalOffset] = 1;

        Assert.Throws<OverflowException>(() => ((Span<byte>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<byte>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Byte_SumTooLarge_ThrowsOverflowException()
    {
        byte[] sourceArray = new byte[] { byte.MaxValue, 0, 5, 20 };

        Assert.Throws<OverflowException>(() => ((Span<byte>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<byte>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Byte_SourceContainsOneElement_ReturnsFirstElement()
    {
        byte[] sourceArray = new byte[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<byte>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<byte>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Byte_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<byte>.Empty.Sum());
        Assert.Equal(0, ReadOnlySpan<byte>.Empty.Sum());
    }

    [Fact]
    public void Sum_SByte()
    {
        sbyte[] sourceArray = new sbyte[] { 1, -2, 3, -4 };

        Assert.Equal(-2, ((Span<sbyte>)sourceArray).Sum());
        Assert.Equal(-2, ((ReadOnlySpan<sbyte>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_SByte_SumTooLarge_VectorHorizontal_ThrowsOverflowException()
    {
        sbyte[] sourceArray = new sbyte[Vector<sbyte>.Count * 4];
        Array.Fill(sourceArray, (sbyte)0);

        for (int i = 0; i < Vector<sbyte>.Count; i++)
        {
            sourceArray[i] = sbyte.MaxValue - 3;
        }
        for (int i = Vector<sbyte>.Count; i < sourceArray.Length; i++)
        {
            sourceArray[i] = 1;
        }

        Assert.Throws<OverflowException>(() => ((Span<sbyte>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<sbyte>)sourceArray).Sum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void Sum_SByte_SumTooLarge_VectorVertical_ThrowsOverflowException(int element, int verticalOffset)
    {
        sbyte[] sourceArray = new sbyte[Vector<sbyte>.Count * 6];
        Array.Fill(sourceArray, (sbyte)0);

        sourceArray[element] = sbyte.MaxValue;
        sourceArray[element + Vector<sbyte>.Count * verticalOffset] = 1;

        Assert.Throws<OverflowException>(() => ((Span<sbyte>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<sbyte>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_SByte_SumTooSmall_ThrowsOverflowException()
    {
        sbyte[] sourceArray = new sbyte[] { sbyte.MinValue, 0, -5, -20 };

        Assert.Throws<OverflowException>(() => ((Span<sbyte>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<sbyte>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_SByte_SumTooLarge_ThrowsOverflowException()
    {
        sbyte[] sourceArray = new sbyte[] { sbyte.MaxValue, 0, 5, 20 };

        Assert.Throws<OverflowException>(() => ((Span<sbyte>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<sbyte>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_SByte_SourceContainsOneElement_ReturnsFirstElement()
    {
        sbyte[] sourceArray = new sbyte[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<sbyte>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<sbyte>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_SByte_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<sbyte>.Empty.Sum());
        Assert.Equal(0, ReadOnlySpan<sbyte>.Empty.Sum());
    }

    [Fact]
    public void Sum_Short()
    {
        short[] sourceArray = new short[] { 1, -2, 3, -4 };

        Assert.Equal(-2, ((Span<short>)sourceArray).Sum());
        Assert.Equal(-2, ((ReadOnlySpan<short>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Short_SumTooLarge_VectorHorizontal_ThrowsOverflowException()
    {
        short[] sourceArray = new short[Vector<short>.Count * 4];
        Array.Fill(sourceArray, (short)0);

        for (int i = 0; i < Vector<short>.Count; i++)
        {
            sourceArray[i] = short.MaxValue - 3;
        }
        for (int i = Vector<short>.Count; i < sourceArray.Length; i++)
        {
            sourceArray[i] = 1;
        }

        Assert.Throws<OverflowException>(() => ((Span<short>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<short>)sourceArray).Sum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void Sum_Short_SumTooLarge_VectorVertical_ThrowsOverflowException(int element, int verticalOffset)
    {
        short[] sourceArray = new short[Vector<short>.Count * 6];
        Array.Fill(sourceArray, (short)0);

        sourceArray[element] = short.MaxValue;
        sourceArray[element + Vector<short>.Count * verticalOffset] = 1;

        Assert.Throws<OverflowException>(() => ((Span<short>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<short>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Short_SumTooSmall_ThrowsOverflowException()
    {
        short[] sourceArray = new short[] { short.MinValue, 0, -5, -20 };

        Assert.Throws<OverflowException>(() => ((Span<short>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<short>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Short_SumTooLarge_ThrowsOverflowException()
    {
        short[] sourceArray = new short[] { short.MaxValue, 0, 5, 20 };

        Assert.Throws<OverflowException>(() => ((Span<short>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<short>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Short_SourceContainsOneElement_ReturnsFirstElement()
    {
        short[] sourceArray = new short[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<short>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<short>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Short_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<short>.Empty.Sum());
        Assert.Equal(0, ReadOnlySpan<short>.Empty.Sum());
    }

    [Fact]
    public void Sum_UShort()
    {
        ushort[] sourceArray = new ushort[] { 1, 2, 3, 4 };

        Assert.Equal(10u, ((Span<ushort>)sourceArray).Sum());
        Assert.Equal(10u, ((ReadOnlySpan<ushort>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_UShort_SumTooLarge_VectorHorizontal_ThrowsOverflowException()
    {
        ushort[] sourceArray = new ushort[Vector<ushort>.Count * 4];
        Array.Fill(sourceArray, (ushort)0);

        for (int i = 0; i < Vector<ushort>.Count; i++)
        {
            sourceArray[i] = ushort.MaxValue - 3;
        }
        for (int i = Vector<ushort>.Count; i < sourceArray.Length; i++)
        {
            sourceArray[i] = 1;
        }

        Assert.Throws<OverflowException>(() => ((Span<ushort>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<ushort>)sourceArray).Sum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void Sum_UShort_SumTooLarge_VectorVertical_ThrowsOverflowException(int element, int verticalOffset)
    {
        ushort[] sourceArray = new ushort[Vector<ushort>.Count * 6];
        Array.Fill(sourceArray, (ushort)0);

        sourceArray[element] = ushort.MaxValue;
        sourceArray[element + Vector<ushort>.Count * verticalOffset] = 1;

        Assert.Throws<OverflowException>(() => ((Span<ushort>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<ushort>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_UShort_SumTooLarge_ThrowsOverflowException()
    {
        ushort[] sourceArray = new ushort[] { ushort.MaxValue, 0, 5, 20 };

        Assert.Throws<OverflowException>(() => ((Span<ushort>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<ushort>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_UShort_SourceContainsOneElement_ReturnsFirstElement()
    {
        ushort[] sourceArray = new ushort[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<ushort>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<ushort>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_UShort_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<ushort>.Empty.Sum());
        Assert.Equal(0, ReadOnlySpan<ushort>.Empty.Sum());
    }

    [Fact]
    public void Sum_Int()
    {
        int[] sourceArray = new int[] { 1, -2, 3, -4 };

        Assert.Equal(-2, ((Span<int>)sourceArray).Sum());
        Assert.Equal(-2, ((ReadOnlySpan<int>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Int_SumTooLarge_VectorHorizontal_ThrowsOverflowException()
    {
        int[] sourceArray = new int[Vector<int>.Count * 4];
        Array.Fill(sourceArray, 0);

        for (int i = 0; i < Vector<int>.Count; i++)
        {
            sourceArray[i] = int.MaxValue - 3;
        }
        for (int i = Vector<int>.Count; i < sourceArray.Length; i++)
        {
            sourceArray[i] = 1;
        }

        Assert.Throws<OverflowException>(() => ((Span<int>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<int>)sourceArray).Sum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void Sum_Int_SumTooLarge_VectorVertical_ThrowsOverflowException(int element, int verticalOffset)
    {
        int[] sourceArray = new int[Vector<int>.Count * 6];
        Array.Fill(sourceArray, 0);

        sourceArray[element] = int.MaxValue;
        sourceArray[element + Vector<int>.Count * verticalOffset] = 1;

        Assert.Throws<OverflowException>(() => ((Span<int>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<int>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Int_SumTooSmall_ThrowsOverflowException()
    {
        int[] sourceArray = new int[] { int.MinValue, 0, -5, -20 };

        Assert.Throws<OverflowException>(() => ((Span<int>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<int>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Int_SumTooLarge_ThrowsOverflowException()
    {
        int[] sourceArray = new int[] { int.MaxValue, 0, 5, 20 };

        Assert.Throws<OverflowException>(() => ((Span<int>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<int>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Int_SourceContainsOneElement_ReturnsFirstElement()
    {
        int[] sourceArray = new int[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<int>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<int>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Int_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<int>.Empty.Sum());
        Assert.Equal(0, ReadOnlySpan<int>.Empty.Sum());
    }

    [Fact]
    public void Sum_UInt()
    {
        uint[] sourceArray = new uint[] { 1u, 2u, 3u, 4u };

        Assert.Equal(10u, ((Span<uint>)sourceArray).Sum());
        Assert.Equal(10u, ((ReadOnlySpan<uint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_UInt_SumTooLarge_VectorHorizontal_ThrowsOverflowException()
    {
        uint[] sourceArray = new uint[Vector<uint>.Count * 4];
        Array.Fill(sourceArray, 0u);

        for (int i = 0; i < Vector<uint>.Count; i++)
        {
            sourceArray[i] = uint.MaxValue - 3u;
        }
        for (int i = Vector<uint>.Count; i < sourceArray.Length; i++)
        {
            sourceArray[i] = 1;
        }

        Assert.Throws<OverflowException>(() => ((Span<uint>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<uint>)sourceArray).Sum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void Sum_UInt_SumTooLarge_VectorVertical_ThrowsOverflowException(int element, int verticalOffset)
    {
        uint[] sourceArray = new uint[Vector<uint>.Count * 6];
        Array.Fill(sourceArray, 0u);

        sourceArray[element] = uint.MaxValue;
        sourceArray[element + Vector<uint>.Count * verticalOffset] = 1u;

        Assert.Throws<OverflowException>(() => ((Span<uint>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<uint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_UInt_SumTooLarge_ThrowsOverflowException()
    {
        uint[] sourceArray = new uint[] { uint.MaxValue, 0, 5u, 20u };

        Assert.Throws<OverflowException>(() => ((Span<uint>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<uint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_UInt_SourceContainsOneElement_ReturnsFirstElement()
    {
        uint[] sourceArray = new uint[] { 20u };

        Assert.Equal(sourceArray[0], ((Span<uint>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<uint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_UInt_EmptySource_ReturnsZero()
    {
        Assert.Equal(0u, Span<uint>.Empty.Sum());
        Assert.Equal(0u, ReadOnlySpan<uint>.Empty.Sum());
    }

    [Fact]
    public void Sum_Long()
    {
        long[] sourceArray = new long[] { 1L, -2L, 3L, -4L };

        Assert.Equal(-2L, ((Span<long>)sourceArray).Sum());
        Assert.Equal(-2L, ((ReadOnlySpan<long>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Long_SumTooLarge_VectorHorizontal_ThrowsOverflowException()
    {
        long[] sourceArray = new long[Vector<long>.Count * 4];
        Array.Fill(sourceArray, 0);

        for (int i = 0; i < Vector<long>.Count; i++)
        {
            sourceArray[i] = long.MaxValue - 3;
        }
        for (int i = Vector<long>.Count; i < sourceArray.Length; i++)
        {
            sourceArray[i] = 1;
        }

        Assert.Throws<OverflowException>(() => ((Span<long>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<long>)sourceArray).Sum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void Sum_Long_SumTooLarge_VectorVertical_ThrowsOverflowException(int element, int verticalOffset)
    {
        long[] sourceArray = new long[Vector<long>.Count * 6];
        Array.Fill(sourceArray, 0);

        sourceArray[element] = long.MaxValue;
        sourceArray[element + Vector<long>.Count * verticalOffset] = 1;

        Assert.Throws<OverflowException>(() => ((Span<long>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<long>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Long_SumTooSmall_ThrowsOverflowException()
    {
        long[] sourceArray = new long[] { long.MinValue, 0L, -5L, -20L };

        Assert.Throws<OverflowException>(() => ((Span<long>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<long>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Long_SumTooLarge_ThrowsOverflowException()
    {
        long[] sourceArray = new long[] { long.MaxValue, 0L, 5L, 20L };

        Assert.Throws<OverflowException>(() => ((Span<long>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<long>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Long_SourceContainsOneElement_ReturnsFirstElement()
    {
        long[] sourceArray = new long[] { int.MaxValue + 20L };

        Assert.Equal(sourceArray[0], ((Span<long>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<long>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Long_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<long>.Empty.Sum());
        Assert.Equal(0, ReadOnlySpan<long>.Empty.Sum());
    }

    [Fact]
    public void Sum_ULong()
    {
        ulong[] sourceArray = new ulong[] { 1ul, 2ul, 3ul, 4ul };

        Assert.Equal(10ul, ((Span<ulong>)sourceArray).Sum());
        Assert.Equal(10ul, ((ReadOnlySpan<ulong>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_ULong_SumTooLarge_VectorHorizontal_ThrowsOverflowException()
    {
        ulong[] sourceArray = new ulong[Vector<ulong>.Count * 4];
        Array.Fill(sourceArray, 0ul);

        for (int i = 0; i < Vector<ulong>.Count; i++)
        {
            sourceArray[i] = ulong.MaxValue - 3ul;
        }
        for (int i = Vector<ulong>.Count; i < sourceArray.Length; i++)
        {
            sourceArray[i] = 1;
        }

        Assert.Throws<OverflowException>(() => ((Span<ulong>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<ulong>)sourceArray).Sum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void Sum_ULong_SumTooLarge_VectorVertical_ThrowsOverflowException(int element, int verticalOffset)
    {
        ulong[] sourceArray = new ulong[Vector<ulong>.Count * 6];
        Array.Fill(sourceArray, 0ul);

        sourceArray[element] = ulong.MaxValue;
        sourceArray[element + Vector<ulong>.Count * verticalOffset] = 1ul;

        Assert.Throws<OverflowException>(() => ((Span<ulong>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<ulong>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_ULong_SumTooLarge_ThrowsOverflowException()
    {
        ulong[] sourceArray = new ulong[] { ulong.MaxValue, 0, 5ul, 20ul };

        Assert.Throws<OverflowException>(() => ((Span<ulong>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<ulong>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_ULong_SourceContainsOneElement_ReturnsFirstElement()
    {
        ulong[] sourceArray = new ulong[] { 20ul };

        Assert.Equal(sourceArray[0], ((Span<ulong>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<ulong>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_ULong_EmptySource_ReturnsZero()
    {
        Assert.Equal(0ul, Span<ulong>.Empty.Sum());
        Assert.Equal(0ul, ReadOnlySpan<ulong>.Empty.Sum());
    }

    [Fact]
    public void Sum_NInt()
    {
        nint[] sourceArray = new nint[] { 1, -2, 3, -4 };

        Assert.Equal(-2, ((Span<nint>)sourceArray).Sum());
        Assert.Equal(-2, ((ReadOnlySpan<nint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_NInt_SumTooLarge_VectorHorizontal_ThrowsOverflowException()
    {
        nint[] sourceArray = new nint[Vector<nint>.Count * 4];
        Array.Fill(sourceArray, 0);

        for (int i = 0; i < Vector<nint>.Count; i++)
        {
            sourceArray[i] = nint.MaxValue - 3;
        }
        for (int i = Vector<nint>.Count; i < sourceArray.Length; i++)
        {
            sourceArray[i] = 1;
        }

        Assert.Throws<OverflowException>(() => ((Span<nint>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nint>)sourceArray).Sum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void Sum_NInt_SumTooLarge_VectorVertical_ThrowsOverflowException(int element, int verticalOffset)
    {
        nint[] sourceArray = new nint[Vector<nint>.Count * 6];
        Array.Fill(sourceArray, 0);

        sourceArray[element] = nint.MaxValue;
        sourceArray[element + Vector<nint>.Count * verticalOffset] = 1;

        Assert.Throws<OverflowException>(() => ((Span<nint>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_NInt_SumTooSmall_ThrowsOverflowException()
    {
        nint[] sourceArray = new nint[] { nint.MinValue, 0, -5, -20 };

        Assert.Throws<OverflowException>(() => ((Span<nint>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_NInt_SumTooLarge_ThrowsOverflowException()
    {
        nint[] sourceArray = new nint[] { nint.MaxValue, 0, 5, 20 };

        Assert.Throws<OverflowException>(() => ((Span<nint>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_NInt_SourceContainsOneElement_ReturnsFirstElement()
    {
        nint[] sourceArray = new nint[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<nint>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<nint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_NInt_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<nint>.Empty.Sum());
        Assert.Equal(0, ReadOnlySpan<nint>.Empty.Sum());
    }

    [Fact]
    public void Sum_NUInt()
    {
        nuint[] sourceArray = new nuint[] { 1, 2, 3, 4 };

        Assert.Equal(10u, ((Span<nuint>)sourceArray).Sum());
        Assert.Equal(10u, ((ReadOnlySpan<nuint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_NUInt_SumTooLarge_VectorHorizontal_ThrowsOverflowException()
    {
        nuint[] sourceArray = new nuint[Vector<nuint>.Count * 4];
        Array.Fill(sourceArray, 0u);

        for (int i = 0; i < Vector<nuint>.Count; i++)
        {
            sourceArray[i] = nuint.MaxValue - 3u;
        }
        for (int i = Vector<nuint>.Count; i < sourceArray.Length; i++)
        {
            sourceArray[i] = 1;
        }

        Assert.Throws<OverflowException>(() => ((Span<nuint>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nuint>)sourceArray).Sum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void Sum_NUInt_SumTooLarge_VectorVertical_ThrowsOverflowException(int element, int verticalOffset)
    {
        nuint[] sourceArray = new nuint[Vector<nuint>.Count * 6];
        Array.Fill(sourceArray, 0u);

        sourceArray[element] = nuint.MaxValue;
        sourceArray[element + Vector<nuint>.Count * verticalOffset] = 1u;

        Assert.Throws<OverflowException>(() => ((Span<nuint>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nuint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_NUInt_SumTooLarge_ThrowsOverflowException()
    {
        nuint[] sourceArray = new nuint[] { nuint.MaxValue, 0, 5, 20 };

        Assert.Throws<OverflowException>(() => ((Span<nuint>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nuint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_NUInt_SourceContainsOneElement_ReturnsFirstElement()
    {
        nuint[] sourceArray = new nuint[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<nuint>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<nuint>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_NUInt_EmptySource_ReturnsZero()
    {
        Assert.Equal(0u, Span<nuint>.Empty.Sum());
        Assert.Equal(0u, ReadOnlySpan<nuint>.Empty.Sum());
    }

    [Fact]
    public void Sum_Float()
    {
        float[] sourceArray = new float[] { 1f, 0.5f, -1f, 0.5f };

        Assert.Equal(1f, ((Span<float>)sourceArray).Sum());
        Assert.Equal(1f, ((ReadOnlySpan<float>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Float_SumTooSmall_ReturnsNegativeInfinity()
    {
        float[] sourceArray = new float[] { float.MinValue, float.MinValue, 0f, -5f, -20f };

        Assert.True(float.IsNegativeInfinity(((Span<float>)sourceArray).Sum()));
        Assert.True(float.IsNegativeInfinity(((ReadOnlySpan<float>)sourceArray).Sum()));
    }

    [Fact]
    public void Sum_Float_SumTooLarge_ReturnsPositiveInfinity()
    {
        float[] sourceArray = new float[] { float.MaxValue, float.MaxValue, 0f, 5f, 20f };

        Assert.True(float.IsPositiveInfinity(((Span<float>)sourceArray).Sum()));
        Assert.True(float.IsPositiveInfinity(((ReadOnlySpan<float>)sourceArray).Sum()));
    }

    [Fact]
    public void Sum_Float_SourceContainsOneElement_ReturnsFirstElement()
    {
        float[] sourceArray = new float[] { 20.51f };

        Assert.Equal(sourceArray[0], ((Span<float>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<float>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Float_SourceContainsNaN_ReturnsNaN()
    {
        float[] sourceArray = { 20.45f, 0f, -10.55f, float.NaN };

        Assert.True(float.IsNaN(((Span<float>)sourceArray).Sum()));
        Assert.True(float.IsNaN(((ReadOnlySpan<float>)sourceArray).Sum()));
    }

    [Fact]
    public void Sum_Float_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<float>.Empty.Sum());
        Assert.Equal(0, ReadOnlySpan<float>.Empty.Sum());
    }

    [Fact]
    public void Sum_Double()
    {
        double[] sourceArray = new double[] { 1d, 0.5d, -1d, 0.5d };

        Assert.Equal(1d, ((Span<double>)sourceArray).Sum());
        Assert.Equal(1d, ((ReadOnlySpan<double>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Double_SumTooSmall_ReturnsNegativeInfinity()
    {
        double[] sourceArray = new double[] { double.MinValue, double.MinValue, 0d, -5d, -20d };

        Assert.True(double.IsNegativeInfinity(((Span<double>)sourceArray).Sum()));
        Assert.True(double.IsNegativeInfinity(((ReadOnlySpan<double>)sourceArray).Sum()));
    }

    [Fact]
    public void Sum_Double_SumTooLarge_ReturnsPositiveInfinity()
    {
        double[] sourceArray = new double[] { double.MaxValue, double.MaxValue, 0d, 5d, 20d };

        Assert.True(double.IsPositiveInfinity(((Span<double>)sourceArray).Sum()));
        Assert.True(double.IsPositiveInfinity(((ReadOnlySpan<double>)sourceArray).Sum()));
    }

    [Fact]
    public void Sum_Double_SourceContainsOneElement_ReturnsFirstElement()
    {
        double[] sourceArray = new double[] { 20.51d };

        Assert.Equal(sourceArray[0], ((Span<double>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<double>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Double_SourceContainsNaN_ReturnsNaN()
    {
        double[] sourceArray = { 20.45d, 0d, -10.55d, double.NaN };

        Assert.True(double.IsNaN(((Span<double>)sourceArray).Sum()));
        Assert.True(double.IsNaN(((ReadOnlySpan<double>)sourceArray).Sum()));
    }

    [Fact]
    public void Sum_Double_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<double>.Empty.Sum());
        Assert.Equal(0, ReadOnlySpan<double>.Empty.Sum());
    }

    [Fact]
    public void Sum_Decimal()
    {
        decimal[] sourceArray = new decimal[] { 1m, 0.5m, -1m, 0.5m };

        Assert.Equal(1m, ((Span<decimal>)sourceArray).Sum());
        Assert.Equal(1m, ((ReadOnlySpan<decimal>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Decimal_SumTooSmall_ThrowsOverflowException()
    {
        decimal[] sourceArray = new decimal[] { decimal.MinValue, 0m, -5m, -20m };

        Assert.Throws<OverflowException>(() => ((Span<decimal>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<decimal>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Decimal_SumTooLarge_ThrowsOverflowException()
    {
        decimal[] sourceArray = new decimal[] { decimal.MaxValue, 0m, 5m, 20m };

        Assert.Throws<OverflowException>(() => ((Span<decimal>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<decimal>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Decimal_SourceContainsOneElement_ReturnsFirstElement()
    {
        decimal[] sourceArray = new decimal[] { 20.51m };

        Assert.Equal(sourceArray[0], ((Span<decimal>)sourceArray).Sum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<decimal>)sourceArray).Sum());
    }

    [Fact]
    public void Sum_Decimal_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<decimal>.Empty.Sum());
        Assert.Equal(0, ReadOnlySpan<decimal>.Empty.Sum());
    }

    public static IEnumerable<object[]> Sum_Generic_TestData()
    {
        yield return new object[] { Array.Empty<byte>(), (byte)0 };
        yield return new object[] { new byte[] { 1, 2, 3, 4 }, (byte)10 };

        yield return new object[] { Array.Empty<sbyte>(), (sbyte)0 };
        yield return new object[] { new sbyte[] { 1, -2, 3, -4 }, (sbyte)-2 };

        yield return new object[] { Array.Empty<short>(), (short)0 };
        yield return new object[] { new short[] { 1, -2, 3, -4 }, (short)-2 };

        yield return new object[] { Array.Empty<ushort>(), (ushort)0 };
        yield return new object[] { new ushort[] { 1, 2, 3, 4 }, (ushort)10 };

        yield return new object[] { Array.Empty<int>(), 0 };
        yield return new object[] { new int[] { 1, -2, 3, -4 }, -2 };

        yield return new object[] { Array.Empty<uint>(), 0u };
        yield return new object[] { new uint[] { 1, 2, 3, 4 }, 10u };

        yield return new object[] { Array.Empty<long>(), 0L };
        yield return new object[] { new long[] { 1, -2, 3, -4 }, -2L };

        yield return new object[] { Array.Empty<ulong>(), 0ul };
        yield return new object[] { new ulong[] { 1, 2, 3, 4 }, 10ul };

        yield return new object[] { Array.Empty<nint>(), (nint)0 };
        yield return new object[] { new nint[] { 1, -2, 3, -4 }, (nint)(-2) };

        yield return new object[] { Array.Empty<nuint>(), (nuint)0 };
        yield return new object[] { new nuint[] { 1, 2, 3, 4 }, (nuint)10 };

        yield return new object[] { Array.Empty<float>(), 0f };
        yield return new object[] { new float[] { 1, -2, 3, -4 }, -2f };

        yield return new object[] { Array.Empty<double>(), 0d };
        yield return new object[] { new double[] { 1, -2, 3, -4 }, -2d };

        yield return new object[] { Array.Empty<decimal>(), 0m };
        yield return new object[] { new decimal[] { 1, -2, 3, -4 }, -2m };

        yield return new object[] { Array.Empty<Half>(), (Half)0 };
        yield return new object[] { new Half[] { (Half)1, -2, (Half)3, -4 }, (Half)(-2) };
    }

    [Theory]
    [MemberData(nameof(Sum_Generic_TestData))]
    public void Sum_Generic<T>(T[] arraySource, T expectedSum) where T : INumberBase<T>
    {
        Assert.Equal(expectedSum, ((Span<T>)arraySource).Sum());
        Assert.Equal(expectedSum, ((ReadOnlySpan<T>)arraySource).Sum());
    }

    [Fact]
    public void UnsafeSum_Byte()
    {
        byte[] sourceArray = new byte[] { 1, 2, 3, 4 };

        Assert.Equal(10u, ((Span<byte>)sourceArray).UnsafeSum());
        Assert.Equal(10u, ((ReadOnlySpan<byte>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Byte_SumTooLarge_VectorHorizontal()
    {
        byte[] sourceArray = new byte[Vector<byte>.Count * 4];
        Array.Fill(sourceArray, (byte)0);

        byte expected = 0;
        for (int i = 0; i < Vector<byte>.Count; i++)
        {
            expected += sourceArray[i] = byte.MaxValue - 3;
        }
        for (int i = Vector<byte>.Count; i < sourceArray.Length; i++)
        {
            expected += sourceArray[i] = 1;
        }

        Assert.Equal(expected, ((Span<byte>)sourceArray).UnsafeSum());
        Assert.Equal(expected, ((ReadOnlySpan<byte>)sourceArray).UnsafeSum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void UnsafeSum_Byte_SumTooLarge_VectorVertical(int element, int verticalOffset)
    {
        byte[] sourceArray = new byte[Vector<byte>.Count * 6];
        Array.Fill(sourceArray, (byte)0);

        sourceArray[element] = byte.MaxValue;
        sourceArray[element + Vector<byte>.Count * verticalOffset] = 1;

        Assert.Equal(byte.MinValue, ((Span<byte>)sourceArray).UnsafeSum());
        Assert.Equal(byte.MinValue, ((ReadOnlySpan<byte>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Byte_SumTooLarge()
    {
        byte[] sourceArray = new byte[] { byte.MaxValue, 0, 5, 20 };

        Assert.Equal(byte.MinValue + 24, ((Span<byte>)sourceArray).UnsafeSum());
        Assert.Equal(byte.MinValue + 24, ((ReadOnlySpan<byte>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Byte_SourceContainsOneElement_ReturnsFirstElement()
    {
        byte[] sourceArray = new byte[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<byte>)sourceArray).UnsafeSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<byte>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Byte_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<byte>.Empty.UnsafeSum());
        Assert.Equal(0, ReadOnlySpan<byte>.Empty.UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_SByte()
    {
        sbyte[] sourceArray = new sbyte[] { 1, -2, 3, -4 };

        Assert.Equal(-2, ((Span<sbyte>)sourceArray).UnsafeSum());
        Assert.Equal(-2, ((ReadOnlySpan<sbyte>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_SByte_SumTooLarge_VectorHorizontal()
    {
        sbyte[] sourceArray = new sbyte[Vector<sbyte>.Count * 4];
        Array.Fill(sourceArray, (sbyte)0);

        sbyte expected = 0;
        for (int i = 0; i < Vector<sbyte>.Count; i++)
        {
            expected += sourceArray[i] = sbyte.MaxValue - 3;
        }
        for (int i = Vector<sbyte>.Count; i < sourceArray.Length; i++)
        {
            expected += sourceArray[i] = 1;
        }

        Assert.Equal(expected, ((Span<sbyte>)sourceArray).UnsafeSum());
        Assert.Equal(expected, ((ReadOnlySpan<sbyte>)sourceArray).UnsafeSum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void UnsafeSum_SByte_SumTooLarge_VectorVertical(int element, int verticalOffset)
    {
        sbyte[] sourceArray = new sbyte[Vector<sbyte>.Count * 6];
        Array.Fill(sourceArray, (sbyte)0);

        sourceArray[element] = sbyte.MaxValue;
        sourceArray[element + Vector<sbyte>.Count * verticalOffset] = 1;

        Assert.Equal(sbyte.MinValue, ((Span<sbyte>)sourceArray).UnsafeSum());
        Assert.Equal(sbyte.MinValue, ((ReadOnlySpan<sbyte>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_SByte_SumTooSmall()
    {
        sbyte[] sourceArray = new sbyte[] { sbyte.MinValue, 0, -5, -20 };

        Assert.Equal(sbyte.MaxValue - 24, ((Span<sbyte>)sourceArray).UnsafeSum());
        Assert.Equal(sbyte.MaxValue - 24, ((ReadOnlySpan<sbyte>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_SByte_SumTooLarge()
    {
        sbyte[] sourceArray = new sbyte[] { sbyte.MaxValue, 0, 5, 20 };

        Assert.Equal(sbyte.MinValue + 24, ((Span<sbyte>)sourceArray).UnsafeSum());
        Assert.Equal(sbyte.MinValue + 24, ((ReadOnlySpan<sbyte>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_SByte_SourceContainsOneElement_ReturnsFirstElement()
    {
        sbyte[] sourceArray = new sbyte[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<sbyte>)sourceArray).UnsafeSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<sbyte>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_SByte_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<sbyte>.Empty.UnsafeSum());
        Assert.Equal(0, ReadOnlySpan<sbyte>.Empty.UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Short()
    {
        short[] sourceArray = new short[] { 1, -2, 3, -4 };

        Assert.Equal(-2, ((Span<short>)sourceArray).UnsafeSum());
        Assert.Equal(-2, ((ReadOnlySpan<short>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Short_SumTooLarge_VectorHorizontal()
    {
        short[] sourceArray = new short[Vector<short>.Count * 4];
        Array.Fill(sourceArray, (short)0);

        short expected = 0;
        for (int i = 0; i < Vector<short>.Count; i++)
        {
            expected += sourceArray[i] = short.MaxValue - 3;
        }
        for (int i = Vector<short>.Count; i < sourceArray.Length; i++)
        {
            expected += sourceArray[i] = 1;
        }

        Assert.Equal(expected, ((Span<short>)sourceArray).UnsafeSum());
        Assert.Equal(expected, ((ReadOnlySpan<short>)sourceArray).UnsafeSum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void UnsafeSum_Short_SumTooLarge_VectorVertical(int element, int verticalOffset)
    {
        short[] sourceArray = new short[Vector<short>.Count * 6];
        Array.Fill(sourceArray, (short)0);

        sourceArray[element] = short.MaxValue;
        sourceArray[element + Vector<short>.Count * verticalOffset] = 1;

        Assert.Equal(short.MinValue, ((Span<short>)sourceArray).UnsafeSum());
        Assert.Equal(short.MinValue, ((ReadOnlySpan<short>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Short_SumTooSmall()
    {
        short[] sourceArray = new short[] { short.MinValue, 0, -5, -20 };

        Assert.Equal(short.MaxValue - 24, ((Span<short>)sourceArray).UnsafeSum());
        Assert.Equal(short.MaxValue - 24, ((ReadOnlySpan<short>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Short_SumTooLarge()
    {
        short[] sourceArray = new short[] { short.MaxValue, 0, 5, 20 };

        Assert.Equal(short.MinValue + 24, ((Span<short>)sourceArray).UnsafeSum());
        Assert.Equal(short.MinValue + 24, ((ReadOnlySpan<short>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Short_SourceContainsOneElement_ReturnsFirstElement()
    {
        short[] sourceArray = new short[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<short>)sourceArray).UnsafeSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<short>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Short_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<short>.Empty.UnsafeSum());
        Assert.Equal(0, ReadOnlySpan<short>.Empty.UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_UShort()
    {
        ushort[] sourceArray = new ushort[] { 1, 2, 3, 4 };

        Assert.Equal(10u, ((Span<ushort>)sourceArray).UnsafeSum());
        Assert.Equal(10u, ((ReadOnlySpan<ushort>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_UShort_SumTooLarge_VectorHorizontal()
    {
        ushort[] sourceArray = new ushort[Vector<ushort>.Count * 4];
        Array.Fill(sourceArray, (ushort)0);

        ushort expected = 0;
        for (int i = 0; i < Vector<ushort>.Count; i++)
        {
            expected += sourceArray[i] = ushort.MaxValue - 3;
        }
        for (int i = Vector<ushort>.Count; i < sourceArray.Length; i++)
        {
            expected += sourceArray[i] = 1;
        }

        Assert.Equal(expected, ((Span<ushort>)sourceArray).UnsafeSum());
        Assert.Equal(expected, ((ReadOnlySpan<ushort>)sourceArray).UnsafeSum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void UnsafeSum_UShort_SumTooLarge_VectorVertical(int element, int verticalOffset)
    {
        ushort[] sourceArray = new ushort[Vector<ushort>.Count * 6];
        Array.Fill(sourceArray, (ushort)0);

        sourceArray[element] = ushort.MaxValue;
        sourceArray[element + Vector<ushort>.Count * verticalOffset] = 1;

        Assert.Equal(ushort.MinValue, ((Span<ushort>)sourceArray).UnsafeSum());
        Assert.Equal(ushort.MinValue, ((ReadOnlySpan<ushort>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_UShort_SumTooLarge()
    {
        ushort[] sourceArray = new ushort[] { ushort.MaxValue, 0, 5, 20 };

        Assert.Equal(ushort.MinValue + 24, ((Span<ushort>)sourceArray).UnsafeSum());
        Assert.Equal(ushort.MinValue + 24, ((ReadOnlySpan<ushort>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_UShort_SourceContainsOneElement_ReturnsFirstElement()
    {
        ushort[] sourceArray = new ushort[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<ushort>)sourceArray).UnsafeSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<ushort>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_UShort_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<ushort>.Empty.UnsafeSum());
        Assert.Equal(0, ReadOnlySpan<ushort>.Empty.UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Int()
    {
        int[] sourceArray = new int[] { 1, -2, 3, -4 };

        Assert.Equal(-2, ((Span<int>)sourceArray).UnsafeSum());
        Assert.Equal(-2, ((ReadOnlySpan<int>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Int_SumTooLarge_VectorHorizontal()
    {
        int[] sourceArray = new int[Vector<int>.Count * 4];
        Array.Fill(sourceArray, 0);

        int expected = 0;
        for (int i = 0; i < Vector<int>.Count; i++)
        {
            expected += sourceArray[i] = int.MaxValue - 3;
        }
        for (int i = Vector<int>.Count; i < sourceArray.Length; i++)
        {
            expected += sourceArray[i] = 1;
        }

        Assert.Equal(expected, ((Span<int>)sourceArray).UnsafeSum());
        Assert.Equal(expected, ((ReadOnlySpan<int>)sourceArray).UnsafeSum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void UnsafeSum_Int_SumTooLarge_VectorVertical(int element, int verticalOffset)
    {
        int[] sourceArray = new int[Vector<int>.Count * 6];
        Array.Fill(sourceArray, 0);

        sourceArray[element] = int.MaxValue;
        sourceArray[element + Vector<int>.Count * verticalOffset] = 1;

        Assert.Equal(int.MinValue, ((Span<int>)sourceArray).UnsafeSum());
        Assert.Equal(int.MinValue, ((ReadOnlySpan<int>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Int_SumTooSmall()
    {
        int[] sourceArray = new int[] { int.MinValue, 0, -5, -20 };

        Assert.Equal(int.MaxValue - 24, ((Span<int>)sourceArray).UnsafeSum());
        Assert.Equal(int.MaxValue - 24, ((ReadOnlySpan<int>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Int_SumTooLarge()
    {
        int[] sourceArray = new int[] { int.MaxValue, 0, 5, 20 };

        Assert.Equal(int.MinValue + 24, ((Span<int>)sourceArray).UnsafeSum());
        Assert.Equal(int.MinValue + 24, ((ReadOnlySpan<int>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Int_SourceContainsOneElement_ReturnsFirstElement()
    {
        int[] sourceArray = new int[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<int>)sourceArray).UnsafeSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<int>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Int_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<int>.Empty.UnsafeSum());
        Assert.Equal(0, ReadOnlySpan<int>.Empty.UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_UInt()
    {
        uint[] sourceArray = new uint[] { 1u, 2u, 3u, 4u };

        Assert.Equal(10u, ((Span<uint>)sourceArray).UnsafeSum());
        Assert.Equal(10u, ((ReadOnlySpan<uint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_UInt_SumTooLarge_VectorHorizontal()
    {
        uint[] sourceArray = new uint[Vector<uint>.Count * 4];
        Array.Fill(sourceArray, 0u);

        uint expected = 0;
        for (int i = 0; i < Vector<uint>.Count; i++)
        {
            expected += sourceArray[i] = uint.MaxValue - 3u;
        }
        for (int i = Vector<uint>.Count; i < sourceArray.Length; i++)
        {
            expected += sourceArray[i] = 1;
        }

        Assert.Equal(expected, ((Span<uint>)sourceArray).UnsafeSum());
        Assert.Equal(expected, ((ReadOnlySpan<uint>)sourceArray).UnsafeSum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void UnsafeSum_UInt_SumTooLarge_VectorVertical(int element, int verticalOffset)
    {
        uint[] sourceArray = new uint[Vector<uint>.Count * 6];
        Array.Fill(sourceArray, 0u);

        sourceArray[element] = uint.MaxValue;
        sourceArray[element + Vector<uint>.Count * verticalOffset] = 1u;

        Assert.Equal(uint.MinValue, ((Span<uint>)sourceArray).UnsafeSum());
        Assert.Equal(uint.MinValue, ((ReadOnlySpan<uint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_UInt_SumTooLarge()
    {
        uint[] sourceArray = new uint[] { uint.MaxValue, 0, 5u, 20u };

        Assert.Equal(uint.MinValue + 24, ((Span<uint>)sourceArray).UnsafeSum());
        Assert.Equal(uint.MinValue + 24, ((ReadOnlySpan<uint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_UInt_SourceContainsOneElement_ReturnsFirstElement()
    {
        uint[] sourceArray = new uint[] { 20u };

        Assert.Equal(sourceArray[0], ((Span<uint>)sourceArray).UnsafeSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<uint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_UInt_EmptySource_ReturnsZero()
    {
        Assert.Equal(0u, Span<uint>.Empty.UnsafeSum());
        Assert.Equal(0u, ReadOnlySpan<uint>.Empty.UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Long()
    {
        long[] sourceArray = new long[] { 1L, -2L, 3L, -4L };

        Assert.Equal(-2L, ((Span<long>)sourceArray).UnsafeSum());
        Assert.Equal(-2L, ((ReadOnlySpan<long>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Long_SumTooLarge_VectorHorizontal()
    {
        long[] sourceArray = new long[Vector<long>.Count * 4];
        Array.Fill(sourceArray, 0);

        long expected = 0;
        for (int i = 0; i < Vector<long>.Count; i++)
        {
            expected += sourceArray[i] = long.MaxValue - 3;
        }
        for (int i = Vector<long>.Count; i < sourceArray.Length; i++)
        {
            expected += sourceArray[i] = 1;
        }

        Assert.Equal(expected, ((Span<long>)sourceArray).UnsafeSum());
        Assert.Equal(expected, ((ReadOnlySpan<long>)sourceArray).UnsafeSum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void UnsafeSum_Long_SumTooLarge_VectorVertical(int element, int verticalOffset)
    {
        long[] sourceArray = new long[Vector<long>.Count * 6];
        Array.Fill(sourceArray, 0);

        sourceArray[element] = long.MaxValue;
        sourceArray[element + Vector<long>.Count * verticalOffset] = 1;

        Assert.Equal(long.MinValue, ((Span<long>)sourceArray).UnsafeSum());
        Assert.Equal(long.MinValue, ((ReadOnlySpan<long>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Long_SumTooSmall()
    {
        long[] sourceArray = new long[] { long.MinValue, 0L, -5L, -20L };

        Assert.Equal(long.MaxValue - 24, ((Span<long>)sourceArray).UnsafeSum());
        Assert.Equal(long.MaxValue - 24, ((ReadOnlySpan<long>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Long_SumTooLarge()
    {
        long[] sourceArray = new long[] { long.MaxValue, 0L, 5L, 20L };

        Assert.Equal(long.MinValue + 24, ((Span<long>)sourceArray).UnsafeSum());
        Assert.Equal(long.MinValue + 24, ((ReadOnlySpan<long>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Long_SourceContainsOneElement_ReturnsFirstElement()
    {
        long[] sourceArray = new long[] { int.MaxValue + 20L };

        Assert.Equal(sourceArray[0], ((Span<long>)sourceArray).UnsafeSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<long>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_Long_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<long>.Empty.UnsafeSum());
        Assert.Equal(0, ReadOnlySpan<long>.Empty.UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_ULong()
    {
        ulong[] sourceArray = new ulong[] { 1ul, 2ul, 3ul, 4ul };

        Assert.Equal(10ul, ((Span<ulong>)sourceArray).UnsafeSum());
        Assert.Equal(10ul, ((ReadOnlySpan<ulong>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_ULong_SumTooLarge_VectorHorizontal()
    {
        ulong[] sourceArray = new ulong[Vector<ulong>.Count * 4];
        Array.Fill(sourceArray, 0ul);

        ulong expected = 0;
        for (int i = 0; i < Vector<ulong>.Count; i++)
        {
            expected += sourceArray[i] = ulong.MaxValue - 3ul;
        }
        for (int i = Vector<ulong>.Count; i < sourceArray.Length; i++)
        {
            expected += sourceArray[i] = 1;
        }

        Assert.Equal(expected, ((Span<ulong>)sourceArray).UnsafeSum());
        Assert.Equal(expected, ((ReadOnlySpan<ulong>)sourceArray).UnsafeSum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void UnsafeSum_ULong_SumTooLarge_VectorVertical(int element, int verticalOffset)
    {
        ulong[] sourceArray = new ulong[Vector<ulong>.Count * 6];
        Array.Fill(sourceArray, 0ul);

        sourceArray[element] = ulong.MaxValue;
        sourceArray[element + Vector<ulong>.Count * verticalOffset] = 1ul;

        Assert.Equal(ulong.MinValue, ((Span<ulong>)sourceArray).UnsafeSum());
        Assert.Equal(ulong.MinValue, ((ReadOnlySpan<ulong>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_ULong_SumTooLarge()
    {
        ulong[] sourceArray = new ulong[] { ulong.MaxValue, 0, 5ul, 20ul };

        Assert.Equal(ulong.MinValue + 24, ((Span<ulong>)sourceArray).UnsafeSum());
        Assert.Equal(ulong.MinValue + 24, ((ReadOnlySpan<ulong>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_ULong_SourceContainsOneElement_ReturnsFirstElement()
    {
        ulong[] sourceArray = new ulong[] { 20ul };

        Assert.Equal(sourceArray[0], ((Span<ulong>)sourceArray).UnsafeSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<ulong>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_ULong_EmptySource_ReturnsZero()
    {
        Assert.Equal(0ul, Span<ulong>.Empty.UnsafeSum());
        Assert.Equal(0ul, ReadOnlySpan<ulong>.Empty.UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_NInt()
    {
        nint[] sourceArray = new nint[] { 1, -2, 3, -4 };

        Assert.Equal(-2, ((Span<nint>)sourceArray).UnsafeSum());
        Assert.Equal(-2, ((ReadOnlySpan<nint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_NInt_SumTooLarge_VectorHorizontal()
    {
        nint[] sourceArray = new nint[Vector<nint>.Count * 4];
        Array.Fill(sourceArray, 0);

        nint expected = 0;
        for (int i = 0; i < Vector<nint>.Count; i++)
        {
            expected += sourceArray[i] = nint.MaxValue - 3;
        }
        for (int i = Vector<nint>.Count; i < sourceArray.Length; i++)
        {
            expected += sourceArray[i] = 1;
        }

        Assert.Equal(expected, ((Span<nint>)sourceArray).UnsafeSum());
        Assert.Equal(expected, ((ReadOnlySpan<nint>)sourceArray).UnsafeSum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void UnsafeSum_NInt_SumTooLarge_VectorVertical(int element, int verticalOffset)
    {
        nint[] sourceArray = new nint[Vector<nint>.Count * 6];
        Array.Fill(sourceArray, 0);

        sourceArray[element] = nint.MaxValue;
        sourceArray[element + Vector<nint>.Count * verticalOffset] = 1;

        Assert.Equal(nint.MinValue, ((Span<nint>)sourceArray).UnsafeSum());
        Assert.Equal(nint.MinValue, ((ReadOnlySpan<nint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_NInt_SumTooSmall()
    {
        nint[] sourceArray = new nint[] { nint.MinValue, 0, -5, -20 };

        Assert.Equal(nint.MaxValue - 24, ((Span<nint>)sourceArray).UnsafeSum());
        Assert.Equal(nint.MaxValue - 24, ((ReadOnlySpan<nint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_NInt_SumTooLarge()
    {
        nint[] sourceArray = new nint[] { nint.MaxValue, 0, 5, 20 };

        Assert.Equal(nint.MinValue + 24, ((Span<nint>)sourceArray).UnsafeSum());
        Assert.Equal(nint.MinValue + 24, ((ReadOnlySpan<nint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_NInt_SourceContainsOneElement_ReturnsFirstElement()
    {
        nint[] sourceArray = new nint[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<nint>)sourceArray).UnsafeSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<nint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_NInt_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<nint>.Empty.UnsafeSum());
        Assert.Equal(0, ReadOnlySpan<nint>.Empty.UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_NUInt()
    {
        nuint[] sourceArray = new nuint[] { 1, 2, 3, 4 };

        Assert.Equal(10u, ((Span<nuint>)sourceArray).UnsafeSum());
        Assert.Equal(10u, ((ReadOnlySpan<nuint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_NUInt_SumTooLarge_VectorHorizontal()
    {
        nuint[] sourceArray = new nuint[Vector<nuint>.Count * 4];
        Array.Fill(sourceArray, 0u);

        nuint expected = 0;
        for (int i = 0; i < Vector<nuint>.Count; i++)
        {
            expected += sourceArray[i] = nuint.MaxValue - 3u;
        }
        for (int i = Vector<nuint>.Count; i < sourceArray.Length; i++)
        {
            expected += sourceArray[i] = 1;
        }

        Assert.Equal(expected, ((Span<nuint>)sourceArray).UnsafeSum());
        Assert.Equal(expected, ((ReadOnlySpan<nuint>)sourceArray).UnsafeSum());
    }

    [Theory]
    [MemberData(nameof(Sum_Integer_SumTooLarge_VectorVertical_TestData))]
    public void UnsafeSum_NUInt_SumTooLarge_VectorVertical(int element, int verticalOffset)
    {
        nuint[] sourceArray = new nuint[Vector<nuint>.Count * 6];
        Array.Fill(sourceArray, 0u);

        sourceArray[element] = nuint.MaxValue;
        sourceArray[element + Vector<nuint>.Count * verticalOffset] = 1u;

        Assert.Equal(nuint.MinValue, ((Span<nuint>)sourceArray).UnsafeSum());
        Assert.Equal(nuint.MinValue, ((ReadOnlySpan<nuint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_NUInt_SumTooLarge()
    {
        nuint[] sourceArray = new nuint[] { nuint.MaxValue, 0, 5, 20 };

        Assert.Equal(nuint.MinValue + 24, ((Span<nuint>)sourceArray).UnsafeSum());
        Assert.Equal(nuint.MinValue + 24, ((ReadOnlySpan<nuint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_NUInt_SourceContainsOneElement_ReturnsFirstElement()
    {
        nuint[] sourceArray = new nuint[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<nuint>)sourceArray).UnsafeSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<nuint>)sourceArray).UnsafeSum());
    }

    [Fact]
    public void UnsafeSum_NUInt_EmptySource_ReturnsZero()
    {
        Assert.Equal(0u, Span<nuint>.Empty.UnsafeSum());
        Assert.Equal(0u, ReadOnlySpan<nuint>.Empty.UnsafeSum());
    }

    public static IEnumerable<object[]> UnsafeSum_Generic_TestData()
    {
        yield return new object[] { Array.Empty<byte>(), (byte)0 };
        yield return new object[] { new byte[] { 1, 2, 3, 4 }, (byte)10 };
        yield return new object[] { new byte[] { byte.MaxValue, 1 }, byte.MinValue };

        yield return new object[] { Array.Empty<sbyte>(), (sbyte)0 };
        yield return new object[] { new sbyte[] { 1, -2, 3, -4 }, (sbyte)-2 };
        yield return new object[] { new sbyte[] { sbyte.MaxValue, 1 }, sbyte.MinValue };
        yield return new object[] { new sbyte[] { sbyte.MinValue, -1 }, sbyte.MaxValue };

        yield return new object[] { Array.Empty<short>(), (short)0 };
        yield return new object[] { new short[] { 1, -2, 3, -4 }, (short)-2 };
        yield return new object[] { new short[] { short.MaxValue, 1 }, short.MinValue };
        yield return new object[] { new short[] { short.MinValue, -1 }, short.MaxValue };

        yield return new object[] { Array.Empty<ushort>(), (ushort)0 };
        yield return new object[] { new ushort[] { 1, 2, 3, 4 }, (ushort)10 };
        yield return new object[] { new ushort[] { ushort.MaxValue, 1 }, ushort.MinValue };

        yield return new object[] { Array.Empty<int>(), 0 };
        yield return new object[] { new int[] { 1, -2, 3, -4 }, -2 };
        yield return new object[] { new int[] { int.MaxValue, 1 }, int.MinValue };
        yield return new object[] { new int[] { int.MinValue, -1 }, int.MaxValue };

        yield return new object[] { Array.Empty<uint>(), 0u };
        yield return new object[] { new uint[] { 1, 2, 3, 4 }, 10u };
        yield return new object[] { new uint[] { uint.MaxValue, 1 }, uint.MinValue };

        yield return new object[] { Array.Empty<long>(), 0L };
        yield return new object[] { new long[] { 1, -2, 3, -4 }, -2L };
        yield return new object[] { new long[] { long.MaxValue, 1 }, long.MinValue };
        yield return new object[] { new long[] { long.MinValue, -1 }, long.MaxValue };

        yield return new object[] { Array.Empty<ulong>(), 0ul };
        yield return new object[] { new ulong[] { 1, 2, 3, 4 }, 10ul };
        yield return new object[] { new ulong[] { ulong.MaxValue, 1 }, ulong.MinValue };

        yield return new object[] { Array.Empty<nint>(), (nint)0 };
        yield return new object[] { new nint[] { 1, -2, 3, -4 }, (nint)(-2) };
        yield return new object[] { new nint[] { nint.MaxValue, 1 }, nint.MinValue };
        yield return new object[] { new nint[] { nint.MinValue, -1 }, nint.MaxValue };

        yield return new object[] { Array.Empty<nuint>(), (nuint)0 };
        yield return new object[] { new nuint[] { 1, 2, 3, 4 }, (nuint)10 };
        yield return new object[] { new nuint[] { nuint.MaxValue, 1 }, nuint.MinValue };

        yield return new object[] { Array.Empty<float>(), 0f };
        yield return new object[] { new float[] { 1, -2, 3, -4 }, -2f };

        yield return new object[] { Array.Empty<double>(), 0d };
        yield return new object[] { new double[] { 1, -2, 3, -4 }, -2d };

        yield return new object[] { Array.Empty<decimal>(), 0m };
        yield return new object[] { new decimal[] { 1, -2, 3, -4 }, -2m };

        yield return new object[] { Array.Empty<Half>(), (Half)0 };
        yield return new object[] { new Half[] { (Half)1, -2, (Half)3, -4 }, (Half)(-2) };
    }

    [Theory]
    [MemberData(nameof(UnsafeSum_Generic_TestData))]
    public void UnsafeSum_Generic<T>(T[] arraySource, T expectedSum) where T : INumberBase<T>
    {
        Assert.Equal(expectedSum, ((Span<T>)arraySource).UnsafeSum());
        Assert.Equal(expectedSum, ((ReadOnlySpan<T>)arraySource).UnsafeSum());
    }
}
