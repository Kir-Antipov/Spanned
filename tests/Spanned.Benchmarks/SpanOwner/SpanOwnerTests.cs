using System.Buffers;

namespace Spanned.Benchmarks.SpanOwner;

public class SpanOwnerTests
{
    [Params(10)]
    public int N { get; set; }

    [Benchmark(Baseline = true), BenchmarkCategory("Int32")]
    public void Without_SpanOwner_Int32() => WithoutSpanOwner<int>(N);

    [Benchmark, BenchmarkCategory("Int32")]
    public void With_SpanOwner_Int32() => WithSpanOwner<int>(N);

    [Benchmark(Baseline = true), BenchmarkCategory("Double")]
    public void Without_SpanOwner_Double() => WithoutSpanOwner<double>(N);

    [Benchmark, BenchmarkCategory("Double")]
    public void With_SpanOwner_Double() => WithSpanOwner<double>(N);

    private static void WithSpanOwner<T>(int size) where T : unmanaged
    {
        SpanOwner<T> owner = SpanOwner<T>.ShouldRent(size) ? SpanOwner<T>.Rent(size) : stackalloc T[size];
        Span<T> span = owner.Span;

        DoSomeWorkWithSpan(span);

        owner.Dispose();
    }

    private static void WithoutSpanOwner<T>(int size) where T : unmanaged
    {
        const int StackAllocByteLimit = 1024;

        T[]? spanSource;
        scoped Span<T> span;

        if (Unsafe.SizeOf<T>() * size > StackAllocByteLimit)
        {
            spanSource = ArrayPool<T>.Shared.Rent(size);
            span = spanSource.AsSpan(0, size);
        }
        else
        {
            spanSource = null;
            span = stackalloc T[size];
        }

        DoSomeWorkWithSpan(span);

        if (spanSource is not null)
        {
            ArrayPool<T>.Shared.Return(spanSource);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DoSomeWorkWithSpan<T>(Span<T> _) { }
}
