using System.Collections;
using System.Diagnostics;

namespace Spanned.Tests.Collections.Generic;

/// <summary>
/// Contains tests that ensure the correctness of any class that implements the generic
/// IEnumerable interface.
/// </summary>
public abstract class IEnumerableTests<T>
{
    /// <summary>
    /// Creates an instance of an IEnumerable{T} that can be used for testing.
    /// </summary>
    /// <param name="count">The number of unique items that the returned IEnumerable{T} contains.</param>
    /// <returns>An instance of an IEnumerable{T} that can be used for testing.</returns>
    protected abstract IEnumerable<T> GenericIEnumerableFactory(int count);

    /// <summary>
    /// Modifies the given IEnumerable such that any enumerators for that IEnumerable will be
    /// invalidated.
    /// </summary>
    /// <param name="enumerable">An IEnumerable to modify</param>
    /// <returns>true if the enumerable was successfully modified. Else false.</returns>
    protected delegate bool ModifyEnumerable(IEnumerable<T> enumerable);

    /// <summary>
    /// To be implemented in the concrete collections test classes. Returns a set of ModifyEnumerable delegates
    /// that modify the enumerable passed to them.
    /// </summary>
    protected abstract IEnumerable<ModifyEnumerable> GetModifyEnumerables(ModifyOperation operations);

    protected virtual ModifyOperation ModifyEnumeratorThrows => ModifyOperation.None;

    protected virtual ModifyOperation ModifyEnumeratorAllowed => ~ModifyOperation.None;

    /// <summary>
    /// When calling Current of the enumerator before the first MoveNext, after the end of the collection,
    /// or after modification of the enumeration, the resulting behavior is undefined. Tests are included
    /// to cover two behavioral scenarios:
    ///   - Throwing an InvalidOperationException
    ///   - Returning an undefined value.
    ///
    /// If this property is set to true, the tests ensure that the exception is thrown. The default value is
    /// false.
    /// </summary>
    protected virtual bool Enumerator_Current_UndefinedOperation_Throws => false;

    /// <summary>
    /// When calling Current of the empty enumerator before the first MoveNext, after the end of the collection,
    /// or after modification of the enumeration, the resulting behavior is undefined. Tests are included
    /// to cover two behavioral scenarios:
    ///   - Throwing an InvalidOperationException
    ///   - Returning an undefined value.
    ///
    /// If this property is set to true, the tests ensure that the exception is thrown. The default value is
    /// <see cref="Enumerator_Current_UndefinedOperation_Throws"/>.
    /// </summary>
    protected virtual bool Enumerator_Empty_Current_UndefinedOperation_Throws => Enumerator_Current_UndefinedOperation_Throws;

    /// <summary>
    /// When calling MoveNext or Reset after modification of the enumeration, the resulting behavior is
    /// undefined. Tests are included to cover two behavioral scenarios:
    ///   - Throwing an InvalidOperationException
    ///   - Execute MoveNext or Reset.
    ///
    /// If this property is set to true, the tests ensure that the exception is thrown. The default value is
    /// true.
    /// </summary>
    protected virtual bool Enumerator_ModifiedDuringEnumeration_ThrowsInvalidOperationException => false;

    /// <summary>
    /// When calling MoveNext or Reset after modification of an empty enumeration, the resulting behavior is
    /// undefined. Tests are included to cover two behavioral scenarios:
    ///   - Throwing an InvalidOperationException
    ///   - Execute MoveNext or Reset.
    ///
    /// If this property is set to true, the tests ensure that the exception is thrown. The default value is
    /// <see cref="Enumerator_ModifiedDuringEnumeration_ThrowsInvalidOperationException"/>.
    /// </summary>
    protected virtual bool Enumerator_Empty_ModifiedDuringEnumeration_ThrowsInvalidOperationException => Enumerator_ModifiedDuringEnumeration_ThrowsInvalidOperationException;

    /// <summary>
    /// Specifies whether this IEnumerable follows some sort of ordering pattern.
    /// </summary>
    protected virtual EnumerableOrder Order => EnumerableOrder.Sequential;

    /// <summary>
    /// An enum to allow specification of the order of the Enumerable. Used in validation for enumerables.
    /// </summary>
    protected enum EnumerableOrder
    {
        Unspecified,
        Sequential
    }

    private void RepeatTest(
        Action<IEnumerator<T>, T[], int> testCode,
        int iters = 3)
    {
        IEnumerable<T> enumerable = GenericIEnumerableFactory(32);
        T[] items = enumerable.ToArray();
        IEnumerator<T> enumerator = enumerable.GetEnumerator();
        for (int i = 0; i < iters; i++)
        {
            testCode(enumerator, items, i);
            enumerator = enumerable.GetEnumerator();
        }
    }

    private void RepeatTest(
        Action<IEnumerator<T>, T[]> testCode,
        int iters = 3)
    {
        RepeatTest((e, i, it) => testCode(e, i), iters);
    }

    private void VerifyModifiedEnumerator(
        IEnumerator<T> enumerator,
        object expectedCurrent,
        bool expectCurrentThrow,
        bool atEnd)
    {
        if (expectCurrentThrow)
        {
            Assert.Throws<InvalidOperationException>(
                () => enumerator.Current);
        }
        else
        {
            object? current = enumerator.Current;
            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(expectedCurrent, current);
                current = enumerator.Current;
            }
        }

        Assert.Throws<InvalidOperationException>(
            () => enumerator.MoveNext());
    }

    private void VerifyEnumerator(
        IEnumerator<T> enumerator,
        T[] expectedItems)
    {
        VerifyEnumerator(
            enumerator,
            expectedItems,
            0,
            expectedItems.Length,
            true,
            true);
    }

    private void VerifyEnumerator(
        IEnumerator<T> enumerator,
        T[] expectedItems,
        int startIndex,
        int count,
        bool validateStart,
        bool validateEnd)
    {
        bool needToMatchAllExpectedItems = count - startIndex == expectedItems.Length;
        if (validateStart)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Enumerator_Current_UndefinedOperation_Throws)
                {
                    Assert.Throws<InvalidOperationException>(() => enumerator.Current);
                }
                else
                {
                    _ = enumerator.Current;
                }
            }
        }

        int iterations;
        if (Order == EnumerableOrder.Unspecified)
        {
            var itemsVisited =
                new BitArray(
                    needToMatchAllExpectedItems
                        ? count
                        : expectedItems.Length,
                    false);
            for (iterations = 0;
                 iterations < count && enumerator.MoveNext();
                 iterations++)
            {
                object? currentItem = enumerator.Current;
                bool itemFound = false;
                for (int i = 0; i < itemsVisited.Length; ++i)
                {
                    if (!itemsVisited[i]
                        && Equals(
                            currentItem,
                            expectedItems[
                                i
                                + (needToMatchAllExpectedItems
                                       ? startIndex
                                       : 0)]))
                    {
                        itemsVisited[i] = true;
                        itemFound = true;
                        break;
                    }
                }
                Assert.True(itemFound, "itemFound");

                for (int i = 0; i < 3; i++)
                {
                    object? tempItem = enumerator.Current;
                    Assert.Equal(currentItem, tempItem);
                }
            }
            if (needToMatchAllExpectedItems)
            {
                for (int i = 0; i < itemsVisited.Length; i++)
                {
                    Assert.True(itemsVisited[i]);
                }
            }
            else
            {
                int visitedItemCount = 0;
                for (int i = 0; i < itemsVisited.Length; i++)
                {
                    if (itemsVisited[i])
                    {
                        ++visitedItemCount;
                    }
                }
                Assert.Equal(count, visitedItemCount);
            }
        }
        else if (Order == EnumerableOrder.Sequential)
        {
            for (iterations = 0;
                 iterations < count && enumerator.MoveNext();
                 iterations++)
            {
                object? currentItem = enumerator.Current;
                Assert.Equal(expectedItems[iterations], currentItem);
                for (int i = 0; i < 3; i++)
                {
                    object? tempItem = enumerator.Current;
                    Assert.Equal(currentItem, tempItem);
                }
            }
        }
        else
        {
            throw new ArgumentException(
                "EnumerableOrder is invalid.");
        }
        Assert.Equal(count, iterations);

        if (validateEnd)
        {
            for (int i = 0; i < 3; i++)
            {
                Assert.False(enumerator.MoveNext(), "enumerator.MoveNext() returned true past the expected end.");

                if (Enumerator_Current_UndefinedOperation_Throws)
                {
                    Assert.Throws<InvalidOperationException>(() => enumerator.Current);
                }
                else
                {
                    _ = enumerator.Current;
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_GetEnumerator_NoExceptionsWhileGetting(int count)
    {
        IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
        enumerable.GetEnumerator().Dispose();
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_GetEnumerator_ReturnsUniqueEnumerator(int count)
    {
        //Tests that the enumerators returned by GetEnumerator operate independently of one another
        IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
        int iterations = 0;
        foreach (T item in enumerable)
            foreach (T item2 in enumerable)
                foreach (T item3 in enumerable)
                    iterations++;
        Assert.Equal(count * count * count, iterations);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_MoveNext_FromStartToFinish(int count)
    {
        int iterations = 0;
        using (IEnumerator<T> enumerator = GenericIEnumerableFactory(count).GetEnumerator())
        {
            while (enumerator.MoveNext())
                iterations++;
            Assert.Equal(count, iterations);
        }
    }

    /// <summary>
    /// For most collections, all calls to MoveNext after disposal of an enumerator will return false.
    /// Some collections (SortedList), however, treat a call to dispose as if it were a call to Reset. Since the docs
    /// specify neither of these as being strictly correct, we leave the method virtual.
    /// </summary>
    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public virtual void Enumerator_MoveNext_AfterDisposal(int count)
    {
        IEnumerator<T> enumerator = GenericIEnumerableFactory(count).GetEnumerator();
        for (int i = 0; i < count; i++)
            enumerator.MoveNext();
        enumerator.Dispose();
        Assert.False(enumerator.MoveNext());
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_MoveNext_AfterEndOfCollection(int count)
    {
        using (IEnumerator<T> enumerator = GenericIEnumerableFactory(count).GetEnumerator())
        {
            for (int i = 0; i < count; i++)
                enumerator.MoveNext();
            Assert.False(enumerator.MoveNext());
            Assert.False(enumerator.MoveNext());
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_MoveNext_ModifiedBeforeEnumeration_ThrowsInvalidOperationException(int count)
    {
        Assert.All(GetModifyEnumerables(ModifyEnumeratorThrows), ModifyEnumerable =>
        {
            IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
            using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
            {
                if (ModifyEnumerable(enumerable))
                {
                    if (count == 0 ? Enumerator_Empty_ModifiedDuringEnumeration_ThrowsInvalidOperationException : Enumerator_ModifiedDuringEnumeration_ThrowsInvalidOperationException)
                    {
                        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
                    }
                    else
                    {
                        enumerator.MoveNext();
                    }
                }
            }
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_MoveNext_ModifiedBeforeEnumeration_Succeeds(int count)
    {
        Assert.All(GetModifyEnumerables(ModifyEnumeratorAllowed), ModifyEnumerable =>
        {
            IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
            using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
            {
                if (ModifyEnumerable(enumerable))
                {
                    if (Enumerator_ModifiedDuringEnumeration_ThrowsInvalidOperationException)
                    {
                        enumerator.MoveNext();
                    }
                }
            }
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_MoveNext_ModifiedDuringEnumeration_ThrowsInvalidOperationException(int count)
    {
        Assert.All(GetModifyEnumerables(ModifyEnumeratorThrows), ModifyEnumerable =>
        {
            IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
            using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
            {
                for (int i = 0; i < count / 2; i++)
                    enumerator.MoveNext();
                if (ModifyEnumerable(enumerable))
                {
                    if (count == 0 ? Enumerator_Empty_ModifiedDuringEnumeration_ThrowsInvalidOperationException : Enumerator_ModifiedDuringEnumeration_ThrowsInvalidOperationException)
                    {
                        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
                    }
                    else
                    {
                        enumerator.MoveNext();
                    }
                }
            }
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_MoveNext_ModifiedDuringEnumeration_Succeeds(int count)
    {
        Assert.All(GetModifyEnumerables(ModifyEnumeratorAllowed), ModifyEnumerable =>
        {
            IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
            using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
            {
                for (int i = 0; i < count / 2; i++)
                    enumerator.MoveNext();
                if (ModifyEnumerable(enumerable))
                {
                    enumerator.MoveNext();
                }
            }
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_MoveNext_ModifiedAfterEnumeration_ThrowsInvalidOperationException(int count)
    {
        Assert.All(GetModifyEnumerables(ModifyEnumeratorThrows), ModifyEnumerable =>
        {
            IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
            using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    ;
                if (ModifyEnumerable(enumerable))
                {
                    if (count == 0 ? Enumerator_Empty_ModifiedDuringEnumeration_ThrowsInvalidOperationException : Enumerator_ModifiedDuringEnumeration_ThrowsInvalidOperationException)
                    {
                        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
                    }
                    else
                    {
                        enumerator.MoveNext();
                    }
                }
            }
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_MoveNext_ModifiedAfterEnumeration_Succeeds(int count)
    {
        Assert.All(GetModifyEnumerables(ModifyEnumeratorAllowed), ModifyEnumerable =>
        {
            IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
            using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                    ;
                if (ModifyEnumerable(enumerable))
                {
                    enumerator.MoveNext();
                }
            }
        });
    }

    [Fact]
    public void IEnumerable_Enumerator_MoveNextHitsAllItems()
    {
        RepeatTest(
            (enumerator, items) =>
            {
                int iterations = 0;
                while (enumerator.MoveNext())
                {
                    iterations++;
                }
                Assert.Equal(items.Length, iterations);
            });
    }

    [Fact]
    public void IEnumerable_Enumerator_MoveNextFalseAfterEndOfCollection()
    {
        RepeatTest(
            (enumerator, items) =>
            {
                while (enumerator.MoveNext())
                {
                }

                Assert.False(enumerator.MoveNext());
            });
    }

    [Fact]
    public void IEnumerable_Enumerator_Current()
    {
        // Verify that current returns proper result.
        RepeatTest(
            (enumerator, items, iteration) =>
            {
                if (iteration == 1)
                {
                    VerifyEnumerator(
                        enumerator,
                        items,
                        0,
                        items.Length / 2,
                        true,
                        false);
                }
                else
                {
                    VerifyEnumerator(enumerator, items);
                }
            });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_Current_ReturnsSameValueOnRepeatedCalls(int count)
    {
        using IEnumerator<T> enumerator = GenericIEnumerableFactory(count).GetEnumerator();
        while (enumerator.MoveNext())
        {
            T current = enumerator.Current;
            Assert.Equal(current, enumerator.Current);
            Assert.Equal(current, enumerator.Current);
            Assert.Equal(current, enumerator.Current);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_Current_ReturnsSameObjectsOnDifferentEnumerators(int count)
    {
        // Ensures that the elements returned from enumeration are exactly the same collection of
        // elements returned from a previous enumeration
        IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
#nullable disable
        Dictionary<T, int> firstValues = new(count);
        Dictionary<T, int> secondValues = new(count);
#nullable restore
        foreach (T item in enumerable)
            firstValues[item] = firstValues.ContainsKey(item) ? firstValues[item]++ : 1;
        foreach (T item in enumerable)
            secondValues[item] = secondValues.ContainsKey(item) ? secondValues[item]++ : 1;
        Assert.Equal(firstValues.Count, secondValues.Count);
        foreach (T key in firstValues.Keys)
            Assert.Equal(firstValues[key], secondValues[key]);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_Current_BeforeFirstMoveNext_UndefinedBehavior(int count)
    {
        T current;
        IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
        using IEnumerator<T> enumerator = enumerable.GetEnumerator();
        if (count == 0 ? Enumerator_Empty_Current_UndefinedOperation_Throws : Enumerator_Current_UndefinedOperation_Throws)
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        else
            current = enumerator.Current;
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_Current_AfterEndOfEnumerable_UndefinedBehavior(int count)
    {
        T current;
        IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
        using IEnumerator<T> enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext())
            ;
        if (count == 0 ? Enumerator_Empty_Current_UndefinedOperation_Throws : Enumerator_Current_UndefinedOperation_Throws)
            Assert.Throws<InvalidOperationException>(() => enumerator.Current);
        else
            current = enumerator.Current;
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_Current_ModifiedDuringEnumeration_UndefinedBehavior(int count)
    {
        Assert.All(GetModifyEnumerables(ModifyEnumeratorThrows), ModifyEnumerable =>
        {
            T current;
            IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
            using IEnumerator<T> enumerator = enumerable.GetEnumerator();
            if (ModifyEnumerable(enumerable))
            {
                if (count == 0 ? Enumerator_Empty_Current_UndefinedOperation_Throws : Enumerator_Current_UndefinedOperation_Throws)
                    Assert.Throws<InvalidOperationException>(() => enumerator.Current);
                else
                    current = enumerator.Current;
            }
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IEnumerable_Enumerator_Current_ModifiedDuringEnumeration_Succeeds(int count)
    {
        Assert.All(GetModifyEnumerables(ModifyEnumeratorAllowed), ModifyEnumerable =>
        {
            T current;
            IEnumerable<T> enumerable = GenericIEnumerableFactory(count);
            using IEnumerator<T> enumerator = enumerable.GetEnumerator();
            if (ModifyEnumerable(enumerable))
            {
                current = enumerator.Current;
            }
        });
    }

    /// <summary>
    /// To be implemented in the concrete collections test classes. Creates an instance of T that
    /// is dependent only on the seed passed as input and will return the same value on repeated
    /// calls with the same seed.
    /// </summary>
    protected abstract T CreateT(int seed);

    /// <summary>
    /// The EqualityComparer that can be used in the overriding class when creating test enumerables
    /// or test collections. Default if not overridden is the default comparator.
    /// </summary>
    protected virtual IEqualityComparer<T> GetIEqualityComparer() => EqualityComparer<T>.Default;

    /// <summary>
    /// The Comparer that can be used in the overriding class when creating test enumerables
    /// or test collections. Default if not overridden is the default comparator.
    protected virtual IComparer<T> GetIComparer() => Comparer<T>.Default;

    /// <summary>
    /// MemberData to be passed to tests that take an IEnumerable{T}. This method returns every permutation of
    /// EnumerableType to test on (e.g. HashSet, Queue), and size of set to test with (e.g. 0, 1, etc.).
    /// </summary>
    public static IEnumerable<object[]> EnumerableTestData() =>
        Enum.GetValues<EnumerableType>().SelectMany(GetEnumerableTestData);

    /// <summary>
    /// MemberData to be passed to tests that take an IEnumerable{T}. This method returns results for various
    /// sizes of sets to test with (e.g. 0, 1, etc.) but only for List.
    /// </summary>
    public static IEnumerable<object[]> ListTestData() =>
        GetEnumerableTestData(EnumerableType.List);

    protected static IEnumerable<object[]> GetEnumerableTestData(EnumerableType enumerableType)
    {
        foreach (object[] collectionSizeArray in ValidCollectionSizes())
        {
            int count = (int)collectionSizeArray[0];
            yield return new object[] { enumerableType, count, 0, 0, 0 };                       // Empty Enumerable
            yield return new object[] { enumerableType, count, count + 1, 0, 0 };               // Enumerable that is 1 larger

            if (count == 1)
            {
                yield return new object[] { enumerableType, count, count, 0, 0 };               // Enumerable of the same size
                yield return new object[] { enumerableType, count, count, count, 0 };           // Enumerable with all elements matching
                yield return new object[] { enumerableType, count, count + 1, count, 0 };       // Enumerable with all elements matching plus one extra
            }

            if (count > 1)
            {
                yield return new object[] { enumerableType, count, count, 0, 0 };               // Enumerable of the same size
                yield return new object[] { enumerableType, count, count - 1, 0, 0 };           // Enumerable that is 1 smaller
                yield return new object[] { enumerableType, count, count, 1, 0 };               // Enumerable of the same size with 1 matching element
                yield return new object[] { enumerableType, count, count + 1, 1, 0 };           // Enumerable that is 1 longer with 1 matching element
                yield return new object[] { enumerableType, count, count, count, 0 };           // Enumerable with all elements matching
                yield return new object[] { enumerableType, count, count + 1, count, 0 };       // Enumerable with all elements matching plus one extra
            }

            if (count >= 2)
            {
                yield return new object[] { enumerableType, count, count - 1, 1, 0 };           // Enumerable that is 1 smaller with 1 matching element
                yield return new object[] { enumerableType, count, count + 2, 2, 0 };           // Enumerable that is 2 longer with 2 matching element
                yield return new object[] { enumerableType, count, count - 1, count - 1, 0 };   // Enumerable with all elements matching minus one
                yield return new object[] { enumerableType, count, count, 2, 0 };               // Enumerable of the same size with 2 matching element
                if (enumerableType is EnumerableType.List or EnumerableType.Queue)
                    yield return new object[] { enumerableType, count, count, 0, 1 };           // Enumerable with 1 element duplicated
            }

            if (count >= 3)
            {
                yield return new object[] { enumerableType, count, count - 1, 2, 0 };           // Enumerable that is 1 smaller with 2 matching elements
            }
        }
    }

    /// <summary>
    /// Helper function to create an enumerable fulfilling the given specific parameters. The function will
    /// create an enumerable of the desired type using the Default constructor for that type and then add values
    /// to it until it is full. It will begin by adding the desired number of matching and duplicate elements,
    /// followed by random (deterministic) elements until the desired count is reached.
    /// </summary>
    protected IEnumerable<T> CreateEnumerable(EnumerableType type, IEnumerable<T>? enumerableToMatchTo, int count, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        Debug.Assert(count >= numberOfMatchingElements);
        Debug.Assert(count >= numberOfDuplicateElements);

        switch (type)
        {
            case EnumerableType.HashSet:
                Debug.Assert(numberOfDuplicateElements == 0, "Can not create a HashSet with duplicate elements - numberOfDuplicateElements must be zero");
                return CreateHashSet(enumerableToMatchTo, count, numberOfMatchingElements);
            case EnumerableType.List:
                return CreateList(enumerableToMatchTo, count, numberOfMatchingElements, numberOfDuplicateElements);
            case EnumerableType.SortedSet:
                Debug.Assert(numberOfDuplicateElements == 0, "Can not create a SortedSet with duplicate elements - numberOfDuplicateElements must be zero");
                return CreateSortedSet(enumerableToMatchTo, count, numberOfMatchingElements);
            case EnumerableType.Queue:
                return CreateQueue(enumerableToMatchTo, count, numberOfMatchingElements, numberOfDuplicateElements);
            case EnumerableType.Lazy:
                return CreateLazyEnumerable(enumerableToMatchTo, count, numberOfMatchingElements, numberOfDuplicateElements);
            default:
                Debug.Assert(false, "Check that the 'EnumerableType' Enum returns only types that are special-cased in the CreateEnumerable function within the ISet_Tests class");
                return null;
        }
    }

    /// <summary>
    /// Helper function to create a Queue fulfilling the given specific parameters. The function will
    /// create an Queue and then add values
    /// to it until it is full. It will begin by adding the desired number of matching,
    /// followed by random (deterministic) elements until the desired count is reached.
    /// </summary>
    protected IEnumerable<T> CreateQueue(IEnumerable<T>? enumerableToMatchTo, int count, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        Queue<T> queue = new(count);
        int seed = 528;
        int duplicateAdded = 0;
        List<T>? match = null;

        // Enqueue Matching elements
        if (enumerableToMatchTo != null)
        {
            match = enumerableToMatchTo.ToList();
            for (int i = 0; i < numberOfMatchingElements; i++)
            {
                queue.Enqueue(match[i]);
                while (duplicateAdded++ < numberOfDuplicateElements)
                    queue.Enqueue(match[i]);
            }
        }

        // Enqueue elements to reach the desired count
        while (queue.Count < count)
        {
            T toEnqueue = CreateT(seed++);
            while (queue.Contains(toEnqueue) || match != null && match.Contains(toEnqueue)) // Don't want any unexpectedly duplicate values
                toEnqueue = CreateT(seed++);
            queue.Enqueue(toEnqueue);
            while (duplicateAdded++ < numberOfDuplicateElements)
                queue.Enqueue(toEnqueue);
        }

        // Validate that the Enumerable fits the guidelines as expected
        Debug.Assert(queue.Count == count);
        if (match != null)
        {
            int actualMatchingCount = 0;
            foreach (T lookingFor in match)
                actualMatchingCount += queue.Contains(lookingFor) ? 1 : 0;
            Assert.Equal(numberOfMatchingElements, actualMatchingCount);
        }

        return queue;
    }

    /// <summary>
    /// Helper function to create an List fulfilling the given specific parameters. The function will
    /// create an List and then add values
    /// to it until it is full. It will begin by adding the desired number of matching,
    /// followed by random (deterministic) elements until the desired count is reached.
    /// </summary>
    protected IEnumerable<T> CreateList(IEnumerable<T>? enumerableToMatchTo, int count, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        List<T> list = new(count);
        int seed = 528;
        int duplicateAdded = 0;
        List<T>? match = null;

        // Add Matching elements
        if (enumerableToMatchTo != null)
        {
            match = enumerableToMatchTo.ToList();
            for (int i = 0; i < numberOfMatchingElements; i++)
            {
                list.Add(match[i]);
                while (duplicateAdded++ < numberOfDuplicateElements)
                    list.Add(match[i]);
            }
        }

        // Add elements to reach the desired count
        while (list.Count < count)
        {
            T toAdd = CreateT(seed++);
            while (list.Contains(toAdd) || (match != null && match.Contains(toAdd))) // Don't want any unexpectedly duplicate values
                toAdd = CreateT(seed++);
            list.Add(toAdd);
            while (duplicateAdded++ < numberOfDuplicateElements)
                list.Add(toAdd);
        }

        // Validate that the Enumerable fits the guidelines as expected
        Debug.Assert(list.Count == count);
        if (match != null)
        {
            int actualMatchingCount = 0;
            foreach (T lookingFor in match)
                actualMatchingCount += list.Contains(lookingFor) ? 1 : 0;
            Assert.Equal(numberOfMatchingElements, actualMatchingCount);
        }

        return list;
    }

    /// <summary>
    /// Helper function to create an HashSet fulfilling the given specific parameters. The function will
    /// create an HashSet using the Comparer constructor and then add values
    /// to it until it is full. It will begin by adding the desired number of matching,
    /// followed by random (deterministic) elements until the desired count is reached.
    /// </summary>
    protected IEnumerable<T> CreateHashSet(IEnumerable<T>? enumerableToMatchTo, int count, int numberOfMatchingElements)
    {
        HashSet<T> set = new(GetIEqualityComparer());
        int seed = 528;
        List<T>? match = null;

        // Add Matching elements
        if (enumerableToMatchTo != null)
        {
            match = enumerableToMatchTo.ToList();
            for (int i = 0; i < numberOfMatchingElements; i++)
                set.Add(match[i]);
        }

        // Add elements to reach the desired count
        while (set.Count < count)
        {
            T toAdd = CreateT(seed++);
            while (set.Contains(toAdd) || match != null && match.Contains(toAdd, GetIEqualityComparer())) // Don't want any unexpectedly duplicate values
                toAdd = CreateT(seed++);
            set.Add(toAdd);
        }

        // Validate that the Enumerable fits the guidelines as expected
        Debug.Assert(set.Count == count);
        if (match != null)
        {
            int actualMatchingCount = 0;
            foreach (T lookingFor in match)
                actualMatchingCount += set.Contains(lookingFor) ? 1 : 0;
            Assert.Equal(numberOfMatchingElements, actualMatchingCount);
        }

        return set;
    }

    /// <summary>
    /// Helper function to create an SortedSet fulfilling the given specific parameters. The function will
    /// create an SortedSet using the Comparer constructor and then add values
    /// to it until it is full. It will begin by adding the desired number of matching,
    /// followed by random (deterministic) elements until the desired count is reached.
    /// </summary>
    protected IEnumerable<T> CreateSortedSet(IEnumerable<T>? enumerableToMatchTo, int count, int numberOfMatchingElements)
    {
        SortedSet<T> set = new(GetIComparer());
        int seed = 528;
        List<T>? match = null;

        // Add Matching elements
        if (enumerableToMatchTo != null)
        {
            match = enumerableToMatchTo.ToList();
            for (int i = 0; i < numberOfMatchingElements; i++)
                set.Add(match[i]);
        }

        // Add elements to reach the desired count
        while (set.Count < count)
        {
            T toAdd = CreateT(seed++);
            while (set.Contains(toAdd) || match != null && match.Contains(toAdd, GetIEqualityComparer())) // Don't want any unexpectedly duplicate values
                toAdd = CreateT(seed++);
            set.Add(toAdd);
        }

        // Validate that the Enumerable fits the guidelines as expected
        Debug.Assert(set.Count == count);
        if (match != null)
        {
            int actualMatchingCount = 0;
            foreach (T lookingFor in match)
                actualMatchingCount += set.Contains(lookingFor) ? 1 : 0;
            Assert.Equal(numberOfMatchingElements, actualMatchingCount);
        }

        return set;
    }

    protected IEnumerable<T> CreateLazyEnumerable(IEnumerable<T>? enumerableToMatchTo, int count, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        IEnumerable<T> list = CreateList(enumerableToMatchTo, count, numberOfMatchingElements, numberOfDuplicateElements);
        return list.Select(item => item);
    }

    public static IEnumerable<object[]> ValidCollectionSizes()
    {
        yield return new object[] { 0 };
        yield return new object[] { 1 };
        yield return new object[] { 75 };
    }

    public static IEnumerable<object[]> ValidPositiveCollectionSizes()
    {
        yield return new object[] { 1 };
        yield return new object[] { 75 };
    }

    public enum EnumerableType
    {
        HashSet,
        SortedSet,
        List,
        Queue,
        Lazy,
    };

    [Flags]
    public enum ModifyOperation
    {
        None = 0,
        Add = 1,
        Insert = 2,
        Overwrite = 4,
        Remove = 8,
        Clear = 16
    }

    protected static void EqualUnordered(ICollection<T> expected, ICollection<T> actual)
    {
        Assert.Equal(expected == null, actual == null);
        if (expected == null || actual == null)
        {
            return;
        }

        // Lookups are an aggregated collections (enumerable contents), but ordered.
        ILookup<object, object> e = expected.Cast<object>().ToLookup(key => key);
        ILookup<object, object> a = actual.Cast<object>().ToLookup(key => key);

        // Dictionaries can't handle null keys, which is a possibility
        Assert.Equal(e.Where(kv => kv.Key != null).ToDictionary(g => g.Key, g => g.Count()), a.Where(kv => kv.Key != null).ToDictionary(g => g.Key, g => g.Count()));

        // Get count of null keys.  Returns an empty sequence (and thus a 0 count) if no null key
        Assert.Equal(e[null!].Count(), a[null!].Count());
    }
}
