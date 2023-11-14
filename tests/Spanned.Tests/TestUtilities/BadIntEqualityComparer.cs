namespace Spanned.Tests.TestUtilities;

public sealed class BadIntEqualityComparer : IEqualityComparer<int>
{
    public bool Equals(int x, int y) => x == y;

    public int GetHashCode(int obj) => obj % 2;

    public override bool Equals(object? obj) => obj is BadIntEqualityComparer;

    public override int GetHashCode() => unchecked((int)0xC001CAFE);
}
