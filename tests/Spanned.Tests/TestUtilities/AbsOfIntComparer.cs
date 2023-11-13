namespace Spanned.Tests.TestUtilities;

public class AbsOfIntComparer : IEqualityComparer<int>, IComparer<int>
{
    public int Compare(int x, int y) => Math.Abs(x) - Math.Abs(y);

    public bool Equals(int x, int y) => Math.Abs(x) == Math.Abs(y);

    public int GetHashCode(int x) => Math.Abs(x);
}
