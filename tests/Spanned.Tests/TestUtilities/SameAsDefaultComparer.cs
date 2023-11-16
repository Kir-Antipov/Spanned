namespace Spanned.Tests.TestUtilities;

public class SameAsDefaultComparer_Int : IEqualityComparer<int>, IComparer<int>
{
    public int Compare(int x, int y) => x - y;

    public bool Equals(int x, int y) => x == y;

    public int GetHashCode(int obj) => obj.GetHashCode();
}
