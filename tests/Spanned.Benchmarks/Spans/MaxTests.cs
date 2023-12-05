namespace Spanned.Benchmarks.Spans;

public class MaxTests : SpansTest
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
    public byte Max_Loop_Byte() => Max<byte>(Values_Byte);

    [Benchmark, BenchmarkCategory("Byte")]
    public byte Max_Linq_Byte() => Values_Byte.AsEnumerable().Max();

    [Benchmark, BenchmarkCategory("Byte")]
    public byte Max_Span_Byte() => Values_Byte.AsSpan().Max();


    [Benchmark(Baseline = true), BenchmarkCategory("Int16")]
    public short Max_Loop_Int16() => Max<short>(Values_Int16);

    [Benchmark, BenchmarkCategory("Int16")]
    public short Max_Linq_Int16() => Values_Int16.AsEnumerable().Max();

    [Benchmark, BenchmarkCategory("Int16")]
    public short Max_Span_Int16() => Values_Int16.AsSpan().Max();


    [Benchmark(Baseline = true), BenchmarkCategory("Int32")]
    public int Max_Loop_Int32() => Max<int>(Values_Int32);

    [Benchmark, BenchmarkCategory("Int32")]
    public int Max_Linq_Int32() => Values_Int32.AsEnumerable().Max();

    [Benchmark, BenchmarkCategory("Int32")]
    public int Max_Span_Int32() => Values_Int32.AsSpan().Max();


    [Benchmark(Baseline = true), BenchmarkCategory("UInt32")]
    public uint Max_Loop_UInt32() => Max<uint>(Values_UInt32);

    [Benchmark, BenchmarkCategory("UInt32")]
    public uint Max_Linq_UInt32() => Values_UInt32.AsEnumerable().Max();

    [Benchmark, BenchmarkCategory("UInt32")]
    public uint Max_Span_UInt32() => Values_UInt32.AsSpan().Max();


    [Benchmark(Baseline = true), BenchmarkCategory("Int64")]
    public long Max_Loop_Int64() => Max<long>(Values_Int64);

    [Benchmark, BenchmarkCategory("Int64")]
    public long Max_Linq_Int64() => Values_Int64.AsEnumerable().Max();

    [Benchmark, BenchmarkCategory("Int64")]
    public long Max_Span_Int64() => Values_Int64.AsSpan().Max();


    [Benchmark(Baseline = true), BenchmarkCategory("Single")]
    public float Max_Loop_Single() => Max<float>(Values_Single);

    [Benchmark, BenchmarkCategory("Single")]
    public float Max_Linq_Single() => Values_Single.AsEnumerable().Max();

    [Benchmark, BenchmarkCategory("Single")]
    public float Max_Span_Single() => Values_Single.AsSpan().Max();

    [Benchmark, BenchmarkCategory("Single")]
    public float UnsafeMax_Span_Single() => Values_Single.AsSpan().UnsafeMax();


    [Benchmark(Baseline = true), BenchmarkCategory("Double")]
    public double Max_Loop_Double() => Max<double>(Values_Double);

    [Benchmark, BenchmarkCategory("Double")]
    public double Max_Linq_Double() => Values_Double.AsEnumerable().Max();

    [Benchmark, BenchmarkCategory("Double")]
    public double Max_Span_Double() => Values_Double.AsSpan().Max();

    [Benchmark, BenchmarkCategory("Double")]
    public double UnsafeMax_Span_Double() => Values_Double.AsSpan().UnsafeMax();


    [Benchmark(Baseline = true), BenchmarkCategory("Decimal")]
    public decimal Max_Loop_Decimal() => Max<decimal>(Values_Decimal);

    [Benchmark, BenchmarkCategory("Decimal")]
    public decimal Max_Linq_Decimal() => Values_Decimal.AsEnumerable().Max();

    [Benchmark, BenchmarkCategory("Decimal")]
    public decimal Max_Span_Decimal() => Values_Decimal.AsSpan().Max();


    [Benchmark(Baseline = true), BenchmarkCategory("String")]
    public string Max_Loop_String() => Max<string>(Values_String);

    [Benchmark, BenchmarkCategory("String")]
    public string? Max_Linq_String() => Values_String.AsEnumerable().Max();

    [Benchmark, BenchmarkCategory("String")]
    public string? Max_Span_String() => Values_String.AsSpan().Max();


    private static T Max<T>(ReadOnlySpan<T> values)
    {
        if (values.IsEmpty)
            throw new InvalidOperationException();

        T max = values[0];
        for (int i = 1; i < values.Length; i++)
        {
            if (Comparer<T>.Default.Compare(values[i], max) > 0)
                max = values[i];
        }

        return max;
    }
}
