using Spanned.Tests.TestUtilities;

namespace Spanned.Tests.Spans;

public class IndexOfTests
{
    public static IEnumerable<object?[]> IndexOf_Generic_TestData()
    {
        for (int length = 2; length < 65; length++)
        {
            yield return new object?[] { Shuffler.Range((byte)length, length), (byte)(length * 2) };
            yield return new object?[] { (byte[])[.. Shuffler.Range((byte)length, length), .. Shuffler.Range((byte)length, length)], (byte)(length * 2) };

            // We should generate data up to one value below `sbyte.MaxValue`, otherwise the type overflows.
            if (length + length < sbyte.MaxValue)
            {
                yield return new object?[] { Shuffler.Range((sbyte)length, length), (sbyte)(length * 2) };
                yield return new object?[] { (sbyte[])[.. Shuffler.Range((sbyte)length, length), .. Shuffler.Range((sbyte)length, length)], (sbyte)(length * 2) };
            }

            yield return new object?[] { Shuffler.Range((short)length, length), (short)(length * 2) };
            yield return new object?[] { (short[])[.. Shuffler.Range((short)length, length), .. Shuffler.Range((short)length, length)], (short)(length * 2) };

            yield return new object?[] { Shuffler.Range((ushort)length, length), (ushort)(length * 2) };
            yield return new object?[] { (ushort[])[.. Shuffler.Range((ushort)length, length), .. Shuffler.Range((ushort)length, length)], (ushort)(length * 2) };

            yield return new object?[] { Shuffler.Range(length, length), length * 2 };
            yield return new object?[] { (int[])[.. Shuffler.Range(length, length), .. Shuffler.Range(length, length)], length * 2 };

            yield return new object?[] { Shuffler.Range((uint)length, length), (uint)(length * 2) };
            yield return new object?[] { (uint[])[.. Shuffler.Range((uint)length, length), .. Shuffler.Range((uint)length, length)], (uint)(length * 2) };

            yield return new object?[] { Shuffler.Range((long)length, length), (long)(length * 2) };
            yield return new object?[] { (long[])[.. Shuffler.Range((long)length, length), .. Shuffler.Range((long)length, length)], (long)(length * 2) };

            yield return new object?[] { Shuffler.Range((ulong)length, length), (ulong)(length * 2) };
            yield return new object?[] { (ulong[])[.. Shuffler.Range((ulong)length, length), .. Shuffler.Range((ulong)length, length)], (ulong)(length * 2) };

            yield return new object?[] { Shuffler.Range((nint)length, length), (nint)(length * 2) };
            yield return new object?[] { (nint[])[.. Shuffler.Range((nint)length, length), .. Shuffler.Range((nint)length, length)], (nint)(length * 2) };

            yield return new object?[] { Shuffler.Range((nuint)length, length), (nuint)(length * 2) };
            yield return new object?[] { (nuint[])[.. Shuffler.Range((nuint)length, length), .. Shuffler.Range((nuint)length, length)], (nuint)(length * 2) };

            yield return new object?[] { Shuffler.Range((float)length, length), (float)(length * 2) };
            yield return new object?[] { (float[])[.. Shuffler.Range((float)length, length), .. Shuffler.Range((float)length, length)], (float)(length * 2) };

            yield return new object?[] { Shuffler.Range((double)length, length), (double)(length * 2) };
            yield return new object?[] { (double[])[.. Shuffler.Range((double)length, length), .. Shuffler.Range((double)length, length)], (double)(length * 2) };

            yield return new object?[] { Shuffler.Range((decimal)length, length), (decimal)(length * 2) };
            yield return new object?[] { (decimal[])[.. Shuffler.Range((decimal)length, length), .. Shuffler.Range((decimal)length, length)], (decimal)(length * 2) };

            yield return new object?[] { Shuffler.Range((Half)length, length), (Half)(length * 2) };
            yield return new object?[] { (Half[])[.. Shuffler.Range((Half)length, length), .. Shuffler.Range((Half)length, length)], (Half)(length * 2) };

            yield return new object?[] { Shuffler.Range((char)length, length), (char)(length * 2) };
            yield return new object?[] { (char[])[.. Shuffler.Range((char)length, length), .. Shuffler.Range((char)length, length)], (char)(length * 2) };

            yield return new object?[] { Shuffler.Range(char.ToString((char)length), length), null };
            yield return new object?[] { (string?[])[.. Shuffler.Range(char.ToString((char)length), length), null], char.ToString((char)(length * 2)) };
            yield return new object?[] { (string?[])[.. Shuffler.Range(char.ToString((char)length), length), .. Shuffler.Range(char.ToString((char)length), length)], char.ToString((char)(length * 2)) };
            yield return new object?[] { (string?[])[.. Shuffler.Range(char.ToString((char)length), length), null, .. Shuffler.Range(char.ToString((char)length), length), null], char.ToString((char)(length * 2)) };
        }
    }

    [Theory]
    [MemberData(nameof(IndexOf_Generic_TestData))]
    public void IndexOf_Generic<T>(T[] sourceArray, T missingValue)
    {
        Span<T> source = sourceArray;
        for (int i = 0; i < source.Length; i++)
        {
            T searchValue = source[i];

            int foundValueIndex = source.IndexOf(searchValue, comparer: null);
            Assert.True(foundValueIndex >= 0 && foundValueIndex < source.Length);
            Assert.StrictEqual(searchValue, source[foundValueIndex]);

            foundValueIndex = source.IndexOf(searchValue, comparer: EqualityComparer<T>.Default);
            Assert.True(foundValueIndex >= 0 && foundValueIndex < source.Length);
            Assert.StrictEqual(searchValue, source[foundValueIndex]);
        }
        Assert.StrictEqual(-1, source.IndexOf(missingValue, comparer: null));
        Assert.StrictEqual(-1, source.IndexOf(missingValue, EqualityComparer<T>.Default));

        // ---------------------------------

        ReadOnlySpan<T> readOnlySource = sourceArray;
        for (int i = 0; i < readOnlySource.Length; i++)
        {
            T searchValue = readOnlySource[i];

            int foundValueIndex = readOnlySource.IndexOf(searchValue, comparer: null);
            Assert.True(foundValueIndex >= 0 && foundValueIndex < readOnlySource.Length);
            Assert.StrictEqual(searchValue, readOnlySource[foundValueIndex]);

            foundValueIndex = readOnlySource.IndexOf(searchValue, comparer: EqualityComparer<T>.Default);
            Assert.True(foundValueIndex >= 0 && foundValueIndex < readOnlySource.Length);
            Assert.StrictEqual(searchValue, readOnlySource[foundValueIndex]);
        }
        Assert.StrictEqual(-1, readOnlySource.IndexOf(missingValue, comparer: null));
        Assert.StrictEqual(-1, readOnlySource.IndexOf(missingValue, EqualityComparer<T>.Default));
    }

    public static IEnumerable<object?[]> IndexOf_Generic_CustomComparer_TestData()
    {
        yield return new object?[] { new string[] { "Aardvark", "Zyzzyva", "Zebra", "Antelope" }, "zebra", null, -1 };
        yield return new object?[] { new string[] { "Aardvark", "Zyzzyva", "Zebra", "Antelope", "zebra" }, "zebra", null, 4 };

        yield return new object?[] { new string[] { "Aardvark", "Zyzzyva", "Zebra", "Antelope" }, "anteater", StringComparer.InvariantCultureIgnoreCase, -1 };
        yield return new object?[] { new string[] { "Aardvark", "Zyzzyva", "Zebra", "Antelope" }, "zebra", StringComparer.InvariantCultureIgnoreCase, 2 };
        yield return new object?[] { new string[] { "Aardvark", "Zyzzyva", "Zebra", "Antelope", "zebra" }, "zebra", StringComparer.InvariantCultureIgnoreCase, 2 };
        yield return new object?[] { new string[] { "Aardvark", "Zyzzyva", "zebra", "Antelope", "Zebra" }, "Zebra", StringComparer.InvariantCultureIgnoreCase, 2 };
    }

    [Theory]
    [MemberData(nameof(IndexOf_Generic_CustomComparer_TestData))]
    public void IndexOf_Generic_CustomComparer<T>(T[] sourceArray, T value, IEqualityComparer<T>? comparer, int expectedOutput)
    {
        Span<T> source = sourceArray;
        Assert.Equal(expectedOutput, source.IndexOf(value, comparer));

        // ---------------------------------

        ReadOnlySpan<T> readOnlySource = sourceArray;
        Assert.Equal(expectedOutput, readOnlySource.IndexOf(value, comparer));
    }

    public static IEnumerable<object[]> IndexOf_Generic_EmptySource_ReturnsMinusOne_TestData()
    {
        yield return new object[] { new[] { default(byte) } };
        yield return new object[] { new[] { default(sbyte) } };
        yield return new object[] { new[] { default(short) } };
        yield return new object[] { new[] { default(ushort) } };
        yield return new object[] { new[] { default(int) } };
        yield return new object[] { new[] { default(uint) } };
        yield return new object[] { new[] { default(long) } };
        yield return new object[] { new[] { default(ulong) } };
        yield return new object[] { new[] { default(nint) } };
        yield return new object[] { new[] { default(nuint) } };
        yield return new object[] { new[] { default(float) } };
        yield return new object[] { new[] { default(double) } };
        yield return new object[] { new[] { default(decimal) } };
        yield return new object[] { new[] { default(Half) } };
        yield return new object[] { new[] { default(char) } };
        yield return new object[] { new[] { default(string) } };
    }

    [Theory]
    [MemberData(nameof(IndexOf_Generic_EmptySource_ReturnsMinusOne_TestData))]
    public void IndexOf_Generic_EmptySource_ReturnsMinusOne<T>(T[] value)
    {
        Span<T> source = Span<T>.Empty;
        Assert.StrictEqual(-1, source.IndexOf(value[0], comparer: null));
        Assert.StrictEqual(-1, source.IndexOf(value[0], EqualityComparer<T?>.Default));

        // ---------------------------------

        ReadOnlySpan<T> readOnlySource = ReadOnlySpan<T>.Empty;
        Assert.StrictEqual(-1, readOnlySource.IndexOf(value[0], comparer: null));
        Assert.StrictEqual(-1, readOnlySource.IndexOf(value[0], EqualityComparer<T>.Default));
    }
}
