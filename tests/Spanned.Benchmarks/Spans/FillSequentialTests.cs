using System.Numerics;

namespace Spanned.Benchmarks.Spans;

public class FillSequentialTests : SpansTest
{
    [ParamsSource(nameof(PositiveSize))]
    public int N { get; set; }

    public byte[] Values_Byte { get; set; } = null!;

    public short[] Values_Int16 { get; set; } = null!;

    public int[] Values_Int32 { get; set; } = null!;

    // FillSequential method does not, or at least should not,
    // care whether the number is signed or unsigned.
    // Therefore, benchmark methods for different sizes (sizeof()),
    // including the `uint` one, just to ensure that there is in
    // fact no difference compared to the `int` overload.
    public uint[] Values_UInt32 { get; set; } = null!;

    public long[] Values_Int64 { get; set; } = null!;

    public float[] Values_Single { get; set; } = null!;

    public double[] Values_Double { get; set; } = null!;

    public decimal[] Values_Decimal { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        Values_Byte = CreateArray<byte>(N, filler: 0);
        Values_Int16 = CreateArray<short>(N, filler: 0);
        Values_Int32 = CreateArray(N, filler: 0);
        Values_UInt32 = CreateArray<uint>(N, filler: 0);
        Values_Int64 = CreateArray<long>(N, filler: 0);
        Values_Single = CreateArray<float>(N, filler: 0);
        Values_Double = CreateArray<double>(N, filler: 0);
        Values_Decimal = CreateArray<decimal>(N, filler: 0);
    }


    [Benchmark(Baseline = true), BenchmarkCategory("Byte")]
    public void FillSequential_Loop_Byte() => FillSequential<byte>(Values_Byte);

    [Benchmark, BenchmarkCategory("Byte")]
    public void FillSequential_Span_Byte() => Values_Byte.AsSpan().FillSequential();


    [Benchmark(Baseline = true), BenchmarkCategory("Int16")]
    public void FillSequential_Loop_Int16() => FillSequential<short>(Values_Int16);

    [Benchmark, BenchmarkCategory("Int16")]
    public void FillSequential_Span_Int16() => Values_Int16.AsSpan().FillSequential();


    [Benchmark(Baseline = true), BenchmarkCategory("Int32")]
    public void FillSequential_Loop_Int32() => FillSequential<int>(Values_Int32);

    [Benchmark, BenchmarkCategory("Int32")]
    public void FillSequential_Span_Int32() => Values_Int32.AsSpan().FillSequential();


    [Benchmark(Baseline = true), BenchmarkCategory("UInt32")]
    public void FillSequential_Loop_UInt32() => FillSequential<uint>(Values_UInt32);

    [Benchmark, BenchmarkCategory("UInt32")]
    public void FillSequential_Span_UInt32() => Values_UInt32.AsSpan().FillSequential();


    [Benchmark(Baseline = true), BenchmarkCategory("Int64")]
    public void FillSequential_Loop_Int64() => FillSequential<long>(Values_Int64);

    [Benchmark, BenchmarkCategory("Int64")]
    public void FillSequential_Span_Int64() => Values_Int64.AsSpan().FillSequential();


    [Benchmark(Baseline = true), BenchmarkCategory("Single")]
    public void FillSequential_Loop_Single() => FillSequential<float>(Values_Single);

    [Benchmark, BenchmarkCategory("Single")]
    public void FillSequential_Span_Single() => Values_Single.AsSpan().FillSequential();


    [Benchmark(Baseline = true), BenchmarkCategory("Double")]
    public void FillSequential_Loop_Double() => FillSequential<double>(Values_Double);

    [Benchmark, BenchmarkCategory("Double")]
    public void FillSequential_Span_Double() => Values_Double.AsSpan().FillSequential();


    [Benchmark(Baseline = true), BenchmarkCategory("Decimal")]
    public void FillSequential_Loop_Decimal() => FillSequential<decimal>(Values_Decimal);

    [Benchmark, BenchmarkCategory("Decimal")]
    public void FillSequential_Span_Decimal() => Values_Decimal.AsSpan().FillSequential();


    private static void FillSequential<T>(Span<T> values) where T : INumberBase<T>
    {
        if (values.IsEmpty)
            return;

        for (int i = 1; i < values.Length; i++)
            values[i] = values[i] + T.One;
    }
}
