using System.Numerics;

namespace Spanned.Tests.Spans;

public class LongSumTests
{
    [Theory]
    [InlineData(new byte[] { 1, 2, 3, 4 }, 10UL)]
    [InlineData(new byte[] { byte.MaxValue, 0, 5, 20 }, byte.MaxValue + 25UL)]
    public void LongSum_Byte(byte[] sourceArray, ulong expectedSum)
    {
        Assert.Equal(expectedSum, ((Span<byte>)sourceArray).LongSum());
        Assert.Equal(expectedSum, ((ReadOnlySpan<byte>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Byte_SourceContainsOneElement_ReturnsFirstElement()
    {
        byte[] sourceArray = new byte[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<byte>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<byte>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Byte_EmptySource_ReturnsZero()
    {
        Assert.Equal(0UL, Span<byte>.Empty.LongSum());
        Assert.Equal(0UL, ReadOnlySpan<byte>.Empty.LongSum());
    }

    [Theory]
    [InlineData(new sbyte[] { 1, -2, 3, -4 }, -2L)]
    [InlineData(new sbyte[] { sbyte.MaxValue, 0, 5, 20 }, sbyte.MaxValue + 25L)]
    [InlineData(new sbyte[] { sbyte.MinValue, 0, -5, -20 }, sbyte.MinValue - 25L)]
    public void LongSum_SByte(sbyte[] sourceArray, long expectedSum)
    {
        Assert.Equal(expectedSum, ((Span<sbyte>)sourceArray).LongSum());
        Assert.Equal(expectedSum, ((ReadOnlySpan<sbyte>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_SByte_SourceContainsOneElement_ReturnsFirstElement()
    {
        sbyte[] sourceArray = new sbyte[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<sbyte>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<sbyte>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_SByte_EmptySource_ReturnsZero()
    {
        Assert.Equal(0L, Span<sbyte>.Empty.LongSum());
        Assert.Equal(0L, ReadOnlySpan<sbyte>.Empty.LongSum());
    }

    [Theory]
    [InlineData(new short[] { 1, -2, 3, -4 }, -2L)]
    [InlineData(new short[] { short.MaxValue, 0, 5, 20 }, short.MaxValue + 25L)]
    [InlineData(new short[] { short.MinValue, 0, -5, -20 }, short.MinValue - 25L)]
    public void LongSum_Short(short[] sourceArray, long expectedSum)
    {
        Assert.Equal(expectedSum, ((Span<short>)sourceArray).LongSum());
        Assert.Equal(expectedSum, ((ReadOnlySpan<short>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Short_SourceContainsOneElement_ReturnsFirstElement()
    {
        short[] sourceArray = new short[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<short>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<short>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Short_EmptySource_ReturnsZero()
    {
        Assert.Equal(0L, Span<short>.Empty.LongSum());
        Assert.Equal(0L, ReadOnlySpan<short>.Empty.LongSum());
    }

    [Theory]
    [InlineData(new ushort[] { 1, 2, 3, 4 }, 10UL)]
    [InlineData(new ushort[] { ushort.MaxValue, 0, 5, 20 }, ushort.MaxValue + 25UL)]
    public void LongSum_UShort(ushort[] sourceArray, ulong expectedSum)
    {
        Assert.Equal(expectedSum, ((Span<ushort>)sourceArray).LongSum());
        Assert.Equal(expectedSum, ((ReadOnlySpan<ushort>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_UShort_SourceContainsOneElement_ReturnsFirstElement()
    {
        ushort[] sourceArray = new ushort[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<ushort>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<ushort>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_UShort_EmptySource_ReturnsZero()
    {
        Assert.Equal(0UL, Span<ushort>.Empty.LongSum());
        Assert.Equal(0UL, ReadOnlySpan<ushort>.Empty.LongSum());
    }

    [Theory]
    [InlineData(new int[] { 1, -2, 3, -4 }, -2L)]
    [InlineData(new int[] { int.MaxValue, 0, 5, 20 }, int.MaxValue + 25L)]
    [InlineData(new int[] { int.MinValue, 0, -5, -20 }, int.MinValue - 25L)]
    public void LongSum_Int(int[] sourceArray, long expectedSum)
    {
        Assert.Equal(expectedSum, ((Span<int>)sourceArray).LongSum());
        Assert.Equal(expectedSum, ((ReadOnlySpan<int>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Int_SourceContainsOneElement_ReturnsFirstElement()
    {
        int[] sourceArray = new int[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<int>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<int>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Int_EmptySource_ReturnsZero()
    {
        Assert.Equal(0L, Span<int>.Empty.LongSum());
        Assert.Equal(0L, ReadOnlySpan<int>.Empty.LongSum());
    }

    [Theory]
    [InlineData(new uint[] { 1, 2, 3, 4 }, 10UL)]
    [InlineData(new uint[] { uint.MaxValue, 0, 5, 20 }, uint.MaxValue + 25UL)]
    public void LongSum_UInt(uint[] sourceArray, ulong expectedSum)
    {
        Assert.Equal(expectedSum, ((Span<uint>)sourceArray).LongSum());
        Assert.Equal(expectedSum, ((ReadOnlySpan<uint>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_UInt_SourceContainsOneElement_ReturnsFirstElement()
    {
        uint[] sourceArray = new uint[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<uint>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<uint>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_UInt_EmptySource_ReturnsZero()
    {
        Assert.Equal(0UL, Span<uint>.Empty.LongSum());
        Assert.Equal(0UL, ReadOnlySpan<uint>.Empty.LongSum());
    }

    [Fact]
    public void LongSum_Long()
    {
        long[] sourceArray = new long[] { 1, -2, 3, -4 };

        Assert.Equal(-2L, ((Span<long>)sourceArray).LongSum());
        Assert.Equal(-2L, ((ReadOnlySpan<long>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Long_SumTooSmall_ThrowsOverflowException()
    {
        long[] sourceArray = new long[] { long.MinValue, 0, -5, -20 };

        Assert.Throws<OverflowException>(() => ((Span<long>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<long>)sourceArray).Sum());
    }

    [Fact]
    public void LongSum_Long_SumTooLarge_ThrowsOverflowException()
    {
        long[] sourceArray = new long[] { long.MaxValue, 0, 5, 20 };

        Assert.Throws<OverflowException>(() => ((Span<long>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<long>)sourceArray).Sum());
    }

    [Fact]
    public void LongSum_Long_SourceContainsOneElement_ReturnsFirstElement()
    {
        long[] sourceArray = new long[] { int.MaxValue + 20L };

        Assert.Equal(sourceArray[0], ((Span<long>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<long>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Long_EmptySource_ReturnsZero()
    {
        Assert.Equal(0L, Span<long>.Empty.LongSum());
        Assert.Equal(0L, ReadOnlySpan<long>.Empty.LongSum());
    }

    [Fact]
    public void LongSum_ULong()
    {
        ulong[] sourceArray = new ulong[] { 1, 2, 3, 4 };

        Assert.Equal(10UL, ((Span<ulong>)sourceArray).LongSum());
        Assert.Equal(10UL, ((ReadOnlySpan<ulong>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_ULong_SumTooLarge_ThrowsOverflowException()
    {
        ulong[] sourceArray = new ulong[] { ulong.MaxValue, 0, 5, 20 };

        Assert.Throws<OverflowException>(() => ((Span<ulong>)sourceArray).Sum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<ulong>)sourceArray).Sum());
    }

    [Fact]
    public void LongSum_ULong_SourceContainsOneElement_ReturnsFirstElement()
    {
        ulong[] sourceArray = new ulong[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<ulong>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<ulong>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_ULong_EmptySource_ReturnsZero()
    {
        Assert.Equal(0UL, Span<ulong>.Empty.LongSum());
        Assert.Equal(0UL, ReadOnlySpan<ulong>.Empty.LongSum());
    }

    [Fact]
    public void LongSum_NInt()
    {
        nint[] sourceArray = new nint[] { 1, -2, 3, -4 };

        Assert.Equal(-2L, ((Span<nint>)sourceArray).LongSum());
        Assert.Equal(-2L, ((ReadOnlySpan<nint>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_NInt_SumTooSmall_ThrowsOverflowException()
    {
        nint[] arraySource = new[] { nint.MinValue, nint.MinValue, -1, -1 };

        if (nint.Size == sizeof(long))
        {
            Assert.Throws<OverflowException>(() => ((Span<nint>)arraySource).LongSum());
            Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nint>)arraySource).LongSum());
        }
        else
        {
            Assert.Equal(-2L + nint.MinValue + nint.MinValue, ((Span<nint>)arraySource).LongSum());
            Assert.Equal(-2L + nint.MinValue + nint.MinValue, ((ReadOnlySpan<nint>)arraySource).LongSum());
        }
    }

    [Fact]
    public void LongSum_NInt_SumTooLarge_ThrowsOverflowException()
    {
        nint[] arraySource = new nint[] { nint.MaxValue, nint.MaxValue, 1, 1 };

        if (nint.Size == sizeof(long))
        {
            Assert.Throws<OverflowException>(() => ((Span<nint>)arraySource).LongSum());
            Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nint>)arraySource).LongSum());
        }
        else
        {
            Assert.Equal(2L + nint.MaxValue + nint.MaxValue, ((Span<nint>)arraySource).LongSum());
            Assert.Equal(2L + nint.MaxValue + nint.MaxValue, ((ReadOnlySpan<nint>)arraySource).LongSum());
        }
    }

    [Fact]
    public void LongSum_NInt_SourceContainsOneElement_ReturnsFirstElement()
    {
        nint[] sourceArray = new nint[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<nint>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<nint>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_NInt_EmptySource_ReturnsZero()
    {
        Assert.Equal(0L, Span<nint>.Empty.LongSum());
        Assert.Equal(0L, ReadOnlySpan<nint>.Empty.LongSum());
    }

    [Fact]
    public void LongSum_NUInt()
    {
        nuint[] sourceArray = new nuint[] { 1, 2, 3, 4 };

        Assert.Equal(10UL, ((Span<nuint>)sourceArray).LongSum());
        Assert.Equal(10UL, ((ReadOnlySpan<nuint>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_NUInt_SumTooLarge_ThrowsOverflowException()
    {
        nuint[] arraySource = new nuint[] { nuint.MaxValue, nuint.MaxValue, 1, 1 };

        if (nuint.Size == sizeof(ulong))
        {
            Assert.Throws<OverflowException>(() => ((Span<nuint>)arraySource).LongSum());
            Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nuint>)arraySource).LongSum());
        }
        else
        {
            Assert.Equal(2UL + nuint.MaxValue + nuint.MaxValue, ((Span<nuint>)arraySource).LongSum());
            Assert.Equal(2UL + nuint.MaxValue + nuint.MaxValue, ((ReadOnlySpan<nuint>)arraySource).LongSum());
        }
    }

    [Fact]
    public void LongSum_NUInt_SourceContainsOneElement_ReturnsFirstElement()
    {
        nuint[] sourceArray = new nuint[] { 20 };

        Assert.Equal(sourceArray[0], ((Span<nuint>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<nuint>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_NUInt_EmptySource_ReturnsZero()
    {
        Assert.Equal(0UL, Span<nuint>.Empty.LongSum());
        Assert.Equal(0UL, ReadOnlySpan<nuint>.Empty.LongSum());
    }

    [Theory]
    [InlineData(new float[] { 1, -2, 3, -4 }, -2.0)]
    [InlineData(new float[] { float.MaxValue, float.MaxValue, 0, 5, 20 }, 25.0 + float.MaxValue + float.MaxValue)]
    [InlineData(new float[] { float.MinValue, float.MinValue, 0, -5, -20 }, -25.0 + float.MinValue + float.MinValue)]
    public void LongSum_Float(float[] sourceArray, double expectedSum)
    {
        Assert.Equal(expectedSum, ((Span<float>)sourceArray).LongSum());
        Assert.Equal(expectedSum, ((ReadOnlySpan<float>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Float_SourceContainsOneElement_ReturnsFirstElement()
    {
        float[] sourceArray = new float[] { 20.51f };

        Assert.Equal(sourceArray[0], ((Span<float>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<float>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Float_SourceContainsNaN_ReturnsNaN()
    {
        float[] sourceArray = { 20.45f, 0f, -10.55f, float.NaN };

        Assert.True(double.IsNaN(((Span<float>)sourceArray).LongSum()));
        Assert.True(double.IsNaN(((ReadOnlySpan<float>)sourceArray).LongSum()));
    }

    [Fact]
    public void LongSum_Float_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<float>.Empty.LongSum());
        Assert.Equal(0, ReadOnlySpan<float>.Empty.LongSum());
    }

    [Fact]
    public void LongSum_Double()
    {
        double[] sourceArray = new double[] { 1d, 0.5d, -1d, 0.5d };

        Assert.Equal(1d, ((Span<double>)sourceArray).LongSum());
        Assert.Equal(1d, ((ReadOnlySpan<double>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Double_SumTooSmall_ReturnsNegativeInfinity()
    {
        double[] sourceArray = new double[] { double.MinValue, double.MinValue, 0d, -5d, -20d };

        Assert.True(double.IsNegativeInfinity(((Span<double>)sourceArray).LongSum()));
        Assert.True(double.IsNegativeInfinity(((ReadOnlySpan<double>)sourceArray).LongSum()));
    }

    [Fact]
    public void LongSum_Double_SumTooLarge_ReturnsPositiveInfinity()
    {
        double[] sourceArray = new double[] { double.MaxValue, double.MaxValue, 0d, 5d, 20d };

        Assert.True(double.IsPositiveInfinity(((Span<double>)sourceArray).LongSum()));
        Assert.True(double.IsPositiveInfinity(((ReadOnlySpan<double>)sourceArray).LongSum()));
    }

    [Fact]
    public void LongSum_Double_SourceContainsOneElement_ReturnsFirstElement()
    {
        double[] sourceArray = new double[] { 20.51d };

        Assert.Equal(sourceArray[0], ((Span<double>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<double>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Double_SourceContainsNaN_ReturnsNaN()
    {
        double[] sourceArray = { 20.45d, 0d, -10.55d, double.NaN };

        Assert.True(double.IsNaN(((Span<double>)sourceArray).LongSum()));
        Assert.True(double.IsNaN(((ReadOnlySpan<double>)sourceArray).LongSum()));
    }

    [Fact]
    public void LongSum_Double_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<double>.Empty.LongSum());
        Assert.Equal(0, ReadOnlySpan<double>.Empty.LongSum());
    }

    [Fact]
    public void LongSum_Decimal()
    {
        decimal[] sourceArray = new decimal[] { 1m, 0.5m, -1m, 0.5m };

        Assert.Equal(1m, ((Span<decimal>)sourceArray).LongSum());
        Assert.Equal(1m, ((ReadOnlySpan<decimal>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Decimal_SumTooSmall_ThrowsOverflowException()
    {
        decimal[] sourceArray = new decimal[] { decimal.MinValue, 0m, -5m, -20m };

        Assert.Throws<OverflowException>(() => ((Span<decimal>)sourceArray).LongSum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<decimal>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Decimal_SumTooLarge_ThrowsOverflowException()
    {
        decimal[] sourceArray = new decimal[] { decimal.MaxValue, 0m, 5m, 20m };

        Assert.Throws<OverflowException>(() => ((Span<decimal>)sourceArray).LongSum());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<decimal>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Decimal_SourceContainsOneElement_ReturnsFirstElement()
    {
        decimal[] sourceArray = new decimal[] { 20.51m };

        Assert.Equal(sourceArray[0], ((Span<decimal>)sourceArray).LongSum());
        Assert.Equal(sourceArray[0], ((ReadOnlySpan<decimal>)sourceArray).LongSum());
    }

    [Fact]
    public void LongSum_Decimal_EmptySource_ReturnsZero()
    {
        Assert.Equal(0, Span<decimal>.Empty.LongSum());
        Assert.Equal(0, ReadOnlySpan<decimal>.Empty.LongSum());
    }

    public static IEnumerable<object[]> LongSum_Generic_TestData()
    {
        yield return new object[] { Array.Empty<byte>(), 0UL };
        yield return new object[] { new byte[] { 1, 2, 3, 4 }, 10UL };
        yield return new object[] { new byte[] { byte.MaxValue, 0, 5, 20 }, byte.MaxValue + 25UL };

        yield return new object[] { Array.Empty<sbyte>(), 0L };
        yield return new object[] { new sbyte[] { 1, -2, 3, -4 }, -2L };
        yield return new object[] { new sbyte[] { sbyte.MaxValue, 0, 5, 20 }, sbyte.MaxValue + 25L };
        yield return new object[] { new sbyte[] { sbyte.MinValue, 0, -5, -20 }, sbyte.MinValue - 25L };

        yield return new object[] { Array.Empty<short>(), 0L };
        yield return new object[] { new short[] { 1, -2, 3, -4 }, -2L };
        yield return new object[] { new short[] { short.MaxValue, 0, 5, 20 }, short.MaxValue + 25L };
        yield return new object[] { new short[] { short.MinValue, 0, -5, -20 }, short.MinValue - 25L };

        yield return new object[] { Array.Empty<ushort>(), 0UL };
        yield return new object[] { new ushort[] { 1, 2, 3, 4 }, 10UL };
        yield return new object[] { new ushort[] { ushort.MaxValue, 0, 5, 20 }, ushort.MaxValue + 25UL };

        yield return new object[] { Array.Empty<int>(), 0L };
        yield return new object[] { new int[] { 1, -2, 3, -4 }, -2L };
        yield return new object[] { new int[] { int.MaxValue, 0, 5, 20 }, int.MaxValue + 25L };
        yield return new object[] { new int[] { int.MinValue, 0, -5, -20 }, int.MinValue - 25L };

        yield return new object[] { Array.Empty<uint>(), 0UL };
        yield return new object[] { new uint[] { 1, 2, 3, 4 }, 10UL };
        yield return new object[] { new uint[] { uint.MaxValue, 0, 5, 20 }, uint.MaxValue + 25UL };

        yield return new object[] { Array.Empty<long>(), 0L };
        yield return new object[] { new long[] { 1, -2, 3, -4 }, -2L };

        yield return new object[] { Array.Empty<ulong>(), 0UL };
        yield return new object[] { new ulong[] { 1, 2, 3, 4 }, 10UL };

        yield return new object[] { Array.Empty<nint>(), 0L };
        yield return new object[] { new nint[] { 1, -2, 3, -4 }, -2L };

        yield return new object[] { Array.Empty<nuint>(), 0UL };
        yield return new object[] { new nuint[] { 1, 2, 3, 4 }, 10UL };

        yield return new object[] { Array.Empty<float>(), 0d };
        yield return new object[] { new float[] { float.MaxValue, float.MaxValue, 0, 5, 20 }, 25.0 + float.MaxValue + float.MaxValue };
        yield return new object[] { new float[] { float.MinValue, float.MinValue, 0, -5, -20 }, -25.0 + float.MinValue + float.MinValue };

        yield return new object[] { Array.Empty<double>(), 0d };
        yield return new object[] { new double[] { 1, -2, 3, -4 }, -2d };

        yield return new object[] { Array.Empty<decimal>(), 0m };
        yield return new object[] { new decimal[] { 1, -2, 3, -4 }, -2m };

        yield return new object[] { Array.Empty<Half>(), 0d };
        yield return new object[] { new Half[] { (Half)1, -2, (Half)3, -4 }, -2d };
        yield return new object[] { new Half[] { Half.MaxValue, Half.MaxValue, (Half)0, (Half)5, (Half)20 }, 25.0 + (double)Half.MaxValue + (double)Half.MaxValue };
        yield return new object[] { new Half[] { Half.MinValue, Half.MinValue, (Half)0, (Half)(-5), (Half)(-20) }, -25.0 + (double)Half.MinValue + (double)Half.MinValue };
    }

    [Theory]
    [MemberData(nameof(LongSum_Generic_TestData))]
    public void LongSum_Generic<T, U>(T[] arraySource, U expectedSum) where T : INumberBase<T> where U : INumberBase<U>
    {
        Assert.Equal(expectedSum, ((Span<T>)arraySource).LongSum<T, U>());
        Assert.Equal(expectedSum, ((ReadOnlySpan<T>)arraySource).LongSum<T, U>());
    }
}
