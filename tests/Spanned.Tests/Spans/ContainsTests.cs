using Spanned.Tests.TestUtilities;

namespace Spanned.Tests.Spans;

public class ContainsTests
{
    public static IEnumerable<object?[]> Contains_Generic_TestData()
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
    [MemberData(nameof(Contains_Generic_TestData))]
    public void Contains_Generic<T>(T[] sourceArray, T missingValue)
    {
        Span<T> source = sourceArray;
        for (int i = 0; i < source.Length; i++)
        {
            Assert.True(source.Contains(source[i], comparer: null));
            Assert.True(source.Contains(source[i], EqualityComparer<T>.Default));
        }
        Assert.False(source.Contains(missingValue, comparer: null));
        Assert.False(source.Contains(missingValue, EqualityComparer<T>.Default));

        // ---------------------------------

        ReadOnlySpan<T> readOnlySource = sourceArray;
        for (int i = 0; i < readOnlySource.Length; i++)
        {
            Assert.True(readOnlySource.Contains(readOnlySource[i], comparer: null));
            Assert.True(readOnlySource.Contains(readOnlySource[i], EqualityComparer<T>.Default));
        }
        Assert.False(readOnlySource.Contains(missingValue, comparer: null));
        Assert.False(readOnlySource.Contains(missingValue, EqualityComparer<T>.Default));
    }

    public static IEnumerable<object?[]> Contains_Generic_CustomComparer_TestData()
    {
        yield return new object?[] { new string[] { "Aardvark", "Zyzzyva", "Zebra", "Antelope" }, "zebra", null, false };
        yield return new object?[] { new string[] { "Aardvark", "Zyzzyva", "Zebra", "Antelope" }, "zebra", StringComparer.InvariantCultureIgnoreCase, true };
    }

    [Theory]
    [MemberData(nameof(Contains_Generic_CustomComparer_TestData))]
    public void Contains_Generic_CustomComparer<T>(T[] sourceArray, T value, IEqualityComparer<T>? comparer, bool expectedOutput)
    {
        Span<T> source = sourceArray;
        Assert.Equal(expectedOutput, source.Contains(value, comparer));

        // ---------------------------------

        ReadOnlySpan<T> readOnlySource = sourceArray;
        Assert.Equal(expectedOutput, readOnlySource.Contains(value, comparer));
    }

    public static IEnumerable<object[]> Contains_Generic_EmptySource_ReturnsFalse_TestData()
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
    [MemberData(nameof(Contains_Generic_EmptySource_ReturnsFalse_TestData))]
    public void Contains_Generic_EmptySource_ReturnsFalse<T>(T[] value)
    {
        Span<T> source = Span<T>.Empty;
        Assert.False(source.Contains(value[0], comparer: null));
        Assert.False(source.Contains(value[0], EqualityComparer<T?>.Default));

        // ---------------------------------

        ReadOnlySpan<T> readOnlySource = ReadOnlySpan<T>.Empty;
        Assert.False(readOnlySource.Contains(value[0], comparer: null));
        Assert.False(readOnlySource.Contains(value[0], EqualityComparer<T>.Default));
    }
}
