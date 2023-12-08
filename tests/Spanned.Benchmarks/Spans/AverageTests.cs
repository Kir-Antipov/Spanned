using System.Numerics;

namespace Spanned.Benchmarks.Spans;

public class AverageTests : SpansTest
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
        Values_Byte = CreateRandomArray<byte>(N);
        Values_SByte = CreateRandomArray<sbyte>(N);
        Values_Int16 = CreateRandomArray<short>(N);
        Values_UInt16 = CreateRandomArray<ushort>(N);
        Values_Int32 = CreateRandomArray<int>(N);
        Values_UInt32 = CreateRandomArray<uint>(N);
        Values_Int64 = CreateRandomArray<long>(N, -1_000_000_000, 1_000_000_000);
        Values_UInt64 = CreateRandomArray<ulong>(N, 0, 2_000_000_000);
        Values_Single = CreateRandomArray<float>(N);
        Values_Double = CreateRandomArray<double>(N);
        Values_Decimal = CreateRandomArray<decimal>(N, -1_000_000_000, 1_000_000_000);
    }


    [Benchmark(Baseline = true), BenchmarkCategory("Byte")]
    public double Average_Loop_Byte() => Average<byte, long, double>(Values_Byte);

    [Benchmark, BenchmarkCategory("Byte")]
    public double Average_Linq_Byte() => Values_Byte.AsEnumerable().Average(long.CreateChecked);

    [Benchmark, BenchmarkCategory("Byte")]
    public double Average_Span_Byte() => Values_Byte.AsSpan().Average();

    [Benchmark, BenchmarkCategory("Byte")]
    public double UnsafeAverage_Span_Byte() => Values_Byte.AsSpan().UnsafeAverage();


    [Benchmark(Baseline = true), BenchmarkCategory("SByte")]
    public double Average_Loop_SByte() => Average<sbyte, long, double>(Values_SByte);

    [Benchmark, BenchmarkCategory("SByte")]
    public double Average_Linq_SByte() => Values_SByte.AsEnumerable().Average(long.CreateChecked);

    [Benchmark, BenchmarkCategory("SByte")]
    public double Average_Span_SByte() => Values_SByte.AsSpan().Average();

    [Benchmark, BenchmarkCategory("SByte")]
    public double UnsafeAverage_Span_SByte() => Values_SByte.AsSpan().UnsafeAverage();


    [Benchmark(Baseline = true), BenchmarkCategory("Int16")]
    public double Average_Loop_Int16() => Average<short, long, double>(Values_Int16);

    [Benchmark, BenchmarkCategory("Int16")]
    public double Average_Linq_Int16() => Values_Int16.AsEnumerable().Average(long.CreateChecked);

    [Benchmark, BenchmarkCategory("Int16")]
    public double Average_Span_Int16() => Values_Int16.AsSpan().Average();

    [Benchmark, BenchmarkCategory("Int16")]
    public double UnsafeAverage_Span_Int16() => Values_Int16.AsSpan().UnsafeAverage();


    [Benchmark(Baseline = true), BenchmarkCategory("UInt16")]
    public double Average_Loop_UInt16() => Average<ushort, long, double>(Values_UInt16);

    [Benchmark, BenchmarkCategory("UInt16")]
    public double Average_Linq_UInt16() => Values_UInt16.AsEnumerable().Average(long.CreateChecked);

    [Benchmark, BenchmarkCategory("UInt16")]
    public double Average_Span_UInt16() => Values_UInt16.AsSpan().Average();

    [Benchmark, BenchmarkCategory("UInt16")]
    public double UnsafeAverage_Span_UInt16() => Values_UInt16.AsSpan().UnsafeAverage();


    [Benchmark(Baseline = true), BenchmarkCategory("Int32")]
    public double Average_Loop_Int32() => Average<int, long, double>(Values_Int32);

    [Benchmark, BenchmarkCategory("Int32")]
    public double Average_Linq_Int32() => Values_Int32.AsEnumerable().Average(long.CreateChecked);

    [Benchmark, BenchmarkCategory("Int32")]
    public double Average_Span_Int32() => Values_Int32.AsSpan().Average();

    [Benchmark, BenchmarkCategory("Int32")]
    public double UnsafeAverage_Span_Int32() => Values_Int32.AsSpan().UnsafeAverage();


    [Benchmark(Baseline = true), BenchmarkCategory("UInt32")]
    public double Average_Loop_UInt32() => Average<uint, long, double>(Values_UInt32);

    [Benchmark, BenchmarkCategory("UInt32")]
    public double Average_Linq_UInt32() => Values_UInt32.AsEnumerable().Average(long.CreateChecked);

    [Benchmark, BenchmarkCategory("UInt32")]
    public double Average_Span_UInt32() => Values_UInt32.AsSpan().Average();

    [Benchmark, BenchmarkCategory("UInt32")]
    public double UnsafeAverage_Span_UInt32() => Values_UInt32.AsSpan().UnsafeAverage();


    [Benchmark(Baseline = true), BenchmarkCategory("Int64")]
    public double Average_Loop_Int64() => Average<long, decimal, double>(Values_Int64);

    [Benchmark, BenchmarkCategory("Int64")]
    public double Average_Linq_Int64() => Values_Int64.AsEnumerable().Average();

    [Benchmark, BenchmarkCategory("Int64")]
    public double Average_Span_Int64() => Values_Int64.AsSpan().Average();

    [Benchmark, BenchmarkCategory("Int64")]
    public double UnsafeAverage_Span_Int64() => Values_Int64.AsSpan().UnsafeAverage();


    [Benchmark(Baseline = true), BenchmarkCategory("UInt64")]
    public double Average_Loop_UInt64() => Average<ulong, decimal, double>(Values_UInt64);

    [Benchmark, BenchmarkCategory("UInt64")]
    public double Average_Linq_UInt64() => (double)Values_UInt64.AsEnumerable().Average(decimal.CreateChecked);

    [Benchmark, BenchmarkCategory("UInt64")]
    public double Average_Span_UInt64() => Values_UInt64.AsSpan().Average();

    [Benchmark, BenchmarkCategory("UInt64")]
    public double UnsafeAverage_Span_UInt64() => Values_UInt64.AsSpan().UnsafeAverage();


    [Benchmark(Baseline = true), BenchmarkCategory("Single")]
    public double Average_Loop_Single() => Average<float, double, double>(Values_Single);

    [Benchmark, BenchmarkCategory("Single")]
    public double Average_Linq_Single() => Values_Single.AsEnumerable().Average();

    [Benchmark, BenchmarkCategory("Single")]
    public double Average_Span_Single() => Values_Single.AsSpan().Average();


    [Benchmark(Baseline = true), BenchmarkCategory("Double")]
    public double Average_Loop_Double() => Average<double, double, double>(Values_Double);

    [Benchmark, BenchmarkCategory("Double")]
    public double Average_Linq_Double() => Values_Double.AsEnumerable().Average();

    [Benchmark, BenchmarkCategory("Double")]
    public double Average_Span_Double() => Values_Double.AsSpan().Average();


    [Benchmark(Baseline = true), BenchmarkCategory("Decimal")]
    public decimal Average_Loop_Decimal() => Average<decimal, decimal, decimal>(Values_Decimal);

    [Benchmark, BenchmarkCategory("Decimal")]
    public decimal Average_Linq_Decimal() => Values_Decimal.AsEnumerable().Average();

    [Benchmark, BenchmarkCategory("Decimal")]
    public decimal Average_Span_Decimal() => Values_Decimal.AsSpan().Average();


    private static TResult Average<TSource, TAccumulator, TResult>(ReadOnlySpan<TSource> values)
        where TSource : INumberBase<TSource>
        where TAccumulator : INumberBase<TAccumulator>
        where TResult : INumberBase<TResult>
    {
        if (values.IsEmpty)
            throw new InvalidOperationException();

        TAccumulator sum = TAccumulator.Zero;
        for (int i = 0; i < values.Length; i++)
            sum += TAccumulator.CreateChecked(values[i]);

        return TResult.CreateChecked(sum) / TResult.CreateChecked(values.Length);
    }
}
