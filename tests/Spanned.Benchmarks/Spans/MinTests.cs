namespace Spanned.Benchmarks.Spans;

public class MinTests : SpansTest
{
    [ParamsSource(nameof(PositiveSize))]
    public int N { get; set; }

    public byte[] Values_Byte { get; set; } = null!;

    public short[] Values_Int16 { get; set; } = null!;

    public int[] Values_Int32 { get; set; } = null!;

    // Min/Max methods do not, or at least should not, care whether
    // the number is signed or unsigned.
    // Therefore, benchmark methods for different sizes (sizeof()),
    // including the `uint` one, just to ensure that there is in
    // fact no difference compared to the `int` overload.
    public uint[] Values_UInt32 { get; set; } = null!;

    public long[] Values_Int64 { get; set; } = null!;

    public float[] Values_Single { get; set; } = null!;

    public double[] Values_Double { get; set; } = null!;

    public decimal[] Values_Decimal { get; set; } = null!;

    public string[] Values_String { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        Values_Byte = CreateRandomArray<byte>(N);
        Values_Int16 = CreateRandomArray<short>(N);
        Values_Int32 = CreateRandomArray<int>(N);
        Values_UInt32 = CreateRandomArray<uint>(N);
        Values_Int64 = CreateRandomArray<long>(N);
        Values_Single = CreateRandomArray<float>(N);
        Values_Double = CreateRandomArray<double>(N);
        Values_Decimal = CreateRandomArray<decimal>(N);
        Values_String = CreateRandomArray(N);
    }


    [Benchmark(Baseline = true), BenchmarkCategory("Byte")]
    public byte Min_Loop_Byte() => Min<byte>(Values_Byte);

    [Benchmark, BenchmarkCategory("Byte")]
    public byte Min_Linq_Byte() => Values_Byte.AsEnumerable().Min();

    [Benchmark, BenchmarkCategory("Byte")]
    public byte Min_Span_Byte() => Values_Byte.AsSpan().Min();


    [Benchmark(Baseline = true), BenchmarkCategory("Int16")]
    public short Min_Loop_Int16() => Min<short>(Values_Int16);

    [Benchmark, BenchmarkCategory("Int16")]
    public short Min_Linq_Int16() => Values_Int16.AsEnumerable().Min();

    [Benchmark, BenchmarkCategory("Int16")]
    public short Min_Span_Int16() => Values_Int16.AsSpan().Min();


    [Benchmark(Baseline = true), BenchmarkCategory("Int32")]
    public int Min_Loop_Int32() => Min<int>(Values_Int32);

    [Benchmark, BenchmarkCategory("Int32")]
    public int Min_Linq_Int32() => Values_Int32.AsEnumerable().Min();

    [Benchmark, BenchmarkCategory("Int32")]
    public int Min_Span_Int32() => Values_Int32.AsSpan().Min();


    [Benchmark(Baseline = true), BenchmarkCategory("UInt32")]
    public uint Min_Loop_UInt32() => Min<uint>(Values_UInt32);

    [Benchmark, BenchmarkCategory("UInt32")]
    public uint Min_Linq_UInt32() => Values_UInt32.AsEnumerable().Min();

    [Benchmark, BenchmarkCategory("UInt32")]
    public uint Min_Span_UInt32() => Values_UInt32.AsSpan().Min();


    [Benchmark(Baseline = true), BenchmarkCategory("Int64")]
    public long Min_Loop_Int64() => Min<long>(Values_Int64);

    [Benchmark, BenchmarkCategory("Int64")]
    public long Min_Linq_Int64() => Values_Int64.AsEnumerable().Min();

    [Benchmark, BenchmarkCategory("Int64")]
    public long Min_Span_Int64() => Values_Int64.AsSpan().Min();


    [Benchmark(Baseline = true), BenchmarkCategory("Single")]
    public float Min_Loop_Single() => Min<float>(Values_Single);

    [Benchmark, BenchmarkCategory("Single")]
    public float Min_Linq_Single() => Values_Single.AsEnumerable().Min();

    [Benchmark, BenchmarkCategory("Single")]
    public float Min_Span_Single() => Values_Single.AsSpan().Min();

    [Benchmark, BenchmarkCategory("Single")]
    public float UnsafeMin_Span_Single() => Values_Single.AsSpan().UnsafeMin();


    [Benchmark(Baseline = true), BenchmarkCategory("Double")]
    public double Min_Loop_Double() => Min<double>(Values_Double);

    [Benchmark, BenchmarkCategory("Double")]
    public double Min_Linq_Double() => Values_Double.AsEnumerable().Min();

    [Benchmark, BenchmarkCategory("Double")]
    public double Min_Span_Double() => Values_Double.AsSpan().Min();

    [Benchmark, BenchmarkCategory("Double")]
    public double UnsafeMin_Span_Double() => Values_Double.AsSpan().UnsafeMin();


    [Benchmark(Baseline = true), BenchmarkCategory("Decimal")]
    public decimal Min_Loop_Decimal() => Min<decimal>(Values_Decimal);

    [Benchmark, BenchmarkCategory("Decimal")]
    public decimal Min_Linq_Decimal() => Values_Decimal.AsEnumerable().Min();

    [Benchmark, BenchmarkCategory("Decimal")]
    public decimal Min_Span_Decimal() => Values_Decimal.AsSpan().Min();


    [Benchmark(Baseline = true), BenchmarkCategory("String")]
    public string Min_Loop_String() => Min<string>(Values_String);

    [Benchmark, BenchmarkCategory("String")]
    public string? Min_Linq_String() => Values_String.AsEnumerable().Min();

    [Benchmark, BenchmarkCategory("String")]
    public string? Min_Span_String() => Values_String.AsSpan().Min();


    private static T Min<T>(ReadOnlySpan<T> values)
    {
        if (values.IsEmpty)
            throw new InvalidOperationException();

        T min = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (Comparer<T>.Default.Compare(values[i], min) < 0)
                min = values[i];
        }

        return min;
    }
}
