using System.Numerics;

namespace Spanned.Benchmarks.Spans;

public abstract class SpansTest
{
    private static readonly int[] s_sizes = new[] { 0, 2, 4, 8, 16, 32, 64, 128, 512, 4096 };

    public static IEnumerable<object[]> NonNegativeSize => s_sizes.Select(x => new object[] { x });

    public static IEnumerable<object[]> PositiveSize => s_sizes.Skip(1).Select(x => new object[] { x });

    protected static T[] CreateArray<T>(int size, T filler)
    {
        T[] array = new T[size];
        Array.Fill(array, filler);
        return array;
    }

    protected static string[] CreateRandomArray(int size)
    {
        string[] array = new string[size];
        Random random = new(42);
        Span<byte> charBuffer = stackalloc byte[512];

        for (int i = 0; i < array.Length; i++)
        {
            int bufferLength = random.Next() % 10 + 5;
            Span<byte> buffer = charBuffer.Slice(0, bufferLength);

            random.NextBytes(buffer);
            string str = Convert.ToBase64String(buffer);

            array[i] = str;
        }

        return array;
    }

    protected static T[] CreateRandomArray<T>(int size) where T : INumber<T>, IMinMaxValue<T>
        => CreateRandomArray(size, T.MinValue, T.MaxValue);

    protected static T[] CreateRandomArray<T>(int size, T min, T max) where T : INumber<T>
    {
        Random random = new(42);
        IEnumerable<int> range = Enumerable.Range(0, size);

        double minDouble = double.CreateTruncating(min);
        double multiplier = double.CreateTruncating(max) - minDouble;
        return range.Select(_ => T.Clamp(T.CreateTruncating(minDouble + random.NextDouble() * multiplier), min, max)).ToArray();
    }
}
