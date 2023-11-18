namespace Spanned.Tests.Spans;

public class AsSpanTests
{
    [Fact]
    public void TryGetSpan_NullSource_ReturnsFalse()
    {
        IEnumerable<int>? enumerable = null;

        bool result = enumerable.TryGetSpan(out ReadOnlySpan<int> span);

        Assert.False(result);
        Assert.True(span.IsEmpty);
    }

    [Fact]
    public void TryGetSpan_EnumerableSource_ReturnsFalse()
    {
        IEnumerable<int> enumerable = Enumerable.Range(1, 100);

        bool result = enumerable.TryGetSpan(out ReadOnlySpan<int> span);

        Assert.False(result);
        Assert.True(span.IsEmpty);
    }

    [Fact]
    public void TryGetSpan_ArraySource_ReturnsTrue()
    {
        IEnumerable<int> enumerable = new int[] { 1, 2, 3, 4, 5 };

        bool result = enumerable.TryGetSpan(out ReadOnlySpan<int> span);

        Assert.True(result);
        Assert.Equal([1, 2, 3, 4, 5], span.ToArray());
    }

    [Fact]
    public void TryGetSpan_ListSource_ReturnsTrue()
    {
        IEnumerable<int> enumerable = new List<int> { 1, 2, 3, 4, 5 };

        bool result = enumerable.TryGetSpan(out ReadOnlySpan<int> span);

        Assert.True(result);
        Assert.Equal([1, 2, 3, 4, 5], span.ToArray());
    }

    [Fact]
    public void TryGetSpan_StringSource_ReturnsTrue()
    {
        IEnumerable<char> enumerable = "abcde";

        bool result = enumerable.TryGetSpan(out ReadOnlySpan<char> span);

        Assert.True(result);
        Assert.Equal(['a', 'b', 'c', 'd', 'e'], span.ToArray());
    }

    [Fact]
    public void AsSpan()
    {
        List<int> list = [1, 2, 3, 4, 5];

        Span<int> span = list.AsSpan();

        Assert.Equal([1, 2, 3, 4, 5], span.ToArray());
    }

    [Fact]
    public void AsSpan_NullSource_ReturnsEmptySpan()
    {
        List<int>? list = null;

        Span<int> span = list.AsSpan();

        Assert.True(span.IsEmpty);
    }

    [Fact]
    public void AsSpan_EmptySource_ReturnsEmptySpan()
    {
        List<int> list = new();

        Span<int> span = list.AsSpan();

        Assert.True(span.IsEmpty);
    }

    [Fact]
    public void AsRemainingSpan()
    {
        List<int> list = new(10) { 1, 2, 3, 4, 5 };

        Span<int> span = list.AsRemainingSpan();

        Assert.Equal([0, 0, 0, 0, 0], span.ToArray());
    }

    [Fact]
    public void AsRemainingSpan_NullSource_ReturnsEmptySpan()
    {
        List<int>? list = null;

        Span<int> span = list.AsRemainingSpan();

        Assert.True(span.IsEmpty);
    }

    [Fact]
    public void AsRemainingSpan_EmptySource_ReturnsCapacitySpan()
    {
        List<int> list = new(10);

        Span<int> span = list.AsRemainingSpan();

        Assert.Equal(list.Capacity, span.Length);
    }

    [Fact]
    public void AsCapacitySpan()
    {
        List<int> list = new(10) { 1, 2, 3, 4, 5 };

        Span<int> span = list.AsCapacitySpan();

        Assert.Equal([1, 2, 3, 4, 5, 0, 0, 0, 0, 0], span.ToArray());
    }

    [Fact]
    public void AsCapacitySpan_NullSource_ReturnsEmptySpan()
    {
        List<int>? list = null;

        Span<int> span = list.AsCapacitySpan();

        Assert.True(span.IsEmpty);
    }

    [Fact]
    public void AsCapacitySpan_EmptySource_ReturnsCapacitySpan()
    {
        List<int> list = new(10);

        Span<int> span = list.AsCapacitySpan();

        Assert.Equal(10, span.Length);
    }
}
