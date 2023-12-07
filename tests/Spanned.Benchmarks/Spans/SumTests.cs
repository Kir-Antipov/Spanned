using System.Numerics;

namespace Spanned.Benchmarks.Spans;

public class SumTests : SpansTest
{
    [ParamsSource(nameof(PositiveSize))]
    public int N { get; set; }

    public byte[] Values_Byte { get; set; } = null!;

    public sbyte[] Values_SByte { get; set; } = null!;

    public short[] Values_Int16 { get; set; } = null!;

    public ushort[] Values_UInt16 { get; set; } = null!;

    public int[] Values_Int32 { get; set; } = null!;

    public uint[] Values_UInt32 { get; set; } = null!;

    public long[] Values_Int64 { get; set; } = null!;

    public ulong[] Values_UInt64 { get; set; } = null!;

    public float[] Values_Single { get; set; } = null!;

    public double[] Values_Double { get; set; } = null!;

    public decimal[] Values_Decimal { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        Values_Byte = CreateArray<byte>(N, filler: 0);
        Values_SByte = CreateArray<sbyte>(N, filler: 0);
        Values_Int16 = CreateRandomArray<short>(N, -6, 6);
        Values_UInt16 = CreateRandomArray<ushort>(N, 0, 12);
        Values_Int32 = CreateRandomArray(N, -500_000, 500_000);
        Values_UInt32 = CreateRandomArray<uint>(N, 0, 1_000_000);
        Values_Int64 = CreateRandomArray<long>(N, -1_000_000_000, 1_000_000_000);
        Values_UInt64 = CreateRandomArray<ulong>(N, 0, 2_000_000_000);
        Values_Single = CreateRandomArray<float>(N);
        Values_Double = CreateRandomArray<double>(N);
        Values_Decimal = CreateRandomArray<decimal>(N, -1_000_000_000, 1_000_000_000);
    }


    [Benchmark(Baseline = true), BenchmarkCategory("Byte")]
    public byte Sum_Loop_Byte() => Sum<byte>(Values_Byte);

    [Benchmark, BenchmarkCategory("Byte")]
    public byte Sum_Linq_Byte() => (byte)Values_Byte.AsEnumerable().Sum(int.CreateChecked);

    [Benchmark, BenchmarkCategory("Byte")]
    public byte Sum_Span_Byte() => Values_Byte.AsSpan().Sum();

    [Benchmark, BenchmarkCategory("Byte")]
    public byte UnsafeSum_Span_Byte() => Values_Byte.AsSpan().UnsafeSum();


    [Benchmark(Baseline = true), BenchmarkCategory("SByte")]
    public sbyte Sum_Loop_SByte() => Sum<sbyte>(Values_SByte);

    [Benchmark, BenchmarkCategory("SByte")]
    public sbyte Sum_Linq_SByte() => (sbyte)Values_SByte.AsEnumerable().Sum(int.CreateChecked);

    [Benchmark, BenchmarkCategory("SByte")]
    public sbyte Sum_Span_SByte() => Values_SByte.AsSpan().Sum();

    [Benchmark, BenchmarkCategory("SByte")]
    public sbyte UnsafeSum_Span_SByte() => Values_SByte.AsSpan().UnsafeSum();


    [Benchmark(Baseline = true), BenchmarkCategory("Int16")]
    public short Sum_Loop_Int16() => Sum<short>(Values_Int16);

    [Benchmark, BenchmarkCategory("Int16")]
    public short Sum_Linq_Int16() => (short)Values_Int16.AsEnumerable().Sum(int.CreateChecked);

    [Benchmark, BenchmarkCategory("Int16")]
    public short Sum_Span_Int16() => Values_Int16.AsSpan().Sum();

    [Benchmark, BenchmarkCategory("Int16")]
    public short UnsafeSum_Span_Int16() => Values_Int16.AsSpan().UnsafeSum();


    [Benchmark(Baseline = true), BenchmarkCategory("UInt16")]
    public ushort Sum_Loop_UInt16() => Sum<ushort>(Values_UInt16);

    [Benchmark, BenchmarkCategory("UInt16")]
    public ushort Sum_Linq_UInt16() => (ushort)Values_UInt16.AsEnumerable().Sum(int.CreateChecked);

    [Benchmark, BenchmarkCategory("UInt16")]
    public ushort Sum_Span_UInt16() => Values_UInt16.AsSpan().Sum();

    [Benchmark, BenchmarkCategory("UInt16")]
    public ushort UnsafeSum_Span_UInt16() => Values_UInt16.AsSpan().UnsafeSum();


    [Benchmark(Baseline = true), BenchmarkCategory("Int32")]
    public int Sum_Loop_Int32() => Sum<int>(Values_Int32);

    [Benchmark, BenchmarkCategory("Int32")]
    public int Sum_Linq_Int32() => Values_Int32.AsEnumerable().Sum();

    [Benchmark, BenchmarkCategory("Int32")]
    public int Sum_Span_Int32() => Values_Int32.AsSpan().Sum();

    [Benchmark, BenchmarkCategory("Int32")]
    public int UnsafeSum_Span_Int32() => Values_Int32.AsSpan().UnsafeSum();


    [Benchmark(Baseline = true), BenchmarkCategory("UInt32")]
    public uint Sum_Loop_UInt32() => Sum<uint>(Values_UInt32);

    [Benchmark, BenchmarkCategory("UInt32")]
    public uint Sum_Linq_UInt32() => (uint)Values_UInt32.AsEnumerable().Sum(long.CreateChecked);

    [Benchmark, BenchmarkCategory("UInt32")]
    public uint Sum_Span_UInt32() => Values_UInt32.AsSpan().Sum();

    [Benchmark, BenchmarkCategory("UInt32")]
    public uint UnsafeSum_Span_UInt32() => Values_UInt32.AsSpan().UnsafeSum();


    [Benchmark(Baseline = true), BenchmarkCategory("Int64")]
    public long Sum_Loop_Int64() => Sum<long>(Values_Int64);

    [Benchmark, BenchmarkCategory("Int64")]
    public long Sum_Linq_Int64() => Values_Int64.AsEnumerable().Sum();

    [Benchmark, BenchmarkCategory("Int64")]
    public long Sum_Span_Int64() => Values_Int64.AsSpan().Sum();

    [Benchmark, BenchmarkCategory("Int64")]
    public long UnsafeSum_Span_Int64() => Values_Int64.AsSpan().UnsafeSum();


    [Benchmark(Baseline = true), BenchmarkCategory("UInt64")]
    public ulong Sum_Loop_UInt64() => Sum<ulong>(Values_UInt64);

    [Benchmark, BenchmarkCategory("UInt64")]
    public ulong Sum_Linq_UInt64() => (ulong)Values_UInt64.AsEnumerable().Sum(decimal.CreateChecked);

    [Benchmark, BenchmarkCategory("UInt64")]
    public ulong Sum_Span_UInt64() => Values_UInt64.AsSpan().Sum();

    [Benchmark, BenchmarkCategory("UInt64")]
    public ulong UnsafeSum_Span_UInt64() => Values_UInt64.AsSpan().UnsafeSum();


    [Benchmark(Baseline = true), BenchmarkCategory("Single")]
    public float Sum_Loop_Single() => Sum<float>(Values_Single);

    [Benchmark, BenchmarkCategory("Single")]
    public float Sum_Linq_Single() => Values_Single.AsEnumerable().Sum();

    [Benchmark, BenchmarkCategory("Single")]
    public float Sum_Span_Single() => Values_Single.AsSpan().Sum();


    [Benchmark(Baseline = true), BenchmarkCategory("Double")]
    public double Sum_Loop_Double() => Sum<double>(Values_Double);

    [Benchmark, BenchmarkCategory("Double")]
    public double Sum_Linq_Double() => Values_Double.AsEnumerable().Sum();

    [Benchmark, BenchmarkCategory("Double")]
    public double Sum_Span_Double() => Values_Double.AsSpan().Sum();


    [Benchmark(Baseline = true), BenchmarkCategory("Decimal")]
    public decimal Sum_Loop_Decimal() => Sum<decimal>(Values_Decimal);

    [Benchmark, BenchmarkCategory("Decimal")]
    public decimal Sum_Linq_Decimal() => Values_Decimal.AsEnumerable().Sum();

    [Benchmark, BenchmarkCategory("Decimal")]
    public decimal Sum_Span_Decimal() => Values_Decimal.AsSpan().Sum();


    private static T Sum<T>(ReadOnlySpan<T> values) where T : INumberBase<T>
    {
        T sum = T.Zero;

        for (int i = 0; i < values.Length; i++)
            sum = checked(sum + values[i]);

        return sum;
    }
}
