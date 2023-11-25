using Spanned.Text;

namespace Spanned.Tests.Text;

public abstract class ValueStringBuilderReplaceTests
{
    [Theory]
    [InlineData("", "a", "!", 0, 0, "")]
    [InlineData("aaaabbbbccccdddd", "a", "!", 0, 16, "!!!!bbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "a", "!", 2, 3, "aa!!bbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "a", "!", 4, 1, "aaaabbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "aab", "!", 2, 2, "aaaabbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "aab", "!", 2, 3, "aa!bbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "aa", "!", 0, 16, "!!bbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "aa", "$!", 0, 16, "$!$!bbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "aa", "$!$", 0, 16, "$!$$!$bbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "aaaa", "!", 0, 16, "!bbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "aaaa", "$!", 0, 16, "$!bbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "a", "", 0, 16, "bbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "b", null, 0, 16, "aaaaccccdddd")]
    [InlineData("aaaabbbbccccdddd", "aaaabbbbccccdddd", "", 0, 16, "")]
    [InlineData("aaaabbbbccccdddd", "aaaabbbbccccdddd", "", 16, 0, "aaaabbbbccccdddd")]
    [InlineData("aaaabbbbccccdddd", "aaaabbbbccccdddde", "", 0, 16, "aaaabbbbccccdddd")]
    [InlineData("aaaaaaaaaaaaaaaa", "a", "b", 0, 16, "bbbbbbbbbbbbbbbb")]
    public void Replace_ValueStringBuilder(string value, string oldValue, string newValue, int startIndex, int count, string expected)
    {
        scoped ValueStringBuilder builder;
        if (startIndex == 0 && count == value.Length)
        {
            // Use Replace(string, string) / Replace(ReadOnlySpan<char>, ReadOnlySpan<char>)
            builder = new ValueStringBuilder(value);
            Replace(ref builder, oldValue, newValue);
            Assert.Equal(expected, builder.AsSpan().ToString());
        }

        // Use Replace(string, string, int, int) / Replace(ReadOnlySpan<char>, ReadOnlySpan<char>, int, int)
        builder = new ValueStringBuilder(value);
        Replace(ref builder, oldValue, newValue, startIndex, count);
        Assert.Equal(expected, builder.AsSpan().ToString());
    }

    [Fact]
    public void Replace_Invalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RunReplace(new ValueStringBuilder("Hello", 10), "", "a")); // Old value is empty
        Assert.Throws<ArgumentOutOfRangeException>(() => RunReplace(new ValueStringBuilder("Hello", 10), "", "a", 0, 0)); // Old value is empty

        Assert.Throws<ArgumentOutOfRangeException>(() => RunReplace(new ValueStringBuilder("Hello", 10), "a", "b", -1, 0)); // Start index < 0
        Assert.Throws<ArgumentOutOfRangeException>(() => RunReplace(new ValueStringBuilder("Hello", 10), "a", "b", 0, -1)); // Count < 0

        Assert.Throws<ArgumentOutOfRangeException>(() => RunReplace(new ValueStringBuilder("Hello", 10), "a", "b", 6, 0)); // Count + start index > builder.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => RunReplace(new ValueStringBuilder("Hello", 10), "a", "b", 5, 1)); // Count + start index > builder.Length
        Assert.Throws<ArgumentOutOfRangeException>(() => RunReplace(new ValueStringBuilder("Hello", 10), "a", "b", 4, 2)); // Count + start index > builder.Length
    }

    protected abstract void Replace(ref ValueStringBuilder builder, string oldValue, string newValue);

    protected abstract void Replace(ref ValueStringBuilder builder, string oldValue, string newValue, int startIndex, int count);

    protected abstract void RunReplace(in ValueStringBuilder builder, string oldValue, string newValue);

    protected abstract void RunReplace(in ValueStringBuilder builder, string oldValue, string newValue, int startIndex, int count);
}

public class ValueStringBuilderReplaceTests_String : ValueStringBuilderReplaceTests
{
    [Fact]
    public void Replace_String_Invalid()
    {
        Assert.Throws<ArgumentNullException>(() => RunReplace(new ValueStringBuilder("Hello", 10), null!, "")); // Old value is null
        Assert.Throws<ArgumentNullException>(() => RunReplace(new ValueStringBuilder("Hello", 10), null!, "a", 0, 0)); // Old value is null
    }

    protected override void Replace(ref ValueStringBuilder builder, string oldValue, string newValue)
        => builder.Replace(oldValue, newValue);

    protected override void Replace(ref ValueStringBuilder builder, string oldValue, string newValue, int startIndex, int count)
        => builder.Replace(oldValue, newValue, startIndex, count);

    protected override void RunReplace(in ValueStringBuilder builder, string oldValue, string newValue)
        => builder.Replace(oldValue, newValue);

    protected override void RunReplace(in ValueStringBuilder builder, string oldValue, string newValue, int startIndex, int count)
        => builder.Replace(oldValue, newValue, startIndex, count);
}

public class ValueStringBuilderReplaceTests_Span : ValueStringBuilderReplaceTests
{
    protected override void Replace(ref ValueStringBuilder builder, string oldValue, string newValue)
        => builder.Replace(oldValue.AsSpan(), newValue.AsSpan());

    protected override void Replace(ref ValueStringBuilder builder, string oldValue, string newValue, int startIndex, int count)
        => builder.Replace(oldValue.AsSpan(), newValue.AsSpan(), startIndex, count);

    protected override void RunReplace(in ValueStringBuilder builder, string oldValue, string newValue)
        => builder.Replace(oldValue.AsSpan(), newValue.AsSpan());

    protected override void RunReplace(in ValueStringBuilder builder, string oldValue, string newValue, int startIndex, int count)
        => builder.Replace(oldValue.AsSpan(), newValue.AsSpan(), startIndex, count);
}
