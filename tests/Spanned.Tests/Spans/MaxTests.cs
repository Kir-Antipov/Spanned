using System.Numerics;
using Spanned.Tests.TestUtilities;

namespace Spanned.Tests.Spans;

public class MaxTests
{
    public static IEnumerable<object[]> Max_AllTypes_TestData()
    {
        for (int length = 2; length < 65; length++)
        {
            yield return new object[] { Shuffler.Range((byte)length, length), (byte)(length + length - 1) };

            // Tests do `+ T.One`, so we should generate data up to one value below `sbyte.MaxValue`, otherwise the type overflows.
            if (length + length < sbyte.MaxValue)
            {
                yield return new object[] { Shuffler.Range((sbyte)length, length), (sbyte)(length + length - 1) };
            }

            yield return new object[] { Shuffler.Range((short)length, length), (short)(length + length - 1) };

            yield return new object[] { Shuffler.Range((ushort)length, length), (ushort)(length + length - 1) };

            yield return new object[] { Shuffler.Range(length, length), length + length - 1 };

            yield return new object[] { Shuffler.Range((uint)length, length), (uint)(length + length - 1) };

            yield return new object[] { Shuffler.Range((long)length, length), (long)(length + length - 1) };

            yield return new object[] { Shuffler.Range((ulong)length, length), (ulong)(length + length - 1) };

            yield return new object[] { Shuffler.Range((nint)length, length), (nint)(length + length - 1) };

            yield return new object[] { Shuffler.Range((nuint)length, length), (nuint)(length + length - 1) };

            yield return new object[] { Shuffler.Range((float)length, length), (float)(length + length - 1) };

            yield return new object[] { Shuffler.Range((double)length, length), (double)(length + length - 1) };

            yield return new object[] { Shuffler.Range((decimal)length, length), (decimal)(length + length - 1) };
        }
    }

    [Theory]
    [MemberData(nameof(Max_AllTypes_TestData))]
    public void Max_AllTypes<T>(T[] sourceArray, T expected) where T : INumber<T>
    {
        Span<T> source = sourceArray;

        Assert.Equal(source.Max(), source.Max());
        Assert.Equal(expected, source.Max());
        Assert.Equal(expected, source.Max(comparer: null));
        Assert.Equal(expected, source.Max(Comparer<T>.Default));
        Assert.Equal(expected, source.Max(Comparer<T>.Create(Comparer<T>.Default.Compare)));

        T first = source[0];
        Assert.Equal(first, source.Max(Comparer<T>.Create((x, y) => x == first ? 1 : -1)));

        ReadOnlySpan<T> sourcePlusOne = Array.ConvertAll(source.ToArray(), x => x + T.One);
        Assert.Equal(expected + T.One, sourcePlusOne.Max());

        // ---------------------------------

        ReadOnlySpan<T> readOnlySource = sourceArray;

        Assert.Equal(readOnlySource.Max(), readOnlySource.Max());
        Assert.Equal(expected, readOnlySource.Max());
        Assert.Equal(expected, readOnlySource.Max(comparer: null));
        Assert.Equal(expected, readOnlySource.Max(Comparer<T>.Default));
        Assert.Equal(expected, readOnlySource.Max(Comparer<T>.Create(Comparer<T>.Default.Compare)));

        T readOnlyFirst = readOnlySource[0];
        Assert.Equal(readOnlyFirst, readOnlySource.Max(Comparer<T>.Create((x, y) => x == readOnlyFirst ? 1 : -1)));

        ReadOnlySpan<T> readOnlySourcePlusOne = Array.ConvertAll(readOnlySource.ToArray(), x => x + T.One);
        Assert.Equal(expected + T.One, readOnlySourcePlusOne.Max());
    }

    public static IEnumerable<object[]> Max_Byte_TestData()
    {
        yield return new object[] { new byte[] { 42 }, (byte)42 };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (byte)x).ToArray(), (byte)100 };
        yield return new object[] { new byte[] { 28, 13, 10, 200, 255 }, (byte)255 };
        yield return new object[] { new byte[] { 255, 100, 200, 235 }, (byte)255 };
        yield return new object[] { new byte[] { 255, 100, 200, 235, byte.MaxValue }, byte.MaxValue };
        yield return new object[] { new byte[] { 20 }, (byte)20 };
        yield return new object[] { Enumerable.Repeat((byte)2, 50).ToArray(), (byte)2 };
        yield return new object[] { new byte[] { 6, 9, 10, 7, 8 }, (byte)10 };
        yield return new object[] { new byte[] { 6, 9, 9, 5, 3 }, (byte)9 };
        yield return new object[] { new byte[] { 6, 0, 9, 0, 10, 0 }, (byte)10 };
    }

    [Theory]
    [MemberData(nameof(Max_Byte_TestData))]
    public void Max_Byte(byte[] sourceArray, byte expected)
    {
        Span<byte> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<byte> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_Byte_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<byte>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<byte>.Empty.Max());
    }

    public static IEnumerable<object[]> Max_SByte_TestData()
    {
        yield return new object[] { new sbyte[] { 42 }, (sbyte)42 };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (sbyte)x).ToArray(), (sbyte)100 };
        yield return new object[] { new sbyte[] { -1, -10, 10, 100, 127 }, (sbyte)127 };
        yield return new object[] { new sbyte[] { 118, 100, 111, 127 }, (sbyte)127 };
        yield return new object[] { new sbyte[] { 118, 100, 111, 127, sbyte.MaxValue }, sbyte.MaxValue };
        yield return new object[] { new sbyte[] { 20 }, (sbyte)20 };
        yield return new object[] { Enumerable.Repeat((sbyte)-2, 50).ToArray(), (sbyte)-2 };
        yield return new object[] { new sbyte[] { 6, 9, 10, 7, 8 }, (sbyte)10 };
        yield return new object[] { new sbyte[] { 6, 9, 9, 0, -5 }, (sbyte)9 };
        yield return new object[] { new sbyte[] { 6, 0, 9, 0, 10, 0 }, (sbyte)10 };
    }

    [Theory]
    [MemberData(nameof(Max_SByte_TestData))]
    public void Max_SByte(sbyte[] sourceArray, sbyte expected)
    {
        Span<sbyte> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<sbyte> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_SByte_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<sbyte>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<sbyte>.Empty.Max());
    }

    public static IEnumerable<object[]> Max_Short_TestData()
    {
        yield return new object[] { new short[] { 42 }, (short)42 };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (short)x).ToArray(), (short)100 };
        yield return new object[] { new short[] { -1, -10, 10, 200, 1000 }, (short)1000 };
        yield return new object[] { new short[] { 3000, 100, 200, 1000 }, (short)3000 };
        yield return new object[] { new short[] { 3000, 100, 200, 1000, short.MaxValue }, short.MaxValue };
        yield return new object[] { new short[] { 20 }, (short)20 };
        yield return new object[] { Enumerable.Repeat((short)-2, 50).ToArray(), (short)-2 };
        yield return new object[] { new short[] { 6, 9, 10, 7, 8 }, (short)10 };
        yield return new object[] { new short[] { 6, 9, 9, 0, -5 }, (short)9 };
        yield return new object[] { new short[] { 6, 0, 9, 0, 10, 0 }, (short)10 };
    }

    [Theory]
    [MemberData(nameof(Max_Short_TestData))]
    public void Max_Short(short[] sourceArray, short expected)
    {
        Span<short> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<short> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    public static IEnumerable<object[]> Max_UShort_TestData()
    {
        yield return new object[] { new ushort[] { 42 }, (ushort)42u };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (ushort)x).ToArray(), (ushort)100u };
        yield return new object[] { new ushort[] { 1000, 200, 10, 200, 1000 }, (ushort)1000u };
        yield return new object[] { new ushort[] { 3000, 100, 200, 1000 }, (ushort)3000u };
        yield return new object[] { new ushort[] { 3000, 100, 200, 1000, ushort.MaxValue }, ushort.MaxValue };
        yield return new object[] { new ushort[] { 20 }, 20u };
        yield return new object[] { Enumerable.Repeat((ushort)2, 50).ToArray(), (ushort)2u };
        yield return new object[] { new ushort[] { 6, 9, 10, 7, 8 }, (ushort)10u };
        yield return new object[] { new ushort[] { 6, 9, 9, 5, 3 }, (ushort)9u };
        yield return new object[] { new ushort[] { 6, 0, 9, 0, 10, 0 }, (ushort)10u };
    }

    [Theory]
    [MemberData(nameof(Max_UShort_TestData))]
    public void Max_UShort(ushort[] sourceArray, ushort expected)
    {
        Span<ushort> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<ushort> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_UShort_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<ushort>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<ushort>.Empty.Max());
    }

    [Fact]
    public void Max_Short_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<short>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<short>.Empty.Max());
    }

    public static IEnumerable<object[]> Max_Int_TestData()
    {
        yield return new object[] { new int[] { 42 }, 42 };
        yield return new object[] { Enumerable.Range(1, 100).ToArray(), 100 };
        yield return new object[] { new int[] { -1, -10, 10, 200, 1000 }, 1000 };
        yield return new object[] { new int[] { 3000, 100, 200, 1000 }, 3000 };
        yield return new object[] { new int[] { 3000, 100, 200, 1000, int.MaxValue }, int.MaxValue };
        yield return new object[] { new int[] { 20 }, 20 };
        yield return new object[] { Enumerable.Repeat(-2, 50).ToArray(), -2 };
        yield return new object[] { new int[] { 6, 9, 10, 7, 8 }, 10 };
        yield return new object[] { new int[] { 6, 9, 9, 0, -5 }, 9 };
        yield return new object[] { new int[] { 6, 0, 9, 0, 10, 0 }, 10 };
    }

    [Theory]
    [MemberData(nameof(Max_Int_TestData))]
    public void Max_Int(int[] sourceArray, int expected)
    {
        Span<int> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<int> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_Int_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<int>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<int>.Empty.Max());
    }

    public static IEnumerable<object[]> Max_UInt_TestData()
    {
        yield return new object[] { new uint[] { 42 }, 42u };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (uint)x).ToArray(), 100u };
        yield return new object[] { new uint[] { 1000, 200, 10, 200, 1000 }, 1000u };
        yield return new object[] { new uint[] { 3000, 100, 200, 1000 }, 3000u };
        yield return new object[] { new uint[] { 3000, 100, 200, 1000, uint.MaxValue }, uint.MaxValue };
        yield return new object[] { new uint[] { 20 }, 20u };
        yield return new object[] { Enumerable.Repeat(2u, 50).ToArray(), 2u };
        yield return new object[] { new uint[] { 6, 9, 10, 7, 8 }, 10u };
        yield return new object[] { new uint[] { 6, 9, 9, 5, 3 }, 9u };
        yield return new object[] { new uint[] { 6, 0, 9, 0, 10, 0 }, 10u };
    }

    [Theory]
    [MemberData(nameof(Max_UInt_TestData))]
    public void Max_UInt(uint[] sourceArray, uint expected)
    {
        Span<uint> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<uint> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_UInt_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<uint>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<uint>.Empty.Max());
    }

    public static IEnumerable<object[]> Max_Long_TestData()
    {
        yield return new object[] { new long[] { 42L }, 42L };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (long)i).ToArray(), 10L };
        yield return new object[] { new long[] { -1, -10, 10, 200, 1000 }, 1000L };
        yield return new object[] { new long[] { 3000, 100, 200, 1000 }, 3000L };
        yield return new object[] { new long[] { 3000, 100, 200, 1000, long.MaxValue }, long.MaxValue };
        yield return new object[] { new long[] { int.MaxValue + 10L }, int.MaxValue + 10L };
        yield return new object[] { Enumerable.Repeat(500L, 5).ToArray(), 500L };
        yield return new object[] { new long[] { -250, 49, 130, 47, 28 }, 130L };
        yield return new object[] { new long[] { 6, 9, 10, 0, int.MaxValue + 50L }, int.MaxValue + 50L };
        yield return new object[] { new long[] { 6, -5, 9, -5, 10, -5 }, 10 };
    }

    [Theory]
    [MemberData(nameof(Max_Long_TestData))]
    public void Max_Long(long[] sourceArray, long expected)
    {
        Span<long> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<long> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_Long_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<long>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<long>.Empty.Max());
    }

    public static IEnumerable<object[]> Max_ULong_TestData()
    {
        yield return new object[] { new ulong[] { 42 }, 42ul };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (ulong)x).ToArray(), 100ul };
        yield return new object[] { new ulong[] { 1000, 200, 10, 200, 1000 }, 1000ul };
        yield return new object[] { new ulong[] { 3000, 100, 200, 1000 }, 3000ul };
        yield return new object[] { new ulong[] { 3000, 100, 200, 1000, ulong.MaxValue }, ulong.MaxValue };
        yield return new object[] { new ulong[] { 20 }, 20ul };
        yield return new object[] { new ulong[] { uint.MaxValue + 10ul }, uint.MaxValue + 10ul };
        yield return new object[] { Enumerable.Repeat(2ul, 50).ToArray(), 2ul };
        yield return new object[] { new ulong[] { 6, 9, 10, 7, 8 }, 10ul };
        yield return new object[] { new ulong[] { 6, 9, 9, 5, 3 }, 9ul };
        yield return new object[] { new ulong[] { 6, 0, 9, 0, 10, 0 }, 10ul };
    }

    [Theory]
    [MemberData(nameof(Max_ULong_TestData))]
    public void Max_ULong(ulong[] sourceArray, ulong expected)
    {
        Span<ulong> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<ulong> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_ULong_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<ulong>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<ulong>.Empty.Max());
    }

    public static IEnumerable<object[]> Max_NInt_TestData()
    {
        yield return new object[] { new nint[] { 42 }, (nint)42 };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (nint)i).ToArray(), (nint)10 };
        yield return new object[] { new nint[] { -1, -10, 10, 200, 1000 }, (nint)1000 };
        yield return new object[] { new nint[] { 3000, 100, 200, 1000 }, (nint)3000 };
        yield return new object[] { new nint[] { 3000, 100, 200, 1000, nint.MaxValue }, nint.MaxValue };
        yield return new object[] { Enumerable.Repeat((nint)500, 5).ToArray(), (nint)500 };
        yield return new object[] { new nint[] { -250, 49, 130, 47, 28 }, (nint)130 };
        yield return new object[] { new nint[] { 6, -5, 9, -5, 10, -5 }, (nint)10 };
    }

    [Theory]
    [MemberData(nameof(Max_NInt_TestData))]
    public void Max_NInt(nint[] sourceArray, nint expected)
    {
        Span<nint> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<nint> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_NInt_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<nint>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<nint>.Empty.Max());
    }

    public static IEnumerable<object[]> Max_NUInt_TestData()
    {
        yield return new object[] { new nuint[] { 42 }, (nuint)42 };
        yield return new object[] { Enumerable.Range(1, 100).Select(x => (nuint)x).ToArray(), (nuint)100 };
        yield return new object[] { new nuint[] { 1000, 200, 10, 200, 1000 }, (nuint)1000 };
        yield return new object[] { new nuint[] { 3000, 100, 200, 1000 }, (nuint)3000 };
        yield return new object[] { new nuint[] { 3000, 100, 200, 1000, nuint.MaxValue }, nuint.MaxValue };
        yield return new object[] { new nuint[] { 20 }, (nuint)20 };
        yield return new object[] { Enumerable.Repeat((nuint)2, 50).ToArray(), (nuint)2 };
        yield return new object[] { new nuint[] { 6, 9, 10, 7, 8 }, (nuint)10 };
        yield return new object[] { new nuint[] { 6, 9, 9, 5, 3 }, (nuint)9 };
        yield return new object[] { new nuint[] { 6, 0, 9, 0, 10, 0 }, (nuint)10 };
    }

    [Theory]
    [MemberData(nameof(Max_NUInt_TestData))]
    public void Max_NUInt(nuint[] sourceArray, nuint expected)
    {
        Span<nuint> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<nuint> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_NUInt_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<nuint>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<nuint>.Empty.Max());
    }

    public static IEnumerable<object[]> Max_Float_TestData()
    {
        yield return new object[] { new[] { 42f }, 42f };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (float)i).ToArray(), 10f };
        yield return new object[] { new float[] { -1, -10, 10, 200, 1000 }, 1000f };
        yield return new object[] { new float[] { 3000, 100, 200, 1000 }, 3000 };
        yield return new object[] { new float[] { 3000, 100, 200, 1000, float.MaxValue }, float.MaxValue };

        yield return new object[] { new[] { 5.5f }, 5.5f };
        yield return new object[] { Enumerable.Repeat(float.NaN, 5).ToArray(), float.NaN };
        yield return new object[] { new float[] { -2.5f, 4.9f, 130f, 4.7f, 28f }, 130f };
        yield return new object[] { new float[] { 6.8f, 9.4f, 10f, 0, -5.6f }, 10f };
        yield return new object[] { new float[] { -5.5f, float.PositiveInfinity, 9.9f, float.PositiveInfinity }, float.PositiveInfinity };

        yield return new object[] { new float[] { float.NaN, 6.8f, 9.4f, 10f, 0, -5.6f }, 10f };
        yield return new object[] { new float[] { 6.8f, 9.4f, 9.4f, 0, -5.6f, float.NaN }, 9.4f };
        yield return new object[] { new float[] { float.NaN, float.NegativeInfinity }, float.NegativeInfinity };
        yield return new object[] { new float[] { float.NegativeInfinity, float.NaN }, float.NegativeInfinity };

        // Normally NaN < anything is false, as is anything < NaN
        // However, this leads to some irksome outcomes in Max and Max.
        // If we use those semantics then Max(NaN, 5.0) is NaN, but
        // Max(5.0, NaN) is 5.0!  To fix this, we impose a total
        // ordering where NaN is smaller than every value, including
        // negative infinity.
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (float)i).Concat(new[] { float.NaN }).ToArray(), 10f };
        yield return new object[] { new float[] { -1F, -10, float.NaN, 10, 200, 1000 }, 1000f };
        yield return new object[] { new float[] { float.MaxValue, 3000F, 100, 200, float.NaN, 1000 }, float.MaxValue };

        yield return new object[] { Enumerable.Repeat(float.NaN, 3).ToArray(), float.NaN };
    }

    [Theory]
    [MemberData(nameof(Max_Float_TestData))]
    public void Max_Float(float[] sourceArray, float expected)
    {
        Span<float> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<float> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_Float_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<float>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<float>.Empty.Max());
    }

    public static IEnumerable<object[]> Max_Double_TestData()
    {
        yield return new object[] { new[] { 42.0 }, 42.0 };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (double)i).ToArray(), 10.0 };
        yield return new object[] { new double[] { -1, -10, 10, 200, 1000 }, 1000.0 };
        yield return new object[] { new double[] { 3000, 100, 200, 1000 }, 3000.0 };
        yield return new object[] { new double[] { 3000, 100, 200, 1000, double.MaxValue }, double.MaxValue };

        yield return new object[] { new[] { 5.5 }, 5.5 };
        yield return new object[] { new double[] { -2.5, 4.9, 130, 4.7, 28 }, 130.0 };
        yield return new object[] { new double[] { 6.8, 9.4, 10, 0, -5.6 }, 10.0 };
        yield return new object[] { new double[] { -5.5, double.PositiveInfinity, 9.9, double.PositiveInfinity }, double.PositiveInfinity };

        yield return new object[] { new double[] { double.NaN, 6.8, 9.4, 10, 0, -5.6 }, 10.0 };
        yield return new object[] { new double[] { 6.8, 9.4, 9.4, 0, -5.6, double.NaN }, 9.4 };
        yield return new object[] { new double[] { double.NaN, double.NegativeInfinity }, double.NegativeInfinity };
        yield return new object[] { new double[] { double.NegativeInfinity, double.NaN }, double.NegativeInfinity };

        // Normally NaN < anything is false, as is anything < NaN
        // However, this leads to some irksome outcomes in Max and Max.
        // If we use those semantics then Max(NaN, 5.0) is NaN, but
        // Max(5.0, NaN) is 5.0!  To fix this, we impose a total
        // ordering where NaN is smaller than every value, including
        // negative infinity.
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (double)i).Concat(new[] { double.NaN }).ToArray(), 10.0 };
        yield return new object[] { new double[] { -1, -10, double.NaN, 10, 200, 1000 }, 1000.0 };
        yield return new object[] { new double[] { double.MaxValue, 3000F, 100, 200, double.NaN, 1000 }, double.MaxValue };

        yield return new object[] { Enumerable.Repeat(double.NaN, 3).ToArray(), double.NaN };
    }

    [Theory]
    [MemberData(nameof(Max_Double_TestData))]
    public void Max_Double(double[] sourceArray, double expected)
    {
        Span<double> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<double> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }


    [Fact]
    public void Max_Double_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<double>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<double>.Empty.Max());
    }

    public static IEnumerable<object[]> Max_Decimal_TestData()
    {
        yield return new object[] { new[] { 42m }, 42m };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (decimal)i).ToArray(), 10m };
        yield return new object[] { new decimal[] { -1, -10, 10, 200, 1000 }, 1000m };
        yield return new object[] { new decimal[] { 3000, 100, 200, 1000 }, 3000m };
        yield return new object[] { new decimal[] { 3000, 100, 200, 1000, decimal.MaxValue }, decimal.MaxValue };

        yield return new object[] { new[] { 5.5m }, 5.5m };
        yield return new object[] { Enumerable.Repeat(-3.4m, 5).ToArray(), -3.4m };
        yield return new object[] { new decimal[] { -2.5m, 4.9m, 130m, 4.7m, 28m }, 130m };
        yield return new object[] { new decimal[] { 6.8m, 9.4m, 10m, 0m, 0m, decimal.MaxValue }, decimal.MaxValue };
        yield return new object[] { new decimal[] { -5.5m, 0m, 9.9m, -5.5m, 5m }, 9.9m };
    }

    [Theory]
    [MemberData(nameof(Max_Decimal_TestData))]
    public void Max_Decimal(decimal[] sourceArray, decimal expected)
    {
        Span<decimal> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<decimal> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_Decimal_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<decimal>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<decimal>.Empty.Max());
    }

    public static IEnumerable<object?[]> Max_NullableInt_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => (int?)i).ToArray(), 10 };
        yield return new object?[] { new int?[] { null, -1, -10, 10, 200, 1000 }, 1000 };
        yield return new object?[] { new int?[] { null, 3000, 100, 200, 1000 }, 3000 };
        yield return new object?[] { new int?[] { null, 3000, 100, 200, 1000, int.MaxValue }, int.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(int?), 100).ToArray(), null };
        yield return new object?[] { new int?[] { 42 }, 42 };

        yield return new object?[] { Array.Empty<int?>(), null };
        yield return new object?[] { new int?[] { int.MaxValue, }, int.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(int?), 5).ToArray(), null };
        yield return new object?[] { new int?[] { 6, null, 9, 10, null, 7, 8 }, 10 };
        yield return new object?[] { new int?[] { null, null, null, null, null, -5 }, -5 };
        yield return new object?[] { new int?[] { 6, null, null, 0, 9, 0, 10, 0 }, 10 };
    }

    [Theory]
    [MemberData(nameof(Max_NullableInt_TestData))]
    public void Max_NullableInt(int?[] sourceArray, int? expected)
    {
        Span<int?> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<int?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    public static IEnumerable<object?[]> Max_NullableLong_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => (long?)i).ToArray(), 10L };
        yield return new object?[] { new long?[] { null, -1, -10, 10, 200, 1000 }, 1000L };
        yield return new object?[] { new long?[] { null, 3000, 100, 200, 1000 }, 3000L };
        yield return new object?[] { new long?[] { null, 3000, 100, 200, 1000, long.MaxValue }, long.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(long?), 100).ToArray(), null };
        yield return new object?[] { new long?[] { 42L }, 42L };

        yield return new object?[] { Array.Empty<long?>(), null };
        yield return new object?[] { new long?[] { long.MaxValue }, long.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(long?), 5).ToArray(), null };
        yield return new object?[] { new long?[] { long.MaxValue, null, 9, 10, null, 7, 8 }, long.MaxValue };
        yield return new object?[] { new long?[] { null, null, null, null, null, -long.MaxValue }, -long.MaxValue };
        yield return new object?[] { new long?[] { 6, null, null, 0, 9, 0, 10, 0 }, 10L };
    }

    [Theory]
    [MemberData(nameof(Max_NullableLong_TestData))]
    public void Max_NullableLong(long?[] sourceArray, long? expected)
    {
        Span<long?> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<long?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    public static IEnumerable<object?[]> Max_NullableFloat_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => (float?)i).ToArray(), 10f };
        yield return new object?[] { new float?[] { null, -1, -10, 10, 200, 1000 }, 1000f };
        yield return new object?[] { new float?[] { null, 3000, 100, 200, 1000 }, 3000f };
        yield return new object?[] { new float?[] { null, 3000, 100, 200, 1000, float.MaxValue }, float.MaxValue };
        yield return new object?[] { new float?[] { 42f }, 42f };

        yield return new object?[] { Array.Empty<float?>(), null };
        yield return new object?[] { new float?[] { float.MaxValue }, float.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(float?), 100).ToArray(), null };
        yield return new object?[] { new float?[] { -4.50f, null, 10.98f, null, 7.5f, 8.6f }, 10.98f };
        yield return new object?[] { new float?[] { null, null, null, null, null, 0f }, 0f };
        yield return new object?[] { new float?[] { 6.4f, null, null, -0.5f, 9.4f, -0.5f, 10.9f, -0.5f }, 10.9f };

        yield return new object?[] { new float?[] { float.NaN, 6.8f, 9.4f, 10f, 0, null, -5.6f }, 10f };
        yield return new object?[] { new float?[] { 6.8f, 9.4f, 10f, 0, null, -5.6f, float.NaN }, 10f };
        yield return new object?[] { new float?[] { float.NaN, float.NegativeInfinity }, float.NegativeInfinity };
        yield return new object?[] { new float?[] { float.NegativeInfinity, float.NaN }, float.NegativeInfinity };
        yield return new object?[] { new float?[] { float.NaN, null, null, null }, float.NaN };
        yield return new object?[] { new float?[] { null, null, null, float.NaN }, float.NaN };
        yield return new object?[] { new float?[] { null, float.NaN, null }, float.NaN };

        yield return new object?[] { new float?[] { float.NaN, float.NaN, float.NaN }, float.NaN };
    }

    [Theory]
    [MemberData(nameof(Max_NullableFloat_TestData))]
    public void Max_NullableFloat(float?[] sourceArray, float? expected)
    {
        Span<float?> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<float?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    public static IEnumerable<object?[]> Max_NullableDouble_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => (double?)i).ToArray(), 10.0 };
        yield return new object?[] { new double?[] { null, -1, -10, 10, 200, 1000 }, 1000.0 };
        yield return new object?[] { new double?[] { null, 3000, 100, 200, 1000 }, 3000.0 };
        yield return new object?[] { new double?[] { null, 3000, 100, 200, 1000, double.MaxValue }, double.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(double?), 100).ToArray(), null };
        yield return new object?[] { new double?[] { 42.0 }, 42.0 };

        yield return new object?[] { Array.Empty<double?>(), null };
        yield return new object?[] { new double?[] { double.MaxValue }, double.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(double?), 5).ToArray(), null };
        yield return new object?[] { new double?[] { -4.50, null, 10.98, null, 7.5, 8.6 }, 10.98 };
        yield return new object?[] { new double?[] { null, null, null, null, null, 0 }, 0.0 };
        yield return new object?[] { new double?[] { 6.4, null, null, -0.5, 9.4, -0.5, 10.9, -0.5 }, 10.9 };

        yield return new object?[] { new double?[] { double.NaN, 6.8, 9.4, 10.0, 0.0, null, -5.6 }, 10.0 };
        yield return new object?[] { new double?[] { 6.8, 9.4, 10, 0.0, null, -5.6f, double.NaN }, 10.0 };
        yield return new object?[] { new double?[] { double.NaN, double.NegativeInfinity }, double.NegativeInfinity };
        yield return new object?[] { new double?[] { double.NegativeInfinity, double.NaN }, double.NegativeInfinity };
        yield return new object?[] { new double?[] { double.NaN, null, null, null }, double.NaN };
        yield return new object?[] { new double?[] { null, null, null, double.NaN }, double.NaN };
        yield return new object?[] { new double?[] { null, double.NaN, null }, double.NaN };

        yield return new object?[] { new double?[] { double.NaN, double.NaN, double.NaN }, double.NaN };
    }

    [Theory]
    [MemberData(nameof(Max_NullableDouble_TestData))]
    public void Max_NullableDouble(double?[] sourceArray, double? expected)
    {
        Span<double?> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<double?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    public static IEnumerable<object?[]> Max_NullableDecimal_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => (decimal?)i).ToArray(), 10m };
        yield return new object?[] { new decimal?[] { null, -1, -10, 10, 200, 1000 }, 1000m };
        yield return new object?[] { new decimal?[] { null, 3000, 100, 200, 1000 }, 3000m };
        yield return new object?[] { new decimal?[] { null, 3000, 100, 200, 1000, decimal.MaxValue }, decimal.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(decimal?), 100).ToArray(), null };
        yield return new object?[] { new decimal?[] { 42m }, 42m };

        yield return new object?[] { Array.Empty<decimal?>(), null };
        yield return new object?[] { new decimal?[] { decimal.MaxValue }, decimal.MaxValue };
        yield return new object?[] { Enumerable.Repeat(default(decimal?), 5).ToArray(), null };
        yield return new object?[] { new decimal?[] { -4.50m, null, null, 10.98m, null, 7.5m, 8.6m }, 10.98m };
        yield return new object?[] { new decimal?[] { null, null, null, null, null, 0m }, 0m };
        yield return new object?[] { new decimal?[] { 6.4m, null, null, decimal.MaxValue, 9.4m, decimal.MaxValue, 10.9m, decimal.MaxValue }, decimal.MaxValue };
    }

    [Theory]
    [MemberData(nameof(Max_NullableDecimal_TestData))]
    public void Max_NullableDecimal(decimal?[] sourceArray, decimal? expected)
    {
        Span<decimal?> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<decimal?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    public static IEnumerable<object[]> Max_DateTime_TestData()
    {
        yield return new object[] { Enumerable.Range(1, 10).Select(i => new DateTime(2000, 1, i)).ToArray(), new DateTime(2000, 1, 10) };
        yield return new object[] { new DateTime[] { new DateTime(2000, 12, 1), new DateTime(2000, 1, 1), new DateTime(2000, 1, 12) }, new DateTime(2000, 12, 1) };

        DateTime[] hundred = new DateTime[]
        {
                new DateTime(3000, 1, 1),
                new DateTime(100, 1, 1),
                new DateTime(200, 1, 1),
                new DateTime(1000, 1, 1)
        };
        yield return new object[] { hundred, new DateTime(3000, 1, 1) };
        yield return new object[] { hundred.Concat(new[] { DateTime.MaxValue }).ToArray(), DateTime.MaxValue };
    }

    [Theory]
    [MemberData(nameof(Max_DateTime_TestData))]
    public void Max_DateTime(DateTime[] sourceArray, DateTime expected)
    {
        Span<DateTime> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<DateTime> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_DateTime_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<DateTime>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<DateTime>.Empty.Max());
    }

    public static IEnumerable<object?[]> Max_String_TestData()
    {
        yield return new object?[] { Enumerable.Range(1, 10).Select(i => i.ToString()).ToArray(), "9" };
        yield return new object?[] { new string?[] { "Alice", "Bob", "Charlie", "Eve", "Mallory", "Trent", "Victor" }, "Victor" };
        yield return new object?[] { new string?[] { null, "Charlie", null, "Victor", "Trent", null, "Eve", "Alice", "Mallory", "Bob" }, "Victor" };

        yield return new object?[] { Array.Empty<string?>(), null };
        yield return new object?[] { new string?[] { "Hello" }, "Hello" };
        yield return new object?[] { Enumerable.Repeat("hi", 5).ToArray(), "hi" };
        yield return new object?[] { new string?[] { "aaa", "abcd", "bark", "temp", "cat" }, "temp" };
        yield return new object?[] { new string?[] { null, null, null, null, "aAa" }, "aAa" };
        yield return new object?[] { new string?[] { "ooo", "www", "www", "ooo", "ooo", "ppp" }, "www" };
        yield return new object?[] { Enumerable.Repeat(default(string?), 5).ToArray(), null };
    }

    [Theory]
    [MemberData(nameof(Max_String_TestData))]
    public void Max_String(string?[] sourceArray, string? expected)
    {
        Span<string?> source = sourceArray;
        Assert.Equal(expected, source.Max());

        // ---------------------------------

        ReadOnlySpan<string?> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max());
    }

    [Fact]
    public void Max_Bool_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<bool>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<bool>.Empty.Max());
    }

    public static IEnumerable<object?[]> Max_Generic_TestData()
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
            expected: 9);

        yield return WrapArgs(
            source: Enumerable.Range(0, 10),
            comparer: Comparer<int>.Create((x, y) => -x.CompareTo(y)),
            expected: 0);

        yield return WrapArgs(
            source: Enumerable.Range(0, 10),
            comparer: Comparer<int>.Create((x, y) => 0),
            expected: 0);

        yield return WrapArgs(
            source: new string[] { "Aardvark", "Zyzzyva", "Zebra", "Antelope" },
            comparer: null,
            expected: "Zyzzyva");

        yield return WrapArgs(
            source: new string[] { "Aardvark", "Zyzzyva", "Zebra", "Antelope" },
            comparer: Comparer<string>.Create((x, y) => -x.CompareTo(y)),
            expected: "Aardvark");

        object?[] WrapArgs<TSource>(IEnumerable<TSource> source, IComparer<TSource>? comparer, TSource? expected)
            => new object?[] { source.ToArray(), comparer, expected };
    }

    [Theory]
    [MemberData(nameof(Max_Generic_TestData))]
    public static void Max_Generic<TSource>(TSource[] sourceArray, IComparer<TSource>? comparer, TSource? expected)
    {
        Span<TSource> source = sourceArray;
        Assert.Equal(expected, source.Max(comparer));

        // ---------------------------------

        ReadOnlySpan<TSource> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.Max(comparer));
    }

    [Fact]
    public static void Max_Generic_EmptyStructSource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<DateTime>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<DateTime>.Empty.Max());
        Assert.Throws<InvalidOperationException>(() => Span<DateTime>.Empty.Max(comparer: null));
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<DateTime>.Empty.Max(comparer: null));
        Assert.Throws<InvalidOperationException>(() => Span<DateTime>.Empty.Max(Comparer<DateTime>.Create((_, _) => 0)));
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<DateTime>.Empty.Max(Comparer<DateTime>.Create((_, _) => 0)));
    }

    public static IEnumerable<object[]> UnsafeMax_Float_TestData()
    {
        yield return new object[] { new[] { 42f }, 42f };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (float)i).ToArray(), 10f };
        yield return new object[] { new float[] { -1, -10, 10, 200, 1000 }, 1000f };
        yield return new object[] { new float[] { 3000, 100, 200, 1000 }, 3000 };
        yield return new object[] { new float[] { 3000, 100, 200, 1000, float.MaxValue }, float.MaxValue };

        yield return new object[] { new[] { 5.5f }, 5.5f };
        yield return new object[] { new float[] { -2.5f, 4.9f, 130f, 4.7f, 28f }, 130f };
        yield return new object[] { new float[] { 6.8f, 9.4f, 10f, 0, -5.6f }, 10f };
        yield return new object[] { new float[] { -5.5f, float.PositiveInfinity, 9.9f, float.PositiveInfinity }, float.PositiveInfinity };
    }

    [Theory]
    [MemberData(nameof(UnsafeMax_Float_TestData))]
    public void UnsafeMax_Float(float[] sourceArray, float expected)
    {
        Span<float> source = sourceArray;
        Assert.Equal(expected, source.UnsafeMax());

        // ---------------------------------

        ReadOnlySpan<float> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.UnsafeMax());
    }

    [Fact]
    public void UnsafeMax_Float_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<float>.Empty.UnsafeMax());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<float>.Empty.UnsafeMax());
    }

    public static IEnumerable<object[]> UnsafeMax_Double_TestData()
    {
        yield return new object[] { new[] { 42.0 }, 42.0 };
        yield return new object[] { Enumerable.Range(1, 10).Select(i => (double)i).ToArray(), 10.0 };
        yield return new object[] { new double[] { -1, -10, 10, 200, 1000 }, 1000.0 };
        yield return new object[] { new double[] { 3000, 100, 200, 1000 }, 3000.0 };
        yield return new object[] { new double[] { 3000, 100, 200, 1000, double.MaxValue }, double.MaxValue };

        yield return new object[] { new[] { 5.5 }, 5.5 };
        yield return new object[] { new double[] { -2.5, 4.9, 130, 4.7, 28 }, 130.0 };
        yield return new object[] { new double[] { 6.8, 9.4, 10, 0, -5.6 }, 10.0 };
        yield return new object[] { new double[] { -5.5, double.PositiveInfinity, 9.9, double.PositiveInfinity }, double.PositiveInfinity };
    }

    [Theory]
    [MemberData(nameof(UnsafeMax_Double_TestData))]
    public void UnsafeMax_Double(double[] sourceArray, double expected)
    {
        Span<double> source = sourceArray;
        Assert.Equal(expected, source.UnsafeMax());

        // ---------------------------------

        ReadOnlySpan<double> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.UnsafeMax());
    }


    [Fact]
    public void UnsafeMax_Double_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<double>.Empty.UnsafeMax());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<double>.Empty.UnsafeMax());
    }

    public static IEnumerable<object[]> UnsafeMax_Generic_TestData()
        => UnsafeMax_Float_TestData()
            .Concat(UnsafeMax_Double_TestData())
            .Concat(Max_Int_TestData());

    [Theory]
    [MemberData(nameof(UnsafeMax_Generic_TestData))]
    public static void UnsafeMax_Generic<TSource>(TSource[] sourceArray, TSource? expected)
    {
        Span<TSource> source = sourceArray;
        Assert.Equal(expected, source.UnsafeMax());

        // ---------------------------------

        ReadOnlySpan<TSource> readOnlySource = sourceArray;
        Assert.Equal(expected, readOnlySource.UnsafeMax());
    }

    [Fact]
    public static void UnsafeMax_Generic_EmptyStructSource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<DateTime>.Empty.UnsafeMax());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<DateTime>.Empty.UnsafeMax());
    }
}
