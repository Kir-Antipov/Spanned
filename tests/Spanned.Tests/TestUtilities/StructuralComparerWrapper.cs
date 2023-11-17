using System.Collections;

namespace Spanned.Tests.TestUtilities;

public sealed class StructuralComparerWrapper_Int : IEqualityComparer<int>, IComparer<int>
{
    public int Compare(int x, int y) => StructuralComparisons.StructuralComparer.Compare(x, y);

    public bool Equals(int x, int y) => StructuralComparisons.StructuralEqualityComparer.Equals(x, y);

    public int GetHashCode(int obj) => StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
}

public sealed class StructuralComparerWrapper_SimpleInt : IEqualityComparer<SimpleInt>, IComparer<SimpleInt>
{
    public int Compare(SimpleInt x, SimpleInt y) => StructuralComparisons.StructuralComparer.Compare(x, y);

    public bool Equals(SimpleInt x, SimpleInt y) => StructuralComparisons.StructuralEqualityComparer.Equals(x, y);

    public int GetHashCode(SimpleInt obj) => StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
}
