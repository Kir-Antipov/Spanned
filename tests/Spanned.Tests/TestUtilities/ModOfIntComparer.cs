namespace Spanned.Tests.TestUtilities;

public sealed class ModOfIntComparer : IEqualityComparer<int>, IComparer<int>
{
    private readonly int _mod;

    public ModOfIntComparer() => _mod = 500;

    public ModOfIntComparer(int mod) => _mod = mod;

    public int Compare(int x, int y) => x % _mod - y % _mod;

    public bool Equals(int x, int y) => x % _mod == y % _mod;

    public int GetHashCode(int x) => x % _mod;
}
