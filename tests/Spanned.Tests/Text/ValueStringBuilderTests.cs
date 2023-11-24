using System.Globalization;
using System.Text;
using Spanned.Tests.TestUtilities;
using Spanned.Text;

namespace Spanned.Tests.Text;

public partial class ValueStringBuilderTests
{
    [Fact]
    public static void Ctor_Empty()
    {
        ValueStringBuilder builder = new();

        Assert.Same(string.Empty, builder.AsSpan().ToString());
        Assert.Equal(string.Empty, builder.AsSpan(0, 0).ToString());
        Assert.Equal(0, builder.Length);
    }

    [Fact]
    public static void Ctor_Int()
    {
        ValueStringBuilder builder = new(42);

        Assert.Same(string.Empty, builder.AsSpan().ToString());
        Assert.Equal(0, builder.Length);
        Assert.True(builder.Capacity >= 42);
    }

    [Fact]
    public static void Ctor_Int_NegativeCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder(-1)); // Capacity < 0
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData("")]
    [InlineData(null)]
    public static void Ctor_String(string value)
    {
        ValueStringBuilder builder = new(value);

        string expected = value ?? "";
        Assert.Equal(expected, builder.AsSpan().ToString());
        Assert.Equal(expected.Length, builder.Length);
    }

    [Theory]
    [InlineData("Hello")]
    [InlineData("")]
    [InlineData(null)]
    public static void Ctor_String_Int(string value)
    {
        ValueStringBuilder builder = new(value, 42);

        string expected = value ?? "";
        Assert.Equal(expected, builder.AsSpan().ToString());
        Assert.Equal(expected.Length, builder.Length);

        Assert.True(builder.Capacity >= 42);
    }

    [Fact]
    public static void Ctor_String_Int_NegativeCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("", -1)); // Capacity < 0
    }

    [Theory]
    [InlineData("Hello", 0, 5)]
    [InlineData("Hello", 2, 3)]
    [InlineData("", 0, 0)]
    [InlineData(null, 0, 0)]
    public static void Ctor_String_Int_Int_Int(string value, int startIndex, int length)
    {
        ValueStringBuilder builder = new(value, startIndex, length, 42);

        string expected = value?.Substring(startIndex, length) ?? "";
        Assert.Equal(expected, builder.AsSpan().ToString());
        Assert.Equal(length, builder.Length);
        Assert.Equal(expected.Length, builder.Length);

        Assert.True(builder.Capacity >= 42);
    }

    [Fact]
    public static void Ctor_String_Int_Int_Int_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("foo", -1, 0, 0)); // Start index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("foo", 0, -1, 0)); // Length < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("foo", 0, 0, -1)); // Capacity < 0

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("foo", 4, 0, 0)); // Start index + length > builder.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("foo", 3, 1, 0)); // Start index + length > builder.Length
    }

    [Fact]
    public static void Item_Get_Set()
    {
        string s = "Hello";
        ValueStringBuilder builder = new(s);

        for (int i = 0; i < s.Length; i++)
        {
            Assert.Equal(s[i], builder[i]);

            char c = (char)(i + '0');
            builder[i] = c;
            Assert.Equal(c, builder[i]);
        }
        Assert.Equal("01234", builder.AsSpan().ToString());
    }

    [Fact]
    public static void Item_Get_Set_InvalidIndex()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello")[-1]); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello")[5]); // Index >= string.Length

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello")[-1] = 'a'); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello")[5] = 'a'); // Index >= string.Length
    }

    [Fact]
    public static void Capacity_Get_Set()
    {
        ValueStringBuilder builder = new("Hello");
        Assert.True(builder.Capacity >= builder.Length);

        builder.Capacity = 10;
        Assert.True(builder.Capacity >= 10);

        builder.Capacity = 5;
        Assert.True(builder.Capacity >= 5);

        // Setting the capacity to the same value does not change anything
        int oldCapacity = builder.Capacity;
        builder.Capacity = 5;
        Assert.Equal(oldCapacity, builder.Capacity);
    }

    [Theory]
    [InlineData(-1)] // Capacity < 0
    [InlineData(4)] // "Hello".Length - 1
    public static void Capacity_Set_Invalid_ThrowsArgumentOutOfRangeException(int invalidCapacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ValueStringBuilder builder = new(10);
            builder.Append("Hello");

            builder.Capacity = invalidCapacity;
        });
    }

    [Fact]
    public static void Length_Get_Set()
    {
        ValueStringBuilder builder = new("Hello");

        builder.Length = 2;
        Assert.Equal(2, builder.Length);
        Assert.Equal("He", builder.AsSpan().ToString());

        builder.Length = 10;
        Assert.Equal(10, builder.Length);
        Assert.Equal("Hello" + new string((char)0, 5), builder.AsSpan().ToString());
    }

    [Theory]
    [InlineData(-1)] // Length < 0
    public static void Length_Set_InvalidValue_ThrowsArgumentOutOfRangeException(int invalidLength)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ValueStringBuilder builder = new(10);
            builder.Append("Hello");

            builder.Length = invalidLength;
        });
    }

    [Theory]
    [InlineData("Hello", (ushort)0, "Hello0")]
    [InlineData("Hello", (ushort)123, "Hello123")]
    [InlineData("", (ushort)456, "456")]
    public static void Append_UShort(string original, ushort value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(value);

        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", true, "HelloTrue")]
    [InlineData("Hello", false, "HelloFalse")]
    [InlineData("", false, "False")]
    public static void Append_Bool(string original, bool value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(value);
        Assert.Equal(expected, builder.ToString());
    }

    public static IEnumerable<object[]> Append_Decimal_TestData()
    {
        yield return new object[] { "Hello", 0m, "Hello0" };
        yield return new object[] { "Hello", 1.23m, "Hello1.23" };
        yield return new object[] { "", -4.56m, "-4.56" };
    }

    [Theory]
    [MemberData(nameof(Append_Decimal_TestData))]
    public static void Append_Decimal(string original, decimal value, string expected)
    {
        using ThreadCultureChange _ = new(CultureInfo.InvariantCulture);

        ValueStringBuilder builder = new(original);
        builder.Append((decimal)value);

        Assert.Equal(expected, builder.ToString());
    }

    public static IEnumerable<object[]> Append_Double_TestData()
    {
        yield return new object[] { "Hello", 0.0, "Hello0" };
        yield return new object[] { "Hello", 1.23, "Hello1.23" };
        yield return new object[] { "", -4.56, "-4.56" };
    }

    [Theory]
    [MemberData(nameof(Append_Double_TestData))]
    public static void Append_Double(string original, double value, string expected)
    {
        using ThreadCultureChange _ = new(CultureInfo.InvariantCulture);

        ValueStringBuilder builder = new(original);
        builder.Append(value);
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", (short)0, "Hello0")]
    [InlineData("Hello", (short)123, "Hello123")]
    [InlineData("", (short)-456, "-456")]
    public static void Append_Short(string original, short value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(value);
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", 0, "Hello0")]
    [InlineData("Hello", 123, "Hello123")]
    [InlineData("", -456, "-456")]
    public static void Append_Int(string original, int value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(value);
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", (long)0, "Hello0")]
    [InlineData("Hello", (long)123, "Hello123")]
    [InlineData("", (long)-456, "-456")]
    public static void Append_Long(string original, long value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(value);
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", "abc", "Helloabc")]
    [InlineData("Hello", "def", "Hellodef")]
    [InlineData("", "g", "g")]
    [InlineData("Hello", "", "Hello")]
    [InlineData("Hello", null, "Hello")]
    public static void Append_Object(string original, object value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(value);
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", (sbyte)0, "Hello0")]
    [InlineData("Hello", (sbyte)123, "Hello123")]
    [InlineData("", (sbyte)-123, "-123")]
    public static void Append_SByte(string original, sbyte value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(value);
        Assert.Equal(expected, builder.ToString());
    }

    public static IEnumerable<object[]> Append_Float_TestData()
    {
        yield return new object[] { "Hello", 0f, "Hello0" };
        yield return new object[] { "Hello", 1.23f, "Hello1.23" };
        yield return new object[] { "", -4.56f, "-4.56" };
    }

    [Theory]
    [MemberData(nameof(Append_Float_TestData))]
    public static void Append_Float(string original, float value, string expected)
    {
        using ThreadCultureChange _ = new(CultureInfo.InvariantCulture);

        ValueStringBuilder builder = new(original);
        builder.Append(value);
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", (byte)0, "Hello0")]
    [InlineData("Hello", (byte)123, "Hello123")]
    [InlineData("", (byte)123, "123")]
    public static void Append_Byte(string original, byte value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(value);
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", (uint)0, "Hello0")]
    [InlineData("Hello", (uint)123, "Hello123")]
    [InlineData("", (uint)456, "456")]
    public static void Append_UInt(string original, uint value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(value);
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", (ulong)0, "Hello0")]
    [InlineData("Hello", (ulong)123, "Hello123")]
    [InlineData("", (ulong)456, "456")]
    public static void Append_ULong(string original, ulong value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(value);
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", '\0', 1, "Hello\0")]
    [InlineData("Hello", 'a', 1, "Helloa")]
    [InlineData("", 'b', 1, "b")]
    [InlineData("Hello", 'c', 2, "Hellocc")]
    [InlineData("Hello", '\0', 0, "Hello")]
    public static void Append_Char(string original, char value, int repeatCount, string expected)
    {
        scoped ValueStringBuilder builder;
        if (repeatCount == 1)
        {
            // Use Append(char)
            builder = new ValueStringBuilder(original);
            builder.Append(value);
            Assert.Equal(expected, builder.AsSpan().ToString());
        }
        // Use Append(char, int)
        builder = new ValueStringBuilder(original);
        builder.Append(value, repeatCount);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public static void Append_Char_NegativeRepeatCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder().Append('a', -1));
    }

    [Theory]
    [InlineData("Hello", new char[] { 'a', 'b', 'c' }, 1, "Helloa")]
    [InlineData("Hello", new char[] { 'a', 'b', 'c' }, 2, "Helloab")]
    [InlineData("Hello", new char[] { 'a', 'b', 'c' }, 3, "Helloabc")]
    [InlineData("", new char[] { 'a' }, 1, "a")]
    [InlineData("", new char[] { 'a' }, 0, "")]
    [InlineData("Hello", new char[0], 0, "Hello")]
    [InlineData("Hello", null, 0, "Hello")]
    public static unsafe void Append_CharPointer(string original, char[] charArray, int valueCount, string expected)
    {
        _ = charArray; // https://github.com/xunit/xunit/issues/1969
        fixed (char* value = charArray)
        {
            ValueStringBuilder builder = new(original);
            builder.Append(value, valueCount);
            Assert.Equal(expected, builder.ToString());
        }
    }

    [Fact]
    public static unsafe void Append_CharPointer_Null_ThrowsNullReferenceException()
    {
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder().Append(null, 2));
    }

    [Fact]
    public static unsafe void Append_CharPointer_NegativeValueCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ValueStringBuilder builder = new();
            builder.Append("Hello");

            fixed (char* value = new char[0])
            {
                builder.Append(value, -1);
            }
        });
    }

    [Theory]
    [InlineData("Hello", "abc", 0, 3, "Helloabc")]
    [InlineData("Hello", "def", 1, 2, "Helloef")]
    [InlineData("Hello", "def", 2, 1, "Hellof")]
    [InlineData("", "g", 0, 1, "g")]
    [InlineData("Hello", "g", 1, 0, "Hello")]
    [InlineData("Hello", "g", 0, 0, "Hello")]
    [InlineData("Hello", "", 0, 0, "Hello")]
    [InlineData("Hello", null, 0, 0, "Hello")]
    public static void Append_String(string original, string value, int startIndex, int count, string expected)
    {
        scoped ValueStringBuilder builder;
        if (startIndex == 0 && count == (value?.Length ?? 0))
        {
            // Use Append(string)
            builder = new ValueStringBuilder(original);
            builder.Append(value);
            Assert.Equal(expected, builder.AsSpan().ToString());
        }
        // Use Append(string, int, int)
        builder = new ValueStringBuilder(original);
        builder.Append(value, startIndex, count);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public static void Append_String_NullValueNonZeroStartIndexCount_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder().Append((string?)null, 1, 1));
    }

    [Theory]
    [InlineData("", -1, 0)]
    [InlineData("hello", 5, 1)]
    [InlineData("hello", 4, 2)]
    public static void Append_String_InvalidIndexPlusCount_ThrowsArgumentOutOfRangeException(string value, int startIndex, int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder().Append(value, startIndex, count));
    }

    [Fact]
    public static void Append_String_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder().Append("", 0, -1));
    }

    [Theory]
    [InlineData("Hello", new char[] { 'a' }, 0, 1, "Helloa")]
    [InlineData("Hello", new char[] { 'b', 'c', 'd' }, 0, 3, "Hellobcd")]
    [InlineData("Hello", new char[] { 'b', 'c', 'd' }, 1, 2, "Hellocd")]
    [InlineData("Hello", new char[] { 'b', 'c', 'd' }, 2, 1, "Hellod")]
    [InlineData("", new char[] { 'e', 'f', 'g' }, 0, 3, "efg")]
    [InlineData("Hello", new char[] { 'e' }, 1, 0, "Hello")]
    [InlineData("Hello", new char[] { 'e' }, 0, 0, "Hello")]
    [InlineData("Hello", new char[0], 0, 0, "Hello")]
    [InlineData("Hello", null, 0, 0, "Hello")]
    public static void Append_CharArray(string original, char[] value, int startIndex, int charCount, string expected)
    {
        scoped ValueStringBuilder builder;
        if (startIndex == 0 && charCount == (value?.Length ?? 0))
        {
            // Use Append(char[])
            builder = new ValueStringBuilder(original);
            builder.Append(value);
            Assert.Equal(expected, builder.AsSpan().ToString());
        }
        // Use Append(char[], int, int)
        builder = new ValueStringBuilder(original);
        builder.Append(value, startIndex, charCount);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public static void Append_CharArray_Invalid()
    {
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).Append((char[]?)null, 1, 1)); // Value is null, startIndex > 0 and count > 0

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello", 10).Append(new char[0], -1, 0)); // Start index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello", 10).Append(new char[0], 0, -1)); // Count < 0

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello", 10).Append(new char[5], 6, 0)); // Start index + count > value.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello", 10).Append(new char[5], 5, 1)); // Start index + count > value.Length
    }

    public static IEnumerable<object?[]> AppendFormat_TestData()
    {
        yield return new object?[] { "", null, "", new object[0], "" };
        yield return new object?[] { "", null, ", ", new object[0], ", " };

        yield return new object?[] { "Hello", null, ", Foo {0  }", new object[] { "Bar" }, "Hello, Foo Bar" }; // Ignores whitespace

        yield return new object?[] { "Hello", null, ", Foo {0}", new object[] { "Bar" }, "Hello, Foo Bar" };
        yield return new object?[] { "Hello", null, ", Foo {0} Baz {1}", new object[] { "Bar", "Foo" }, "Hello, Foo Bar Baz Foo" };
        yield return new object?[] { "Hello", null, ", Foo {0} Baz {1} Bar {2}", new object[] { "Bar", "Foo", "Baz" }, "Hello, Foo Bar Baz Foo Bar Baz" };
        yield return new object?[] { "Hello", null, ", Foo {0} Baz {1} Bar {2} Foo {3}", new object[] { "Bar", "Foo", "Baz", "Bar" }, "Hello, Foo Bar Baz Foo Bar Baz Foo Bar" };

        // Length is positive
        yield return new object?[] { "Hello", null, ", Foo {0,2}", new object[] { "Bar" }, "Hello, Foo Bar" }; // MiValue's length > minimum length (so don't prepend whitespace)
        yield return new object?[] { "Hello", null, ", Foo {0,3}", new object[] { "B" }, "Hello, Foo   B" }; // Value's length < minimum length (so prepend whitespace)
        yield return new object?[] { "Hello", null, ", Foo {0,     3}", new object[] { "B" }, "Hello, Foo   B" }; // Same as above, but verify AppendFormat ignores whitespace
        yield return new object?[] { "Hello", null, ", Foo {0,0}", new object[] { "Bar" }, "Hello, Foo Bar" }; // Minimum length == 0
        yield return new object?[] { "Hello", null, ", Foo {0,  2 }", new object[] { "Bar" }, "Hello, Foo Bar" }; // whitespace before and after length

        // Length is negative
        yield return new object?[] { "Hello", null, ", Foo {0,-2}", new object[] { "Bar" }, "Hello, Foo Bar" }; // Value's length > |minimum length| (so don't prepend whitespace)
        yield return new object?[] { "Hello", null, ", Foo {0,-3}", new object[] { "B" }, "Hello, Foo B  " }; // Value's length < |minimum length| (so append whitespace)
        yield return new object?[] { "Hello", null, ", Foo {0,     -3}", new object[] { "B" }, "Hello, Foo B  " }; // Same as above, but verify AppendFormat ignores whitespace
        yield return new object?[] { "Hello", null, ", Foo {0,0}", new object[] { "Bar" }, "Hello, Foo Bar" }; // Minimum length == 0
        yield return new object?[] { "Hello", null, ", Foo {0, -2  }", new object[] { "Bar" }, "Hello, Foo Bar" }; // whitespace before and after length

        yield return new object?[] { "Hello", null, ", Foo {0:D6}", new object[] { 1 }, "Hello, Foo 000001" }; // Custom format
        yield return new object?[] { "Hello", null, ", Foo {0     :D6}", new object[] { 1 }, "Hello, Foo 000001" }; // Custom format with ignored whitespace
        yield return new object?[] { "Hello", null, ", Foo {0:}", new object[] { 1 }, "Hello, Foo 1" }; // Missing custom format

        yield return new object?[] { "Hello", null, ", Foo {0,9:D6}", new object[] { 1 }, "Hello, Foo    000001" }; // Positive minimum length and custom format
        yield return new object?[] { "Hello", null, ", Foo {0,-9:D6}", new object[] { 1 }, "Hello, Foo 000001   " }; // Negative length and custom format

        yield return new object?[] { "Hello", null, ", Foo {{{0}", new object[] { 1 }, "Hello, Foo {1" }; // Escaped open curly braces
        yield return new object?[] { "Hello", null, ", Foo }}{0}", new object[] { 1 }, "Hello, Foo }1" }; // Escaped closed curly braces
        yield return new object?[] { "Hello", null, ", Foo {0} {{0}}", new object[] { 1 }, "Hello, Foo 1 {0}" }; // Escaped placeholder


        yield return new object?[] { "Hello", null, ", Foo {0}", new object?[] { null }, "Hello, Foo " }; // Values has null only
        yield return new object?[] { "Hello", null, ", Foo {0} {1} {2}", new object?[] { "Bar", null, "Baz" }, "Hello, Foo Bar  Baz" }; // Values has null

        yield return new object?[] { "Hello", CultureInfo.InvariantCulture, ", Foo {0,9:D6}", new object[] { 1 }, "Hello, Foo    000001" }; // Positive minimum length, custom format and custom format provider

        yield return new object?[] { "", new CustomFormatter(), "{0}", new object[] { 1.2 }, "abc" }; // Custom format provider
        yield return new object?[] { "", new CustomFormatter(), "{0:0}", new object[] { 1.2 }, "abc" }; // Custom format provider

        // ISpanFormattable inputs: simple validation of known types that implement the interface
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (byte)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { 'A' }, "A" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0:r}", new object[] { DateTime.ParseExact("2021-03-15T14:52:51.5058563Z", "o", null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal) }, "Mon, 15 Mar 2021 14:52:51 GMT" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0:r}", new object[] { DateTimeOffset.ParseExact("2021-03-15T14:52:51.5058563Z", "o", null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal) }, "Mon, 15 Mar 2021 14:52:51 GMT" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (decimal)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (double)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { Guid.Parse("68d9cfaf-feab-4d5b-96d8-a3fd889ae89f") }, "68d9cfaf-feab-4d5b-96d8-a3fd889ae89f" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (Half)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (short)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { 42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (long)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (IntPtr)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { new Rune('A') }, "A" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (sbyte)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (float)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { TimeSpan.FromSeconds(42) }, "00:00:42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (ushort)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (uint)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (ulong)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { (UIntPtr)42 }, "42" };
        yield return new object?[] { "", CultureInfo.InvariantCulture, "{0}", new object[] { new Version(1, 2, 3, 4) }, "1.2.3.4" };
    }

    [Theory]
    [MemberData(nameof(AppendFormat_TestData))]
    public static void AppendFormat(string original, IFormatProvider provider, string format, object[] values, string expected)
    {
        scoped ValueStringBuilder builder;
        if (values != null)
        {
            if (values.Length == 1)
            {
                // Use AppendFormat(string, object) or AppendFormat(IFormatProvider, string, object)
                if (provider == null)
                {
                    // Use AppendFormat(string, object)
                    builder = new ValueStringBuilder(original);
                    builder.AppendFormat(format, values[0]);
                    Assert.Equal(expected, builder.AsSpan().ToString());
                }
                // Use AppendFormat(IFormatProvider, string, object)
                builder = new ValueStringBuilder(original);
                builder.AppendFormat(provider, format, values[0]);
                Assert.Equal(expected, builder.AsSpan().ToString());
            }
            else if (values.Length == 2)
            {
                // Use AppendFormat(string, object, object) or AppendFormat(IFormatProvider, string, object, object)
                if (provider == null)
                {
                    // Use AppendFormat(string, object, object)
                    builder = new ValueStringBuilder(original);
                    builder.AppendFormat(format, values[0], values[1]);
                    Assert.Equal(expected, builder.AsSpan().ToString());
                }
                // Use AppendFormat(IFormatProvider, string, object, object)
                builder = new ValueStringBuilder(original);
                builder.AppendFormat(provider, format, values[0], values[1]);
                Assert.Equal(expected, builder.AsSpan().ToString());
            }
            else if (values.Length == 3)
            {
                // Use AppendFormat(string, object, object, object) or AppendFormat(IFormatProvider, string, object, object, object)
                if (provider == null)
                {
                    // Use AppendFormat(string, object, object, object)
                    builder = new ValueStringBuilder(original);
                    builder.AppendFormat(format, values[0], values[1], values[2]);
                    Assert.Equal(expected, builder.AsSpan().ToString());
                }
                // Use AppendFormat(IFormatProvider, string, object, object, object)
                builder = new ValueStringBuilder(original);
                builder.AppendFormat(provider, format, values[0], values[1], values[2]);
                Assert.Equal(expected, builder.AsSpan().ToString());
            }
        }
        // Use AppendFormat(string, object[]) or AppendFormat(IFormatProvider, string, object[])
        if (provider == null)
        {
            // Use AppendFormat(string, object[])
            builder = new ValueStringBuilder(original);
            builder.AppendFormat(format, values!);
            Assert.Equal(expected, builder.AsSpan().ToString());
        }
        // Use AppendFormat(IFormatProvider, string, object[])
        builder = new ValueStringBuilder(original);
        builder.AppendFormat(provider, format, values!);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public static void AppendFormat_Invalid()
    {
        IFormatProvider? formatter = null;
        object obj1 = new();
        object obj2 = new();
        object obj3 = new();
        object obj4 = new();

        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(null!, obj1)); // Format is null
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(null!, obj1, obj2, obj3)); // Format is null
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(null!, obj1, obj2, obj3)); // Format is null
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(null!, obj1, obj2, obj3, obj4)); // Format is null
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("", null!)); // Args is null
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(null!, (object?[])null!)); // Both format and args are null
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, (string)null!, obj1)); // Format is null
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, (string)null!, obj1, obj2)); // Format is null
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, (string)null!, obj1, obj2, obj3)); // Format is null
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, (string)null!, obj1, obj2, obj3, obj4)); // Format is null
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, "", null!)); // Args is null
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, (string)null!, null!)); // Both format and args are null

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{-1}", obj1)); // Format has value < 0
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{-1}", obj1, obj2)); // Format has value < 0
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{-1}", obj1, obj2, obj3)); // Format has value < 0
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{-1}", obj1, obj2, obj3, obj4)); // Format has value < 0
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, "{-1}", obj1)); // Format has value < 0
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, "{-1}", obj1, obj2)); // Format has value < 0
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, "{-1}", obj1, obj2, obj3)); // Format has value < 0
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, "{-1}", obj1, obj2, obj3, obj4)); // Format has value < 0

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{1}", obj1)); // Format has value >= 1
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{2}", obj1, obj2)); // Format has value >= 2
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{3}", obj1, obj2, obj3)); // Format has value >= 3
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{4}", obj1, obj2, obj3, obj4)); // Format has value >= 4
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, "{1}", obj1)); // Format has value >= 1
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, "{2}", obj1, obj2)); // Format has value >= 2
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, "{3}", obj1, obj2, obj3)); // Format has value >= 3
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat(formatter, "{4}", obj1, obj2, obj3, obj4)); // Format has value >= 4

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{", "")); // Format has unescaped {
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{a", "")); // Format has unescaped {

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("}", "")); // Format has unescaped }
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("}a", "")); // Format has unescaped }
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0:}}", "")); // Format has unescaped }

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{\0", "")); // Format has invalid character after {
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{a", "")); // Format has invalid character after {

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0     ", "")); // Format with index and spaces is not closed

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{1000000", new string[10])); // Format index is too long
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{10000000}", new string[10])); // Format index is too long

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0,", "")); // Format with comma is not closed
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0,   ", "")); // Format with comma and spaces is not closed
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0,-", "")); // Format with comma and minus sign is not closed

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0,-\0", "")); // Format has invalid character after minus sign
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0,-a", "")); // Format has invalid character after minus sign

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0,1000000", new string[10])); // Format length is too long
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0,10000000}", new string[10])); // Format length is too long

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0:", new string[10])); // Format with colon is not closed
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0:    ", new string[10])); // Format with colon and spaces is not closed

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0:{", new string[10])); // Format with custom format contains unescaped {
        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0:{}", new string[10])); // Format with custom format contains unescaped {

        Assert.Throws<FormatException>(() => new ValueStringBuilder("Hello", 10).AppendFormat("{0}", new TooManyCharsWrittenSpanFormattable())); // ISpanFormattable that returns more characters than it actually wrote
    }

    private struct TooManyCharsWrittenSpanFormattable : ISpanFormattable
    {
        public readonly string ToString(string? format, IFormatProvider? formatProvider) => "abc";

        public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            "abc".TryCopyTo(destination);
            charsWritten = 1_000_000;
            return true;
        }
    }

    [Fact]
    public static void AppendFormat_NoEscapedBracesInCustomFormatSpecifier()
    {
        Assert.Throws<FormatException>(() =>
        {
            // Tests new rule which does not allow escaped braces in the custom format specifier
            ValueStringBuilder builder = new();
            builder.AppendFormat("{0:}}}", 0);

            // Previous behavior: first two closing braces would be escaped and passed in as the custom format specifier, thus result = "}"
            // New behavior: first closing brace closes the argument hole and next two are escaped as part of the format, thus result = "0}"
            Assert.Equal("0}", builder.AsSpan().ToString());

            // Previously this would be allowed and escaped brace would be passed into the custom format, now this is unsupported
            builder.AppendFormat("{0:{{}", 0); // Format with custom format contains {
        });
    }

    public static IEnumerable<object?[]> AppendLine_TestData()
    {
        yield return new object?[] { "Hello", "abc", "Helloabc" + Environment.NewLine };
        yield return new object?[] { "Hello", "", "Hello" + Environment.NewLine };
        yield return new object?[] { "Hello", null, "Hello" + Environment.NewLine };
    }

    [Theory]
    [MemberData(nameof(AppendLine_TestData))]
    public static void AppendLine(string original, string value, string expected)
    {
        scoped ValueStringBuilder builder;
        if (string.IsNullOrEmpty(value))
        {
            // Use AppendLine()
            builder = new ValueStringBuilder(original);
            builder.AppendLine();
            Assert.Equal(expected, builder.AsSpan().ToString());
        }
        // Use AppendLine(string)
        builder = new ValueStringBuilder(original);
        builder.AppendLine(value);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public static void Clear()
    {
        ValueStringBuilder builder = new("Hello");
        builder.Clear();
        Assert.Equal(0, builder.Length);
        Assert.Same(string.Empty, builder.AsSpan().ToString());
    }

    [Fact]
    public static void Clear_Empty_CapacityStaysUnchanged()
    {
        ValueStringBuilder builder = new(14);
        int oldCapacity = builder.Capacity;
        builder.Clear();

        Assert.Equal(oldCapacity, builder.Capacity);
    }

    [Fact]
    public static void Clear_Full_CapacityStaysUnchanged()
    {
        ValueStringBuilder builder = new(14);
        builder.Append("Hello World!!!");
        int oldCapacity = builder.Capacity;

        builder.Clear();
        Assert.Equal(oldCapacity, builder.Capacity);
    }

    [Theory]
    [InlineData("Hello", 0, new char[] { '\0', '\0', '\0', '\0', '\0' }, 0, 5, new char[] { 'H', 'e', 'l', 'l', 'o' })]
    [InlineData("Hello", 0, new char[] { '\0', '\0', '\0', '\0', '\0', '\0' }, 1, 5, new char[] { '\0', 'H', 'e', 'l', 'l', 'o' })]
    [InlineData("Hello", 0, new char[] { '\0', '\0', '\0', '\0' }, 0, 4, new char[] { 'H', 'e', 'l', 'l' })]
    [InlineData("Hello", 1, new char[] { '\0', '\0', '\0', '\0', '\0', '\0', '\0' }, 2, 4, new char[] { '\0', '\0', 'e', 'l', 'l', 'o', '\0' })]
    public static void CopyTo(string value, int sourceIndex, char[] destination, int destinationIndex, int count, char[] expected)
    {
        ValueStringBuilder builder = new(value);
        builder.CopyTo(sourceIndex, destination, destinationIndex, count);
        Assert.Equal(expected, destination);
    }

    [Fact]
    public static void CopyTo_Invalid()
    {
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello").CopyTo(0, null!, 0, 0)); // Destination is null

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(-1, new char[10], 0, 0)); // Source index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(6, new char[10], 0, 0)); // Source index > builder.Length

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(0, new char[10], -1, 0)); // Destination index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(0, new char[10], 0, -1)); // Count < 0

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(5, new char[10], 0, 1)); // Source index + count > builder.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(4, new char[10], 0, 2)); // Source index + count > builder.Length

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(0, new char[10], 10, 1)); // Destination index + count > destinationArray.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(0, new char[10], 9, 2)); // Destination index + count > destinationArray.Length
    }

    [Fact]
    public static void EnsureCapacity()
    {
        ValueStringBuilder builder = new(40);

        builder.EnsureCapacity(20);
        Assert.True(builder.Capacity >= 20);

        builder.EnsureCapacity(20000);
        Assert.True(builder.Capacity >= 20000);

        // Ensuring a capacity less than the current capacity does not change anything
        int oldCapacity = builder.Capacity;
        builder.EnsureCapacity(10);
        Assert.Equal(oldCapacity, builder.Capacity);
    }

    [Fact]
    public static void EnsureCapacity_InvalidCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello", 10).EnsureCapacity(-1)); // Capacity < 0
    }

    public static IEnumerable<object[]> Equals_TestData()
    {
        ValueStringBuilder sb1 = new("Hello");
        ValueStringBuilder sb2 = new("HelloX");

        ValueStringBuilder sb3 = new(32);
        ValueStringBuilder sb4 = new(32);

        ValueStringBuilder sb5 = new(32);
        ValueStringBuilder sb6 = new(32);
        sb5.Append("Hello");
        sb6.Append("HelloX");

        ValueStringBuilder sb7 = new(32);
        sb7.Clear();

        return
        [
            Wrap(sb1, sb1, true),
            Wrap(sb1, sb2, false),

            Wrap(sb3, sb4, true),

            Wrap(sb5, sb5, true),
            Wrap(sb5, sb6, false),

            Wrap(sb1, default, false),

            Wrap(new ValueStringBuilder(), new ValueStringBuilder(), true),
            Wrap(new ValueStringBuilder(), sb7, true),
        ];

        static object[] Wrap(scoped ValueStringBuilder sb1, scoped ValueStringBuilder sb2, bool expected)
            => new object[] { sb1.AsCapacitySpan().ToArray(), sb1.Length, sb2.AsCapacitySpan().ToArray(), sb2.Length, expected };
    }

    [Theory]
    [MemberData(nameof(Equals_TestData))]
    public static void EqualsTest(char[] buffer1, int length1, char[] buffer2, int length2, bool expected)
    {
        ValueStringBuilder builder1 = new(buffer1.AsSpan()) { Length = length1 };
        ValueStringBuilder builder2 = new(buffer2.AsSpan()) { Length = length2 };

        Assert.Equal(expected, builder1.Equals(builder2));
    }

    [Theory]
    [InlineData("Hello", 0, (uint)0, "0Hello")]
    [InlineData("Hello", 3, (uint)123, "Hel123lo")]
    [InlineData("Hello", 5, (uint)456, "Hello456")]
    public static void Insert_UInt(string original, int index, uint value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_UInt_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, (uint)1)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, (uint)1)); // Index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, true, "TrueHello")]
    [InlineData("Hello", 3, false, "HelFalselo")]
    [InlineData("Hello", 5, false, "HelloFalse")]
    public static void Insert_Bool(string original, int index, bool value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_Bool_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, true)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, true)); // Index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, (byte)0, "0Hello")]
    [InlineData("Hello", 3, (byte)123, "Hel123lo")]
    [InlineData("Hello", 5, (byte)123, "Hello123")]
    public static void Insert_Byte(string original, int index, byte value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_Byte_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, (byte)1)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, (byte)1)); // Index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, (ulong)0, "0Hello")]
    [InlineData("Hello", 3, (ulong)123, "Hel123lo")]
    [InlineData("Hello", 5, (ulong)456, "Hello456")]
    public static void Insert_ULong(string original, int index, ulong value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_ULong_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, (ulong)1)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, (ulong)1)); // Index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, (ushort)0, "0Hello")]
    [InlineData("Hello", 3, (ushort)123, "Hel123lo")]
    [InlineData("Hello", 5, (ushort)456, "Hello456")]
    public static void Insert_UShort(string original, int index, ushort value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_UShort_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, (ushort)1)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, (ushort)1)); // Index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, '\0', "\0Hello")]
    [InlineData("Hello", 3, 'a', "Helalo")]
    [InlineData("Hello", 5, 'b', "Hellob")]
    public static void Insert_Char(string original, int index, char value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_Char_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, '\0')); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, '\0')); // Index > builder.Length
    }

    public static IEnumerable<object[]> Insert_Float_TestData()
    {
        yield return new object[] { "Hello", 0, 0f, "0Hello" };
        yield return new object[] { "Hello", 3, 1.23f, "Hel1.23lo" };
        yield return new object[] { "Hello", 5, -4.56f, "Hello-4.56" };
    }

    [Theory]
    [MemberData(nameof(Insert_Float_TestData))]
    public static void Insert_Float(string original, int index, float value, string expected)
    {
        using ThreadCultureChange _ = new(CultureInfo.InvariantCulture);

        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_Float_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, (float)1)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, (float)1)); // Index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, "\0", "\0Hello")]
    [InlineData("Hello", 3, "abc", "Helabclo")]
    [InlineData("Hello", 5, "def", "Hellodef")]
    [InlineData("Hello", 0, "", "Hello")]
    [InlineData("Hello", 0, null, "Hello")]
    public static void Insert_Object(string original, int index, object value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public static void Insert_Object_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, new object())); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, new object())); // Index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, (long)0, "0Hello")]
    [InlineData("Hello", 3, (long)123, "Hel123lo")]
    [InlineData("Hello", 5, (long)-456, "Hello-456")]
    public static void Insert_Long(string original, int index, long value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_Long_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, (long)1)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, (long)1)); // Index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, 0, "0Hello")]
    [InlineData("Hello", 3, 123, "Hel123lo")]
    [InlineData("Hello", 5, -456, "Hello-456")]
    public static void Insert_Int(string original, int index, int value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_Int_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, 1)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, 1)); // Index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, (short)0, "0Hello")]
    [InlineData("Hello", 3, (short)123, "Hel123lo")]
    [InlineData("Hello", 5, (short)-456, "Hello-456")]
    public static void Insert_Short(string original, int index, short value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_Short_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, (short)1)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, (short)1)); // Index > builder.Length
    }

    public static IEnumerable<object[]> Insert_Double_TestData()
    {
        yield return new object[] { "Hello", 0, 0.0, "0Hello" };
        yield return new object[] { "Hello", 3, 1.23, "Hel1.23lo" };
        yield return new object[] { "Hello", 5, -4.56, "Hello-4.56" };
    }

    [Theory]
    [MemberData(nameof(Insert_Double_TestData))]
    public static void Insert_Double(string original, int index, double value, string expected)
    {
        using ThreadCultureChange _ = new(CultureInfo.InvariantCulture);

        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_Double_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, (double)1)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, (double)1)); // Index > builder.Length
    }

    public static IEnumerable<object[]> Insert_Decimal_TestData()
    {
        yield return new object[] { "Hello", 0, 0m, "0Hello" };
        yield return new object[] { "Hello", 3, 1.23m, "Hel1.23lo" };
        yield return new object[] { "Hello", 5, -4.56m, "Hello-4.56" };
    }

    [Theory]
    [MemberData(nameof(Insert_Decimal_TestData))]
    public static void Insert_Decimal(string original, int index, decimal value, string expected)
    {
        using ThreadCultureChange _ = new(CultureInfo.InvariantCulture);

        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_Decimal_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, (decimal)1)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, (decimal)1)); // Index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, (sbyte)0, "0Hello")]
    [InlineData("Hello", 3, (sbyte)123, "Hel123lo")]
    [InlineData("Hello", 5, (sbyte)-123, "Hello-123")]
    public static void Insert_SByte(string original, int index, sbyte value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, value);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_SByte_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, (sbyte)1)); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, (sbyte)1)); // Index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, "\0", 0, "Hello")]
    [InlineData("Hello", 0, "\0", 1, "\0Hello")]
    [InlineData("Hello", 3, "abc", 1, "Helabclo")]
    [InlineData("Hello", 5, "def", 1, "Hellodef")]
    [InlineData("Hello", 0, "", 1, "Hello")]
    [InlineData("Hello", 0, null, 1, "Hello")]
    [InlineData("Hello", 3, "abc", 2, "Helabcabclo")]
    [InlineData("Hello", 5, "def", 2, "Hellodefdef")]
    public static void Insert_String_Count(string original, int index, string value, int count, string expected)
    {
        scoped ValueStringBuilder builder;
        if (count == 1)
        {
            // Use Insert(int, string)
            builder = new ValueStringBuilder(original);
            builder.Insert(index, value);
            Assert.Equal(expected, builder.AsSpan().ToString());
        }
        // Use Insert(int, string, int)
        builder = new ValueStringBuilder(original);
        builder.Insert(index, value, count);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public static void Insert_String_Count_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, "")); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, "", 0)); // Index < 0

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, "")); // Index > builder.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, "", 0)); // Index > builder.Length

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(0, "", -1)); // Count < 0
    }

    [Theory]
    [InlineData("Hello", 0, new char[] { '\0' }, 0, 1, "\0Hello")]
    [InlineData("Hello", 3, new char[] { 'a', 'b', 'c' }, 0, 1, "Helalo")]
    [InlineData("Hello", 3, new char[] { 'a', 'b', 'c' }, 0, 3, "Helabclo")]
    [InlineData("Hello", 5, new char[] { 'd', 'e', 'f' }, 0, 1, "Hellod")]
    [InlineData("Hello", 5, new char[] { 'd', 'e', 'f' }, 0, 3, "Hellodef")]
    [InlineData("Hello", 0, new char[0], 0, 0, "Hello")]
    [InlineData("Hello", 0, null, 0, 0, "Hello")]
    [InlineData("Hello", 3, new char[] { 'a', 'b', 'c' }, 1, 1, "Helblo")]
    [InlineData("Hello", 3, new char[] { 'a', 'b', 'c' }, 1, 2, "Helbclo")]
    [InlineData("Hello", 3, new char[] { 'a', 'b', 'c' }, 0, 2, "Helablo")]
    public static void Insert_CharArray(string original, int index, char[] value, int startIndex, int charCount, string expected)
    {
        scoped ValueStringBuilder builder;
        if (startIndex == 0 && charCount == (value?.Length ?? 0))
        {
            // Use Insert(int, char[])
            builder = new ValueStringBuilder(original);
            builder.Insert(index, value);
            Assert.Equal(expected, builder.AsSpan().ToString());
        }
        // Use Insert(int, char[], int, int)
        builder = new ValueStringBuilder(original);
        builder.Insert(index, value, startIndex, charCount);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public static void Insert_CharArray_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, new char[1])); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, new char[0], 0, 0)); // Index < 0

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, new char[1])); // Index > builder.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, new char[0], 0, 0)); // Index > builder.Length

        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder("Hello").Insert(0, (char[])null!, 1, 1)); // Value is null (startIndex and count are not zero)
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(0, new char[0], -1, 0)); // Start index < 0

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(0, new char[3], 4, 0)); // Start index + char count > value.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(0, new char[3], 3, 1)); // Start index + char count > value.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(0, new char[3], 2, 2)); // Start index + char count > value.Length
    }

    [Fact]
    public static void Insert_CharArray_InvalidCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(0, new char[0], 0, -1)); // Char count < 0
    }

    [Fact]
    public static void Insert_CharArray_InvalidCharCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(0, new char[0], 0, -1)); // Char count < 0
    }

    [Theory]
    [InlineData("", 0, 0, "")]
    [InlineData("Hello", 0, 5, "")]
    [InlineData("Hello", 1, 3, "Ho")]
    [InlineData("Hello", 1, 4, "H")]
    [InlineData("Hello", 1, 0, "Hello")]
    [InlineData("Hello", 5, 0, "Hello")]
    public static void Remove(string value, int startIndex, int length, string expected)
    {
        ValueStringBuilder builder = new(value);
        builder.Remove(startIndex, length);
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Remove_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Remove(-1, 0)); // Start index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Remove(0, -1)); // Length < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Remove(6, 0)); // Start index + length > 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Remove(5, 1)); // Start index + length > 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Remove(4, 2)); // Start index + length > 0
    }

    [Theory]
    [InlineData("", 'a', '!', 0, 0, "")]
    [InlineData("aaaabbbbccccdddd", 'a', '!', 0, 16, "!!!!bbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", 'a', '!', 0, 4, "!!!!bbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", 'a', '!', 2, 3, "aa!!bbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", 'a', '!', 4, 1, "aaaabbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", 'b', '!', 0, 0, "aaaabbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", 'a', '!', 16, 0, "aaaabbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", 'e', '!', 0, 16, "aaaabbbbccccdddd")]
    public static void Replace_Char(string value, char oldChar, char newChar, int startIndex, int count, string expected)
    {
        scoped ValueStringBuilder builder;
        if (startIndex == 0 && count == value.Length)
        {
            // Use Replace(char, char)
            builder = new ValueStringBuilder(value);
            builder.Replace(oldChar, newChar);
            Assert.Equal(expected, builder.AsSpan().ToString());
        }
        // Use Replace(char, char, int, int)
        builder = new ValueStringBuilder(value);
        builder.Replace(oldChar, newChar, startIndex, count);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public static void Replace_Char_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Replace('a', 'b', -1, 0)); // Start index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Replace('a', 'b', 0, -1)); // Count < 0

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Replace('a', 'b', 6, 0)); // Count + start index > builder.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Replace('a', 'b', 5, 1)); // Count + start index > builder.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Replace('a', 'b', 4, 2)); // Count + start index > builder.Length
    }

    [Theory]
    [InlineData("Hello", 0, 5, "Hello")]
    [InlineData("Hello", 2, 3, "llo")]
    [InlineData("Hello", 2, 2, "ll")]
    [InlineData("Hello", 5, 0, "")]
    [InlineData("Hello", 4, 0, "")]
    [InlineData("Hello", 0, 0, "")]
    [InlineData("", 0, 0, "")]
    public static void ToStringTest(string value, int startIndex, int length, string expected)
    {
        ValueStringBuilder builder = new(value);
        if (startIndex == 0 && length == value.Length)
        {
            Assert.Equal(expected, builder.AsSpan().ToString());
        }
        Assert.Equal(expected, builder.ToString(startIndex, length));
    }

    [Fact]
    public static void ToString_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").ToString(-1, 0)); // Start index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").ToString(0, -1)); // Length < 0

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").ToString(6, 0)); // Length + start index > builder.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").ToString(5, 1)); // Length + start index > builder.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").ToString(4, 2)); // Length + start index > builder.Length
    }

    public class CustomFormatter : ICustomFormatter, IFormatProvider
    {
        public string Format(string? format, object? arg, IFormatProvider? formatProvider) => "abc";

        public object GetFormat(Type? formatType) => this;
    }

    [Fact]
    public static void AppendJoin_NullValues_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder().AppendJoin('|', (object[])null!));
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder().AppendJoin('|', (IEnumerable<object>)null!));
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder().AppendJoin('|', (string[])null!));
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder().AppendJoin("|", (object[])null!));
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder().AppendJoin("|", (IEnumerable<object>)null!));
        Assert.Throws<ArgumentNullException>(() => new ValueStringBuilder().AppendJoin("|", (string[])null!));
    }

    private sealed class NullToStringObject
    {
        public override string? ToString() => null;
    }

    public static IEnumerable<object?[]> AppendJoin_TestValues_TestData()
    {
        yield return [new object?[0], ""];
        yield return [new object?[] { null }, ""];
        yield return [new object?[] { 10 }, "10"];
        yield return [new object?[] { null, null }, "|"];
        yield return [new object?[] { null, 20 }, "|20"];
        yield return [new object?[] { 10, null }, "10|"];
        yield return [new object?[] { 10, 20 }, "10|20"];
        yield return [new object?[] { null, null, null }, "||"];
        yield return [new object?[] { null, null, 30 }, "||30"];
        yield return [new object?[] { null, 20, null }, "|20|"];
        yield return [new object?[] { null, 20, 30 }, "|20|30"];
        yield return [new object?[] { 10, null, null }, "10||"];
        yield return [new object?[] { 10, null, 30 }, "10||30"];
        yield return [new object?[] { 10, 20, null }, "10|20|"];
        yield return [new object?[] { 10, 20, 30 }, "10|20|30"];
        yield return [new object?[] { "" }, ""];
        yield return [new object?[] { "", "" }, "|"];

        yield return [new object[] { new NullToStringObject() }, ""];
        yield return [new object[] { new NullToStringObject(), new NullToStringObject() }, "|"];
    }

    [Theory]
    [MemberData(nameof(AppendJoin_TestValues_TestData))]
    public static void AppendJoin_TestValues(object[] values, string expected)
    {
        scoped ValueStringBuilder builder;
        string?[] stringValues = Array.ConvertAll(values, _ => _?.ToString());
        IEnumerable<object> enumerable = values.Select(_ => _);

        builder = new();
        builder.AppendJoin('|', values);
        Assert.Equal(expected, builder.ToString());

        builder = new();
        builder.AppendJoin('|', enumerable);
        Assert.Equal(expected, builder.ToString());

        builder = new();
        builder.AppendJoin('|', stringValues);
        Assert.Equal(expected, builder.ToString());

        builder = new();
        builder.AppendJoin("|", values);
        Assert.Equal(expected, builder.ToString());

        builder = new();
        builder.AppendJoin("|", enumerable);
        Assert.Equal(expected, builder.ToString());

        builder = new();
        builder.AppendJoin("|", stringValues);
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData(null, "123")]
    [InlineData("", "123")]
    [InlineData(" ", "1 2 3")]
    [InlineData(", ", "1, 2, 3")]
    public static void AppendJoin_TestStringSeparators(string separator, string expected)
    {
        scoped ValueStringBuilder builder;
        object?[] values = new object[] { 1, 2, 3 };
        string?[] stringValues = Array.ConvertAll(values, _ => _?.ToString());
        IEnumerable<object?> enumerable = values.Select(_ => _);

        builder = new();
        builder.AppendJoin(separator, values);
        Assert.Equal(expected, builder.ToString());

        builder = new();
        builder.AppendJoin(separator, enumerable);
        Assert.Equal(expected, builder.ToString());

        builder = new();
        builder.AppendJoin(separator, stringValues);
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", new char[] { 'a' }, "Helloa")]
    [InlineData("Hello", new char[] { 'b', 'c', 'd' }, "Hellobcd")]
    [InlineData("Hello", new char[] { 'b', '\0', 'd' }, "Hellob\0d")]
    [InlineData("", new char[] { 'e', 'f', 'g' }, "efg")]
    [InlineData("Hello", new char[0], "Hello")]
    public static void Append_CharSpan(string original, char[] value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(new ReadOnlySpan<char>(value));
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData("Hello", new char[] { 'a' }, "Helloa")]
    [InlineData("Hello", new char[] { 'b', 'c', 'd' }, "Hellobcd")]
    [InlineData("Hello", new char[] { 'b', '\0', 'd' }, "Hellob\0d")]
    [InlineData("", new char[] { 'e', 'f', 'g' }, "efg")]
    [InlineData("Hello", new char[0], "Hello")]
    public static void Append_CharMemory(string original, char[] value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Append(value.AsMemory());
        Assert.Equal(expected, builder.ToString());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10000)]
    public static void Clear_AppendAndInsertBeforeClearManyTimes_CapacityStaysWithinRange(int times)
    {
        ValueStringBuilder builder = new();
        var originalCapacity = builder.Capacity;
        var s = new string(' ', 10);
        int oldLength = 0;
        for (int i = 0; i < times; i++)
        {
            builder.Append(s);
            builder.Append(s);
            builder.Append(s);
            builder.Insert(0, s);
            builder.Insert(0, s);
            oldLength = builder.Length;

            builder.Clear();
        }
        Assert.InRange(builder.Capacity, 1, oldLength * 1.4);
    }

    [Theory]
    [InlineData("Hello", 0, new char[] { '\0', '\0', '\0', '\0', '\0' }, 5, new char[] { 'H', 'e', 'l', 'l', 'o' })]
    [InlineData("Hello", 0, new char[] { '\0', '\0', '\0', '\0' }, 4, new char[] { 'H', 'e', 'l', 'l' })]
    [InlineData("Hello", 1, new char[] { '\0', '\0', '\0', '\0', '\0' }, 4, new char[] { 'e', 'l', 'l', 'o', '\0' })]
    public static void CopyTo_CharSpan(string value, int sourceIndex, char[] destination, int count, char[] expected)
    {
        ValueStringBuilder builder = new(value);
        builder.CopyTo(sourceIndex, new Span<char>(destination), count);
        Assert.Equal(expected, destination);
    }

    [Fact]
    public static void CopyTo_CharSpan_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(-1, new Span<char>(new char[10]), 0)); // Source index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(6, new Span<char>(new char[10]), 0)); // Source index > builder.Length

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(0, new Span<char>(new char[10]), -1)); // Count < 0

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(5, new Span<char>(new char[10]), 1)); // Source index + count > builder.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(4, new Span<char>(new char[10]), 2)); // Source index + count > builder.Length

        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").CopyTo(0, new Span<char>(new char[10]), 11)); // count > destinationArray.Length
    }

    [Theory]
    [InlineData("Hello", 0, new char[] { '\0' }, "\0Hello")]
    [InlineData("Hello", 3, new char[] { 'a', 'b', 'c' }, "Helabclo")]
    [InlineData("Hello", 5, new char[] { 'd', 'e', 'f' }, "Hellodef")]
    [InlineData("Hello", 0, new char[0], "Hello")]
    public static void Insert_CharSpan(string original, int index, char[] value, string expected)
    {
        ValueStringBuilder builder = new(original);
        builder.Insert(index, new ReadOnlySpan<char>(value));
        Assert.Equal(expected, builder.ToString());
    }

    [Fact]
    public static void Insert_CharSpan_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(-1, new ReadOnlySpan<char>(new char[0]))); // Index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Insert(6, new ReadOnlySpan<char>(new char[0]))); // Index > builder.Length
    }

    public static IEnumerable<object[]> Append_StringBuilder_TestData()
    {
        string mediumString = new('a', 30);
        string largeString = new('b', 1000);

        ValueStringBuilder sb1 = new("Hello");
        ValueStringBuilder sb2 = new("one");
        ValueStringBuilder sb3 = new(32);
        sb3.Append(mediumString);

        return
        [
            Wrap(new ValueStringBuilder("Hello"), sb1, "HelloHello"),
            Wrap(new ValueStringBuilder("Hello"), sb2, "Helloone"),
            Wrap(new ValueStringBuilder("Hello"), new ValueStringBuilder(), "Hello"),

            Wrap(new ValueStringBuilder("one"), sb3, "one" + mediumString),

            WrapAppend(32, mediumString, sb3, mediumString + mediumString),

            WrapAppend(32, largeString, sb3, largeString + mediumString),

            Wrap(new ValueStringBuilder(10), sb3, mediumString),
            Wrap(new ValueStringBuilder(32), sb3, mediumString),
            Wrap(new ValueStringBuilder(32), new ValueStringBuilder(32), string.Empty),
        ];

        static object[] Wrap(in ValueStringBuilder sb1, in ValueStringBuilder sb2, string expected)
            => new object[] { sb1.AsCapacitySpan().ToArray(), sb1.Length, sb2.AsCapacitySpan().ToArray(), sb2.Length, expected };

        static object[] WrapAppend(int capacity1, string value1, in ValueStringBuilder sb2, string expected)
        {
            ValueStringBuilder builder = new(capacity1);
            builder.Append(value1);
            return Wrap(builder, sb2, expected);
        }
    }

    [Theory]
    [MemberData(nameof(Append_StringBuilder_TestData))]
    public static void Append_StringBuilder(char[] buffer1, int length1, char[] buffer2, int length2, string expected)
    {
        ValueStringBuilder builder1 = new(buffer1.AsSpan()) { Length = length1 };
        ValueStringBuilder builder2 = new(buffer2.AsSpan()) { Length = length2 };

        builder1.Append(builder2);

        Assert.Equal(expected, builder1.ToString());
    }

    public static IEnumerable<object[]> Append_StringBuilder_Substring_TestData()
    {
        string mediumString = new('a', 30);
        string largeString = new('b', 1000);

        ValueStringBuilder sb1 = new(5);
        ValueStringBuilder sb2 = new(3);
        ValueStringBuilder sb3 = new(20);

        sb1.Append("Hello");
        sb2.Append("one");
        sb3.Append(mediumString);

        return
        [
            Wrap(new ValueStringBuilder("Hello"), sb1, 0, 5, "HelloHello"),
            Wrap(new ValueStringBuilder("Hello"), sb1, 0, 0, "Hello"),
            Wrap(new ValueStringBuilder("Hello"), sb1, 2, 3, "Hellollo"),
            Wrap(new ValueStringBuilder("Hello"), sb1, 2, 2, "Helloll"),
            Wrap(new ValueStringBuilder("Hello"), sb1, 2, 0, "Hello"),
            Wrap(new ValueStringBuilder("Hello"), new ValueStringBuilder(), 0, 0, "Hello"),
            Wrap(new ValueStringBuilder(), new ValueStringBuilder("Hello"), 2, 3, "llo"),
            Wrap(new ValueStringBuilder("Hello"), sb2, 0, 3, "Helloone"),

            Wrap(new ValueStringBuilder("one"), sb3, 5, 25, "one" + new string('a', 25)),
            Wrap(new ValueStringBuilder("one"), sb3, 5, 20, "one" + new string('a', 20)),
            Wrap(new ValueStringBuilder("one"), sb3, 10, 10, "one" + new string('a', 10)),

            WrapAppend1(20, mediumString, sb3, 20, 10, new string('a', 40)),
            WrapAppend1(10, mediumString, sb3, 10, 10, new string('a', 40)),

            WrapAppend2(20, largeString, 20, largeString, 100, 50, largeString + new string('b', 50)),
            WrapAppend2(10, mediumString, 20, largeString, 20, 10, mediumString + new string('b', 10)),
            WrapAppend2(10, mediumString, 20, largeString, 100, 50, mediumString + new string('b', 50)),

            Wrap(sb2, sb2, 2, 0, "one"),
        ];

        static object[] Wrap(in ValueStringBuilder sb1, in ValueStringBuilder sb2, int startIndex, int count, string s)
            => new object[] { sb1.AsCapacitySpan().ToArray(), sb1.Length, sb2.AsCapacitySpan().ToArray(), sb2.Length, startIndex, count, s };

        static object[] WrapAppend1(int capacity1, string value1, in ValueStringBuilder sb2, int startIndex, int count, string s)
        {
            ValueStringBuilder sb1 = new(capacity1);
            sb1.Append(value1);
            return Wrap(sb1, sb2, startIndex, count, s);
        }

        static object[] WrapAppend2(int capacity1, string value1, int capacity2, string value2, int startIndex, int count, string s)
        {
            ValueStringBuilder sb1 = new(capacity1);
            sb1.Append(value1);

            ValueStringBuilder sb2 = new(capacity2);
            sb2.Append(value2);

            return Wrap(sb1, sb2, startIndex, count, s);
        }
    }

    [Theory]
    [MemberData(nameof(Append_StringBuilder_Substring_TestData))]
    public static void Append_StringBuilder_Substring(char[] buffer1, int length1, char[] buffer2, int length2, int startIndex, int count, string s)
    {
        ValueStringBuilder builder1 = new(buffer1.AsSpan()) { Length = length1 };
        ValueStringBuilder builder2 = new(buffer2.AsSpan()) { Length = length2 };

        builder1.Append(builder2, startIndex, count);
        Assert.Equal(s, builder1.ToString());
    }

    [Fact]
    public static void Append_StringBuilder_InvalidInput()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Append(new ValueStringBuilder("Hello"), -1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Append(new ValueStringBuilder("Hello"), 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStringBuilder("Hello").Append(new ValueStringBuilder("Hello"), 4, 5));
    }

    public static IEnumerable<object[]> Equals_String_TestData()
    {
        string mediumString = new('a', 30);
        string largeString = new('a', 1000);
        string extraLargeString = new('a', 41000);

        ValueStringBuilder sb1 = new(5);
        ValueStringBuilder sb2 = new(20);
        ValueStringBuilder sb3 = new(20);
        ValueStringBuilder sb4 = new(20);

        sb1.Append("Hello");
        sb2.Append(mediumString);
        sb3.Append(largeString);
        sb4.Append(extraLargeString);

        return
        [
            Wrap(sb1, "Hello", true),
            Wrap(sb1, "Hel", false),
            Wrap(sb1, "Hellz", false),
            Wrap(sb1, "Helloz", false),
            Wrap(sb1, "", false),
            Wrap(new ValueStringBuilder(), "", true),
            Wrap(new ValueStringBuilder(), "Hello", false),
            Wrap(sb2, mediumString, true),
            Wrap(sb2, "H", false),
            Wrap(sb3, largeString, true),
            Wrap(sb3, "H", false),
            Wrap(sb3, new string('a', 999) + 'b', false),
            Wrap(sb4, extraLargeString, true),
            Wrap(sb4, "H", false),
        ];

        static object[] Wrap(in ValueStringBuilder sb1, string value, bool expected)
            => new object[] { sb1.AsCapacitySpan().ToArray(), sb1.Length, value, expected };
    }

    [Theory]
    [MemberData(nameof(Equals_String_TestData))]
    public static void Equals_String(char[] buffer1, int length1, string value, bool expected)
    {
        ValueStringBuilder builder1 = new(buffer1.AsSpan()) { Length = length1 };

        Assert.Equal(expected, builder1.Equals(value.AsSpan()));
    }

    [Fact]
    public static void EqualsIgnoresCapacity()
    {
        ValueStringBuilder sb1 = new(5);
        ValueStringBuilder sb2 = new(10);

        Assert.True(sb1.Equals(sb2));

        sb1.Append("12345");
        sb2.Append("12345");

        Assert.True(sb1.Equals(sb2));
    }
}
