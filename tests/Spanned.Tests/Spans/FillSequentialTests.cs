using System.Numerics;

namespace Spanned.Tests.Spans;

public class FillSequentialTests
{
    [Fact]
    public void FillSequential_Byte()
    {
        Span<byte> source = new byte[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_Byte_CustomOffsetAndStep()
    {
        Span<byte> source = new byte[5];

        source.FillSequential(offset: 2, step: 3);

        Assert.Equal([2, 5, 8, 11, 14], source.ToArray());
    }

    [Fact]
    public void FillSequential_Byte_SumTooLarge_Overflows()
    {
        Span<byte> source = new byte[5];

        source.FillSequential(offset: byte.MaxValue, step: 2);

        Assert.Equal([byte.MaxValue, byte.MinValue + 1, byte.MinValue + 3, byte.MinValue + 5, byte.MinValue + 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_Byte_EmptySource()
    {
        Span<byte> source = Span<byte>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<byte>(), source.ToArray());
    }

    [Fact]
    public void FillSequential_SByte()
    {
        Span<sbyte> source = new sbyte[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_SByte_CustomOffsetAndStep()
    {
        Span<sbyte> source = new sbyte[5];

        source.FillSequential(offset: 2, step: 3);

        Assert.Equal([2, 5, 8, 11, 14], source.ToArray());
    }

    [Fact]
    public void FillSequential_SByte_NegativeStep()
    {
        Span<sbyte> source = new sbyte[5];

        source.FillSequential(step: -1);

        Assert.Equal([0, -1, -2, -3, -4], source.ToArray());
    }

    [Fact]
    public void FillSequential_SByte_NegativeStepAndNegativeOffset()
    {
        Span<sbyte> source = new sbyte[5];

        source.FillSequential(offset: -2, step: -3);

        Assert.Equal([-2, -5, -8, -11, -14], source.ToArray());
    }

    [Fact]
    public void FillSequential_SByte_SumTooSmall_Overflows()
    {
        Span<sbyte> source = new sbyte[5];

        source.FillSequential(offset: sbyte.MinValue, step: -2);

        Assert.Equal([sbyte.MinValue, sbyte.MaxValue - 1, sbyte.MaxValue - 3, sbyte.MaxValue - 5, sbyte.MaxValue - 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_SByte_SumTooLarge_Overflows()
    {
        Span<sbyte> source = new sbyte[5];

        source.FillSequential(offset: sbyte.MaxValue, step: 2);

        Assert.Equal([sbyte.MaxValue, sbyte.MinValue + 1, sbyte.MinValue + 3, sbyte.MinValue + 5, sbyte.MinValue + 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_SByte_EmptySource()
    {
        Span<sbyte> source = Span<sbyte>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<sbyte>(), source.ToArray());
    }

    [Fact]
    public void FillSequential_Short()
    {
        Span<short> source = new short[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_Short_CustomOffsetAndStep()
    {
        Span<short> source = new short[5];

        source.FillSequential(offset: 2, step: 3);

        Assert.Equal([2, 5, 8, 11, 14], source.ToArray());
    }

    [Fact]
    public void FillSequential_Short_NegativeStep()
    {
        Span<short> source = new short[5];

        source.FillSequential(step: -1);

        Assert.Equal([0, -1, -2, -3, -4], source.ToArray());
    }

    [Fact]
    public void FillSequential_Short_NegativeStepAndNegativeOffset()
    {
        Span<short> source = new short[5];

        source.FillSequential(offset: -2, step: -3);

        Assert.Equal([-2, -5, -8, -11, -14], source.ToArray());
    }

    [Fact]
    public void FillSequential_Short_SumTooSmall_Overflows()
    {
        Span<short> source = new short[5];

        source.FillSequential(offset: short.MinValue, step: -2);

        Assert.Equal([short.MinValue, short.MaxValue - 1, short.MaxValue - 3, short.MaxValue - 5, short.MaxValue - 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_Short_SumTooLarge_Overflows()
    {
        Span<short> source = new short[5];

        source.FillSequential(offset: short.MaxValue, step: 2);

        Assert.Equal([short.MaxValue, short.MinValue + 1, short.MinValue + 3, short.MinValue + 5, short.MinValue + 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_Short_EmptySource()
    {
        Span<short> source = Span<short>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<short>(), source.ToArray());
    }

    [Fact]
    public void FillSequential_UShort()
    {
        Span<ushort> source = new ushort[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_UShort_CustomOffsetAndStep()
    {
        Span<ushort> source = new ushort[5];

        source.FillSequential(offset: 2, step: 3);

        Assert.Equal([2, 5, 8, 11, 14], source.ToArray());
    }

    [Fact]
    public void FillSequential_UShort_SumTooLarge_Overflows()
    {
        Span<ushort> source = new ushort[5];

        source.FillSequential(offset: ushort.MaxValue, step: 2);

        Assert.Equal([ushort.MaxValue, ushort.MinValue + 1, ushort.MinValue + 3, ushort.MinValue + 5, ushort.MinValue + 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_UShort_EmptySource()
    {
        Span<ushort> source = Span<ushort>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<ushort>(), source.ToArray());
    }

    [Fact]
    public void FillSequential_Int()
    {
        Span<int> source = new int[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_Int_CustomOffsetAndStep()
    {
        Span<int> source = new int[5];

        source.FillSequential(offset: 2, step: 3);

        Assert.Equal([2, 5, 8, 11, 14], source.ToArray());
    }

    [Fact]
    public void FillSequential_Int_NegativeStep()
    {
        Span<int> source = new int[5];

        source.FillSequential(step: -1);

        Assert.Equal([0, -1, -2, -3, -4], source.ToArray());
    }

    [Fact]
    public void FillSequential_Int_NegativeStepAndNegativeOffset()
    {
        Span<int> source = new int[5];

        source.FillSequential(offset: -2, step: -3);

        Assert.Equal([-2, -5, -8, -11, -14], source.ToArray());
    }

    [Fact]
    public void FillSequential_Int_SumTooSmall_Overflows()
    {
        Span<int> source = new int[5];

        source.FillSequential(offset: int.MinValue, step: -2);

        Assert.Equal([int.MinValue, int.MaxValue - 1, int.MaxValue - 3, int.MaxValue - 5, int.MaxValue - 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_Int_SumTooLarge_Overflows()
    {
        Span<int> source = new int[5];

        source.FillSequential(offset: int.MaxValue, step: 2);

        Assert.Equal([int.MaxValue, int.MinValue + 1, int.MinValue + 3, int.MinValue + 5, int.MinValue + 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_Int_EmptySource()
    {
        Span<int> source = Span<int>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<int>(), source.ToArray());
    }

    [Fact]
    public void FillSequential_UInt()
    {
        Span<uint> source = new uint[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_UInt_CustomOffsetAndStep()
    {
        Span<uint> source = new uint[5];

        source.FillSequential(offset: 2, step: 3);

        Assert.Equal([2, 5, 8, 11, 14], source.ToArray());
    }

    [Fact]
    public void FillSequential_UInt_SumTooLarge_Overflows()
    {
        Span<uint> source = new uint[5];

        source.FillSequential(offset: uint.MaxValue, step: 2);

        Assert.Equal([uint.MaxValue, uint.MinValue + 1, uint.MinValue + 3, uint.MinValue + 5, uint.MinValue + 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_UInt_EmptySource()
    {
        Span<uint> source = Span<uint>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<uint>(), source.ToArray());
    }

    [Fact]
    public void FillSequential_Long()
    {
        Span<long> source = new long[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_Long_CustomOffsetAndStep()
    {
        Span<long> source = new long[5];

        source.FillSequential(offset: 2, step: 3);

        Assert.Equal([2, 5, 8, 11, 14], source.ToArray());
    }

    [Fact]
    public void FillSequential_Long_NegativeStep()
    {
        Span<long> source = new long[5];

        source.FillSequential(step: -1);

        Assert.Equal([0, -1, -2, -3, -4], source.ToArray());
    }

    [Fact]
    public void FillSequential_Long_NegativeStepAndNegativeOffset()
    {
        Span<long> source = new long[5];

        source.FillSequential(offset: -2, step: -3);

        Assert.Equal([-2, -5, -8, -11, -14], source.ToArray());
    }

    [Fact]
    public void FillSequential_Long_SumTooSmall_Overflows()
    {
        Span<long> source = new long[5];

        source.FillSequential(offset: long.MinValue, step: -2);

        Assert.Equal([long.MinValue, long.MaxValue - 1, long.MaxValue - 3, long.MaxValue - 5, long.MaxValue - 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_Long_SumTooLarge_Overflows()
    {
        Span<long> source = new long[5];

        source.FillSequential(offset: long.MaxValue, step: 2);

        Assert.Equal([long.MaxValue, long.MinValue + 1, long.MinValue + 3, long.MinValue + 5, long.MinValue + 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_Long_EmptySource()
    {
        Span<long> source = Span<long>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<long>(), source.ToArray());
    }

    [Fact]
    public void FillSequential_ULong()
    {
        Span<ulong> source = new ulong[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_ULong_CustomOffsetAndStep()
    {
        Span<ulong> source = new ulong[5];

        source.FillSequential(offset: 2, step: 3);

        Assert.Equal([2, 5, 8, 11, 14], source.ToArray());
    }

    [Fact]
    public void FillSequential_ULong_SumTooLarge_Overflows()
    {
        Span<ulong> source = new ulong[5];

        source.FillSequential(offset: ulong.MaxValue, step: 2);

        Assert.Equal([ulong.MaxValue, ulong.MinValue + 1, ulong.MinValue + 3, ulong.MinValue + 5, ulong.MinValue + 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_ULong_EmptySource()
    {
        Span<ulong> source = Span<ulong>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<ulong>(), source.ToArray());
    }

    [Fact]
    public void FillSequential_NInt()
    {
        Span<nint> source = new nint[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_NInt_CustomOffsetAndStep()
    {
        Span<nint> source = new nint[5];

        source.FillSequential(offset: 2, step: 3);

        Assert.Equal([2, 5, 8, 11, 14], source.ToArray());
    }

    [Fact]
    public void FillSequential_NInt_NegativeStep()
    {
        Span<nint> source = new nint[5];

        source.FillSequential(step: -1);

        Assert.Equal([0, -1, -2, -3, -4], source.ToArray());
    }

    [Fact]
    public void FillSequential_NInt_NegativeStepAndNegativeOffset()
    {
        Span<nint> source = new nint[5];

        source.FillSequential(offset: -2, step: -3);

        Assert.Equal([-2, -5, -8, -11, -14], source.ToArray());
    }

    [Fact]
    public void FillSequential_NInt_SumTooSmall_Overflows()
    {
        Span<nint> source = new nint[5];

        source.FillSequential(offset: nint.MinValue, step: -2);

        Assert.Equal([nint.MinValue, nint.MaxValue - 1, nint.MaxValue - 3, nint.MaxValue - 5, nint.MaxValue - 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_NInt_SumTooLarge_Overflows()
    {
        Span<nint> source = new nint[5];

        source.FillSequential(offset: nint.MaxValue, step: 2);

        Assert.Equal([nint.MaxValue, nint.MinValue + 1, nint.MinValue + 3, nint.MinValue + 5, nint.MinValue + 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_NInt_EmptySource()
    {
        Span<nint> source = Span<nint>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<nint>(), source.ToArray());
    }

    [Fact]
    public void FillSequential_NUInt()
    {
        Span<nuint> source = new nuint[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_NUInt_CustomOffsetAndStep()
    {
        Span<nuint> source = new nuint[5];

        source.FillSequential(offset: 2, step: 3);

        Assert.Equal([2, 5, 8, 11, 14], source.ToArray());
    }

    [Fact]
    public void FillSequential_NUInt_SumTooLarge_Overflows()
    {
        Span<nuint> source = new nuint[5];

        source.FillSequential(offset: nuint.MaxValue, step: 2);

        Assert.Equal([nuint.MaxValue, nuint.MinValue + 1, nuint.MinValue + 3, nuint.MinValue + 5, nuint.MinValue + 7], source.ToArray());
    }

    [Fact]
    public void FillSequential_NUInt_EmptySource()
    {
        Span<nuint> source = Span<nuint>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<nuint>(), source.ToArray());
    }

    [Fact]
    public void FillSequential_Float()
    {
        Span<float> source = new float[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_Float_CustomOffsetAndStep()
    {
        Span<float> source = new float[5];

        source.FillSequential(offset: 2.5f, step: 1.25f);

        Assert.Equal([2.5f, 3.75f, 5f, 6.25f, 7.5f], source.ToArray());
    }

    [Fact]
    public void FillSequential_Float_NegativeStep()
    {
        Span<float> source = new float[5];

        source.FillSequential(step: -0.5f);

        Assert.Equal([0, -0.5f, -1f, -1.5f, -2f], source.ToArray());
    }

    [Fact]
    public void FillSequential_Float_NegativeStepAndNegativeOffset()
    {
        Span<float> source = new float[5];

        source.FillSequential(offset: -2.5f, step: -1.25f);

        Assert.Equal([-2.5f, -3.75f, -5f, -6.25f, -7.5f], source.ToArray());
    }

    [Fact]
    public void FillSequential_Float_SumTooSmall_OverflowsToNegativeInfinity()
    {
        Span<float> source = new float[5];

        source.FillSequential(offset: float.MinValue, step: float.MinValue);

        Assert.Equal([float.MinValue, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity], source.ToArray());
    }

    [Fact]
    public void FillSequential_Float_SumTooLarge_OverflowsToPositiveInfinity()
    {
        Span<float> source = new float[5];

        source.FillSequential(offset: float.MaxValue, step: float.MaxValue);

        Assert.Equal([float.MaxValue, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity], source.ToArray());
    }

    [Fact]
    public void FillSequential_Float_NaNOffsetResultsInNaN()
    {
        Span<float> source = new float[5];

        source.FillSequential(offset: float.NaN);

        Assert.Equal([float.NaN, float.NaN, float.NaN, float.NaN, float.NaN], source.ToArray());
    }

    [Fact]
    public void FillSequential_Float_NaNStepResultsInNaN()
    {
        Span<float> source = new float[5];

        source.FillSequential(step: float.NaN);

        Assert.Equal([float.NaN, float.NaN, float.NaN, float.NaN, float.NaN], source.ToArray());
    }

    [Fact]
    public void FillSequential_Float_EmptySource()
    {
        Span<float> source = Span<float>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<float>(), source.ToArray());
    }

    [Fact]
    public void FillSequential_Double()
    {
        Span<double> source = new double[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_Double_CustomOffsetAndStep()
    {
        Span<double> source = new double[5];

        source.FillSequential(offset: 2.5, step: 1.25);

        Assert.Equal([2.5, 3.75, 5, 6.25, 7.5], source.ToArray());
    }

    [Fact]
    public void FillSequential_Double_NegativeStep()
    {
        Span<double> source = new double[5];

        source.FillSequential(step: -0.5);

        Assert.Equal([0, -0.5, -1, -1.5, -2], source.ToArray());
    }

    [Fact]
    public void FillSequential_Double_NegativeStepAndNegativeOffset()
    {
        Span<double> source = new double[5];

        source.FillSequential(offset: -2.5, step: -1.25);

        Assert.Equal([-2.5, -3.75, -5, -6.25, -7.5], source.ToArray());
    }

    [Fact]
    public void FillSequential_Double_SumTooSmall_OverflowsToNegativeInfinity()
    {
        Span<double> source = new double[5];

        source.FillSequential(offset: double.MinValue, step: double.MinValue);

        Assert.Equal([double.MinValue, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity], source.ToArray());
    }

    [Fact]
    public void FillSequential_Double_SumTooLarge_OverflowsToPositiveInfinity()
    {
        Span<double> source = new double[5];

        source.FillSequential(offset: double.MaxValue, step: double.MaxValue);

        Assert.Equal([double.MaxValue, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity], source.ToArray());
    }

    [Fact]
    public void FillSequential_Double_NaNOffsetResultsInNaN()
    {
        Span<double> source = new double[5];

        source.FillSequential(offset: double.NaN);

        Assert.Equal([double.NaN, double.NaN, double.NaN, double.NaN, double.NaN], source.ToArray());
    }

    [Fact]
    public void FillSequential_Double_NaNStepResultsInNaN()
    {
        Span<double> source = new double[5];

        source.FillSequential(step: double.NaN);

        Assert.Equal([double.NaN, double.NaN, double.NaN, double.NaN, double.NaN], source.ToArray());
    }

    [Fact]
    public void FillSequential_Double_EmptySource()
    {
        Span<double> source = Span<double>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<double>(), source.ToArray());
    }


    [Fact]
    public void FillSequential_Decimal()
    {
        Span<decimal> source = new decimal[5];

        source.FillSequential();

        Assert.Equal([0, 1, 2, 3, 4], source.ToArray());
    }

    [Fact]
    public void FillSequential_Decimal_CustomOffsetAndStep()
    {
        Span<decimal> source = new decimal[5];

        source.FillSequential(offset: 2, step: 3);

        Assert.Equal([2, 5, 8, 11, 14], source.ToArray());
    }

    [Fact]
    public void FillSequential_Decimal_NegativeStep()
    {
        Span<decimal> source = new decimal[5];

        source.FillSequential(step: -1);

        Assert.Equal([0, -1, -2, -3, -4], source.ToArray());
    }

    [Fact]
    public void FillSequential_Decimal_NegativeStepAndNegativeOffset()
    {
        Span<decimal> source = new decimal[5];

        source.FillSequential(offset: -2, step: -3);

        Assert.Equal([-2, -5, -8, -11, -14], source.ToArray());
    }

    [Fact]
    public void FillSequential_Decimal_SumTooSmall_ThrowsOverflowException()
    {
        decimal[] sourceArray = new decimal[5];

        Assert.Throws<OverflowException>(() => ((Span<decimal>)sourceArray).FillSequential(offset: decimal.MinValue, step: -2));
    }

    [Fact]
    public void FillSequential_Decimal_SumTooLarge_ThrowsOverflowException()
    {
        decimal[] sourceArray = new decimal[5];

        Assert.Throws<OverflowException>(() => ((Span<decimal>)sourceArray).FillSequential(offset: decimal.MaxValue, step: 2));
    }

    [Fact]
    public void FillSequential_Decimal_EmptySource()
    {
        Span<decimal> source = Span<decimal>.Empty;

        source.FillSequential();

        Assert.Equal(Array.Empty<decimal>(), source.ToArray());
    }

    public static IEnumerable<object[]> FillSequential_Generic_TestData()
    {
        yield return [(byte)0, (byte)1, new byte[] { 0, 1, 2, 3, 4 }];
        yield return [(byte)2, (byte)3, new byte[] { 2, 5, 8, 11, 14 }];
        yield return [byte.MaxValue, (byte)2, new byte[] { byte.MaxValue, byte.MinValue + 1, byte.MinValue + 3, byte.MinValue + 5, byte.MinValue + 7 }];

        yield return [(sbyte)0, (sbyte)1, new sbyte[] { 0, 1, 2, 3, 4 }];
        yield return [(sbyte)2, (sbyte)3, new sbyte[] { 2, 5, 8, 11, 14 }];
        yield return [(sbyte)-2, (sbyte)-3, new sbyte[] { -2, -5, -8, -11, -14 }];
        yield return [sbyte.MinValue, (sbyte)-2, new sbyte[] { sbyte.MinValue, sbyte.MaxValue - 1, sbyte.MaxValue - 3, sbyte.MaxValue - 5, sbyte.MaxValue - 7 }];
        yield return [sbyte.MaxValue, (sbyte)2, new sbyte[] { sbyte.MaxValue, sbyte.MinValue + 1, sbyte.MinValue + 3, sbyte.MinValue + 5, sbyte.MinValue + 7 }];

        yield return [(short)0, (short)1, new short[] { 0, 1, 2, 3, 4 }];
        yield return [(short)2, (short)3, new short[] { 2, 5, 8, 11, 14 }];
        yield return [(short)-2, (short)-3, new short[] { -2, -5, -8, -11, -14 }];
        yield return [short.MinValue, (short)-2, new short[] { short.MinValue, short.MaxValue - 1, short.MaxValue - 3, short.MaxValue - 5, short.MaxValue - 7 }];
        yield return [short.MaxValue, (short)2, new short[] { short.MaxValue, short.MinValue + 1, short.MinValue + 3, short.MinValue + 5, short.MinValue + 7 }];

        yield return [(ushort)0, (ushort)1, new ushort[] { 0, 1, 2, 3, 4 }];
        yield return [(ushort)2, (ushort)3, new ushort[] { 2, 5, 8, 11, 14 }];
        yield return [ushort.MaxValue, (ushort)2, new ushort[] { ushort.MaxValue, ushort.MinValue + 1, ushort.MinValue + 3, ushort.MinValue + 5, ushort.MinValue + 7 }];

        yield return [0, 1, new int[] { 0, 1, 2, 3, 4 }];
        yield return [2, 3, new int[] { 2, 5, 8, 11, 14 }];
        yield return [-2, -3, new int[] { -2, -5, -8, -11, -14 }];
        yield return [int.MinValue, -2, new int[] { int.MinValue, int.MaxValue - 1, int.MaxValue - 3, int.MaxValue - 5, int.MaxValue - 7 }];
        yield return [int.MaxValue, 2, new int[] { int.MaxValue, int.MinValue + 1, int.MinValue + 3, int.MinValue + 5, int.MinValue + 7 }];

        yield return [(uint)0, (uint)1, new uint[] { 0, 1, 2, 3, 4 }];
        yield return [(uint)2, (uint)3, new uint[] { 2, 5, 8, 11, 14 }];
        yield return [uint.MaxValue, (uint)2, new uint[] { uint.MaxValue, uint.MinValue + 1, uint.MinValue + 3, uint.MinValue + 5, uint.MinValue + 7 }];

        yield return [(long)0, (long)1, new long[] { 0, 1, 2, 3, 4 }];
        yield return [(long)2, (long)3, new long[] { 2, 5, 8, 11, 14 }];
        yield return [(long)-2, (long)-3, new long[] { -2, -5, -8, -11, -14 }];
        yield return [long.MinValue, (long)-2, new long[] { long.MinValue, long.MaxValue - 1, long.MaxValue - 3, long.MaxValue - 5, long.MaxValue - 7 }];
        yield return [long.MaxValue, (long)2, new long[] { long.MaxValue, long.MinValue + 1, long.MinValue + 3, long.MinValue + 5, long.MinValue + 7 }];

        yield return [(ulong)0, (ulong)1, new ulong[] { 0, 1, 2, 3, 4 }];
        yield return [(ulong)2, (ulong)3, new ulong[] { 2, 5, 8, 11, 14 }];
        yield return [ulong.MaxValue, (ulong)2, new ulong[] { ulong.MaxValue, ulong.MinValue + 1, ulong.MinValue + 3, ulong.MinValue + 5, ulong.MinValue + 7 }];

        yield return [(nint)0, (nint)1, new nint[] { 0, 1, 2, 3, 4 }];
        yield return [(nint)2, (nint)3, new nint[] { 2, 5, 8, 11, 14 }];
        yield return [(nint)(-2), (nint)(-3), new nint[] { -2, -5, -8, -11, -14 }];
        yield return [nint.MinValue, (nint)(-2), new nint[] { nint.MinValue, nint.MaxValue - 1, nint.MaxValue - 3, nint.MaxValue - 5, nint.MaxValue - 7 }];
        yield return [nint.MaxValue, (nint)2, new nint[] { nint.MaxValue, nint.MinValue + 1, nint.MinValue + 3, nint.MinValue + 5, nint.MinValue + 7 }];

        yield return [(nuint)0, (nuint)1, new nuint[] { 0, 1, 2, 3, 4 }];
        yield return [(nuint)2, (nuint)3, new nuint[] { 2, 5, 8, 11, 14 }];
        yield return [nuint.MaxValue, (nuint)2, new nuint[] { nuint.MaxValue, nuint.MinValue + 1, nuint.MinValue + 3, nuint.MinValue + 5, nuint.MinValue + 7 }];

        yield return [0f, 1f, new float[] { 0, 1, 2, 3, 4 }];
        yield return [2.5f, 1.25f, new float[] { 2.5f, 3.75f, 5f, 6.25f, 7.5f }];
        yield return [-2.5f, -1.25f, new float[] { -2.5f, -3.75f, -5f, -6.25f, -7.5f }];
        yield return [float.MinValue, float.MinValue, new float[] { float.MinValue, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity }];
        yield return [float.MaxValue, float.MaxValue, new float[] { float.MaxValue, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity }];

        yield return [0d, 1d, new double[] { 0, 1, 2, 3, 4 }];
        yield return [2.5d, 1.25d, new double[] { 2.5, 3.75, 5, 6.25, 7.5 }];
        yield return [-2.5d, -1.25d, new double[] { -2.5, -3.75, -5, -6.25, -7.5 }];
        yield return [double.MinValue, double.MinValue, new double[] { double.MinValue, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity }];
        yield return [double.MaxValue, double.MaxValue, new double[] { double.MaxValue, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity }];

        yield return [(decimal)0, (decimal)1, new decimal[] { 0, 1, 2, 3, 4 }];
        yield return [(decimal)2, (decimal)3, new decimal[] { 2, 5, 8, 11, 14 }];
        yield return [(decimal)-2, (decimal)-3, new decimal[] { -2, -5, -8, -11, -14 }];

        yield return [(Half)0, (Half)1, new Half[] { (Half)0, (Half)1, (Half)2, (Half)3, (Half)4 }];
        yield return [(Half)2.5, (Half)1.25, new Half[] { (Half)2.5, (Half)3.75, (Half)5, (Half)6.25, (Half)7.5 }];
        yield return [(Half)(-2.5), (Half)(-1.25), new Half[] { (Half)(-2.5), (Half)(-3.75), (Half)(-5), (Half)(-6.25), (Half)(-7.5) }];
        yield return [Half.MinValue, Half.MinValue, new Half[] { Half.MinValue, Half.NegativeInfinity, Half.NegativeInfinity, Half.NegativeInfinity, Half.NegativeInfinity }];
        yield return [Half.MaxValue, Half.MaxValue, new Half[] { Half.MaxValue, Half.PositiveInfinity, Half.PositiveInfinity, Half.PositiveInfinity, Half.PositiveInfinity }];
    }

    [Theory]
    [MemberData(nameof(FillSequential_Generic_TestData))]
    public void FillSequential_Generic<T>(T offset, T step, T[] result) where T : IAdditionOperators<T, T, T>
    {
        Span<T> source = new T[result.Length];

        source.FillSequential(offset, step);

        Assert.Equal(result, source.ToArray());
    }
}
