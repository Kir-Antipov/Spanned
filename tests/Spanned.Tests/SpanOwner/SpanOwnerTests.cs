namespace Spanned.Tests.SpanOwner;

public class SpanOwnerTests
{
    [Fact]
    public void Rent_ReturnsSpanOfRequestedLength()
    {
        int length = 10;
        SpanOwner<int> spanOwner = SpanOwner<int>.Rent(length);

        Assert.Equal(length, spanOwner.Span.Length);
        Assert.Equal(length, ((Span<int>)spanOwner).Length);
    }

    [Fact]
    public void Dispose_ThrowsNoException()
    {
        int length = 10;
        SpanOwner<int> spanOwner = SpanOwner<int>.Rent(length);

        spanOwner.Dispose();
    }

    [Fact]
    public void MayStackalloc_ReturnsTrueForSmallLengths()
    {
        Assert.True(SpanOwner<int>.MayStackalloc(0));
        Assert.True(SpanOwner<int>.MayStackalloc(1));
        Assert.True(SpanOwner<int>.MayStackalloc(2));
        Assert.True(SpanOwner<int>.MayStackalloc(4));
        Assert.True(SpanOwner<int>.MayStackalloc(8));
        Assert.True(SpanOwner<int>.MayStackalloc(10));
        Assert.True(SpanOwner<int>.MayStackalloc(32));
    }

    [Fact]
    public void MayStackalloc_ReturnsFalseForBigLengths()
    {
        Assert.False(SpanOwner<int>.MayStackalloc(1024));
        Assert.False(SpanOwner<int>.MayStackalloc(2048));
        Assert.False(SpanOwner<int>.MayStackalloc(3000));
        Assert.False(SpanOwner<int>.MayStackalloc(4096));
        Assert.False(SpanOwner<int>.MayStackalloc(5000));
        Assert.False(SpanOwner<int>.MayStackalloc(6000));
        Assert.False(SpanOwner<int>.MayStackalloc(7000));
    }

    [Fact]
    public void ShouldRent_ReturnsTrueForBigLengths()
    {
        Assert.True(SpanOwner<int>.ShouldRent(1024));
        Assert.True(SpanOwner<int>.ShouldRent(2048));
        Assert.True(SpanOwner<int>.ShouldRent(3000));
        Assert.True(SpanOwner<int>.ShouldRent(4096));
        Assert.True(SpanOwner<int>.ShouldRent(5000));
        Assert.True(SpanOwner<int>.ShouldRent(6000));
        Assert.True(SpanOwner<int>.ShouldRent(7000));
    }

    [Fact]
    public void ShouldRent_ReturnsFalseForSmallLengths()
    {
        Assert.False(SpanOwner<int>.ShouldRent(0));
        Assert.False(SpanOwner<int>.ShouldRent(1));
        Assert.False(SpanOwner<int>.ShouldRent(2));
        Assert.False(SpanOwner<int>.ShouldRent(4));
        Assert.False(SpanOwner<int>.ShouldRent(8));
        Assert.False(SpanOwner<int>.ShouldRent(10));
        Assert.False(SpanOwner<int>.ShouldRent(32));
    }
}
