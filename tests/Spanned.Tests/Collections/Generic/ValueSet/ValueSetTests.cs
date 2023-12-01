using System.Collections;
using Spanned.Tests.TestUtilities;

namespace Spanned.Tests.Collections.Generic.ValueSet;

public class ValueSetTests_String : ValueSetTests<string>
{
    protected override string CreateT(int seed)
    {
        int stringLength = seed % 10 + 5;
        Random rand = new(seed);
        byte[] bytes = new byte[stringLength];
        rand.NextBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}

public class ValueSetTests_Int : ValueSetTests<int>
{
    protected override bool DefaultValueAllowed => true;

    protected override int CreateT(int seed) => new Random(seed).Next();
}

public class ValueSetTests_Int_With_StructuralComparerWrapper_Int : ValueSetTests<int>
{
    protected override IEqualityComparer<int> GetIEqualityComparer() => new StructuralComparerWrapper_Int();

    protected override IComparer<int> GetIComparer() => new StructuralComparerWrapper_Int();

    protected override int CreateT(int seed) => new Random(seed).Next();

    protected override ISet<int> GenericISetFactory() => new ValueSetWrapper<int>(new StructuralComparerWrapper_Int());
}

public class ValueSetTests_Int_With_StructuralComparerWrapper_SimpleInt : ValueSetTests<SimpleInt>
{
    protected override IEqualityComparer<SimpleInt> GetIEqualityComparer() => new StructuralComparerWrapper_SimpleInt();

    protected override IComparer<SimpleInt> GetIComparer() => new StructuralComparerWrapper_SimpleInt();

    protected override SimpleInt CreateT(int seed) => new(new Random(seed).Next());

    protected override ISet<SimpleInt> GenericISetFactory() => new ValueSetWrapper<SimpleInt>(new StructuralComparerWrapper_SimpleInt());
}

public class ValueSetTests_EquatableBackwardsOrder : ValueSetTests<EquatableBackwardsOrder>
{
    protected override EquatableBackwardsOrder CreateT(int seed) => new(new Random(seed).Next());

    protected override ISet<EquatableBackwardsOrder> GenericISetFactory() => new ValueSetWrapper<EquatableBackwardsOrder>();
}

public class ValueSetTests_Int_With_SameAsDefaultComparer : ValueSetTests<int>
{
    protected override IEqualityComparer<int> GetIEqualityComparer() => new SameAsDefaultComparer_Int();

    protected override int CreateT(int seed) => new Random(seed).Next();

    protected override ISet<int> GenericISetFactory() => new ValueSetWrapper<int>(new SameAsDefaultComparer_Int());
}

public class ValueSetTests_Int_With_HashCodeAlwaysReturnsZeroComparer : ValueSetTests<int>
{
    protected override IEqualityComparer<int> GetIEqualityComparer() => new HashCodeAlwaysReturnsZeroComparer_Int();

    protected override int CreateT(int seed) => new Random(seed).Next();

    protected override ISet<int> GenericISetFactory() => new ValueSetWrapper<int>(new HashCodeAlwaysReturnsZeroComparer_Int());
}

public class ValueSetTests_Int_With_ModOfIntComparer : ValueSetTests<int>
{
    protected override IEqualityComparer<int> GetIEqualityComparer() => new ModOfIntComparer(15000);

    protected override IComparer<int> GetIComparer() => new ModOfIntComparer(15000);

    protected override int CreateT(int seed) => new Random(seed).Next();

    protected override ISet<int> GenericISetFactory() => new ValueSetWrapper<int>(new ModOfIntComparer(15000));
}

public class ValueSetTests_Int_With_AbsOfIntComparer : ValueSetTests<int>
{
    protected override IEqualityComparer<int> GetIEqualityComparer() => new AbsOfIntComparer();

    protected override int CreateT(int seed) => new Random(seed).Next();

    protected override ISet<int> GenericISetFactory() => new ValueSetWrapper<int>(new AbsOfIntComparer());
}

public class ValueSetTests_Int_With_Comparer_BadIntEqualityComparer : ValueSetTests<int>
{
    protected override IEqualityComparer<int> GetIEqualityComparer() => new BadIntEqualityComparer();

    protected override int CreateT(int seed) => new Random(seed).Next();

    protected override ISet<int> GenericISetFactory() => new ValueSetWrapper<int>(new BadIntEqualityComparer());
}

/// <summary>
/// Contains tests that ensure the correctness of the ValueSetWrapper class.
/// </summary>
public abstract class ValueSetTests<T> : ISetTests<T>
{
    protected override ISet<T> GenericISetFactory() => new ValueSetWrapper<T>();

    private static IEnumerable<int> NonSquares(int limit)
    {
        for (int i = 0; i != limit; ++i)
        {
            int root = (int)Math.Sqrt(i);
            if (i != root * root)
                yield return i;
        }
    }

    [Fact]
    public void ValueSet_Constructor()
    {
        ValueSetWrapper<T> set = new();
        Assert.Empty(set);
    }

    [Fact]
    public void ValueSet_Constructor_IEqualityComparer()
    {
        IEqualityComparer<T> comparer = GetIEqualityComparer();
        ValueSetWrapper<T> set = new(comparer);
        if (comparer == null)
            Assert.Equal(EqualityComparer<T>.Default, set.Comparer);
        else
            Assert.Equal(comparer, set.Comparer);
    }

    [Fact]
    public void ValueSet_Constructor_NullIEqualityComparer()
    {
        ValueSetWrapper<T> set = new(comparer: null);
        Assert.Equal(EqualityComparer<T>.Default, set.Comparer);
    }

    [Theory]
    [MemberData(nameof(EnumerableTestData))]
    public void ValueSet_Constructor_IEnumerable(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        _ = setLength;
        _ = numberOfMatchingElements;
        IEnumerable<T> enumerable = CreateEnumerable(enumerableType, null, enumerableLength, 0, numberOfDuplicateElements);
        ValueSetWrapper<T> set = new(enumerable);
        Assert.True(set.SetEquals(enumerable));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_Constructor_IEnumerable_WithManyDuplicates(int count)
    {
        IEnumerable<T> items = CreateEnumerable(EnumerableType.List, null, count, 0, 0);
        ValueSetWrapper<T> setFromDuplicates = new ValueSetWrapper<T>(Enumerable.Range(0, 40).SelectMany(i => items).ToArray());
        ValueSetWrapper<T> setFromNoDuplicates = new ValueSetWrapper<T>(items);
        Assert.True(setFromNoDuplicates.SetEquals(setFromDuplicates));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_Constructor_ValueSet_SparselyFilled(int count)
    {
        HashSet<T> source = (HashSet<T>)CreateEnumerable(EnumerableType.HashSet, null, count, 0, 0);
        List<T> sourceElements = source.ToList();
        foreach (int i in NonSquares(count))
            source.Remove(sourceElements[i]);// Unevenly spaced survivors increases chance of catching any spacing-related bugs.


        ValueSetWrapper<T> set = new(source, GetIEqualityComparer());
        Assert.True(set.SetEquals(source));
    }

    [Fact]
    public void ValueSet_Constructor_IEnumerable_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new ValueSetWrapper<T>((IEnumerable<T>)null!));
        Assert.Throws<ArgumentNullException>(() => new ValueSetWrapper<T>((IEnumerable<T>)null!, EqualityComparer<T>.Default));
    }

    [Theory]
    [MemberData(nameof(EnumerableTestData))]
    public void ValueSet_Constructor_IEnumerable_IEqualityComparer(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        _ = setLength;
        _ = numberOfMatchingElements;
        _ = numberOfDuplicateElements;
        IEnumerable<T> enumerable = CreateEnumerable(enumerableType, null, enumerableLength, 0, 0);
        ValueSetWrapper<T> set = new(enumerable, GetIEqualityComparer());
        Assert.True(set.SetEquals(enumerable));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void ValueSet_CreateWithCapacity_CapacityAtLeastPassedValue(int capacity)
    {
        ValueSetWrapper<T> set = new(capacity);
        Assert.True(set.Capacity >= capacity);
    }

    [Fact]
    public void ValueSet_Resized_CapacityChanged()
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(3);
        int initialCapacity = set.Capacity;

        int seed = 85877;
        for (int i = initialCapacity - set.Count; i >= 0; i--)
            set.Add(CreateT(seed++));

        Assert.True(set.Capacity > initialCapacity);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_RemoveWhere_AllElements(int setLength)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
        int removedCount = set.RemoveWhere(value => true);
        Assert.Equal(setLength, removedCount);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_RemoveWhere_NoElements(int setLength)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
        int removedCount = set.RemoveWhere(value => false);
        Assert.Equal(0, removedCount);
        Assert.Equal(setLength, set.Count);
    }

    [Fact]
    public void ValueSet_RemoveWhere_NewObject()
    {
        object[] array = new object[2];
        object obj = new();
        ValueSetWrapper<object> set = new();

        set.Add(obj);
        set.Remove(obj);
        foreach (object o in set)
            ;

        set.CopyTo(array, 0, 2);
        set.RemoveWhere(element => false);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_RemoveWhere_NullMatchPredicate(int setLength)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
        Assert.Throws<ArgumentNullException>(() => set.RemoveWhere(null!));
    }

    [Theory]
    [InlineData(1, -1)]
    [InlineData(2, 1)]
    public void ValueSet_TrimAccessWithInvalidArg_ThrowOutOfRange(int size, int newCapacity)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(size);

        Assert.Throws<ArgumentOutOfRangeException>(() => set.TrimExcess(newCapacity));
    }

    [Fact]
    public void TrimExcess_LargeInitialCapacity_TrimReducesSize()
    {
        ValueSetWrapper<T> set = new(100);
        int initialCapacity = set.Capacity;

        set.TrimExcess(1);

        Assert.True(set.Capacity < initialCapacity);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_TrimExcess_OnValidSetThatHasntBeenRemovedFrom(int setLength)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
        set.TrimExcess();
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_TrimExcess_Repeatedly(int setLength)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
        List<T> expected = set.ToList();
        set.TrimExcess();
        set.TrimExcess();
        set.TrimExcess();
        Assert.True(set.SetEquals(expected));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_TrimExcess_AfterRemovingOneElement(int setLength)
    {
        if (setLength > 0)
        {
            ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
            List<T> expected = set.ToList();
            T elementToRemove = set.ElementAt(0);

            set.TrimExcess();
            Assert.True(set.Remove(elementToRemove));
            expected.Remove(elementToRemove);
            set.TrimExcess();

            Assert.True(set.SetEquals(expected));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_TrimExcess_AfterClearingAndAddingSomeElementsBack(int setLength)
    {
        if (setLength > 0)
        {
            ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
            set.TrimExcess();
            set.Clear();
            set.TrimExcess();
            Assert.Empty(set);

            AddToCollection(set, setLength / 10);
            set.TrimExcess();
            Assert.Equal(setLength / 10, set.Count);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_TrimExcess_AfterClearingAndAddingAllElementsBack(int setLength)
    {
        if (setLength > 0)
        {
            ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
            set.TrimExcess();
            set.Clear();
            set.TrimExcess();
            Assert.Empty(set);

            AddToCollection(set, setLength);
            set.TrimExcess();
            Assert.Equal(setLength, set.Count);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_CopyTo_NegativeCount_ThrowsArgumentOutOfRangeException(int count)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(count);
        T[] arr = new T[count];
        Assert.Throws<ArgumentOutOfRangeException>(() => set.CopyTo(arr, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => set.CopyTo(arr, 0, int.MinValue));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_CopyTo_NoIndexDefaultsToZero(int count)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(count);
        T[] arr1 = new T[count];
        T[] arr2 = new T[count];
        set.CopyTo(arr1);
        set.CopyTo(arr2, 0);
        Assert.True(arr1.SequenceEqual(arr2));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_Constructor_int(int capacity)
    {
        ValueSetWrapper<T> set = new(capacity);
        Assert.Empty(set);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_Constructor_Int_AddUpToAndBeyondCapacity(int capacity)
    {
        ValueSetWrapper<T> set = new(capacity);

        AddToCollection(set, capacity);
        Assert.Equal(capacity, set.Count);

        AddToCollection(set, capacity + 1);
        Assert.Equal(capacity + 1, set.Count);
    }

    [Fact]
    public void ValueSet_Constructor_Int_Negative_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueSetWrapper<T>(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueSetWrapper<T>(int.MinValue));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_Constructor_Int_IEqualityComparer(int capacity)
    {
        IEqualityComparer<T> comparer = GetIEqualityComparer();
        ValueSetWrapper<T> set = new(capacity, comparer);
        Assert.Empty(set);
        if (comparer == null)
            Assert.Equal(EqualityComparer<T>.Default, set.Comparer);
        else
            Assert.Equal(comparer, set.Comparer);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueSet_Constructor_Int_IEqualityComparer_AddUpToAndBeyondCapacity(int capacity)
    {
        IEqualityComparer<T> comparer = GetIEqualityComparer();
        ValueSetWrapper<T> set = new(capacity, comparer);

        AddToCollection(set, capacity);
        Assert.Equal(capacity, set.Count);

        AddToCollection(set, capacity + 1);
        Assert.Equal(capacity + 1, set.Count);
    }

    [Fact]
    public void ValueSet_Constructor_Int_IEqualityComparer_Negative_ThrowsArgumentOutOfRangeException()
    {
        IEqualityComparer<T> comparer = GetIEqualityComparer();
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueSetWrapper<T>(-1, comparer));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueSetWrapper<T>(int.MinValue, comparer));
    }

    [Fact]
    public void ValueSet_TryGetValue_Contains()
    {
        T value = CreateT(1);
        ValueSetWrapper<T> set = new() { value };
        T equalValue = CreateT(1);
        Assert.True(set.TryGetValue(equalValue, out T? actualValue));
        Assert.Equal(value, actualValue);
        if (!typeof(T).IsValueType)
        {
#pragma warning disable xUnit2005 // Do not use Assert.Same() on value type 'T'. Value types do not have identity. Use Assert.Equal instead.
            Assert.Same(value, actualValue);
#pragma warning restore xUnit2005
        }
    }

    [Fact]
    public void ValueSet_TryGetValue_Contains_OverwriteOutputParam()
    {
        T value = CreateT(1);
        ValueSetWrapper<T> set = new() { value };
        T equalValue = CreateT(1);
        Assert.True(set.TryGetValue(equalValue, out T? actualValue));
        Assert.Equal(value, actualValue);
        if (!typeof(T).IsValueType)
        {
#pragma warning disable xUnit2005 // Do not use Assert.Same() on value type 'T'. Value types do not have identity. Use Assert.Equal instead.
            Assert.Same(value, actualValue);
#pragma warning restore xUnit2005
        }
    }

    [Fact]
    public void ValueSet_TryGetValue_NotContains()
    {
        T value = CreateT(1);
        ValueSetWrapper<T> set = new() { value };
        T equalValue = CreateT(2);
        Assert.False(set.TryGetValue(equalValue, out T? actualValue));
        Assert.Equal(default, actualValue);
    }

    [Fact]
    public void ValueSet_TryGetValue_NotContains_OverwriteOutputParam()
    {
        T value = CreateT(1);
        ValueSetWrapper<T> set = new() { value };
        T equalValue = CreateT(2);
        Assert.False(set.TryGetValue(equalValue, out T? actualValue));
        Assert.Equal(default, actualValue);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void EnsureCapacity_RequestingLargerCapacity_DoesNotInvalidateEnumeration(int setLength)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
        int capacity = set.EnsureCapacity(0);
        IEnumerator valuesEnum = set.GetEnumerator();
        IEnumerator valuesListEnum = new List<T>(set).GetEnumerator();

        set.EnsureCapacity(capacity + 1); // Verify EnsureCapacity does not invalidate enumeration

        while (valuesEnum.MoveNext())
        {
            valuesListEnum.MoveNext();
            Assert.Equal(valuesListEnum.Current, valuesEnum.Current);
        }
    }

    [Fact]
    public void EnsureCapacity_NegativeCapacityRequested_Throws()
    {
        ValueSetWrapper<T> set = new();
        Assert.Throws<ArgumentOutOfRangeException>(() => set.EnsureCapacity(-1));
    }

    [Fact]
    public void EnsureCapacity_ValueSetNotInitialized_RequestedZero_ReturnsZero()
    {
        ValueSetWrapper<T> set = new();
        Assert.Equal(0, set.EnsureCapacity(0));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void EnsureCapacity_ValueSetNotInitialized_RequestedNonZero_CapacityIsSetToAtLeastTheRequested(int requestedCapacity)
    {
        ValueSetWrapper<T> set = new();
        Assert.InRange(set.EnsureCapacity(requestedCapacity), requestedCapacity, int.MaxValue);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(7)]
    public void EnsureCapacity_RequestedCapacitySmallerThanCurrent_CapacityUnchanged(int currentCapacity)
    {
        ValueSetWrapper<T> set = new(currentCapacity);
        int initialCapacity = set.Capacity;

        // assert capacity remains the same when ensuring a capacity smaller or equal than existing
        for (int i = 0; i <= currentCapacity; i++)
        {
            Assert.Equal(initialCapacity, set.EnsureCapacity(i));
        }
    }

    [Theory]
    [InlineData(7)]
    [InlineData(89)]
    public void EnsureCapacity_ExistingCapacityRequested_SameValueReturned(int capacity)
    {
        ValueSetWrapper<T> set = new(capacity);
        int initialCapacity = set.Capacity;
        Assert.Equal(initialCapacity, set.EnsureCapacity(capacity));

        set = (ValueSetWrapper<T>)GenericISetFactory(capacity);
        initialCapacity = set.Capacity;
        Assert.Equal(initialCapacity, set.EnsureCapacity(capacity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void EnsureCapacity_EnsureCapacityCalledTwice_ReturnsSameValue(int setLength)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
        int capacity = set.EnsureCapacity(0);
        Assert.Equal(capacity, set.EnsureCapacity(0));

        set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
        capacity = set.EnsureCapacity(setLength);
        Assert.Equal(capacity, set.EnsureCapacity(setLength));

        set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
        capacity = set.EnsureCapacity(setLength + 1);
        Assert.Equal(capacity, set.EnsureCapacity(setLength + 1));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(8)]
    public void EnsureCapacity_ValueSetNotEmpty_RequestedSmallerThanCount_ReturnsAtLeastSizeOfCount(int setLength)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);
        Assert.InRange(set.EnsureCapacity(setLength - 1), setLength, int.MaxValue);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(20)]
    public void EnsureCapacity_ValueSetNotEmpty_SetsToAtLeastTheRequested(int setLength)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);

        // get current capacity
        int currentCapacity = set.EnsureCapacity(0);

        // assert we can update to a larger capacity
        int newCapacity = set.EnsureCapacity(currentCapacity * 2);
        Assert.InRange(newCapacity, currentCapacity * 2, int.MaxValue);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    public void EnsureCapacity_GrowCapacityWithFreeList(int setLength)
    {
        ValueSetWrapper<T> set = (ValueSetWrapper<T>)GenericISetFactory(setLength);

        // Remove the first element to ensure we have a free list.
        Assert.True(set.Remove(set.ElementAt(0)));

        int currentCapacity = set.EnsureCapacity(0);
        Assert.True(currentCapacity > 0);

        int newCapacity = set.EnsureCapacity(currentCapacity + 1);
        Assert.True(newCapacity > currentCapacity);
    }

    [Theory]
    [MemberData(nameof(ValidPositiveCollectionSizes))]
    public void Remove_NonDefaultComparer_ComparerUsed(int capacity)
    {
        TrackingEqualityComparer<T> c = new();
        ValueSetWrapper<T> set = new(capacity, c);

        AddToCollection(set, capacity);
        T first = set.First();
        c.EqualsCalls = 0;
        c.GetHashCodeCalls = 0;

        Assert.Equal(capacity, set.Count);
        set.Remove(first);
        Assert.Equal(capacity - 1, set.Count);

        Assert.InRange(c.EqualsCalls, 1, int.MaxValue);
    }
}
