using System.Numerics;

namespace Spanned.Benchmarks.Spans;

public class LongSumTests : SpansTest
{
    [ParamsSource(nameof(PositiveSize))]
    public int N { get; set; }

    public byte[] Values_Byte { get; set; } = null!;

    public sbyte[] Values_SByte { get; set; } = null!;

    public short[] Values_Int16 { get; set; } = null!;

    public ushort[] Values_UInt16 { get; set; } = null!;

    public int[] Values_Int32 { get; set; } = null!;

    public uint[] Values_UInt32 { get; set; } = null!;

    public float[] Values_Single { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        Values_Byte = CreateArray<byte>(N, filler: 0);
        Values_SByte = CreateArray<sbyte>(N, filler: 0);
        Values_Int16 = CreateRandomArray<short>(N, -6, 6);
        Values_UInt16 = CreateRandomArray<ushort>(N, 0, 12);
        Values_Int32 = CreateRandomArray(N, -500_000, 500_000);
        Values_UInt32 = CreateRandomArray<uint>(N, 0, 1_000_000);
        Values_Single = CreateRandomArray<float>(N);
    }


    [Benchmark(Baseline = true), BenchmarkCategory("Byte")]
    public ulong LongSum_Loop_Byte() => LongSum<byte, ulong>(Values_Byte);

    [Benchmark, BenchmarkCategory("Byte")]
    public ulong LongSum_Linq_Byte() => (ulong)Values_Byte.AsEnumerable().Sum(long.CreateChecked);

    [Benchmark, BenchmarkCategory("Byte")]
    public ulong LongSum_Span_Byte() => Values_Byte.AsSpan().LongSum();


    [Benchmark(Baseline = true), BenchmarkCategory("SByte")]
    public long LongSum_Loop_SByte() => LongSum<sbyte, long>(Values_SByte);

    [Benchmark, BenchmarkCategory("SByte")]
    public long LongSum_Linq_SByte() => Values_SByte.AsEnumerable().Sum(long.CreateChecked);

    [Benchmark, BenchmarkCategory("SByte")]
    public long LongSum_Span_SByte() => Values_SByte.AsSpan().LongSum();


    [Benchmark(Baseline = true), BenchmarkCategory("Int16")]
    public long LongSum_Loop_Int16() => LongSum<short, long>(Values_Int16);

    [Benchmark, BenchmarkCategory("Int16")]
    public long LongSum_Linq_Int16() => Values_Int16.AsEnumerable().Sum(long.CreateChecked);

    [Benchmark, BenchmarkCategory("Int16")]
    public long LongSum_Span_Int16() => Values_Int16.AsSpan().LongSum();


    [Benchmark(Baseline = true), BenchmarkCategory("UInt16")]
    public ulong LongSum_Loop_UInt16() => LongSum<ushort, ulong>(Values_UInt16);

    [Benchmark, BenchmarkCategory("UInt16")]
    public ulong LongSum_Linq_UInt16() => (ulong)Values_UInt16.AsEnumerable().Sum(long.CreateChecked);

    [Benchmark, BenchmarkCategory("UInt16")]
    public ulong LongSum_Span_UInt16() => Values_UInt16.AsSpan().LongSum();


    [Benchmark(Baseline = true), BenchmarkCategory("Int32")]
    public long LongSum_Loop_Int32() => LongSum<int, long>(Values_Int32);

    [Benchmark, BenchmarkCategory("Int32")]
    public long LongSum_Linq_Int32() => Values_Int32.AsEnumerable().Sum(long.CreateChecked);

    [Benchmark, BenchmarkCategory("Int32")]
    public long LongSum_Span_Int32() => Values_Int32.AsSpan().LongSum();


    [Benchmark(Baseline = true), BenchmarkCategory("UInt32")]
    public ulong LongSum_Loop_UInt32() => LongSum<uint, ulong>(Values_UInt32);

    [Benchmark, BenchmarkCategory("UInt32")]
    public ulong LongSum_Linq_UInt32() => (ulong)Values_UInt32.AsEnumerable().Sum(long.CreateChecked);

    [Benchmark, BenchmarkCategory("UInt32")]
    public ulong LongSum_Span_UInt32() => Values_UInt32.AsSpan().LongSum();


    [Benchmark(Baseline = true), BenchmarkCategory("Single")]
    public double LongSum_Loop_Single() => LongSum<float, double>(Values_Single);

    [Benchmark, BenchmarkCategory("Single")]
    public double LongSum_Linq_Single() => Values_Single.AsEnumerable().Sum(double.CreateChecked);

    [Benchmark, BenchmarkCategory("Single")]
    public double LongSum_Span_Single() => Values_Single.AsSpan().LongSum();


    private static TResult LongSum<TSource, TResult>(ReadOnlySpan<TSource> values)
        where TSource : INumberBase<TSource>
        where TResult : INumberBase<TResult>
    {
        TResult sum = TResult.Zero;

        for (int i = 0; i < values.Length; i++)
            sum += TResult.CreateChecked(values[i]);

        return sum;
    }
}
