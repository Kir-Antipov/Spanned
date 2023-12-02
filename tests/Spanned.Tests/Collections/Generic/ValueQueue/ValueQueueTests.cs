namespace Spanned.Tests.Collections.Generic.ValueQueue;

public class ValueQueueTests_String : ValueQueueTests<string>
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

public class ValueQueueTests_Int : ValueQueueTests<int>
{
    protected override int CreateT(int seed) => new Random(seed).Next();
}

/// <summary>
/// Contains tests that ensure the correctness of the Queue class.
/// </summary>
public abstract class ValueQueueTests<T> : IGenericSharedAPITests<T>
{
    protected ValueQueueWrapper<T> GenericQueueFactory()
    {
        return new ValueQueueWrapper<T>();
    }

    protected ValueQueueWrapper<T> GenericQueueFactory(int count, int? capacity = null)
    {
        ValueQueueWrapper<T> queue = new(capacity ?? count);
        int seed = count * 34;
        for (int i = 0; i < count; i++)
            queue.Enqueue(CreateT(seed++));
        return queue;
    }

    protected override IEnumerable<T> GenericIEnumerableFactory()
    {
        return GenericQueueFactory();
    }

    protected override IEnumerable<T> GenericIEnumerableFactory(int count)
    {
        return GenericQueueFactory(count);
    }

    protected override int Count(IEnumerable<T> enumerable) => ((ValueQueueWrapper<T>)enumerable).Count;
    protected override void Add(IEnumerable<T> enumerable, T value) => ((ValueQueueWrapper<T>)enumerable).Enqueue(value);
    protected override void Clear(IEnumerable<T> enumerable) => ((ValueQueueWrapper<T>)enumerable).Clear();
    protected override bool Contains(IEnumerable<T> enumerable, T value) => ((ValueQueueWrapper<T>)enumerable).Contains(value);
    protected override void CopyTo(IEnumerable<T> enumerable, T[] array, int index) => ((ValueQueueWrapper<T>)enumerable).CopyTo(array, index);
    protected override bool Remove(IEnumerable<T> enumerable) => ((ValueQueueWrapper<T>)enumerable).TryDequeue(out _);

    [Theory]
    [MemberData(nameof(EnumerableTestData))]
    public void ValueQueue_Constructor_IEnumerable(EnumerableType enumerableType, int setLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        _ = setLength;
        _ = numberOfMatchingElements;
        IEnumerable<T> enumerable = CreateEnumerable(enumerableType, null, enumerableLength, 0, numberOfDuplicateElements);
        ValueQueueWrapper<T> queue = new(enumerable);
        Assert.Equal(enumerable, queue);
    }

    [Fact]
    public void ValueQueue_Constructor_IEnumerable_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ValueQueueWrapper<T>(null!));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_Constructor_int(int count)
    {
        ValueQueueWrapper<T> queue = new(count);
        Assert.Equal(Array.Empty<T>(), queue.ToArray());
        queue.Clear();
        Assert.Equal(Array.Empty<T>(), queue.ToArray());
    }

    [Fact]
    public void ValueQueue_Constructor_int_Negative_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueQueueWrapper<T>(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueQueueWrapper<T>(int.MinValue));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void ValueQueue_CreateWithCapacity_EqualsCapacityProperty(int capacity)
    {
        ValueQueueWrapper<T> queue = new(capacity);
        Assert.True(queue.Capacity >= capacity);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_Dequeue_AllElements(int count)
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(count);
        List<T> elements = queue.ToList();
        foreach (T element in elements)
            Assert.Equal(element, queue.Dequeue());
    }

    [Fact]
    public void ValueQueue_Dequeue_OnEmptyValueQueue_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new ValueQueueWrapper<T>().Dequeue());
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 1)]
    [InlineData(3, 100)]
    public void ValueQueue_EnqueueAndDequeue(int capacity, int items)
    {
        int seed = 53134;
        ValueQueueWrapper<T> q = new(capacity);
        Assert.Empty(q);

        // Enqueue some values and make sure the count is correct
        List<T> source = (List<T>)CreateEnumerable(EnumerableType.List, null, items, 0, 0);
        foreach (T val in source)
        {
            q.Enqueue(val);
        }
        Assert.Equal(source, q);

        // Dequeue to make sure the values are removed in the right order and the count is updated
        for (int i = 0; i < items; i++)
        {
            T itemToRemove = source[0];
            source.RemoveAt(0);
            Assert.Equal(itemToRemove, q.Dequeue());
            Assert.Equal(items - i - 1, q.Count);
        }

        // Can't dequeue when empty
        Assert.Throws<InvalidOperationException>(() => q.Dequeue());

        // But can still be used after a failure and after bouncing at empty
        T itemToAdd = CreateT(seed++);
        q.Enqueue(itemToAdd);
        Assert.Equal(itemToAdd, q.Dequeue());
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_ToArray(int count)
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(count);
        Assert.True(queue.ToArray().SequenceEqual(queue.ToArray<T>()));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_ToArray_NonWrappedQueue(int count)
    {
        ValueQueueWrapper<T> collection = new(count + 1);
        AddToCollection(collection, count);
        T[] elements = collection.ToArray();
        Assert.True(Enumerable.SequenceEqual(elements, collection.ToArray<T>()));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_Peek_AllElements(int count)
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(count);
        List<T> elements = queue.ToList();
        foreach (T element in elements)
        {
            Assert.Equal(element, queue.Peek());
            queue.Dequeue();
        }
    }

    [Fact]
    public void ValueQueue_Peek_OnEmptyValueQueue_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => new ValueQueueWrapper<T>().Peek());
    }

    [Theory]
    [InlineData(1, -1)]
    [InlineData(2, 1)]
    public void ValueQueue_TrimAccessWithInvalidArg_ThrowOutOfRange(int size, int newCapacity)
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(size);

        Assert.Throws<ArgumentOutOfRangeException>(() => queue.TrimExcess(newCapacity));
    }

    [Fact]
    public void ValueQueue_TrimAccessCurrentCount_ReducesToCount()
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(20, capacity: 1000);
        int initialCount = queue.Count;
        int initialCapacity = queue.Capacity;

        Assert.Equal(initialCount, queue.Count);
        Assert.Equal(initialCapacity, queue.Capacity);

        queue.TrimExcess(queue.Count);

        Assert.Equal(initialCount, queue.Count);
        Assert.True(queue.Capacity < initialCapacity);
        Assert.True(queue.Capacity >= initialCount);

        queue.TrimExcess(queue.Count);

        Assert.Equal(initialCount, queue.Count);
        Assert.True(queue.Capacity < initialCapacity);
        Assert.True(queue.Capacity >= initialCount);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_TrimExcess_OnValidQueueThatHasntBeenRemovedFrom(int count)
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(count);
        queue.TrimExcess();
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_TrimExcess_Repeatedly(int count)
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(count);
        List<T> expected = queue.ToList();
        queue.TrimExcess();
        queue.TrimExcess();
        queue.TrimExcess();
        Assert.True(queue.SequenceEqual(expected));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_TrimExcess_AfterRemovingOneElement(int count)
    {
        if (count > 0)
        {
            ValueQueueWrapper<T> queue = GenericQueueFactory(count);
            List<T> expected = queue.ToList();
            queue.TrimExcess();
            T removed = queue.Dequeue();
            expected.Remove(removed);
            queue.TrimExcess();

            Assert.True(queue.SequenceEqual(expected));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_TrimExcess_AfterClearingAndAddingSomeElementsBack(int count)
    {
        if (count > 0)
        {
            ValueQueueWrapper<T> queue = GenericQueueFactory(count);
            queue.TrimExcess();
            queue.Clear();
            queue.TrimExcess();
            Assert.Empty(queue);

            AddToCollection(queue, count / 10);
            queue.TrimExcess();
            Assert.Equal(count / 10, queue.Count);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_TrimExcess_AfterClearingAndAddingAllElementsBack(int count)
    {
        if (count > 0)
        {
            ValueQueueWrapper<T> queue = GenericQueueFactory(count);
            queue.TrimExcess();
            queue.Clear();
            queue.TrimExcess();
            Assert.Empty(queue);

            AddToCollection(queue, count);
            queue.TrimExcess();
            Assert.Equal(count, queue.Count);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_TryDequeue_AllElements(int count)
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(count);
        List<T> elements = queue.ToList();
        foreach (T element in elements)
        {
            Assert.True(queue.TryDequeue(out T? result));
            Assert.Equal(element, result);
        }
    }

    [Fact]
    public void ValueQueue_TryDequeue_EmptyValueQueue_ReturnsFalse()
    {
        Assert.False(new ValueQueueWrapper<T>().TryDequeue(out T? result));
        Assert.Equal(default, result);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_TryPeek_AllElements(int count)
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(count);
        List<T> elements = queue.ToList();
        foreach (T element in elements)
        {
            Assert.True(queue.TryPeek(out T? result));
            Assert.Equal(element, result);

            queue.Dequeue();
        }
    }

    [Fact]
    public void ValueQueue_TryPeek_EmptyValueQueue_ReturnsFalse()
    {
        Assert.False(new ValueQueueWrapper<T>().TryPeek(out T? result));
        Assert.Equal(default, result);
    }

    [Fact]
    public void ValueQueue_EnsureCapacity_NotInitialized_RequestedZero_ReturnsZero()
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory();
        Assert.Equal(0, queue.EnsureCapacity(0));
    }

    [Fact]
    public void ValueQueue_EnsureCapacity_NegativeCapacityRequested_Throws()
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory();
        Assert.Throws<ArgumentOutOfRangeException>(() => queue.EnsureCapacity(-1));
    }

    [Theory]
    [InlineData(5)]
    public void ValueQueue_EnsureCapacity_RequestedCapacitySmallerThanOrEqualToCurrent_CapacityUnchanged(int currentCapacity)
    {
        ValueQueueWrapper<T> queue = new(currentCapacity);
        int initialCapacity = queue.Capacity;

        for (int requestCapacity = 0; requestCapacity <= currentCapacity; requestCapacity++)
        {
            Assert.Equal(initialCapacity, queue.EnsureCapacity(requestCapacity));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_EnsureCapacity_RequestedCapacitySmallerThanOrEqualToCount_CapacityUnchanged(int count)
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(count);
        int initialCapacity = queue.Capacity;

        for (int requestCapacity = 0; requestCapacity <= count; requestCapacity++)
        {
            Assert.Equal(initialCapacity, queue.EnsureCapacity(requestCapacity));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void ValueQueue_EnsureCapacity_CapacityIsAtLeastTheRequested(int count)
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(count);

        int requestCapacity = count + 1;
        int newCapacity = queue.EnsureCapacity(requestCapacity);
        Assert.InRange(newCapacity, requestCapacity, int.MaxValue);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueQueue_EnsureCapacity_RequestingLargerCapacity_DoesNotImpactQueueContent(int count)
    {
        ValueQueueWrapper<T> queue = GenericQueueFactory(count);
        var copiedList = new List<T>(queue);

        queue.EnsureCapacity(count + 1);
        Assert.Equal(copiedList, queue);

        for (int i = 0; i < count; i++)
        {
            Assert.Equal(copiedList[i], queue.Dequeue());
        }
    }

    [Fact]
    public void QueueResized_CapacityUpdates()
    {
        ValueQueueWrapper<T> queue = new(10);
        int initialCapacity = queue.Capacity;

        for (int i = initialCapacity - queue.Count; i >= 0; i--)
            queue.Enqueue(CreateT(i));

        Assert.True(initialCapacity < queue.Capacity);
    }
}
