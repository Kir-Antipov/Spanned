using System.Numerics;
using Spanned.Tests.TestUtilities;

namespace Spanned.Tests.Spans;

public class MinTests
{
    public static IEnumerable<object[]> Min_AllTypes_TestData()
    {
        for (int length = 2; length < 65; length++)
        {
            yield return new object[] { Shuffler.Range((byte)length, length), (byte)length };

            // Tests do `+ T.One`, so we should generate data up to one value below `sbyte.MaxValue`, otherwise the type overflows.
            if (length + length < sbyte.MaxValue)
            {
                yield return new object[] { Shuffler.Range((sbyte)length, length), (sbyte)length };
            }

            yield return new object[] { Shuffler.Range((short)length, length), (short)length };

            yield return new object[] { Shuffler.Range((ushort)length, length), (ushort)length };

            yield return new object[] { Shuffler.Range(length, length), length };

            yield return new object[] { Shuffler.Range((uint)length, length), (uint)length };

            yield return new object[] { Shuffler.Range((long)length, length), (long)length };

            yield return new object[] { Shuffler.Range((ulong)length, length), (ulong)length };

            yield return new object[] { Shuffler.Range((nint)length, length), (nint)length };

            yield return new object[] { Shuffler.Range((nuint)length, length), (nuint)length };

            yield return new object[] { Shuffler.Range((float)length, length), (float)length };

            yield return new object[] { Shuffler.Range((double)length, length), (double)length };

            yield return new object[] { Shuffler.Range((decimal)length, length), (decimal)length };
        }
    }

    [Theory]
    [MemberData(nameof(Min_AllTypes_TestData))]
    public void Min_AllTypes<T>(T[] sourceArray, T expected) where T : INumber<T>
    {
        Span<T> source = sourceArray;

        Assert.Equal(source.Min(), source.Min());
        Assert.Equal(expected, source.Min());
        Assert.Equal(expected, source.Min(comparer: null));
        Assert.Equal(expected, source.Min(Comparer<T>.Default));
        Assert.Equal(expected, source.Min(Comparer<T>.Create(Comparer<T>.Default.Compare)));

        T first = source[0];
        Assert.Equal(first, source.Min(Comparer<T>.Create((x, y) => x == first ? -1 : 1)));

        ReadOnlySpan<T> sourcePlusOne = Array.ConvertAll(source.ToArray(), x => x + T.One);
        Assert.Equal(expected + T.One, sourcePlusOne.Min());

        // ---------------------------------

        ReadOnlySpan<T> readOnlySource = sourceArray;

        Assert.Equal(readOnlySource.Min(), readOnlySource.Min());
        Assert.Equal(expected, readOnlySource.Min());
        Assert.Equal(expected, readOnlySource.Min(comparer: null));
        Assert.Equal(expected, readOnlySource.Min(Comparer<T>.Default));
        Assert.Equal(expected, readOnlySource.Min(Comparer<T>.Create(Comparer<T>.Default.Compare)));

        T readOnlyFirst = readOnlySource[0];
        Assert.Equal(readOnlyFirst, readOnlySource.Min(Comparer<T>.Create((x, y) => x == readOnlyFirst ? -1 : 1)));

        ReadOnlySpan<T> readOnlySourcePlusOne = Array.ConvertAll(readOnlySource.ToArray(), x => x + T.One);
        Assert.Equal(expected + T.One, readOnlySourcePlusOne.Min());
    }

    public static IEnumerable<object[]> Min_Byte_TestData()
    {
        yield return new object[] { new byte[] { 42 }, (byte)42 };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (byte)x).ToArray(), (byte)1 };
        yield return new object[] { new byte[] { 28, 13, 10, 200, 255 }, (byte)10 };
        yield return new object[] { new byte[] { 255, 100, 200, 235 }, (byte)100 };
        yield return new object[] { new byte[] { 255, 100, 200, 235, byte.MinValue }, byte.MinValue };
        yield return new object[] { new byte[] { 20 }, (byte)20 };
        yield return new object[] { Enumerable.Repeat((byte)2, 50).ToArray(), (byte)2 };
        yield return new object[] { new byte[] { 6, 9, 10, 7, 8 }, (byte)6 };
        yield return new object[] { new byte[] { 6, 9, 10, 5, 3 }, (byte)3 };
        yield return new object[] { new byte[] { 6, 0, 9, 0, 10, 0 }, (byte)0 };
    }

    [Theory]
    [MemberData(nameof(Min_Byte_TestData))]
    public void Min_Byte(byte[] sourceArray, byte expected)
    {
        Span<byte> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<byte> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_Byte_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<byte>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<byte>.Empty.Min());
    }

    public static IEnumerable<object[]> Min_SByte_TestData()
    {
        yield return new object[] { new sbyte[] { 42 }, (sbyte)42 };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (sbyte)x).ToArray(), (sbyte)1 };
        yield return new object[] { new sbyte[] { -1, -10, 10, 100, 127 }, (sbyte)-10 };
        yield return new object[] { new sbyte[] { 118, 100, 111, 127 }, (sbyte)100 };
        yield return new object[] { new sbyte[] { 118, 100, 111, 127, sbyte.MinValue }, sbyte.MinValue };
        yield return new object[] { new sbyte[] { 20 }, (sbyte)20 };
        yield return new object[] { Enumerable.Repeat((sbyte)-2, 50).ToArray(), (sbyte)-2 };
        yield return new object[] { new sbyte[] { 6, 9, 10, 7, 8 }, (sbyte)6 };
        yield return new object[] { new sbyte[] { 6, 9, 10, 0, -5 }, (sbyte)-5 };
        yield return new object[] { new sbyte[] { 6, 0, 9, 0, 10, 0 }, (sbyte)0 };
    }

    [Theory]
    [MemberData(nameof(Min_SByte_TestData))]
    public void Min_SByte(sbyte[] sourceArray, sbyte expected)
    {
        Span<sbyte> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<sbyte> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_SByte_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<sbyte>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<sbyte>.Empty.Min());
    }

    public static IEnumerable<object[]> Min_Short_TestData()
    {
        yield return new object[] { new short[] { 42 }, (short)42 };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (short)x).ToArray(), (short)1 };
        yield return new object[] { new short[] { -1, -10, 10, 200, 1000 }, (short)-10 };
        yield return new object[] { new short[] { 3000, 100, 200, 1000 }, (short)100 };
        yield return new object[] { new short[] { 3000, 100, 200, 1000, short.MinValue }, short.MinValue };
        yield return new object[] { new short[] { 20 }, (short)20 };
        yield return new object[] { Enumerable.Repeat((short)-2, 50).ToArray(), (short)-2 };
        yield return new object[] { new short[] { 6, 9, 10, 7, 8 }, (short)6 };
        yield return new object[] { new short[] { 6, 9, 10, 0, -5 }, (short)-5 };
        yield return new object[] { new short[] { 6, 0, 9, 0, 10, 0 }, (short)0 };
    }

    [Theory]
    [MemberData(nameof(Min_Short_TestData))]
    public void Min_Short(short[] sourceArray, short expected)
    {
        Span<short> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<short> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    public static IEnumerable<object[]> Min_UShort_TestData()
    {
        yield return new object[] { new ushort[] { 42 }, (ushort)42u };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (ushort)x).ToArray(), (ushort)1u };
        yield return new object[] { new ushort[] { 1000, 200, 10, 200, 1000 }, (ushort)10u };
        yield return new object[] { new ushort[] { 3000, 100, 200, 1000 }, (ushort)100u };
        yield return new object[] { new ushort[] { 3000, 100, 200, 1000, ushort.MinValue }, ushort.MinValue };
        yield return new object[] { new ushort[] { 20 }, 20u };
        yield return new object[] { Enumerable.Repeat((ushort)2, 50).ToArray(), (ushort)2u };
        yield return new object[] { new ushort[] { 6, 9, 10, 7, 8 }, (ushort)6u };
        yield return new object[] { new ushort[] { 6, 9, 10, 5, 3 }, (ushort)3u };
        yield return new object[] { new ushort[] { 6, 0, 9, 0, 10, 0 }, (ushort)0u };
    }

    [Theory]
    [MemberData(nameof(Min_UShort_TestData))]
    public void Min_UShort(ushort[] sourceArray, ushort expected)
    {
        Span<ushort> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<ushort> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_UShort_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<ushort>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<ushort>.Empty.Min());
    }

    [Fact]
    public void Min_Short_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<short>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<short>.Empty.Min());
    }

    public static IEnumerable<object[]> Min_Int_TestData()
    {
        yield return new object[] { new int[] { 42 }, 42 };
        yield return new object[] { Enumerable.Range(1, 100).ToArray(), 1 };
        yield return new object[] { new int[] { -1, -10, 10, 200, 1000 }, -10 };
        yield return new object[] { new int[] { 3000, 100, 200, 1000 }, 100 };
        yield return new object[] { new int[] { 3000, 100, 200, 1000, int.MinValue }, int.MinValue };
        yield return new object[] { new int[] { 20 }, 20 };
        yield return new object[] { Enumerable.Repeat(-2, 50).ToArray(), -2 };
        yield return new object[] { new int[] { 6, 9, 10, 7, 8 }, 6 };
        yield return new object[] { new int[] { 6, 9, 10, 0, -5 }, -5 };
        yield return new object[] { new int[] { 6, 0, 9, 0, 10, 0 }, 0 };
    }

    [Theory]
    [MemberData(nameof(Min_Int_TestData))]
    public void Min_Int(int[] sourceArray, int expected)
    {
        Span<int> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<int> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_Int_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<int>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<int>.Empty.Min());
    }

    public static IEnumerable<object[]> Min_UInt_TestData()
    {
        yield return new object[] { new uint[] { 42 }, 42u };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (uint)x).ToArray(), 1u };
        yield return new object[] { new uint[] { 1000, 200, 10, 200, 1000 }, 10u };
        yield return new object[] { new uint[] { 3000, 100, 200, 1000 }, 100u };
        yield return new object[] { new uint[] { 3000, 100, 200, 1000, uint.MinValue }, uint.MinValue };
        yield return new object[] { new uint[] { 20 }, 20u };
        yield return new object[] { Enumerable.Repeat(2u, 50).ToArray(), 2u };
        yield return new object[] { new uint[] { 6, 9, 10, 7, 8 }, 6u };
        yield return new object[] { new uint[] { 6, 9, 10, 5, 3 }, 3u };
        yield return new object[] { new uint[] { 6, 0, 9, 0, 10, 0 }, 0u };
    }

    [Theory]
    [MemberData(nameof(Min_UInt_TestData))]
    public void Min_UInt(uint[] sourceArray, uint expected)
    {
        Span<uint> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<uint> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_UInt_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<uint>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<uint>.Empty.Min());
    }

    public static IEnumerable<object[]> Min_Long_TestData()
    {
        yield return new object[] { new long[] { 42L }, 42L };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (long)i).ToArray(), 1L };
        yield return new object[] { new long[] { -1, -10, 10, 200, 1000 }, -10L };
        yield return new object[] { new long[] { 3000, 100, 200, 1000 }, 100L };
        yield return new object[] { new long[] { 3000, 100, 200, 1000, long.MinValue }, long.MinValue };
        yield return new object[] { new long[] { int.MaxValue + 10L }, int.MaxValue + 10L };
        yield return new object[] { Enumerable.Repeat(500L, 5).ToArray(), 500L };
        yield return new object[] { new long[] { -250, 49, 130, 47, 28 }, -250L };
        yield return new object[] { new long[] { 6, 9, 10, 0, -int.MaxValue - 50L }, -int.MaxValue - 50L };
        yield return new object[] { new long[] { 6, -5, 9, -5, 10, -5 }, -5 };
    }

    [Theory]
    [MemberData(nameof(Min_Long_TestData))]
    public void Min_Long(long[] sourceArray, long expected)
    {
        Span<long> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<long> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_Long_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<long>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<long>.Empty.Min());
    }

    public static IEnumerable<object[]> Min_ULong_TestData()
    {
        yield return new object[] { new ulong[] { 42 }, 42ul };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (ulong)x).ToArray(), 1ul };
        yield return new object[] { new ulong[] { 1000, 200, 10, 200, 1000 }, 10ul };
        yield return new object[] { new ulong[] { 3000, 100, 200, 1000 }, 100ul };
        yield return new object[] { new ulong[] { 3000, 100, 200, 1000, ulong.MinValue }, ulong.MinValue };
        yield return new object[] { new ulong[] { 20 }, 20ul };
        yield return new object[] { new ulong[] { uint.MaxValue + 10ul }, uint.MaxValue + 10ul };
        yield return new object[] { Enumerable.Repeat(2ul, 50).ToArray(), 2ul };
        yield return new object[] { new ulong[] { 6, 9, 10, 7, 8 }, 6ul };
        yield return new object[] { new ulong[] { 6, 9, 10, 5, 3 }, 3ul };
        yield return new object[] { new ulong[] { 6, 0, 9, 0, 10, 0 }, 0ul };
    }

    [Theory]
    [MemberData(nameof(Min_ULong_TestData))]
    public void Min_ULong(ulong[] sourceArray, ulong expected)
    {
        Span<ulong> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<ulong> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_ULong_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<ulong>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<ulong>.Empty.Min());
    }

    public static IEnumerable<object[]> Min_NInt_TestData()
    {
        yield return new object[] { new nint[] { 42 }, (nint)42 };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (nint)i).ToArray(), (nint)1 };
        yield return new object[] { new nint[] { -1, -10, 10, 200, 1000 }, (nint)(-10) };
        yield return new object[] { new nint[] { 3000, 100, 200, 1000 }, (nint)100 };
        yield return new object[] { new nint[] { 3000, 100, 200, 1000, nint.MinValue }, nint.MinValue };
        yield return new object[] { Enumerable.Repeat((nint)500, 5).ToArray(), (nint)500 };
        yield return new object[] { new nint[] { -250, 49, 130, 47, 28 }, (nint)(-250) };
        yield return new object[] { new nint[] { 6, -5, 9, -5, 10, -5 }, (nint)(-5) };
    }

    [Theory]
    [MemberData(nameof(Min_NInt_TestData))]
    public void Min_NInt(nint[] sourceArray, nint expected)
    {
        Span<nint> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<nint> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_NInt_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<nint>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<nint>.Empty.Min());
    }

    public static IEnumerable<object[]> Min_NUInt_TestData()
    {
        yield return new object[] { new nuint[] { 42 }, (nuint)42 };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (nuint)x).ToArray(), (nuint)1 };
        yield return new object[] { new nuint[] { 1000, 200, 10, 200, 1000 }, (nuint)10 };
        yield return new object[] { new nuint[] { 3000, 100, 200, 1000 }, (nuint)100 };
        yield return new object[] { new nuint[] { 3000, 100, 200, 1000, nuint.MinValue }, nuint.MinValue };
        yield return new object[] { new nuint[] { 20 }, (nuint)20 };
        yield return new object[] { Enumerable.Repeat((nuint)2, 50).ToArray(), (nuint)2 };
        yield return new object[] { new nuint[] { 6, 9, 10, 7, 8 }, (nuint)6 };
        yield return new object[] { new nuint[] { 6, 9, 10, 5, 3 }, (nuint)3 };
        yield return new object[] { new nuint[] { 6, 0, 9, 0, 10, 0 }, (nuint)0 };
    }

    [Theory]
    [MemberData(nameof(Min_NUInt_TestData))]
    public void Min_NUInt(nuint[] sourceArray, nuint expected)
    {
        Span<nuint> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<nuint> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_NUInt_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<nuint>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<nuint>.Empty.Min());
    }

    public static IEnumerable<object[]> Min_Float_TestData()
    {
        yield return new object[] { new[] { 42f }, 42f };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (float)i).ToArray(), 1f };
        yield return new object[] { new float[] { -1, -10, 10, 200, 1000 }, -10f };
        yield return new object[] { new float[] { 3000, 100, 200, 1000 }, 100 };
        yield return new object[] { new float[] { 3000, 100, 200, 1000, float.MinValue }, float.MinValue };

        yield return new object[] { new[] { 5.5f }, 5.5f };
        yield return new object[] { Enumerable.Repeat(float.NaN, 5).ToArray(), float.NaN };
        yield return new object[] { new float[] { -2.5f, 4.9f, 130f, 4.7f, 28f }, -2.5f };
        yield return new object[] { new float[] { 6.8f, 9.4f, 10f, 0, -5.6f }, -5.6f };
        yield return new object[] { new float[] { -5.5f, float.NegativeInfinity, 9.9f, float.NegativeInfinity }, float.NegativeInfinity };

        yield return new object[] { new float[] { float.NaN, 6.8f, 9.4f, 10f, 0, -5.6f }, float.NaN };
        yield return new object[] { new float[] { 6.8f, 9.4f, 10f, 0, -5.6f, float.NaN }, float.NaN };
        yield return new object[] { new float[] { float.NaN, float.NegativeInfinity }, float.NaN };
        yield return new object[] { new float[] { float.NegativeInfinity, float.NaN }, float.NaN };

        // Normally NaN < anything is false, as is anything < NaN
        // However, this leads to some irksome outcomes in Min and Max.
        // If we use those semantics then Min(NaN, 5.0) is NaN, but
        // Min(5.0, NaN) is 5.0!  To fix this, we impose a total
        // ordering where NaN is smaller than every value, including
        // negative infinity.
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (float)i).Concat(new[] { float.NaN }).ToArray(), float.NaN };
        yield return new object[] { new float[] { -1F, -10, float.NaN, 10, 200, 1000 }, float.NaN };
        yield return new object[] { new float[] { float.MinValue, 3000F, 100, 200, float.NaN, 1000 }, float.NaN };

        yield return new object[] { Enumerable.Repeat(float.NaN, 3).ToArray(), float.NaN };
    }

    [Theory]
    [MemberData(nameof(Min_Float_TestData))]
    public void Min_Float(float[] sourceArray, float expected)
    {
        Span<float> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<float> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_Float_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<float>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<float>.Empty.Min());
    }

    public static IEnumerable<object[]> Min_Double_TestData()
    {
        yield return new object[] { new[] { 42.0 }, 42.0 };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (double)i).ToArray(), 1.0 };
        yield return new object[] { new double[] { -1, -10, 10, 200, 1000 }, -10.0 };
        yield return new object[] { new double[] { 3000, 100, 200, 1000 }, 100.0 };
        yield return new object[] { new double[] { 3000, 100, 200, 1000, double.MinValue }, double.MinValue };

        yield return new object[] { new[] { 5.5 }, 5.5 };
        yield return new object[] { new double[] { -2.5, 4.9, 130, 4.7, 28 }, -2.5 };
        yield return new object[] { new double[] { 6.8, 9.4, 10, 0, -5.6 }, -5.6 };
        yield return new object[] { new double[] { -5.5, double.NegativeInfinity, 9.9, double.NegativeInfinity }, double.NegativeInfinity };

        yield return new object[] { new double[] { double.NaN, 6.8, 9.4, 10, 0, -5.6 }, double.NaN };
        yield return new object[] { new double[] { 6.8, 9.4, 10, 0, -5.6, double.NaN }, double.NaN };
        yield return new object[] { new double[] { double.NaN, double.NegativeInfinity }, double.NaN };
        yield return new object[] { new double[] { double.NegativeInfinity, double.NaN }, double.NaN };

        // Normally NaN < anything is false, as is anything < NaN
        // However, this leads to some irksome outcomes in Min and Max.
        // If we use those semantics then Min(NaN, 5.0) is NaN, but
        // Min(5.0, NaN) is 5.0!  To fix this, we impose a total
        // ordering where NaN is smaller than every value, including
        // negative infinity.
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (double)i).Concat(new[] { double.NaN }).ToArray(), double.NaN };
        yield return new object[] { new double[] { -1, -10, double.NaN, 10, 200, 1000 }, double.NaN };
        yield return new object[] { new double[] { double.MinValue, 3000F, 100, 200, double.NaN, 1000 }, double.NaN };

        yield return new object[] { Enumerable.Repeat(double.NaN, 3).ToArray(), double.NaN };
    }

    [Theory]
    [MemberData(nameof(Min_Double_TestData))]
    public void Min_Double(double[] sourceArray, double expected)
    {
        Span<double> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<double> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }


    [Fact]
    public void Min_Double_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<double>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<double>.Empty.Min());
    }

    public static IEnumerable<object[]> Min_Decimal_TestData()
    {
        yield return new object[] { new[] { 42m }, 42m };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (decimal)i).ToArray(), 1m };
        yield return new object[] { new decimal[] { -1, -10, 10, 200, 1000 }, -10m };
        yield return new object[] { new decimal[] { 3000, 100, 200, 1000 }, 100m };
        yield return new object[] { new decimal[] { 3000, 100, 200, 1000, decimal.MinValue }, decimal.MinValue };

        yield return new object[] { new[] { 5.5m }, 5.5m };
        yield return new object[] { Enumerable.Repeat(-3.4m, 5).ToArray(), -3.4m };
        yield return new object[] { new decimal[] { -2.5m, 4.9m, 130m, 4.7m, 28m }, -2.5m };
        yield return new object[] { new decimal[] { 6.8m, 9.4m, 10m, 0m, 0m, decimal.MinValue }, decimal.MinValue };
        yield return new object[] { new decimal[] { -5.5m, 0m, 9.9m, -5.5m, 5m }, -5.5m };
    }

    [Theory]
    [MemberData(nameof(Min_Decimal_TestData))]
    public void Min_Decimal(decimal[] sourceArray, decimal expected)
    {
        Span<decimal> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<decimal> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_Decimal_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<decimal>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<decimal>.Empty.Min());
    }

    public static IEnumerable<object?[]> Min_NullableInt_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => (int?)i).ToArray(), 1 };
        yield return new object?[] { new int?[] { null, -1, -10, 10, 200, 1000 }, -10 };
        yield return new object?[] { new int?[] { null, 3000, 100, 200, 1000 }, 100 };
        yield return new object?[] { new int?[] { null, 3000, 100, 200, 1000, int.MinValue }, int.MinValue };
        yield return new object?[] { Enumerable.Repeat(default(int?), 100).ToArray(), null };
        yield return new object?[] { new int?[] { 42 }, 42 };

        yield return new object?[] { Array.Empty<int?>(), null };
        yield return new object?[] { new int?[] { int.MaxValue, }, int.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(int?), 5).ToArray(), null };
        yield return new object?[] { new int?[] { 6, null, 9, 10, null, 7, 8 }, 6 };
        yield return new object?[] { new int?[] { null, null, null, null, null, -5 }, -5 };
        yield return new object?[] { new int?[] { 6, null, null, 0, 9, 0, 10, 0 }, 0 };
    }

    [Theory]
    [MemberData(nameof(Min_NullableInt_TestData))]
    public void Min_NullableInt(int?[] sourceArray, int? expected)
    {
        Span<int?> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<int?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    public static IEnumerable<object?[]> Min_NullableLong_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => (long?)i).ToArray(), 1L };
        yield return new object?[] { new long?[] { null, -1, -10, 10, 200, 1000 }, -10L };
        yield return new object?[] { new long?[] { null, 3000, 100, 200, 1000 }, 100L };
        yield return new object?[] { new long?[] { null, 3000, 100, 200, 1000, long.MinValue }, long.MinValue };
        yield return new object?[] { Enumerable.Repeat(default(long?), 100).ToArray(), null };
        yield return new object?[] { new long?[] { 42L }, 42L };

        yield return new object?[] { Array.Empty<long?>(), null };
        yield return new object?[] { new long?[] { long.MaxValue }, long.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(long?), 5).ToArray(), null };
        yield return new object?[] { new long?[] { long.MinValue, null, 9, 10, null, 7, 8 }, long.MinValue };
        yield return new object?[] { new long?[] { null, null, null, null, null, -long.MaxValue }, -long.MaxValue };
        yield return new object?[] { new long?[] { 6, null, null, 0, 9, 0, 10, 0 }, 0L };
    }

    [Theory]
    [MemberData(nameof(Min_NullableLong_TestData))]
    public void Min_NullableLong(long?[] sourceArray, long? expected)
    {
        Span<long?> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<long?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    public static IEnumerable<object?[]> Min_NullableFloat_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => (float?)i).ToArray(), 1f };
        yield return new object?[] { new float?[] { null, -1, -10, 10, 200, 1000 }, -10f };
        yield return new object?[] { new float?[] { null, 3000, 100, 200, 1000 }, 100f };
        yield return new object?[] { new float?[] { null, 3000, 100, 200, 1000, float.MinValue }, float.MinValue };
        yield return new object?[] { new float?[] { 42f }, 42f };

        yield return new object?[] { Array.Empty<float?>(), null };
        yield return new object?[] { new float?[] { float.MinValue }, float.MinValue };
        yield return new object?[] { Enumerable.Repeat(default(float?), 100).ToArray(), null };
        yield return new object?[] { new float?[] { -4.50f, null, 10.98f, null, 7.5f, 8.6f }, -4.5f };
        yield return new object?[] { new float?[] { null, null, null, null, null, 0f }, 0f };
        yield return new object?[] { new float?[] { 6.4f, null, null, -0.5f, 9.4f, -0.5f, 10.9f, -0.5f }, -0.5f };

        yield return new object?[] { new float?[] { float.NaN, 6.8f, 9.4f, 10f, 0, null, -5.6f }, float.NaN };
        yield return new object?[] { new float?[] { 6.8f, 9.4f, 10f, 0, null, -5.6f, float.NaN }, float.NaN };
        yield return new object?[] { new float?[] { float.NaN, float.NegativeInfinity }, float.NaN };
        yield return new object?[] { new float?[] { float.NegativeInfinity, float.NaN }, float.NaN };
        yield return new object?[] { new float?[] { float.NaN, null, null, null }, float.NaN };
        yield return new object?[] { new float?[] { null, null, null, float.NaN }, float.NaN };
        yield return new object?[] { new float?[] { null, float.NaN, null }, float.NaN };

        yield return new object?[] { new float?[] { float.NaN, float.NaN, float.NaN }, float.NaN };
    }

    [Theory]
    [MemberData(nameof(Min_NullableFloat_TestData))]
    public void Min_NullableFloat(float?[] sourceArray, float? expected)
    {
        Span<float?> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<float?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    public static IEnumerable<object?[]> Min_NullableDouble_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => (double?)i).ToArray(), 1.0 };
        yield return new object?[] { new double?[] { null, -1, -10, 10, 200, 1000 }, -10.0 };
        yield return new object?[] { new double?[] { null, 3000, 100, 200, 1000 }, 100.0 };
        yield return new object?[] { new double?[] { null, 3000, 100, 200, 1000, double.MinValue }, double.MinValue };
        yield return new object?[] { Enumerable.Repeat(default(double?), 100).ToArray(), null };
        yield return new object?[] { new double?[] { 42.0 }, 42.0 };

        yield return new object?[] { Array.Empty<double?>(), null };
        yield return new object?[] { new double?[] { double.MinValue }, double.MinValue };
        yield return new object?[] { Enumerable.Repeat(default(double?), 5).ToArray(), null };
        yield return new object?[] { new double?[] { -4.50, null, 10.98, null, 7.5, 8.6 }, -4.5 };
        yield return new object?[] { new double?[] { null, null, null, null, null, 0 }, 0.0 };
        yield return new object?[] { new double?[] { 6.4, null, null, -0.5, 9.4, -0.5, 10.9, -0.5 }, -0.5 };

        yield return new object?[] { new double?[] { double.NaN, 6.8, 9.4, 10.0, 0.0, null, -5.6 }, double.NaN };
        yield return new object?[] { new double?[] { 6.8, 9.4, 10, 0.0, null, -5.6f, double.NaN }, double.NaN };
        yield return new object?[] { new double?[] { double.NaN, double.NegativeInfinity }, double.NaN };
        yield return new object?[] { new double?[] { double.NegativeInfinity, double.NaN }, double.NaN };
        yield return new object?[] { new double?[] { double.NaN, null, null, null }, double.NaN };
        yield return new object?[] { new double?[] { null, null, null, double.NaN }, double.NaN };
        yield return new object?[] { new double?[] { null, double.NaN, null }, double.NaN };

        yield return new object?[] { new double?[] { double.NaN, double.NaN, double.NaN }, double.NaN };
    }

    [Theory]
    [MemberData(nameof(Min_NullableDouble_TestData))]
    public void Min_NullableDouble(double?[] sourceArray, double? expected)
    {
        Span<double?> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<double?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    public static IEnumerable<object?[]> Min_NullableDecimal_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => (decimal?)i).ToArray(), 1m };
        yield return new object?[] { new decimal?[] { null, -1, -10, 10, 200, 1000 }, -10m };
        yield return new object?[] { new decimal?[] { null, 3000, 100, 200, 1000 }, 100m };
        yield return new object?[] { new decimal?[] { null, 3000, 100, 200, 1000, decimal.MinValue }, decimal.MinValue };
        yield return new object?[] { Enumerable.Repeat(default(decimal?), 100).ToArray(), null };
        yield return new object?[] { new decimal?[] { 42m }, 42m };

        yield return new object?[] { Array.Empty<decimal?>(), null };
        yield return new object?[] { new decimal?[] { decimal.MaxValue }, decimal.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(decimal?), 5).ToArray(), null };
        yield return new object?[] { new decimal?[] { -4.50m, null, null, 10.98m, null, 7.5m, 8.6m }, -4.5m };
        yield return new object?[] { new decimal?[] { null, null, null, null, null, 0m }, 0m };
        yield return new object?[] { new decimal?[] { 6.4m, null, null, decimal.MinValue, 9.4m, decimal.MinValue, 10.9m, decimal.MinValue }, decimal.MinValue };
    }

    [Theory]
    [MemberData(nameof(Min_NullableDecimal_TestData))]
    public void Min_NullableDecimal(decimal?[] sourceArray, decimal? expected)
    {
        Span<decimal?> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<decimal?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    public static IEnumerable<object[]> Min_DateTime_TestData()
    {
        yield return new object[] { Enumerable.Range(1, 10).Select(i => new DateTime(2000, 1, i)).ToArray(), new DateTime(2000, 1, 1) };
        yield return new object[] { new DateTime[] { new DateTime(2000, 12, 1), new DateTime(2000, 1, 1), new DateTime(2000, 1, 12) }, new DateTime(2000, 1, 1) };

        DateTime[] hundred = new DateTime[]
        {
                new DateTime(3000, 1, 1),
                new DateTime(100, 1, 1),
                new DateTime(200, 1, 1),
                new DateTime(1000, 1, 1)
        };
        yield return new object[] { hundred, new DateTime(100, 1, 1) };
        yield return new object[] { hundred.Concat(new[] { DateTime.MinValue }).ToArray(), DateTime.MinValue };
    }

    [Theory]
    [MemberData(nameof(Min_DateTime_TestData))]
    public void Min_DateTime(DateTime[] sourceArray, DateTime expected)
    {
        Span<DateTime> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<DateTime> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_DateTime_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<DateTime>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<DateTime>.Empty.Min());
    }

    public static IEnumerable<object?[]> Min_String_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => i.ToString()).ToArray(), "1" };
        yield return new object?[] { new string?[] { "Alice", "Bob", "Charlie", "Eve", "Mallory", "Trent", "Victor" }, "Alice" };
        yield return new object?[] { new string?[] { null, "Charlie", null, "Victor", "Trent", null, "Eve", "Alice", "Mallory", "Bob" }, "Alice" };

        yield return new object?[] { Array.Empty<string?>(), null };
        yield return new object?[] { new string?[] { "Hello" }, "Hello" };
        yield return new object?[] { Enumerable.Repeat("hi", 5).ToArray(), "hi" };
        yield return new object?[] { new string?[] { "aaa", "abcd", "bark", "temp", "cat" }, "aaa" };
        yield return new object?[] { new string?[] { null, null, null, null, "aAa" }, "aAa" };
        yield return new object?[] { new string?[] { "ooo", "www", "www", "ooo", "ooo", "ppp" }, "ooo" };
        yield return new object?[] { Enumerable.Repeat(default(string?), 5).ToArray(), null };
    }

    [Theory]
    [MemberData(nameof(Min_String_TestData))]
    public void Min_String(string?[] sourceArray, string? expected)
    {
        Span<string?> source = sourceArray;
        Assert.Equal(expected, source.Min());

        // ---------------------------------

        ReadOnlySpan<string?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min());
    }

    [Fact]
    public void Min_Bool_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<bool>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<bool>.Empty.Min());
    }

    public static IEnumerable<object?[]> Min_Generic_TestData()
    {
        yield return WrapArgs(
            source: Array.Empty<int?>(),
            comparer: null,
            expected: null);

        yield return WrapArgs(
            source: Array.Empty<int?>(),
            comparer: Comparer<int?>.Create((_, _) => 0),
            expected: null);

        yield return WrapArgs(
            source: Enumerable.Range(0, 10),
            comparer: null,
            expected: 0);

        yield return WrapArgs(
            source: Enumerable.Range(0, 10),
            comparer: Comparer<int>.Create((x, y) => -x.CompareTo(y)),
            expected: 9);

        yield return WrapArgs(
            source: Enumerable.Range(0, 10),
            comparer: Comparer<int>.Create((x, y) => 0),
            expected: 0);

        yield return WrapArgs(
            source: new string[] { "Aardvark", "Zyzzyva", "Zebra", "Antelope" },
            comparer: null,
            expected: "Aardvark");

        yield return WrapArgs(
            source: new string[] { "Aardvark", "Zyzzyva", "Zebra", "Antelope" },
            comparer: Comparer<string>.Create((x, y) => -x.CompareTo(y)),
            expected: "Zyzzyva");

        object?[] WrapArgs<TSource>(IEnumerable<TSource> source, IComparer<TSource>? comparer, TSource? expected)
            => new object?[] { source.ToArray(), comparer, expected };
    }

    [Theory]
    [MemberData(nameof(Min_Generic_TestData))]
    public static void Min_Generic<TSource>(TSource[] sourceArray, IComparer<TSource>? comparer, TSource? expected)
    {
        Span<TSource> source = sourceArray;
        Assert.Equal(expected, source.Min(comparer));

        // ---------------------------------

        ReadOnlySpan<TSource> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Min(comparer));
    }

    [Fact]
    public static void Min_Generic_EmptyStructSource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<DateTime>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<DateTime>.Empty.Min());
        Assert.Throws<InvalidOperationException>(() => Span<DateTime>.Empty.Min(comparer: null));
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<DateTime>.Empty.Min(comparer: null));
        Assert.Throws<InvalidOperationException>(() => Span<DateTime>.Empty.Min(Comparer<DateTime>.Create((_, _) => 0)));
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<DateTime>.Empty.Min(Comparer<DateTime>.Create((_, _) => 0)));
    }

    public static IEnumerable<object[]> UnsafeMin_Float_TestData()
    {
        yield return new object[] { new[] { 42f }, 42f };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (float)i).ToArray(), 1f };
        yield return new object[] { new float[] { -1, -10, 10, 200, 1000 }, -10f };
        yield return new object[] { new float[] { 3000, 100, 200, 1000 }, 100 };
        yield return new object[] { new float[] { 3000, 100, 200, 1000, float.MinValue }, float.MinValue };

        yield return new object[] { new[] { 5.5f }, 5.5f };
        yield return new object[] { new float[] { -2.5f, 4.9f, 130f, 4.7f, 28f }, -2.5f };
        yield return new object[] { new float[] { 6.8f, 9.4f, 10f, 0, -5.6f }, -5.6f };
        yield return new object[] { new float[] { -5.5f, float.NegativeInfinity, 9.9f, float.NegativeInfinity }, float.NegativeInfinity };
    }

    [Theory]
    [MemberData(nameof(UnsafeMin_Float_TestData))]
    public void UnsafeMin_Float(float[] sourceArray, float expected)
    {
        Span<float> source = sourceArray;
        Assert.Equal(expected, source.UnsafeMin());

        // ---------------------------------

        ReadOnlySpan<float> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.UnsafeMin());
    }

    [Fact]
    public void UnsafeMin_Float_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<float>.Empty.UnsafeMin());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<float>.Empty.UnsafeMin());
    }

    public static IEnumerable<object[]> UnsafeMin_Double_TestData()
    {
        yield return new object[] { new[] { 42.0 }, 42.0 };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (double)i).ToArray(), 1.0 };
        yield return new object[] { new double[] { -1, -10, 10, 200, 1000 }, -10.0 };
        yield return new object[] { new double[] { 3000, 100, 200, 1000 }, 100.0 };
        yield return new object[] { new double[] { 3000, 100, 200, 1000, double.MinValue }, double.MinValue };

        yield return new object[] { new[] { 5.5 }, 5.5 };
        yield return new object[] { new double[] { -2.5, 4.9, 130, 4.7, 28 }, -2.5 };
        yield return new object[] { new double[] { 6.8, 9.4, 10, 0, -5.6 }, -5.6 };
        yield return new object[] { new double[] { -5.5, double.NegativeInfinity, 9.9, double.NegativeInfinity }, double.NegativeInfinity };
    }

    [Theory]
    [MemberData(nameof(UnsafeMin_Double_TestData))]
    public void UnsafeMin_Double(double[] sourceArray, double expected)
    {
        Span<double> source = sourceArray;
        Assert.Equal(expected, source.UnsafeMin());

        // ---------------------------------

        ReadOnlySpan<double> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.UnsafeMin());
    }


    [Fact]
    public void UnsafeMin_Double_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<double>.Empty.UnsafeMin());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<double>.Empty.UnsafeMin());
    }

    public static IEnumerable<object[]> UnsafeMin_Generic_TestData()
        => UnsafeMin_Float_TestData()
            .Concat(UnsafeMin_Double_TestData())
            .Concat(Min_Int_TestData());

    [Theory]
    [MemberData(nameof(UnsafeMin_Generic_TestData))]
    public static void UnsafeMin_Generic<TSource>(TSource[] sourceArray, TSource? expected)
    {
        Span<TSource> source = sourceArray;
        Assert.Equal(expected, source.UnsafeMin());

        // ---------------------------------

        ReadOnlySpan<TSource> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.UnsafeMin());
    }

    [Fact]
    public static void UnsafeMin_Generic_EmptyStructSource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<DateTime>.Empty.UnsafeMin());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<DateTime>.Empty.UnsafeMin());
    }
}
