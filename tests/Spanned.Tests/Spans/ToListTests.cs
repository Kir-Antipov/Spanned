namespace Spanned.Tests.Spans;

public class ToListTests
{
    [Fact]
    public void ToList()
    {
        Span<int> source = [1, 2, 3, 4, 5];

        List<int> list = source.ToList();

        Assert.Equal([1, 2, 3, 4, 5], list);
    }

    [Fact]
    public void ToList_EmptySource()
    {
        Span<int> source = Span<int>.Empty;

        List<int> list = source.ToList();

        Assert.Empty(list);
    }

    [Fact]
    public void ToList_CreatesIndependentList()
    {
        Span<int> source = [1, 2, 3, 4, 5];

        List<int> list = source.ToList();

        // Modify the source.
        source[0] = 0;

        Assert.Equal([1, 2, 3, 4, 5], list);
    }

    [Fact]
    public void ToList_SourceContainsReferences()
    {
        Span<string> source = ["a", "b", "c"];

        List<string> list = source.ToList();

        Assert.Equal(["a", "b", "c"], list);
    }
}
