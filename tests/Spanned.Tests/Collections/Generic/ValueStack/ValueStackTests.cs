namespace Spanned.Tests.Collections.Generic.ValueStack;

public class ValueStackTests_String : ValueStackTests<string>
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

public class ValueStackTests_Int : ValueStackTests<int>
{
    protected override int CreateT(int seed) => new Random(seed).Next();
}

/// <summary>
/// Contains tests that ensure the correctness of the Stack class.
/// </summary>
public abstract class ValueStackTests<T> : IGenericSharedAPITests<T>
{
    protected ValueStackWrapper<T> GenericStackFactory()
    {
        return new ValueStackWrapper<T>();
    }

    protected ValueStackWrapper<T> GenericStackFactory(int count)
    {
        ValueStackWrapper<T> stack = new(count);
        int seed = count * 34;
        for (int i = 0; i < count; i++)
            stack.Push(CreateT(seed++));
        return stack;
    }

    protected override IEnumerable<T> GenericIEnumerableFactory()
    {
        return GenericStackFactory();
    }

    protected override IEnumerable<T> GenericIEnumerableFactory(int count)
    {
        return GenericStackFactory(count);
    }

    protected override int Count(IEnumerable<T> enumerable) => ((ValueStackWrapper<T>)enumerable).Count;
    protected override void Add(IEnumerable<T> enumerable, T value) => ((ValueStackWrapper<T>)enumerable).Push(value);
    protected override void Clear(IEnumerable<T> enumerable) => ((ValueStackWrapper<T>)enumerable).Clear();
    protected override bool Contains(IEnumerable<T> enumerable, T value) => ((ValueStackWrapper<T>)enumerable).Contains(value);
    protected override void CopyTo(IEnumerable<T> enumerable, T[] array, int index) => ((ValueStackWrapper<T>)enumerable).CopyTo(array, index);
    protected override bool Remove(IEnumerable<T> enumerable) { ((ValueStackWrapper<T>)enumerable).Pop(); return true; }

    [Fact]
    public void ValueStack_Constructor_InitialValues()
    {
        ValueStackWrapper<T> stack = new();
        Assert.Empty(stack);
        Assert.Empty(stack.ToArray());
    }

    [Theory]
    [MemberData(nameof(EnumerableTestData))]
    public void ValueStack_Constructor_IEnumerable(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        _ = setLength;
        _ = numberOfMatchingElements;
        IEnumerable<T> enumerable = CreateEnumerable(enumerableType, null, enumerableLength, 0, numberOfDuplicateElements);
        ValueStackWrapper<T> stack = new(enumerable);
        Assert.Equal(enumerable.ToArray().Reverse(), stack.ToArray());
    }

    [Fact]
    public void ValueStack_Constructor_IEnumerable_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ValueStackWrapper<T>(null!));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_Constructor_int(int count)
    {
        ValueStackWrapper<T> stack = new(count);
        Assert.Equal(Array.Empty<T>(), stack.ToArray());
    }

    [Fact]
    public void ValueStack_Constructor_int_Negative_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStackWrapper<T>(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueStackWrapper<T>(int.MinValue));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void ValueStack_CreateWithCapacity_EqualsCapacityProperty(int capacity)
    {
        var stack = new ValueStackWrapper<T>(capacity);
        Assert.True(stack.Capacity >= capacity);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_Pop_AllElements(int count)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(count);
        List<T> elements = stack.ToList();
        foreach (T element in elements)
            Assert.Equal(element, stack.Pop());
    }

    [Fact]
    public void ValueStack_Pop_OnEmptyValueStack_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new ValueStackWrapper<T>().Pop());
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_ToArray(int count)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(count);
        Assert.Equal(Enumerable.ToArray(stack), stack.ToArray());
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_Peek_AllElements(int count)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(count);
        List<T> elements = stack.ToList();
        foreach (T element in elements)
        {
            Assert.Equal(element, stack.Peek());
            stack.Pop();
        }
    }

    [Fact]
    public void ValueStack_Peek_OnEmptyValueStack_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new ValueStackWrapper<T>().Peek());
    }

    [Theory]
    [InlineData(1, -1)]
    [InlineData(2, 1)]
    public void ValueStack_TrimAccessWithInvalidArg_ThrowOutOfRange(int size, int newCapacity)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(size);

        Assert.Throws<ArgumentOutOfRangeException>(() => stack.TrimExcess(newCapacity));
    }

    [Fact]
    public void ValueStack_TrimAccessCurrentCount_DoesNothing()
    {
        ValueStackWrapper<T> stack = GenericStackFactory(10);
        stack.TrimExcess(stack.Count);
        int capacity = stack.Capacity;
        stack.TrimExcess(stack.Count);

        Assert.Equal(capacity, stack.Capacity);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_TrimExcess_OnValidStackThatHasntBeenRemovedFrom(int count)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(count);
        stack.TrimExcess();
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_TrimExcess_Repeatedly(int count)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(count);
        List<T> expected = stack.ToList();
        stack.TrimExcess();
        stack.TrimExcess();
        stack.TrimExcess();
        Assert.Equal(expected, stack);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_TrimExcess_AfterRemovingOneElement(int count)
    {
        if (count > 0)
        {
            ValueStackWrapper<T> stack = GenericStackFactory(count);
            List<T> expected = stack.ToList();
            _ = stack.ElementAt(0);

            stack.TrimExcess();
            stack.Pop();
            expected.RemoveAt(0);
            stack.TrimExcess();

            Assert.Equal(expected, stack);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_TrimExcess_AfterClearingAndAddingSomeElementsBack(int count)
    {
        if (count > 0)
        {
            ValueStackWrapper<T> stack = GenericStackFactory(count);
            stack.TrimExcess();
            stack.Clear();
            stack.TrimExcess();
            Assert.Empty(stack);

            AddToCollection(stack, count / 10);
            stack.TrimExcess();
            Assert.Equal(count / 10, stack.Count);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_TrimExcess_AfterClearingAndAddingAllElementsBack(int count)
    {
        if (count > 0)
        {
            ValueStackWrapper<T> stack = GenericStackFactory(count);
            stack.TrimExcess();
            stack.Clear();
            stack.TrimExcess();
            Assert.Empty(stack);

            AddToCollection(stack, count);
            stack.TrimExcess();
            Assert.Equal(count, stack.Count);
        }
    }

    [Fact]
    public void ValueStack_TrimExcess_DoesNotInvalidateEnumeration()
    {
        ValueStackWrapper<T> stack = GenericStackFactory(10);
        stack.EnsureCapacity(100);

        IEnumerator<T> enumerator = stack.GetEnumerator();
        stack.TrimExcess();
        enumerator.MoveNext();
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_TryPop_AllElements(int count)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(count);
        List<T> elements = stack.ToList();
        foreach (T element in elements)
        {
            Assert.True(stack.TryPop(out T? result));
            Assert.Equal(element, result);
        }
    }

    [Fact]
    public void ValueStack_TryPop_EmptyValueStack_ReturnsFalse()
    {
        Assert.False(new ValueStackWrapper<T>().TryPop(out T? result));
        Assert.Equal(default, result);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_TryPeek_AllElements(int count)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(count);
        List<T> elements = stack.ToList();
        foreach (T element in elements)
        {
            Assert.True(stack.TryPeek(out T? result));
            Assert.Equal(element, result);

            stack.Pop();
        }
    }

    [Fact]
    public void ValueStack_TryPeek_EmptyValueStack_ReturnsFalse()
    {
        Assert.False(new ValueStackWrapper<T>().TryPeek(out T? result));
        Assert.Equal(default, result);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_EnsureCapacity_RequestingLargerCapacity_DoesNotInvalidateEnumeration(int count)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(count);
        _ = new List<T>(stack).GetEnumerator();
        IEnumerator<T> enumerator = stack.GetEnumerator();

        stack.EnsureCapacity(count + 1);

        enumerator.MoveNext();
    }

    [Fact]
    public void ValueStack_EnsureCapacity_NotInitialized_RequestedZero_ReturnsZero()
    {
        ValueStackWrapper<T> stack = GenericStackFactory();
        Assert.Equal(0, stack.EnsureCapacity(0));
    }

    [Fact]
    public void ValueStack_EnsureCapacity_NegativeCapacityRequested_Throws()
    {
        ValueStackWrapper<T> stack = GenericStackFactory();
        Assert.Throws<ArgumentOutOfRangeException>(() => stack.EnsureCapacity(-1));
    }

    [Theory]
    [InlineData(5)]
    public void ValueStack_EnsureCapacity_RequestedCapacitySmallerThanOrEqualToCurrent_CapacityUnchanged(int currentCapacity)
    {
        ValueStackWrapper<T> stack = new(currentCapacity);
        int initialCapacity = stack.Capacity;

        for (int requestCapacity = 0; requestCapacity <= currentCapacity; requestCapacity++)
        {
            Assert.Equal(initialCapacity, stack.EnsureCapacity(requestCapacity));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_EnsureCapacity_RequestedCapacitySmallerThanOrEqualToCount_CapacityUnchanged(int count)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(count);
        int initialCapacity = stack.Capacity;

        for (int requestCapacity = 0; requestCapacity <= count; requestCapacity++)
        {
            Assert.Equal(initialCapacity, stack.EnsureCapacity(requestCapacity));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void ValueStack_EnsureCapacity_CapacityIsAtLeastTheRequested(int count)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(count);

        int requestCapacity = count + 1;
        int newCapacity = stack.EnsureCapacity(requestCapacity);
        Assert.InRange(newCapacity, requestCapacity, int.MaxValue);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueStack_EnsureCapacity_RequestingLargerCapacity_DoesNotImpactStackContent(int count)
    {
        ValueStackWrapper<T> stack = GenericStackFactory(count);
        var copiedList = new List<T>(stack);

        stack.EnsureCapacity(count + 1);
        Assert.Equal(copiedList, stack);

        for (int i = 0; i < count; i++)
        {
            Assert.Equal(copiedList[i], stack.Pop());
        }
    }

    [Fact]
    public void StackResized_CapacityUpdates()
    {
        ValueStackWrapper<T> stack = GenericStackFactory(10);
        int initialCapacity = stack.Capacity;

        for (int i = initialCapacity - stack.Count; i >= 0; i--)
            stack.Push(CreateT(i));

        Assert.True(initialCapacity < stack.Capacity);
    }
}
