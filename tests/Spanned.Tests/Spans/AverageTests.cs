using System.Numerics;
using Spanned.Tests.TestUtilities;

namespace Spanned.Tests.Spans;

public class AverageTests
{
    public static IEnumerable<object[]> Average_Byte_TestData()
    {
        yield return new object[] { new byte[] { byte.MaxValue }, byte.MaxValue };
        yield return new object[] { new byte[] { byte.MaxValue, byte.MaxValue }, byte.MaxValue };
        yield return new object[] { new byte[] { 0, 0, 0, 0, 0 }, 0.0 };
        yield return new object[] { new byte[] { 5, 10, 15, 40, 28 }, 19.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((byte)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_Byte_TestData))]
    public void Average_Byte(byte[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<byte>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<byte>)sourceArray).Average());
    }

    [Fact]
    public void Average_Byte_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<byte>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<byte>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_SByte_TestData()
    {
        yield return new object[] { new sbyte[] { sbyte.MaxValue }, sbyte.MaxValue };
        yield return new object[] { new sbyte[] { sbyte.MaxValue, sbyte.MaxValue }, sbyte.MaxValue };
        yield return new object[] { new sbyte[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new sbyte[] { 5, -10, 15, 40, 28 }, 15.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((sbyte)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_SByte_TestData))]
    public void Average_SByte(sbyte[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<sbyte>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<sbyte>)sourceArray).Average());
    }

    [Fact]
    public void Average_SByte_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<sbyte>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<sbyte>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_Short_TestData()
    {
        yield return new object[] { new short[] { short.MaxValue }, short.MaxValue };
        yield return new object[] { new short[] { short.MaxValue, short.MaxValue }, short.MaxValue };
        yield return new object[] { new short[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new short[] { 5, -10, 15, 40, 28 }, 15.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((short)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_Short_TestData))]
    public void Average_Short(short[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<short>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<short>)sourceArray).Average());
    }

    [Fact]
    public void Average_Short_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<short>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<short>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_UShort_TestData()
    {
        yield return new object[] { new ushort[] { ushort.MaxValue }, ushort.MaxValue };
        yield return new object[] { new ushort[] { ushort.MaxValue, ushort.MaxValue }, ushort.MaxValue };
        yield return new object[] { new ushort[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new ushort[] { 5, 10, 15, 40, 28 }, 19.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((ushort)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_UShort_TestData))]
    public void Average_UShort(ushort[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<ushort>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<ushort>)sourceArray).Average());
    }

    [Fact]
    public void Average_UShort_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<ushort>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<ushort>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_Int_TestData()
    {
        yield return new object[] { new int[] { int.MaxValue }, int.MaxValue };
        yield return new object[] { new int[] { int.MaxValue, int.MaxValue }, int.MaxValue };
        yield return new object[] { new int[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new int[] { 5, -10, 15, 40, 28 }, 15.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range(1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_Int_TestData))]
    public void Average_Int(int[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<int>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<int>)sourceArray).Average());
    }

    [Fact]
    public void Average_Int_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<int>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<int>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_UInt_TestData()
    {
        yield return new object[] { new uint[] { uint.MaxValue }, uint.MaxValue };
        yield return new object[] { new uint[] { uint.MaxValue, uint.MaxValue }, uint.MaxValue };
        yield return new object[] { new uint[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new uint[] { 5, 10, 15, 40, 28 }, 19.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range(1u, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_UInt_TestData))]
    public void Average_UInt(uint[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<uint>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<uint>)sourceArray).Average());
    }

    [Fact]
    public void Average_UInt_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<uint>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<uint>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_Long_TestData()
    {
        yield return new object[] { new long[] { long.MaxValue }, long.MaxValue };
        yield return new object[] { new long[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new long[] { 5, -10, 15, 40, 28 }, 15.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range(1L, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_Long_TestData))]
    public void Average_Long(long[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<long>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<long>)sourceArray).Average());
    }

    [Fact]
    public void Average_Long_SumTooLarge_ThrowsOverflowException()
    {
        long[] arraySource = new[] { long.MaxValue, long.MaxValue, 1L, 1L };

        Assert.Throws<OverflowException>(() => ((Span<long>)arraySource).Average());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<long>)arraySource).Average());
    }

    [Fact]
    public void Average_Long_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<long>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<long>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_ULong_TestData()
    {
        yield return new object[] { new ulong[] { ulong.MaxValue }, ulong.MaxValue };
        yield return new object[] { new ulong[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new ulong[] { 5, 10, 15, 40, 28 }, 19.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range(1ul, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_ULong_TestData))]
    public void Average_ULong(ulong[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<ulong>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<ulong>)sourceArray).Average());
    }

    [Fact]
    public void Average_ULong_SumTooLarge_ThrowsOverflowException()
    {
        ulong[] arraySource = new[] { ulong.MaxValue, ulong.MaxValue, 1ul, 1ul };

        Assert.Throws<OverflowException>(() => ((Span<ulong>)arraySource).Average());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<ulong>)arraySource).Average());
    }

    [Fact]
    public void Average_ULong_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<ulong>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<ulong>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_NInt_TestData()
    {
        yield return new object[] { new nint[] { nint.MaxValue }, (double)nint.MaxValue };
        yield return new object[] { new nint[] { 0, 0, 0, 0, 0 }, 0.0 };
        yield return new object[] { new nint[] { 5, -10, 15, 40, 28 }, 15.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((nint)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_NInt_TestData))]
    public void Average_NInt(nint[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<nint>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<nint>)sourceArray).Average());
    }

    [Fact]
    public void Average_NInt_SumTooLarge_ThrowsOverflowException()
    {
        nint[] arraySource = new[] { nint.MaxValue, nint.MaxValue, 1, 1 };

        if (nint.Size == sizeof(long))
        {
            Assert.Throws<OverflowException>(() => ((Span<nint>)arraySource).Average());
            Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nint>)arraySource).Average());
        }
        else
        {
            Assert.Equal((nint.MaxValue + 1.0) / 2.0, ((Span<nint>)arraySource).Average());
            Assert.Equal((nint.MaxValue + 1.0) / 2.0, ((ReadOnlySpan<nint>)arraySource).Average());
        }
    }

    [Fact]
    public void Average_NInt_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<nint>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<nint>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_NUint_TestData()
    {
        yield return new object[] { new nuint[] { nuint.MaxValue }, (double)nuint.MaxValue };
        yield return new object[] { new nuint[] { 0, 0, 0, 0, 0 }, 0.0 };
        yield return new object[] { new nuint[] { 5, 10, 15, 40, 28 }, 19.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((nuint)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_NUint_TestData))]
    public void Average_NUint(nuint[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<nuint>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<nuint>)sourceArray).Average());
    }

    [Fact]
    public void Average_NUint_SumTooLarge_ThrowsOverflowException()
    {
        nuint[] arraySource = new[] { nuint.MaxValue, nuint.MaxValue, 1u, 1u };

        if (nuint.Size == sizeof(ulong))
        {
            Assert.Throws<OverflowException>(() => ((Span<nuint>)arraySource).Average());
            Assert.Throws<OverflowException>(() => ((ReadOnlySpan<nuint>)arraySource).Average());
        }
        else
        {
            Assert.Equal((nuint.MaxValue + 1.0) / 2.0, ((Span<nuint>)arraySource).Average());
            Assert.Equal((nuint.MaxValue + 1.0) / 2.0, ((ReadOnlySpan<nuint>)arraySource).Average());
        }
    }

    [Fact]
    public void Average_NUint_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<nuint>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<nuint>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_Float_TestData()
    {
        yield return new object[] { new float[] { float.MaxValue }, float.MaxValue };
        yield return new object[] { new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f }, 0f };
        yield return new object[] { new float[] { 5.5f, -10f, 15.5f, 40.5f, 28.5f }, 16f };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            float expected = (float)sum / i;

            yield return new object[] { Shuffler.Range(1f, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_Float_TestData))]
    public void Average_Float(float[] sourceArray, float expected)
    {
        Assert.Equal(expected, ((Span<float>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<float>)sourceArray).Average());
    }

    [Fact]
    public void Average_Float_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<float>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<float>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_Double_TestData()
    {
        yield return new object[] { new double[] { double.MaxValue }, double.MaxValue };
        yield return new object[] { new double[] { 0.0, 0.0, 0.0, 0.0, 0.0 }, 0.0 };
        yield return new object[] { new double[] { 5.5, -10, 15.5, 40.5, 28.5 }, 16 };
        yield return new object[] { new double[] { 5.58, double.NaN, 30, 4.55, 19.38 }, double.NaN };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range(1.0, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_Double_TestData))]
    public void Average_Double(double[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<double>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<double>)sourceArray).Average());
    }

    [Fact]
    public void Average_Double_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<double>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<double>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_Decimal_TestData()
    {
        yield return new object[] { new decimal[] { decimal.MaxValue }, decimal.MaxValue };
        yield return new object[] { new decimal[] { 0.0m, 0.0m, 0.0m, 0.0m, 0.0m }, 0.0m };
        yield return new object[] { new decimal[] { 5.5m, -10m, 15.5m, 40.5m, 28.5m }, 16.0m };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            decimal expected = (decimal)sum / i;

            yield return new object[] { Shuffler.Range(1m, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_Decimal_TestData))]
    public void Average_Decimal(decimal[] sourceArray, decimal expected)
    {
        Assert.Equal(expected, ((Span<decimal>)sourceArray).Average());
        Assert.Equal(expected, ((ReadOnlySpan<decimal>)sourceArray).Average());
    }

    [Fact]
    public void Average_Decimal_SumTooLarge_ThrowsOverflowException()
    {
        decimal[] arraySource = new[] { decimal.MaxValue, decimal.MaxValue };

        Assert.Throws<OverflowException>(() => ((Span<decimal>)arraySource).Average());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<decimal>)arraySource).Average());
    }

    [Fact]
    public void Average_Decimal_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<decimal>.Empty.Average());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<decimal>.Empty.Average());
    }

    public static IEnumerable<object[]> Average_Generic_TestData()
    {
        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((byte)1, i), expected };
            yield return new object[] { Shuffler.Range((sbyte)1, i), expected };
            yield return new object[] { Shuffler.Range((short)1, i), expected };
            yield return new object[] { Shuffler.Range((ushort)1, i), expected };
            yield return new object[] { Shuffler.Range(1, i), expected };
            yield return new object[] { Shuffler.Range(1u, i), expected };
            yield return new object[] { Shuffler.Range(1L, i), expected };
            yield return new object[] { Shuffler.Range(1ul, i), expected };
            yield return new object[] { Shuffler.Range(1f, i), expected };
            yield return new object[] { Shuffler.Range(1d, i), expected };
            yield return new object[] { Shuffler.Range(1m, i), expected };
            yield return new object[] { Shuffler.Range((Half)1, i), expected };
            yield return new object[] { Shuffler.Range((Int128)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(Average_Generic_TestData))]
    public void Average_Generic<T>(T[] sourceArray, double expected) where T : INumberBase<T>
    {
        Assert.Equal(expected, ((Span<T>)sourceArray).Average<T, double>());
        Assert.Equal(expected, ((ReadOnlySpan<T>)sourceArray).Average<T, double>());
    }

    [Fact]
    public void Average_Generic_SumTooLarge_ThrowsOverflowException()
    {
        decimal[] arraySource = new[] { decimal.MaxValue, decimal.MaxValue };

        Assert.Throws<OverflowException>(() => ((Span<decimal>)arraySource).Average<decimal, decimal>());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<decimal>)arraySource).Average<decimal, decimal>());
    }

    [Fact]
    public void Average_Generic_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<decimal>.Empty.Average<decimal, decimal>());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<decimal>.Empty.Average<decimal, decimal>());
    }

    public static IEnumerable<object[]> UnsafeAverage_Byte_TestData()
    {
        yield return new object[] { new byte[] { byte.MaxValue }, byte.MaxValue };
        yield return new object[] { new byte[] { byte.MaxValue, byte.MaxValue, 1, 1 }, 0 };
        yield return new object[] { new byte[] { 0, 0, 0, 0, 0 }, 0.0 };
        yield return new object[] { new byte[] { 5, 10, 15, 40, 28 }, 19.6 };

        for (int i = 1; i <= 18; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((byte)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeAverage_Byte_TestData))]
    public void UnsafeAverage_Byte(byte[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<byte>)sourceArray).UnsafeAverage());
        Assert.Equal(expected, ((ReadOnlySpan<byte>)sourceArray).UnsafeAverage());
    }

    [Fact]
    public void UnsafeAverage_Byte_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<byte>.Empty.UnsafeAverage());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<byte>.Empty.UnsafeAverage());
    }

    public static IEnumerable<object[]> UnsafeAverage_SByte_TestData()
    {
        yield return new object[] { new sbyte[] { sbyte.MaxValue }, sbyte.MaxValue };
        yield return new object[] { new sbyte[] { sbyte.MaxValue, sbyte.MaxValue, 1, 1 }, 0 };
        yield return new object[] { new sbyte[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new sbyte[] { 5, -10, 15, 40, 28 }, 15.6 };

        for (int i = 1; i <= 14; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((sbyte)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeAverage_SByte_TestData))]
    public void UnsafeAverage_SByte(sbyte[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<sbyte>)sourceArray).UnsafeAverage());
        Assert.Equal(expected, ((ReadOnlySpan<sbyte>)sourceArray).UnsafeAverage());
    }

    [Fact]
    public void UnsafeAverage_SByte_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<sbyte>.Empty.UnsafeAverage());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<sbyte>.Empty.UnsafeAverage());
    }

    public static IEnumerable<object[]> UnsafeAverage_Short_TestData()
    {
        yield return new object[] { new short[] { short.MaxValue }, short.MaxValue };
        yield return new object[] { new short[] { short.MaxValue, short.MaxValue, 1, 1 }, 0 };
        yield return new object[] { new short[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new short[] { 5, -10, 15, 40, 28 }, 15.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((short)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeAverage_Short_TestData))]
    public void UnsafeAverage_Short(short[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<short>)sourceArray).UnsafeAverage());
        Assert.Equal(expected, ((ReadOnlySpan<short>)sourceArray).UnsafeAverage());
    }

    [Fact]
    public void UnsafeAverage_Short_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<short>.Empty.UnsafeAverage());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<short>.Empty.UnsafeAverage());
    }

    public static IEnumerable<object[]> UnsafeAverage_UShort_TestData()
    {
        yield return new object[] { new ushort[] { ushort.MaxValue }, ushort.MaxValue };
        yield return new object[] { new ushort[] { ushort.MaxValue, ushort.MaxValue, 1, 1 }, 0 };
        yield return new object[] { new ushort[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new ushort[] { 5, 10, 15, 40, 28 }, 19.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((ushort)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeAverage_UShort_TestData))]
    public void UnsafeAverage_UShort(ushort[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<ushort>)sourceArray).UnsafeAverage());
        Assert.Equal(expected, ((ReadOnlySpan<ushort>)sourceArray).UnsafeAverage());
    }

    [Fact]
    public void UnsafeAverage_UShort_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<ushort>.Empty.UnsafeAverage());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<ushort>.Empty.UnsafeAverage());
    }

    public static IEnumerable<object[]> UnsafeAverage_Int_TestData()
    {
        yield return new object[] { new int[] { int.MaxValue }, int.MaxValue };
        yield return new object[] { new int[] { int.MaxValue, int.MaxValue, 1, 1 }, 0 };
        yield return new object[] { new int[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new int[] { 5, -10, 15, 40, 28 }, 15.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range(1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeAverage_Int_TestData))]
    public void UnsafeAverage_Int(int[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<int>)sourceArray).UnsafeAverage());
        Assert.Equal(expected, ((ReadOnlySpan<int>)sourceArray).UnsafeAverage());
    }

    [Fact]
    public void UnsafeAverage_Int_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<int>.Empty.UnsafeAverage());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<int>.Empty.UnsafeAverage());
    }

    public static IEnumerable<object[]> UnsafeAverage_UInt_TestData()
    {
        yield return new object[] { new uint[] { uint.MaxValue }, uint.MaxValue };
        yield return new object[] { new uint[] { uint.MaxValue, uint.MaxValue, 1u, 1u }, 0 };
        yield return new object[] { new uint[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new uint[] { 5, 10, 15, 40, 28 }, 19.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range(1u, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeAverage_UInt_TestData))]
    public void UnsafeAverage_UInt(uint[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<uint>)sourceArray).UnsafeAverage());
        Assert.Equal(expected, ((ReadOnlySpan<uint>)sourceArray).UnsafeAverage());
    }

    [Fact]
    public void UnsafeAverage_UInt_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<uint>.Empty.UnsafeAverage());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<uint>.Empty.UnsafeAverage());
    }

    public static IEnumerable<object[]> UnsafeAverage_Long_TestData()
    {
        yield return new object[] { new long[] { long.MaxValue }, long.MaxValue };
        yield return new object[] { new long[] { long.MaxValue, long.MaxValue, 1, 1 }, 0 };
        yield return new object[] { new long[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new long[] { 5, -10, 15, 40, 28 }, 15.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range(1L, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeAverage_Long_TestData))]
    public void UnsafeAverage_Long(long[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<long>)sourceArray).UnsafeAverage());
        Assert.Equal(expected, ((ReadOnlySpan<long>)sourceArray).UnsafeAverage());
    }

    [Fact]
    public void UnsafeAverage_Long_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<long>.Empty.UnsafeAverage());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<long>.Empty.UnsafeAverage());
    }

    public static IEnumerable<object[]> UnsafeAverage_ULong_TestData()
    {
        yield return new object[] { new ulong[] { ulong.MaxValue }, ulong.MaxValue };
        yield return new object[] { new ulong[] { ulong.MaxValue, ulong.MaxValue, 1u, 1u }, 0 };
        yield return new object[] { new ulong[] { 0, 0, 0, 0, 0 }, 0 };
        yield return new object[] { new ulong[] { 5, 10, 15, 40, 28 }, 19.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range(1ul, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeAverage_ULong_TestData))]
    public void UnsafeAverage_ULong(ulong[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<ulong>)sourceArray).UnsafeAverage());
        Assert.Equal(expected, ((ReadOnlySpan<ulong>)sourceArray).UnsafeAverage());
    }

    [Fact]
    public void UnsafeAverage_ULong_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<ulong>.Empty.UnsafeAverage());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<ulong>.Empty.UnsafeAverage());
    }

    public static IEnumerable<object[]> UnsafeAverage_NInt_TestData()
    {
        yield return new object[] { new nint[] { nint.MaxValue }, (double)nint.MaxValue };
        yield return new object[] { new nint[] { nint.MaxValue, nint.MaxValue, 1, 1 }, 0.0 };
        yield return new object[] { new nint[] { 0, 0, 0, 0, 0 }, 0.0 };
        yield return new object[] { new nint[] { 5, -10, 15, 40, 28 }, 15.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((nint)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeAverage_NInt_TestData))]
    public void UnsafeAverage_NInt(nint[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<nint>)sourceArray).UnsafeAverage());
        Assert.Equal(expected, ((ReadOnlySpan<nint>)sourceArray).UnsafeAverage());
    }

    [Fact]
    public void UnsafeAverage_NInt_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<nint>.Empty.UnsafeAverage());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<nint>.Empty.UnsafeAverage());
    }

    public static IEnumerable<object[]> UnsafeAverage_NUint_TestData()
    {
        yield return new object[] { new nuint[] { nuint.MaxValue }, (double)nuint.MaxValue };
        yield return new object[] { new nuint[] { nuint.MaxValue, nuint.MaxValue, 1u, 1u }, 0 };
        yield return new object[] { new nuint[] { 0, 0, 0, 0, 0 }, 0.0 };
        yield return new object[] { new nuint[] { 5, 10, 15, 40, 28 }, 19.6 };

        for (int i = 1; i <= 33; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((nuint)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeAverage_NUint_TestData))]
    public void UnsafeAverage_NUint(nuint[] sourceArray, double expected)
    {
        Assert.Equal(expected, ((Span<nuint>)sourceArray).UnsafeAverage());
        Assert.Equal(expected, ((ReadOnlySpan<nuint>)sourceArray).UnsafeAverage());
    }

    [Fact]
    public void UnsafeAverage_NUint_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<nuint>.Empty.UnsafeAverage());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<nuint>.Empty.UnsafeAverage());
    }

    public static IEnumerable<object[]> UnsafeAverage_Generic_TestData()
    {
        for (int i = 1; i <= 14; i++)
        {
            int sum = 0;
            for (int c = 1; c <= i; c++)
            {
                sum += c;
            }
            double expected = (double)sum / i;

            yield return new object[] { Shuffler.Range((byte)1, i), expected };
            yield return new object[] { Shuffler.Range((sbyte)1, i), expected };
            yield return new object[] { Shuffler.Range((short)1, i), expected };
            yield return new object[] { Shuffler.Range((ushort)1, i), expected };
            yield return new object[] { Shuffler.Range(1, i), expected };
            yield return new object[] { Shuffler.Range(1u, i), expected };
            yield return new object[] { Shuffler.Range(1L, i), expected };
            yield return new object[] { Shuffler.Range(1ul, i), expected };
            yield return new object[] { Shuffler.Range(1f, i), expected };
            yield return new object[] { Shuffler.Range(1d, i), expected };
            yield return new object[] { Shuffler.Range(1m, i), expected };
            yield return new object[] { Shuffler.Range((Half)1, i), expected };
            yield return new object[] { Shuffler.Range((Int128)1, i), expected };
        }
    }

    [Theory]
    [MemberData(nameof(UnsafeAverage_Generic_TestData))]
    public void UnsafeAverage_Generic<T>(T[] sourceArray, double expected) where T : INumberBase<T>
    {
        Assert.Equal(expected, ((Span<T>)sourceArray).UnsafeAverage<T, double>());
        Assert.Equal(expected, ((ReadOnlySpan<T>)sourceArray).UnsafeAverage<T, double>());
    }

    [Fact]
    public void UnsafeAverage_Generic_SumTooLarge_ThrowsOverflowException()
    {
        decimal[] arraySource = new[] { decimal.MaxValue, decimal.MaxValue };

        Assert.Throws<OverflowException>(() => ((Span<decimal>)arraySource).UnsafeAverage<decimal, decimal>());
        Assert.Throws<OverflowException>(() => ((ReadOnlySpan<decimal>)arraySource).UnsafeAverage<decimal, decimal>());
    }

    [Fact]
    public void UnsafeAverage_Generic_EmptySource_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => Span<decimal>.Empty.UnsafeAverage<decimal, decimal>());
        Assert.Throws<InvalidOperationException>(() => ReadOnlySpan<decimal>.Empty.UnsafeAverage<decimal, decimal>());
    }
}
