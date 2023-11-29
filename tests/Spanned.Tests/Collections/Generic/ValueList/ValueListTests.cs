using System.Collections;

namespace Spanned.Tests.Collections.Generic.ValueList;

#nullable disable

public class ValueListTests_String : ValueListTests<string>
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

public class ValueListTests_Int : ValueListTests<int>
{
    protected override int CreateT(int seed) => new Random(seed).Next();
}

/// <summary>
/// Contains tests that ensure the correctness of the ValueList class.
/// </summary>
public abstract partial class ValueListTests<T> : IListTests<T>
{
    protected override IList<T> GenericIListFactory()
    {
        return GenericListFactory();
    }

    protected override IList<T> GenericIListFactory(int count)
    {
        return GenericListFactory(count);
    }

    protected virtual ValueListWrapper<T> GenericListFactory()
    {
        return new ValueListWrapper<T>();
    }

    protected virtual ValueListWrapper<T> GenericListFactory(int count)
    {
        IEnumerable<T> toCreateFrom = CreateEnumerable(EnumerableType.List, null, count, 0, 0);
        return new ValueListWrapper<T>(toCreateFrom);
    }

    protected void VerifyList(ValueListWrapper<T> list, ValueListWrapper<T> expectedItems)
    {
        Assert.Equal(expectedItems.Count, list.Count);

        //Only verify the indexer. List should be in a good enough state that we
        //do not have to verify consistency with any other method.
        for (int i = 0; i < list.Count; ++i)
        {
            Assert.True(list[i] == null ? expectedItems[i] == null : list[i].Equals(expectedItems[i]));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void CopyTo_ArgumentValidity(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(0, Array.Empty<T>(), 0, count + 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(count, Array.Empty<T>(), 0, 1));
    }

    public static IEnumerable<object[]> ValidCollectionSizes_GreaterThanOne()
    {
        yield return new object[] { 2 };
        yield return new object[] { 20 };
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes_GreaterThanOne))]
    public void Sort_WithoutDuplicates(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        Comparer<T> comparer = Comparer<T>.Default;
        list.Sort();

        Assert.All(Enumerable.Range(0, count - 2), i => Assert.True(comparer.Compare(list[i], list[i + 1]) < 0));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes_GreaterThanOne))]
    public void Sort_WithDuplicates(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        Comparer<T> comparer = Comparer<T>.Default;
        list.Add(list[0]);
        list.Sort();

        Assert.All(Enumerable.Range(0, count - 2), i => Assert.True(comparer.Compare(list[i], list[i + 1]) <= 0));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes_GreaterThanOne))]
    public void Sort_IComparer_WithoutDuplicates(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        IComparer<T> comparer = GetIComparer();
        list.Sort(comparer);

        Assert.All(Enumerable.Range(0, count - 2), i => Assert.True(comparer.Compare(list[i], list[i + 1]) < 0));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes_GreaterThanOne))]
    public void Sort_IComparer_WithDuplicates(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        IComparer<T> comparer = GetIComparer();
        list.Add(list[0]);
        list.Sort(comparer);

        Assert.All(Enumerable.Range(0, count - 2), i => Assert.True(comparer.Compare(list[i], list[i + 1]) <= 0));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes_GreaterThanOne))]
    public void Sort_Comparison_WithoutDuplicates(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        IComparer<T> iComparer = GetIComparer();
        Comparison<T> comparer = iComparer.Compare;
        list.Sort(comparer);

        Assert.All(Enumerable.Range(0, count - 2), i => Assert.True(iComparer.Compare(list[i], list[i + 1]) < 0));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes_GreaterThanOne))]
    public void Sort_Comparison_WithDuplicates(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        IComparer<T> iComparer = GetIComparer();
        Comparison<T> comparer = iComparer.Compare;
        list.Add(list[0]);
        list.Sort(comparer);

        Assert.All(Enumerable.Range(0, count - 2), i => Assert.True(iComparer.Compare(list[i], list[i + 1]) <= 0));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes_GreaterThanOne))]
    public void Sort_Int_Int_IComparer_WithoutDuplicates(int count)
    {
        ValueListWrapper<T> unsortedList = GenericListFactory(count);
        IComparer<T> comparer = GetIComparer();
        for (int startIndex = 0; startIndex < count - 2; startIndex++)
            for (int sortCount = 1; sortCount < count - startIndex; sortCount++)
            {
                ValueListWrapper<T> list = new(unsortedList);
                list.Sort(startIndex, sortCount + 1, comparer);
                for (int i = startIndex; i < sortCount; i++)
                    Assert.InRange(comparer.Compare(list[i], list[i + 1]), int.MinValue, 0);
            }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes_GreaterThanOne))]
    public void Sort_Int_Int_IComparer_WithDuplicates(int count)
    {
        ValueListWrapper<T> unsortedList = GenericListFactory(count);
        IComparer<T> comparer = GetIComparer();
        unsortedList.Add(unsortedList[0]);
        for (int startIndex = 0; startIndex < count - 2; startIndex++)
            for (int sortCount = 2; sortCount < count - startIndex; sortCount++)
            {
                ValueListWrapper<T> list = new(unsortedList);
                list.Sort(startIndex, sortCount + 1, comparer);
                for (int i = startIndex; i < sortCount; i++)
                    Assert.InRange(comparer.Compare(list[i], list[i + 1]), int.MinValue, 1);
            }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Sort_Int_Int_IComparer_NegativeRange_ThrowsArgumentOutOfRangeException(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        Tuple<int, int>[] InvalidParameters = new Tuple<int, int>[]
        {
                Tuple.Create(-1,-1),
                Tuple.Create(-1, 0),
                Tuple.Create(-1, 1),
                Tuple.Create(-1, 2),
                Tuple.Create(-2, 0),
                Tuple.Create(int.MinValue, 0),
                Tuple.Create(0 ,-1),
                Tuple.Create(0 ,-2),
                Tuple.Create(0 , int.MinValue),
                Tuple.Create(1 ,-1),
                Tuple.Create(2 ,-1),
        };

        Assert.All(InvalidParameters, invalidSet => Assert.Throws<ArgumentOutOfRangeException>(() => list.Sort(invalidSet.Item1, invalidSet.Item2, GetIComparer())));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Sort_Int_Int_IComparer_InvalidRange_ThrowsArgumentException(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        Tuple<int, int>[] InvalidParameters = new Tuple<int, int>[]
        {
                Tuple.Create(count, 1),
                Tuple.Create(count + 1, 0),
                Tuple.Create(int.MaxValue, 0),
        };

        Assert.All(InvalidParameters, invalidSet => Assert.Throws<ArgumentOutOfRangeException>(() => list.Sort(invalidSet.Item1, invalidSet.Item2, GetIComparer())));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Reverse(int listLength)
    {
        ValueListWrapper<T> list = GenericListFactory(listLength);
        ValueListWrapper<T> listBefore = new(list);

        list.Reverse();

        for (int i = 0; i < listBefore.Count; i++)
        {
            Assert.Equal(list[i], listBefore[listBefore.Count - (i + 1)]); //"Expect them to be the same."
        }
    }

    [Theory]
    [InlineData(10, 0, 10)]
    [InlineData(10, 3, 3)]
    [InlineData(10, 10, 0)]
    [InlineData(10, 5, 5)]
    [InlineData(10, 0, 5)]
    [InlineData(10, 1, 9)]
    [InlineData(10, 9, 1)]
    [InlineData(10, 2, 8)]
    [InlineData(10, 8, 2)]
    public void Reverse_Int_int(int listLength, int index, int count)
    {
        ValueListWrapper<T> list = GenericListFactory(listLength);
        ValueListWrapper<T> listBefore = new(list);

        list.Reverse(index, count);

        for (int i = 0; i < index; i++)
        {
            Assert.Equal(list[i], listBefore[i]); //"Expect them to be the same."
        }

        int j = 0;
        for (int i = index; i < index + count; i++)
        {
            Assert.Equal(list[i], listBefore[index + count - (j + 1)]); //"Expect them to be the same."
            j++;
        }

        for (int i = index + count; i < listBefore.Count; i++)
        {
            Assert.Equal(list[i], listBefore[i]); //"Expect them to be the same."
        }
    }

    [Theory]
    [InlineData(10, 3, 3)]
    [InlineData(10, 0, 10)]
    [InlineData(10, 10, 0)]
    [InlineData(10, 5, 5)]
    [InlineData(10, 0, 5)]
    [InlineData(10, 1, 9)]
    [InlineData(10, 9, 1)]
    [InlineData(10, 2, 8)]
    [InlineData(10, 8, 2)]
    public void Reverse_RepeatedValues(int listLength, int index, int count)
    {
        ValueListWrapper<T> list = GenericListFactory(1);
        for (int i = 1; i < listLength; i++)
            list.Add(list[0]);
        ValueListWrapper<T> listBefore = new(list);

        list.Reverse(index, count);

        for (int i = 0; i < index; i++)
        {
            Assert.Equal(list[i], listBefore[i]); //"Expect them to be the same."
        }

        int j = 0;
        for (int i = index; i < index + count; i++)
        {
            Assert.Equal(list[i], listBefore[index + count - (j + 1)]); //"Expect them to be the same."
            j++;
        }

        for (int i = index + count; i < listBefore.Count; i++)
        {
            Assert.Equal(list[i], listBefore[i]); //"Expect them to be the same."
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Reverse_InvalidParameters(int listLength)
    {
        if (listLength % 2 != 0)
            listLength++;
        ValueListWrapper<T> list = GenericListFactory(listLength);
        Tuple<int, int>[] InvalidParameters = new Tuple<int, int>[]
        {
                Tuple.Create(listLength     ,1             ),
                Tuple.Create(listLength+1   ,0             ),
                Tuple.Create(listLength+1   ,1             ),
                Tuple.Create(listLength     ,2             ),
                Tuple.Create(listLength/2   ,listLength/2+1),
                Tuple.Create(listLength-1   ,2             ),
                Tuple.Create(listLength-2   ,3             ),
                Tuple.Create(1              ,listLength    ),
                Tuple.Create(0              ,listLength+1  ),
                Tuple.Create(1              ,listLength+1  ),
                Tuple.Create(2              ,listLength    ),
                Tuple.Create(listLength/2+1 ,listLength/2  ),
                Tuple.Create(2              ,listLength-1  ),
                Tuple.Create(3              ,listLength-2  ),
        };

        Assert.All(InvalidParameters, invalidSet =>
        {
            if (invalidSet.Item1 >= 0 && invalidSet.Item2 >= 0)
                Assert.Throws<ArgumentOutOfRangeException>(() => list.Reverse(invalidSet.Item1, invalidSet.Item2));
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Reverse_NegativeParameters(int listLength)
    {
        if (listLength % 2 != 0)
            listLength++;
        ValueListWrapper<T> list = GenericListFactory(listLength);
        Tuple<int, int>[] InvalidParameters = new Tuple<int, int>[]
        {
                Tuple.Create(-1,-1),
                Tuple.Create(-1, 0),
                Tuple.Create(-1, 1),
                Tuple.Create(-1, 2),
                Tuple.Create(0 ,-1),
                Tuple.Create(1 ,-1),
                Tuple.Create(2 ,-1),
        };

        Assert.All(InvalidParameters, invalidSet => Assert.Throws<ArgumentOutOfRangeException>(() => list.Reverse(invalidSet.Item1, invalidSet.Item2)));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void RemoveAll_AllElements(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int removedCount = list.RemoveAll((value) => { return true; });
        Assert.Equal(count, removedCount);
        Assert.Empty(list);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void RemoveAll_NoElements(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int removedCount = list.RemoveAll((value) => { return false; });
        Assert.Equal(0, removedCount);
        Assert.Equal(count, list.Count);
        VerifyList(list, beforeList);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void RemoveAll_DefaultElements(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        Predicate<T> EqualsDefaultElement = (value) => default(T) == null ? value == null : default(T).Equals(value);
        int expectedCount = beforeList.Count((value) => EqualsDefaultElement(value));
        int removedCount = list.RemoveAll(EqualsDefaultElement);
        Assert.Equal(expectedCount, removedCount);
    }

    [Fact]
    public void RemoveAll_NullMatchPredicate()
    {
        Assert.Throws<ArgumentNullException>(() => new ValueListWrapper<T>().RemoveAll(null));
    }

    [Theory]
    [InlineData(10, 3, 3)]
    [InlineData(10, 0, 10)]
    [InlineData(10, 10, 0)]
    [InlineData(10, 5, 5)]
    [InlineData(10, 0, 5)]
    [InlineData(10, 1, 9)]
    [InlineData(10, 9, 1)]
    [InlineData(10, 2, 8)]
    [InlineData(10, 8, 2)]
    public void Remove_Range(int listLength, int index, int count)
    {
        ValueListWrapper<T> list = GenericListFactory(listLength);
        ValueListWrapper<T> beforeList = new(list);

        list.RemoveRange(index, count);
        Assert.Equal(list.Count, listLength - count); //"Expected them to be the same."
        for (int i = 0; i < index; i++)
        {
            Assert.Equal(list[i], beforeList[i]); //"Expected them to be the same."
        }

        for (int i = index; i < count - (index + count); i++)
        {
            Assert.Equal(list[i], beforeList[i + count]); //"Expected them to be the same."
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void RemoveRange_InvalidParameters(int listLength)
    {
        if (listLength % 2 != 0)
            listLength++;
        ValueListWrapper<T> list = GenericListFactory(listLength);
        Tuple<int, int>[] InvalidParameters = new Tuple<int, int>[]
        {
                Tuple.Create(listLength     ,1             ),
                Tuple.Create(listLength+1   ,0             ),
                Tuple.Create(listLength+1   ,1             ),
                Tuple.Create(listLength     ,2             ),
                Tuple.Create(listLength/2   ,listLength/2+1),
                Tuple.Create(listLength-1   ,2             ),
                Tuple.Create(listLength-2   ,3             ),
                Tuple.Create(1              ,listLength    ),
                Tuple.Create(0              ,listLength+1  ),
                Tuple.Create(1              ,listLength+1  ),
                Tuple.Create(2              ,listLength    ),
                Tuple.Create(listLength/2+1 ,listLength/2  ),
                Tuple.Create(2              ,listLength-1  ),
                Tuple.Create(3              ,listLength-2  ),
        };

        Assert.All(InvalidParameters, invalidSet =>
        {
            if (invalidSet.Item1 >= 0 && invalidSet.Item2 >= 0)
                Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(invalidSet.Item1, invalidSet.Item2));
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void RemoveRange_NegativeParameters(int listLength)
    {
        if (listLength % 2 != 0)
            listLength++;
        ValueListWrapper<T> list = GenericListFactory(listLength);
        Tuple<int, int>[] InvalidParameters = new Tuple<int, int>[]
        {
                Tuple.Create(-1,-1),
                Tuple.Create(-1, 0),
                Tuple.Create(-1, 1),
                Tuple.Create(-1, 2),
                Tuple.Create(0 ,-1),
                Tuple.Create(1 ,-1),
                Tuple.Create(2 ,-1),
        };

        Assert.All(InvalidParameters, invalidSet => Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveRange(invalidSet.Item1, invalidSet.Item2)));
    }

    [Fact]
    public static void InsertTests()
    {
        Driver<int> IntDriver = new Driver<int>();
        int[] intArr1 = new int[100];
        for (int i = 0; i < 100; i++)
            intArr1[i] = i;

        int[] intArr2 = new int[100];
        for (int i = 0; i < 100; i++)
            intArr2[i] = i + 100;

        IntDriver.BasicInsert(new int[0], 1, 0, 3);
        IntDriver.BasicInsert(intArr1, 101, 50, 4);
        IntDriver.BasicInsert(intArr1, 100, 100, 5);
        IntDriver.BasicInsert(intArr1, 100, 99, 6);
        IntDriver.BasicInsert(intArr1, 50, 0, 7);
        IntDriver.BasicInsert(intArr1, 50, 1, 8);
        IntDriver.BasicInsert(intArr1, 100, 50, 50);

        Driver<string> StringDriver = new Driver<string>();
        string[] stringArr1 = new string[100];
        for (int i = 0; i < 100; i++)
            stringArr1[i] = "SomeTestString" + i.ToString();
        string[] stringArr2 = new string[100];
        for (int i = 0; i < 100; i++)
            stringArr2[i] = "SomeTestString" + (i + 100).ToString();

        StringDriver.BasicInsert(stringArr1, "strobia", 99, 2);
        StringDriver.BasicInsert(stringArr1, "strobia", 100, 3);
        StringDriver.BasicInsert(stringArr1, "strobia", 0, 4);
        StringDriver.BasicInsert(stringArr1, "strobia", 1, 5);
        StringDriver.BasicInsert(stringArr1, "strobia", 50, 51);
        StringDriver.BasicInsert(stringArr1, "strobia", 0, 100);
        StringDriver.BasicInsert(new string[] { null, null, null, "strobia", null }, null, 2, 3);
        StringDriver.BasicInsert(new string[] { null, null, null, null, null }, "strobia", 0, 5);
        StringDriver.BasicInsert(new string[] { null, null, null, null, null }, "strobia", 5, 1);
    }

    [Fact]
    public static void InsertTests_negative()
    {
        Driver<int> IntDriver = new Driver<int>();
        int[] intArr1 = new int[100];
        for (int i = 0; i < 100; i++)
            intArr1[i] = i;
        IntDriver.InsertValidations(intArr1);

        Driver<string> StringDriver = new Driver<string>();
        string[] stringArr1 = new string[100];
        for (int i = 0; i < 100; i++)
            stringArr1[i] = "SomeTestString" + i.ToString();
        StringDriver.InsertValidations(stringArr1);
    }

    [Fact]
    public static void InsertRangeTests()
    {
        Driver<int> IntDriver = new Driver<int>();
        int[] intArr1 = new int[100];
        for (int i = 0; i < 100; i++)
            intArr1[i] = i;

        int[] intArr2 = new int[10];
        for (int i = 0; i < 10; i++)
        {
            intArr2[i] = i + 100;
        }

        foreach (Func<int[], IEnumerable<int>> collectionGenerator in IntDriver.CollectionGenerators)
        {
            IntDriver.InsertRangeIEnumerable(new int[0], intArr1, 0, 1, collectionGenerator);
            IntDriver.InsertRangeIEnumerable(intArr1, intArr2, 0, 1, collectionGenerator);
            IntDriver.InsertRangeIEnumerable(intArr1, intArr2, 1, 1, collectionGenerator);
            IntDriver.InsertRangeIEnumerable(intArr1, intArr2, 99, 1, collectionGenerator);
            IntDriver.InsertRangeIEnumerable(intArr1, intArr2, 100, 1, collectionGenerator);
            IntDriver.InsertRangeIEnumerable(intArr1, intArr2, 50, 50, collectionGenerator);
        }

        Driver<string> StringDriver = new Driver<string>();
        string[] stringArr1 = new string[100];
        for (int i = 0; i < 100; i++)
            stringArr1[i] = "SomeTestString" + i.ToString();
        string[] stringArr2 = new string[10];
        for (int i = 0; i < 10; i++)
            stringArr2[i] = "SomeTestString" + (i + 100).ToString();

        foreach (Func<string[], IEnumerable<string>> collectionGenerator in StringDriver.CollectionGenerators)
        {
            StringDriver.InsertRangeIEnumerable(new string[0], stringArr1, 0, 1, collectionGenerator);
            StringDriver.InsertRangeIEnumerable(stringArr1, stringArr2, 0, 1, collectionGenerator);
            StringDriver.InsertRangeIEnumerable(stringArr1, stringArr2, 1, 1, collectionGenerator);
            StringDriver.InsertRangeIEnumerable(stringArr1, stringArr2, 99, 1, collectionGenerator);
            StringDriver.InsertRangeIEnumerable(stringArr1, stringArr2, 100, 1, collectionGenerator);
            StringDriver.InsertRangeIEnumerable(stringArr1, stringArr2, 50, 50, collectionGenerator);
            StringDriver.InsertRangeIEnumerable(new string[] { null, null, null, null }, stringArr2, 0, 1, collectionGenerator);
            StringDriver.InsertRangeIEnumerable(new string[] { null, null, null, null }, stringArr2, 4, 1, collectionGenerator);
            StringDriver.InsertRangeIEnumerable(new string[] { null, null, null, null }, new string[] { null, null, null, null }, 0, 1, collectionGenerator);
            StringDriver.InsertRangeIEnumerable(new string[] { null, null, null, null }, new string[] { null, null, null, null }, 4, 50, collectionGenerator);
        }
    }

    [Fact]
    public static void InsertRangeTests_Negative()
    {
        Driver<int> IntDriver = new Driver<int>();
        int[] intArr1 = new int[100];
        for (int i = 0; i < 100; i++)
            intArr1[i] = i;
        Driver<string> StringDriver = new Driver<string>();
        string[] stringArr1 = new string[100];
        for (int i = 0; i < 100; i++)
            stringArr1[i] = "SomeTestString" + i.ToString();

        IntDriver.InsertRangeValidations(intArr1, IntDriver.ConstructTestEnumerable);
        StringDriver.InsertRangeValidations(stringArr1, StringDriver.ConstructTestEnumerable);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public static void GetRangeTests(bool useSlice)
    {
        Driver<int> IntDriver = new Driver<int>();
        int[] intArr1 = new int[100];
        for (int i = 0; i < 100; i++)
            intArr1[i] = i;

        IntDriver.BasicGetRange(intArr1, 50, 50, useSlice);
        IntDriver.BasicGetRange(intArr1, 0, 50, useSlice);
        IntDriver.BasicGetRange(intArr1, 50, 25, useSlice);
        IntDriver.BasicGetRange(intArr1, 0, 25, useSlice);
        IntDriver.BasicGetRange(intArr1, 75, 25, useSlice);
        IntDriver.BasicGetRange(intArr1, 0, 100, useSlice);
        IntDriver.BasicGetRange(intArr1, 0, 99, useSlice);
        IntDriver.BasicGetRange(intArr1, 1, 1, useSlice);
        IntDriver.BasicGetRange(intArr1, 99, 1, useSlice);
        IntDriver.EnsureRangeIsReference(intArr1, 101, 0, 10, useSlice);
        IntDriver.EnsureThrowsAfterModification(intArr1, 10, 10, 10, useSlice);

        Driver<string> StringDriver = new Driver<string>();
        string[] stringArr1 = new string[100];
        for (int i = 0; i < 100; i++)
            stringArr1[i] = "SomeTestString" + i.ToString();

        StringDriver.BasicGetRange(stringArr1, 50, 50, useSlice);
        StringDriver.BasicGetRange(stringArr1, 0, 50, useSlice);
        StringDriver.BasicGetRange(stringArr1, 50, 25, useSlice);
        StringDriver.BasicGetRange(stringArr1, 0, 25, useSlice);
        StringDriver.BasicGetRange(stringArr1, 75, 25, useSlice);
        StringDriver.BasicGetRange(stringArr1, 0, 100, useSlice);
        StringDriver.BasicGetRange(stringArr1, 0, 99, useSlice);
        StringDriver.BasicGetRange(stringArr1, 1, 1, useSlice);
        StringDriver.BasicGetRange(stringArr1, 99, 1, useSlice);
        StringDriver.EnsureRangeIsReference(stringArr1, "SometestString101", 0, 10, useSlice);
        StringDriver.EnsureThrowsAfterModification(stringArr1, "str", 10, 10, useSlice);
    }

    [Fact]
    public static void SlicingWorks()
    {
        Driver<int> IntDriver = new Driver<int>();
        int[] intArr1 = new int[100];
        for (int i = 0; i < 100; i++)
            intArr1[i] = i;

        IntDriver.BasicSliceSyntax(intArr1, 50, 50);
        IntDriver.BasicSliceSyntax(intArr1, 0, 50);
        IntDriver.BasicSliceSyntax(intArr1, 50, 25);
        IntDriver.BasicSliceSyntax(intArr1, 0, 25);
        IntDriver.BasicSliceSyntax(intArr1, 75, 25);
        IntDriver.BasicSliceSyntax(intArr1, 0, 100);
        IntDriver.BasicSliceSyntax(intArr1, 0, 99);
        IntDriver.BasicSliceSyntax(intArr1, 1, 1);
        IntDriver.BasicSliceSyntax(intArr1, 99, 1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public static void GetRangeTests_Negative(bool useSlice)
    {
        Driver<int> IntDriver = new Driver<int>();
        int[] intArr1 = new int[100];
        for (int i = 0; i < 100; i++)
            intArr1[i] = i;

        Driver<string> StringDriver = new Driver<string>();
        string[] stringArr1 = new string[100];
        for (int i = 0; i < 100; i++)
            stringArr1[i] = "SomeTestString" + i.ToString();

        StringDriver.GetRangeValidations(stringArr1, useSlice);
        IntDriver.GetRangeValidations(intArr1, useSlice);
    }

    [Fact]
    public static void ExistsTests()
    {
        Driver<int> intDriver = new Driver<int>();
        Driver<string> stringDriver = new Driver<string>();
        int[] intArray;
        string[] stringArray;
        int arraySize = 16;

        intArray = new int[arraySize];
        stringArray = new string[arraySize];

        for (int i = 0; i < arraySize; ++i)
        {
            intArray[i] = i + 1;
            stringArray[i] = (i + 1).ToString();
        }

        intDriver.Exists_Verify(new int[0]);
        intDriver.Exists_Verify(new int[] { 1 });
        intDriver.Exists_Verify(intArray);

        stringDriver.Exists_Verify(new string[0]);
        stringDriver.Exists_Verify(new string[] { "1" });
        stringDriver.Exists_Verify(stringArray);
    }

    [Fact]
    public static void ExistsTests_Negative()
    {
        Driver<int> intDriver = new Driver<int>();
        Driver<string> stringDriver = new Driver<string>();
        int[] intArray;
        string[] stringArray;
        int arraySize = 16;

        intArray = new int[arraySize];
        stringArray = new string[arraySize];

        for (int i = 0; i < arraySize; ++i)
        {
            intArray[i] = i + 1;
            stringArray[i] = (i + 1).ToString();
        }

        intDriver.Exists_VerifyExceptions(intArray);
        stringDriver.Exists_VerifyExceptions(stringArray);
    }

    [Fact]
    public static void ContainsTests()
    {
        Driver<int> IntDriver = new Driver<int>();
        int[] intArr1 = new int[10];
        for (int i = 0; i < 10; i++)
        {
            intArr1[i] = i;
        }

        int[] intArr2 = new int[10];
        for (int i = 0; i < 10; i++)
        {
            intArr2[i] = i + 10;
        }

        IntDriver.BasicContains(intArr1);
        IntDriver.NonExistingValues(intArr1, intArr2);
        IntDriver.RemovedValues(intArr1);
        IntDriver.AddRemoveValues(intArr1);
        IntDriver.MultipleValues(intArr1, 3);
        IntDriver.MultipleValues(intArr1, 5);
        IntDriver.MultipleValues(intArr1, 17);


        Driver<string> StringDriver = new Driver<string>();
        string[] stringArr1 = new string[10];
        for (int i = 0; i < 10; i++)
        {
            stringArr1[i] = "SomeTestString" + i.ToString();
        }
        string[] stringArr2 = new string[10];
        for (int i = 0; i < 10; i++)
        {
            stringArr2[i] = "SomeTestString" + (i + 10).ToString();
        }

        StringDriver.BasicContains(stringArr1);
        StringDriver.NonExistingValues(stringArr1, stringArr2);
        StringDriver.RemovedValues(stringArr1);
        StringDriver.AddRemoveValues(stringArr1);
        StringDriver.MultipleValues(stringArr1, 3);
        StringDriver.MultipleValues(stringArr1, 5);
        StringDriver.MultipleValues(stringArr1, 17);
        StringDriver.ContainsNullWhenReference(stringArr1, null);
    }

    [Fact]
    public static void ClearTests()
    {
        Driver<int> IntDriver = new Driver<int>();
        int[] intArr = new int[10];
        for (int i = 0; i < 10; i++)
        {
            intArr[i] = i;
        }

        IntDriver.ClearEmptyList();
        IntDriver.ClearMultipleTimesEmptyList(1);
        IntDriver.ClearMultipleTimesEmptyList(10);
        IntDriver.ClearMultipleTimesEmptyList(100);
        IntDriver.ClearNonEmptyList(intArr);
        IntDriver.ClearMultipleTimesNonEmptyList(intArr, 2);
        IntDriver.ClearMultipleTimesNonEmptyList(intArr, 7);
        IntDriver.ClearMultipleTimesNonEmptyList(intArr, 31);

        Driver<string> StringDriver = new Driver<string>();
        string[] stringArr = new string[10];
        for (int i = 0; i < 10; i++)
        {
            stringArr[i] = "SomeTestString" + i.ToString();
        }

        StringDriver.ClearEmptyList();
        StringDriver.ClearMultipleTimesEmptyList(1);
        StringDriver.ClearMultipleTimesEmptyList(10);
        StringDriver.ClearMultipleTimesEmptyList(100);
        StringDriver.ClearNonEmptyList(stringArr);
        StringDriver.ClearMultipleTimesNonEmptyList(stringArr, 2);
        StringDriver.ClearMultipleTimesNonEmptyList(stringArr, 7);
        StringDriver.ClearMultipleTimesNonEmptyList(stringArr, 31);
    }

    [Fact]
    public static void TrueForAllTests()
    {
        Driver<int> intDriver = new Driver<int>();
        Driver<string> stringDriver = new Driver<string>();
        int[] intArray;
        string[] stringArray;
        int arraySize = 16;

        intArray = new int[arraySize];
        stringArray = new string[arraySize];

        for (int i = 0; i < arraySize; ++i)
        {
            intArray[i] = i + 1;
            stringArray[i] = (i + 1).ToString();
        }

        intDriver.TrueForAll_VerifyVanilla(new int[0]);
        intDriver.TrueForAll_VerifyVanilla(new int[] { 1 });
        intDriver.TrueForAll_VerifyVanilla(intArray);

        stringDriver.TrueForAll_VerifyVanilla(new string[0]);
        stringDriver.TrueForAll_VerifyVanilla(new string[] { "1" });
        stringDriver.TrueForAll_VerifyVanilla(stringArray);
    }

    [Fact]
    public static void TrueForAllTests_Negative()
    {
        Driver<int> intDriver = new Driver<int>();
        Driver<string> stringDriver = new Driver<string>();
        int[] intArray;
        string[] stringArray;
        int arraySize = 16;

        intArray = new int[arraySize];
        stringArray = new string[arraySize];

        for (int i = 0; i < arraySize; ++i)
        {
            intArray[i] = i + 1;
            stringArray[i] = (i + 1).ToString();
        }
        intDriver.TrueForAll_VerifyExceptions(intArray);
        stringDriver.TrueForAll_VerifyExceptions(stringArray);
    }

    [Fact]
    public static void TrueForAll_ListSizeCanBeChanged()
    {
        ValueListWrapper<int> list = new() { 1, 2, 3 };
        ValueListWrapper<int> expectedList = new() { 1, 2, 3, 2, 3, 4, 3, 4, 4 };

        bool result = list.TrueForAll(i =>
        {
            if (i < 4)
            {
                list.Add(i + 1);
            }

            return true;
        });

        Assert.True(result);
        Assert.Equal(expectedList, list);
    }

    public delegate int IndexOfDelegate(ValueListWrapper<T> list, T value);
    public enum IndexOfMethod
    {
        IndexOf_T,
        IndexOf_T_int,
        IndexOf_T_Int_int,
        LastIndexOf_T,
        LastIndexOf_T_int,
        LastIndexOf_T_Int_int,
    };

    private IndexOfDelegate IndexOfDelegateFromType(IndexOfMethod methodType)
    {
        switch (methodType)
        {
            case (IndexOfMethod.IndexOf_T):
                return ((ValueListWrapper<T> list, T value) => { return list.IndexOf(value); });
            case (IndexOfMethod.IndexOf_T_int):
                return ((ValueListWrapper<T> list, T value) => { return list.IndexOf(value, 0); });
            case (IndexOfMethod.IndexOf_T_Int_int):
                return ((ValueListWrapper<T> list, T value) => { return list.IndexOf(value, 0, list.Count); });
            case (IndexOfMethod.LastIndexOf_T):
                return ((ValueListWrapper<T> list, T value) => { return list.LastIndexOf(value); });
            case (IndexOfMethod.LastIndexOf_T_int):
                return ((ValueListWrapper<T> list, T value) => { return list.LastIndexOf(value, list.Count - 1); });
            case (IndexOfMethod.LastIndexOf_T_Int_int):
                return ((ValueListWrapper<T> list, T value) => { return list.LastIndexOf(value, list.Count - 1, list.Count); });
            default:
                throw new Exception("Invalid IndexOfMethod");
        }
    }

    /// <summary>
    /// MemberData for a Theory to test the IndexOf methods for List. To avoid high code reuse of tests for the 6 IndexOf
    /// methods in List, delegates are used to cover the basic behavioral cases shared by all IndexOf methods. A bool
    /// is used to specify the ordering (front-to-back or back-to-front (e.g. LastIndexOf)) that the IndexOf method
    /// searches in.
    /// </summary>
    public static IEnumerable<object[]> IndexOfTestData()
    {
        foreach (object[] sizes in ValidCollectionSizes())
        {
            int count = (int)sizes[0];
            yield return new object[] { IndexOfMethod.IndexOf_T, count, true };
            yield return new object[] { IndexOfMethod.LastIndexOf_T, count, false };

            if (count > 0) // 0 is an invalid index for IndexOf when the count is 0.
            {
                yield return new object[] { IndexOfMethod.IndexOf_T_int, count, true };
                yield return new object[] { IndexOfMethod.LastIndexOf_T_int, count, false };
                yield return new object[] { IndexOfMethod.IndexOf_T_Int_int, count, true };
                yield return new object[] { IndexOfMethod.LastIndexOf_T_Int_int, count, false };
            }
        }
    }

    [Theory]
    [MemberData(nameof(IndexOfTestData))]
    public void IndexOf_NoDuplicates(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
    {
        _ = frontToBackOrder;
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> expectedList = new(list);
        IndexOfDelegate IndexOf = IndexOfDelegateFromType(indexOfMethod);

        Assert.All(Enumerable.Range(0, count), i =>
        {
            Assert.Equal(i, IndexOf(list, expectedList[i]));
        });
    }

    [Theory]
    [MemberData(nameof(IndexOfTestData))]
    public void IndexOf_NonExistingValues(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
    {
        _ = frontToBackOrder;
        ValueListWrapper<T> list = GenericListFactory(count);
        IEnumerable<T> nonexistentValues = CreateEnumerable(EnumerableType.List, list, count: count, numberOfMatchingElements: 0, numberOfDuplicateElements: 0);
        IndexOfDelegate IndexOf = IndexOfDelegateFromType(indexOfMethod);

        Assert.All(nonexistentValues, nonexistentValue =>
        {
            Assert.Equal(-1, IndexOf(list, nonexistentValue));
        });
    }

    [Theory]
    [MemberData(nameof(IndexOfTestData))]
    public void IndexOf_DefaultValue(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
    {
        _ = frontToBackOrder;
        T defaultValue = default;
        ValueListWrapper<T> list = GenericListFactory(count);
        IndexOfDelegate IndexOf = IndexOfDelegateFromType(indexOfMethod);
        while (list.Remove(defaultValue))
            count--;
        list.Add(defaultValue);
        Assert.Equal(count, IndexOf(list, defaultValue));
    }

    [Theory]
    [MemberData(nameof(IndexOfTestData))]
    public void IndexOf_OrderIsCorrect(IndexOfMethod indexOfMethod, int count, bool frontToBackOrder)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> withoutDuplicates = new(list);
        list.AddRange(list);
        IndexOfDelegate IndexOf = IndexOfDelegateFromType(indexOfMethod);

        Assert.All(Enumerable.Range(0, count), i =>
        {
            if (frontToBackOrder)
                Assert.Equal(i, IndexOf(list, withoutDuplicates[i]));
            else
                Assert.Equal(count + i, IndexOf(list, withoutDuplicates[i]));
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IndexOf_Int_OrderIsCorrectWithManyDuplicates(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> withoutDuplicates = new(list);
        list.AddRange(list);
        list.AddRange(list);
        list.AddRange(list);

        Assert.All(Enumerable.Range(0, count), i =>
        {
            Assert.All(Enumerable.Range(0, 4), j =>
            {
                int expectedIndex = (j * count) + i;
                Assert.Equal(expectedIndex, list.IndexOf(withoutDuplicates[i], (count * j)));
                Assert.Equal(expectedIndex, list.IndexOf(withoutDuplicates[i], (count * j), count));
            });
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void LastIndexOf_Int_OrderIsCorrectWithManyDuplicates(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> withoutDuplicates = new(list);
        list.AddRange(list);
        list.AddRange(list);
        list.AddRange(list);

        Assert.All(Enumerable.Range(0, count), i => Assert.All(Enumerable.Range(0, 4), j =>
        {
            int expectedIndex = j * count + i;
            Assert.Equal(expectedIndex, list.LastIndexOf(withoutDuplicates[i], count * (j + 1) - 1));
            Assert.Equal(expectedIndex, list.LastIndexOf(withoutDuplicates[i], count * (j + 1) - 1, count));
        }));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IndexOf_Int_OutOfRangeExceptions(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        T element = CreateT(234);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count + 1)); //"Expect ArgumentOutOfRangeException for index greater than length of list.."
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count + 10)); //"Expect ArgumentOutOfRangeException for index greater than length of list.."
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, -1)); //"Expect ArgumentOutOfRangeException for negative index."
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, int.MinValue)); //"Expect ArgumentOutOfRangeException for negative index."
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IndexOf_Int_Int_OutOfRangeExceptions(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        T element = CreateT(234);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count, 1)); //"ArgumentOutOfRangeException expected on index larger than array."
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count + 1, 1)); //"ArgumentOutOfRangeException expected  on index larger than array."
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, 0, count + 1)); //"ArgumentOutOfRangeException expected  on count larger than array."
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, count / 2, count / 2 + 2)); //"ArgumentOutOfRangeException expected.."
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, 0, count + 1)); //"ArgumentOutOfRangeException expected  on count larger than array."
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, 0, -1)); //"ArgumentOutOfRangeException expected on negative count."
        Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(element, -1, 1)); //"ArgumentOutOfRangeException expected on negative index."
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void LastIndexOf_Int_OutOfRangeExceptions(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        T element = CreateT(234);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count)); //"ArgumentOutOfRangeException expected."
        if (count == 0)  // IndexOf with a 0 count List is special cased to return -1.
            Assert.Equal(-1, list.LastIndexOf(element, -1));
        else
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, -1));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void LastIndexOf_Int_Int_OutOfRangeExceptions(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        T element = CreateT(234);

        if (count > 0)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, 0, count + 1)); //"Expected ArgumentOutOfRangeException."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count / 2, count / 2 + 2)); //"Expected ArgumentOutOfRangeException."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, 0, count + 1)); //"Expected ArgumentOutOfRangeException."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, 0, -1)); //"Expected ArgumentOutOfRangeException."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, -1, count)); //"Expected ArgumentOutOfRangeException."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, -1, 1)); //"Expected ArgumentOutOfRangeException."                Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count, 0)); //"Expected ArgumentOutOfRangeException."
            Assert.Throws<ArgumentOutOfRangeException>(() => list.LastIndexOf(element, count, 1)); //"Expected ArgumentOutOfRangeException."
        }
        else // IndexOf with a 0 count List is special cased to return -1.
        {
            Assert.Equal(-1, list.LastIndexOf(element, 0, count + 1));
            Assert.Equal(-1, list.LastIndexOf(element, count / 2, count / 2 + 2));
            Assert.Equal(-1, list.LastIndexOf(element, 0, count + 1));
            Assert.Equal(-1, list.LastIndexOf(element, 0, -1));
            Assert.Equal(-1, list.LastIndexOf(element, -1, count));
            Assert.Equal(-1, list.LastIndexOf(element, -1, 1));
            Assert.Equal(-1, list.LastIndexOf(element, count, 0));
            Assert.Equal(-1, list.LastIndexOf(element, count, 1));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ForEach_Verify(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> visitedItems = new ValueListWrapper<T>();
        Action<T> action = delegate (T item)
        { visitedItems.Add(item); };

        //[] Verify ForEach looks at every item
        visitedItems.Clear();
        list.ForEach(action);
        VerifyList(list, visitedItems);
    }

    [Fact]
    public void ForEach_NullAction_ThrowsArgumentNullException()
    {
        ValueListWrapper<T> list = GenericListFactory();
        Assert.Throws<ArgumentNullException>(() => list.ForEach(null));
    }

    [Fact]
    public void InsertRange_MatchesExpectedContents()
    {
        ValueListWrapper<int> list = new();

        list.InsertRange(0, ReadOnlySpan<int>.Empty);
        Assert.Empty(list);

        list.InsertRange(0, (ReadOnlySpan<int>)new int[] { 3, 2, 1 });
        Assert.Equal(3, list.Count);
        Assert.Equal([3, 2, 1], list);

        list.InsertRange(0, (ReadOnlySpan<int>)new int[] { 6, 5, 4 });
        Assert.Equal(6, list.Count);
        Assert.Equal([6, 5, 4, 3, 2, 1], list);

        list.InsertRange(6, (ReadOnlySpan<int>)new int[] { 0, -1, -2 });
        Assert.Equal(9, list.Count);
        Assert.Equal([6, 5, 4, 3, 2, 1, 0, -1, -2], list);

        list.InsertRange(3, (ReadOnlySpan<int>)new int[] { 100, 99, 98 });
        Assert.Equal(12, list.Count);
        Assert.Equal([6, 5, 4, 100, 99, 98, 3, 2, 1, 0, -1, -2], list);
    }

    private readonly Predicate<T> _equalsDefaultDelegate = (T item) => { return default(T) == null ? item == null : default(T).Equals(item); };
    private readonly Predicate<T> _alwaysTrueDelegate = (T item) => true;
    private readonly Predicate<T> _alwaysFalseDelegate = (T item) => false;

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindVerifyExceptions(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);

        //[] Verify Null match Find
        Assert.Throws<ArgumentNullException>(() => list.Find(null)); //"Err_858ahia Expected null match to throw ArgumentNullException"

        //[] Verify Null match FindLast
        Assert.Throws<ArgumentNullException>(() => list.FindLast(null)); //"Err_858ahia Expected null match to throw ArgumentNullException"

        //[] Verify Null match FindLastIndex
        Assert.Throws<ArgumentNullException>(() => list.FindLastIndex(null)); //"Err_858ahia Expected null match to throw ArgumentNullException"

        //[] Verify Null match FindAll
        Assert.Throws<ArgumentNullException>(() => list.FindAll(null)); //"Err_858ahia Expected null match to throw ArgumentNullException"
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindLastIndexInt_VerifyExceptions(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        Predicate<T> predicate = _alwaysTrueDelegate;


        //[] Verify Null match
        Assert.Throws<ArgumentNullException>(() => list.FindLastIndex(0, null)); //"Err_858ahia Expected null match to throw ArgumentNullException"

        /******************************************************************************
        index
        ******************************************************************************/
        //[] Verify index=Int32.MinValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(int.MinValue, predicate)); //"Err_948ahid Expected index=Int32.MinValue to throw ArgumentOutOfRangeException"

        if (0 < list.Count)
        {
            //[] Verify index=-1
            Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(-1, predicate)); //"Err_328ahuaw Expected index=-1 to throw ArgumentOutOfRangeException"
        }

        //[] Verify index=list.Count + 1
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(list.Count + 1, predicate)); //"Err_488ajdi Expected index=list.Count + 1 to throw ArgumentOutOfRangeException"

        //[] Verify index=list.Count
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(list.Count, predicate)); //"Err_9689ajis Expected index=list.Count to throw ArgumentOutOfRangeException"

        //[] Verify index=Int32.MaxValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(int.MaxValue, predicate)); //"Err_238ajwisa Expected index=Int32.MaxValue to throw ArgumentOutOfRangeException"
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindIndexIntInt_VerifyExceptions(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        Predicate<T> predicate = delegate (T item)
        { return true; };


        //[] Verify Null match
        Assert.Throws<ArgumentNullException>(() => list.FindIndex(0, 0, null)); //"Err_858ahia Expected null match to throw ArgumentNullException"

        /******************************************************************************
        index
        ******************************************************************************/
        //[] Verify index=Int32.MinValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(int.MinValue, 0, predicate)); //"Err_948ahid Expected index=Int32.MinValue to throw ArgumentOutOfRangeException"

        //[] Verify index=-1
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(-1, 0, predicate)); //"Err_328ahuaw Expected index=-1 to throw ArgumentOutOfRangeException"

        //[] Verify index=list.Count + 1
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(list.Count + 1, 0, predicate)); //"Err_488ajdi Expected index=list.Count + 1 to throw ArgumentOutOfRangeException"

        //[] Verify index=list.Count
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(list.Count, 1, predicate)); //"Err_9689ajis Expected index=list.Count to throw ArgumentOutOfRangeException"

        //[] Verify index=Int32.MaxValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(int.MaxValue, 0, predicate)); //"Err_238ajwisa Expected index=Int32.MaxValue to throw ArgumentOutOfRangeException"

        /******************************************************************************
        count
        ******************************************************************************/
        //[] Verify count=Int32.MinValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(0, int.MinValue, predicate)); //Err_948ahid Expected count=Int32.MinValue to throw ArgumentOutOfRangeException"

        //[] Verify count=-1
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(0, -1, predicate)); //"Err_328ahuaw Expected count=-1 to throw ArgumentOutOfRangeException"

        //[] Verify count=list.Count + 1
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(0, list.Count + 1, predicate)); //"Err_488ajdi Expected count=list.Count + 1 to throw ArgumentOutOfRangeException"

        //[] Verify count=Int32.MaxValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(0, int.MaxValue, predicate)); //"Err_238ajwisa Expected count=Int32.MaxValue to throw ArgumentOutOfRangeException"

        /******************************************************************************
        index and count
        ******************************************************************************/
        if (0 < count)
        {
            //[] Verify index=1 count=list.Length
            Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(1, count, predicate)); //"Err_018188avbiw Expected index=1 count=list.Length to throw ArgumentOutOfRangeException"

            //[] Verify index=0 count=list.Length + 1
            Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(0, count + 1, predicate)); //"Err_6848ajiodxbz Expected index=0 count=list.Length + 1 to throw ArgumentOutOfRangeException"
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindLastIndexIntInt_VerifyExceptions(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        Predicate<T> predicate = _alwaysTrueDelegate;

        //[] Verify Null match
        Assert.Throws<ArgumentNullException>(() => list.FindLastIndex(0, 0, null)); //"Err_858ahia Expected null match to throw ArgumentNullException"

        /******************************************************************************
        index
        ******************************************************************************/
        //[] Verify index=Int32.MinValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(int.MinValue, 0, predicate)); //Err_948ahid Expected index=Int32.MinValue to throw ArgumentOutOfRangeException"

        if (0 < list.Count)
        {
            //[] Verify index=-1
            Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(-1, 0, predicate)); //"Err_328ahuaw Expected index=-1 to throw ArgumentOutOfRangeException"
        }

        //[] Verify index=list.Count + 1
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(list.Count + 1, 0, predicate)); //"Err_488ajdi Expected index=list.Count + 1 to throw ArgumentOutOfRangeException"

        //[] Verify index=list.Count
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(list.Count, 1, predicate)); //"Err_9689ajis Expected index=list.Count to throw ArgumentOutOfRangeException"

        //[] Verify index=Int32.MaxValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(int.MaxValue, 0, predicate)); //"Err_238ajwisa Expected index=Int32.MaxValue to throw ArgumentOutOfRangeException"

        /******************************************************************************
        count
        ******************************************************************************/
        //[] Verify count=Int32.MinValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(list.Count - 1, int.MinValue, predicate)); //"Err_948ahid Expected count=Int32.MinValue to throw ArgumentOutOfRangeException"

        //[] Verify count=-1
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(list.Count - 1, -1, predicate)); //"Err_328ahuaw Expected count=-1 to throw ArgumentOutOfRangeException"

        //[] Verify count=list.Count + 1
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(list.Count - 1, list.Count + 1, predicate)); //"Err_488ajdi Expected count=list.Count + 1 to throw ArgumentOutOfRangeException"

        //[] Verify count=Int32.MaxValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(list.Count - 1, int.MaxValue, predicate)); //"Err_238ajwisa Expected count=Int32.MaxValue to throw ArgumentOutOfRangeException"

        /******************************************************************************
        index and count
        ******************************************************************************/
        if (0 < count)
        {
            //[] Verify index=1 count=list.Length
            Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(count - 2, count, predicate)); //"Err_018188avbiw Expected index=1 count=list.Length to throw ArgumentOutOfRangeException"

            //[] Verify index=0 count=list.Length + 1
            Assert.Throws<ArgumentOutOfRangeException>(() => list.FindLastIndex(count - 1, count + 1, predicate)); //"Err_6848ajiodxbz Expected index=0 count=list.Length + 1 to throw ArgumentOutOfRangeException"
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindIndexInt_VerifyExceptions(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        Predicate<T> predicate = delegate (T item)
        { return true; };

        //[] Verify Null match
        Assert.Throws<ArgumentNullException>(() => list.FindIndex(0, null)); //"Err_858ahia Expected null match to throw ArgumentNullException"

        /******************************************************************************
        index
        ******************************************************************************/
        //[] Verify index=Int32.MinValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(int.MinValue, predicate)); //"Err_948ahid Expected index=Int32.MinValue to throw ArgumentOutOfRangeException"

        //[] Verify index=-1
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(-1, predicate)); //"Err_328ahuaw Expected index=-1 to throw ArgumentOutOfRangeException"

        //[] Verify index=list.Count + 1
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(list.Count + 1, predicate)); //"Err_488ajdi Expected index=list.Count + 1 to throw ArgumentOutOfRangeException"

        //[] Verify index=Int32.MaxValue
        Assert.Throws<ArgumentOutOfRangeException>(() => list.FindIndex(int.MaxValue, predicate)); //"Err_238ajwisa Expected index=Int32.MaxValue to throw ArgumentOutOfRangeException"
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Find_VerifyVanilla(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        T expectedItem = default(T);
        T foundItem;
        Predicate<T> EqualsDelegate = (T item) => expectedItem == null ? item == null : expectedItem.Equals(item);

        //[] Verify Find returns the correct index
        for (int i = 0; i < count; ++i)
        {
            expectedItem = beforeList[i];
            foundItem = list.Find(EqualsDelegate);

            Assert.Equal(expectedItem, foundItem); //"Err_282308ahid Verifying value returned from Find FAILED\n"
        }

        //[] Verify Find returns the first item if the match returns true on every item
        foundItem = list.Find(_alwaysTrueDelegate);
        Assert.Equal(0 < count ? beforeList[0] : default(T), foundItem); //"Err_548ahid Verify Find returns the first item if the match returns true on every item FAILED\n"

        //[] Verify Find returns T.Default if the match returns false on every item
        foundItem = list.Find(_alwaysFalseDelegate);
        Assert.Equal(default(T), foundItem); //"Err_30848ahidi Verify Find returns T.Default if the match returns false on every item FAILED\n"

        //[] Verify with default(T)
        list.Add(default(T));
        foundItem = list.Find((T item) => { return item == null ? default(T) == null : item.Equals(default(T)); });
        Assert.Equal(default(T), foundItem); //"Err_541848ajodi Verify with default(T) FAILED\n"
        list.RemoveAt(list.Count - 1);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void Find_VerifyDuplicates(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        T foundItem;
        Predicate<T> EqualsDelegate = (T item) => { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        if (0 < count)
        {
            list.Add(beforeList[0]);

            //[] Verify first item is duplicated
            expectedItem = beforeList[0];
            foundItem = list.Find(EqualsDelegate);
            Assert.Equal(expectedItem, foundItem); //"Err_2879072qaiadf  Verify first item is duplicated FAILED\n"
        }

        if (1 < count)
        {
            list.Add(beforeList[1]);

            //[] Verify second item is duplicated
            expectedItem = beforeList[1];
            foundItem = list.Find(EqualsDelegate);
            Assert.Equal(expectedItem, foundItem); //"Err_4588ajdia Verify second item is duplicated FAILED\n"

            //[] Verify with match that matches more then one item
            expectedItem = beforeList[0];
            foundItem = list.Find(EqualsDelegate);
            Assert.Equal(expectedItem, foundItem); //"Err_4489ajodoi Verify with match that matches more then one item FAILED\n"
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindLast_VerifyVanilla(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        T expectedItem = default(T);
        T foundItem;
        Predicate<T> EqualsDelegate = (T item) => { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        for (int i = 0; i < count; ++i)
            list.Add(beforeList[i]);

        //[] Verify FindLast returns the correct item
        for (int i = 0; i < count; ++i)
        {
            expectedItem = beforeList[i];
            foundItem = list.FindLast(EqualsDelegate);

            Assert.Equal(expectedItem, foundItem); //"Err_282308ahid Verifying value returned from find FAILED\n"
        }

        //[] Verify FindLast returns the last item if the match returns true on every item
        foundItem = list.FindLast(_alwaysTrueDelegate);
        T expected = 0 < count ? beforeList[count - 1] : default(T);
        Assert.Equal(expected, foundItem); //"Err_548ahid Verify FindLast returns the last item if the match returns true on every item FAILED\n"

        //[] Verify FindLast returns default(T) if the match returns false on every item
        foundItem = list.FindLast(_alwaysFalseDelegate);
        Assert.Equal(default(T), foundItem); //"Err_30848ahidi Verify FindLast returns t.default if the match returns false on every item FAILED\n"

        //[] Verify with default(T)
        list.Add(default(T));
        foundItem = list.FindLast((T item) => { return item == null ? default(T) == null : item.Equals(default(T)); });
        Assert.Equal(default(T), foundItem); //"Err_541848ajodi Verify with default(T) FAILED\n"
        list.RemoveAt(list.Count - 1);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindLast_VerifyDuplicates(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        T foundItem;
        Predicate<T> EqualsDelegate = (T item) => { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        if (0 < count)
        {
            list.Add(beforeList[0]);

            //[] Verify first item is duplicated
            expectedItem = beforeList[0];
            foundItem = list.FindLast(EqualsDelegate);
            Assert.Equal(beforeList[0], foundItem); //"Err_2879072qaiadf  Verify first item is duplicated FAILED\n"
        }

        if (1 < count)
        {
            list.Add(beforeList[1]);

            //[] Verify second item is duplicated
            expectedItem = beforeList[1];
            foundItem = list.FindLast(EqualsDelegate);
            Assert.Equal(beforeList[1], foundItem); //"Err_4588ajdia Verify second item is duplicated FAILED\n"

            //[] Verify with match that matches more then one item
            foundItem = list.FindLast((T item) => { return item != null && (item.Equals(beforeList[0]) || item.Equals(beforeList[1])); });
            Assert.Equal(beforeList[1], foundItem); //"Err_4489ajodoi Verify with match that matches more then one item FAILED\n"
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindIndex_VerifyVanilla(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int index;
        Predicate<T> EqualsDefaultDelegate = (T item) => { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        for (int i = 0; i < count; ++i)
            list.Add(beforeList[i]);

        //[] Verify FinIndex returns the correct index
        for (int i = 0; i < count; ++i)
        {
            expectedItem = beforeList[i];
            index = list.FindIndex(EqualsDefaultDelegate);
            Assert.Equal(i, index); //"Err_282308ahid Expected FindIndex to return the same."
        }

        //[] Verify FindIndex returns 0 if the match returns true on every item
        int expected = count == 0 ? -1 : 0;
        index = list.FindIndex(_alwaysTrueDelegate);
        Assert.Equal(expected, index); //"Err_15198ajid Verify FindIndex returns 0 if the match returns true on every item expected"

        //[] Verify FindIndex returns -1 if the match returns false on every item
        index = list.FindIndex(_alwaysFalseDelegate);
        Assert.Equal(-1, index); //"Err_305981ajodd Verify FindIndex returns -1 if the match returns false on every item"
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindIndex_VerifyDuplicates(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        T expectedItem = default(T);
        int index;
        Predicate<T> EqualsDelegate = (T item) => { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        if (0 < count)
        {
            list.Add(beforeList[0]);

            //[] Verify first item is duplicated
            expectedItem = beforeList[0];
            index = list.FindIndex(EqualsDelegate);
            Assert.Equal(0, index); //"Err_3282iahid Verify first item is duplicated"
        }

        if (1 < count)
        {
            list.Add(beforeList[1]);

            //[] Verify second item is duplicated
            expectedItem = beforeList[1];
            index = list.FindIndex(EqualsDelegate);
            Assert.Equal(1, index); //"Err_29892adewiu Verify second item is duplicated"
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindIndexInt_VerifyVanilla(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int index;
        Predicate<T> EqualsDelegate = delegate (T item)
        { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        //[] Verify FinIndex returns the correct index
        for (int i = 0; i < count; ++i)
        {
            expectedItem = beforeList[i];
            index = list.FindIndex(0, EqualsDelegate);
            Assert.Equal(i, index); //"Err_282308ahid Expected FindIndex to return the same"
        }

        //[] Verify FindIndex returns 0 if the match returns true on every item
        int expected = count == 0 ? -1 : 0;
        index = list.FindIndex(0, delegate (T item)
        { return true; });
        Assert.Equal(expected, index); //"Err_15198ajid Verify FindIndex returns 0 if the match returns true on every item "

        //[] Verify FindIndex returns -1 if the match returns false on every item
        index = list.FindIndex(0, delegate (T item)
        { return false; });
        Assert.Equal(-1, index); //"Err_305981ajodd Verify FindIndex returns -1 if the match returns false on every item"

        //[] Verify FindIndex returns -1 if the index == count
        index = list.FindIndex(count, delegate (T item)
        { return true; });
        Assert.Equal(-1, index); //"Err_4858ajodoa Verify FindIndex returns -1 if the index == count"

        if (0 < count)
        {
            //[] Verify NEG FindIndex uses the index
            expectedItem = beforeList[0];
            index = list.FindIndex(1, EqualsDelegate);
            Assert.Equal(-1, index); //"Err_548797ahjid Verify NEG FindIndex uses the index"
        }

        if (1 < count)
        {
            //[] Verify POS FindIndex uses the index LOWER
            expectedItem = beforeList[1];
            index = list.FindIndex(1, EqualsDelegate);
            Assert.Equal(1, index); //"Err_68797ahid Verify POS FindIndex uses the index LOWER"

            //[] Verify POS FindIndex uses the index UPPER
            expectedItem = beforeList[count - 1];
            index = list.FindIndex(1, EqualsDelegate);
            Assert.Equal(count - 1, index); //"Err_51488ajod Verify POS FindIndex uses the index UPPER"
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindIndexInt_VerifyDuplicates(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int index;
        Predicate<T> EqualsDelegate = delegate (T item)
        { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        if (0 < count)
        {
            list.Add(beforeList[0]);

            //[] Verify first item is duplicated
            expectedItem = beforeList[0];
            index = list.FindIndex(0, EqualsDelegate);
            Assert.Equal(0, index); //"Err_3282iahid Verify first item is duplicated"

            //[] Verify first item is duplicated and index=1
            expectedItem = beforeList[0];
            index = list.FindIndex(1, EqualsDelegate);
            Assert.Equal(count, index); //"Err_8588ahidi Verify first item is duplicated and index=1"
        }

        if (1 < count)
        {
            list.Add(beforeList[1]);

            //[] Verify second item is duplicated
            expectedItem = beforeList[1];
            index = list.FindIndex(0, EqualsDelegate);
            Assert.Equal(1, index); //"Err_29892adewiu Verify second item is duplicated"

            //[] Verify second item is duplicated and index=2
            expectedItem = beforeList[1];
            index = list.FindIndex(2, EqualsDelegate);
            Assert.Equal(count + 1, index); //"Err_1580ahisdf Verify second item is duplicated and index=2 "
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindIndexIntInt_VerifyVanilla(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int index;
        Predicate<T> EqualsDelegate = delegate (T item)
        { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        //[] Verify FinIndex returns the correct index
        for (int i = 0; i < count; ++i)
        {
            expectedItem = beforeList[i];
            index = list.FindIndex(0, count, delegate (T item)
            { return expectedItem == null ? item == null : expectedItem.Equals(item); });
            Assert.Equal(i, index); //"Err_282308ahid Expected FindIndex to return the same."
        }

        //[] Verify FindIndex returns 0 if the match returns true on every item
        index = list.FindIndex(0, count, delegate (T item)
        { return true; });
        int expected = count == 0 ? -1 : 0;
        Assert.Equal(expected, index); //"Err_15198ajid Verify FindIndex returns 0 if the match returns true on every item"

        //[] Verify FindIndex returns -1 if the match returns false on every item
        index = list.FindIndex(0, count, delegate (T item)
        { return false; });
        Assert.Equal(-1, index); //"Err_305981ajodd Verify FindIndex returns -1 if the match returns false on every item"

        //[] Verify FindIndex returns -1 if the index == count
        index = list.FindIndex(count, 0, delegate (T item)
        { return true; });
        Assert.Equal(-1, index); //"Err_4858ajodoa Verify FindIndex returns -1 if the index == count"

        if (0 < count)
        {
            //[] Verify NEG FindIndex uses the index
            expectedItem = beforeList[0];
            index = list.FindIndex(1, count - 1, EqualsDelegate);
            Assert.Equal(-1, index); //"Err_548797ahjid Verify NEG FindIndex uses the index "

            //[] Verify NEG FindIndex uses the count
            expectedItem = beforeList[count - 1];
            index = list.FindIndex(0, count - 1, EqualsDelegate);
            Assert.Equal(-1, index); //"Err_7894ahoid Verify NEG FindIndex uses the count "
        }

        if (1 < count)
        {
            //[] Verify POS FindIndex uses the index
            expectedItem = beforeList[1];
            index = list.FindIndex(1, count - 1, EqualsDelegate);
            Assert.Equal(1, index); //"Err_68797ahid Verify POS FindIndex uses the index"

            //[] Verify POS FindIndex uses the count
            expectedItem = beforeList[count - 2];
            index = list.FindIndex(0, count - 1, EqualsDelegate);
            Assert.Equal(count - 2, index); //"Err_28278ahdii Verify POS FindIndex uses the count"

            //[] Verify NEG FindIndex uses the index and count LOWER
            expectedItem = beforeList[0];
            index = list.FindIndex(1, count - 2, EqualsDelegate);
            Assert.Equal(-1, index); //"Err_384984ahjiod Verify NEG FindIndex uses the index and count LOWER "

            //[] Verify NEG FindIndex uses the index and count UPPER
            expectedItem = beforeList[count - 1];
            index = list.FindIndex(1, count - 2, EqualsDelegate);
            Assert.Equal(-1, index); //"Err_1489haidid Verify NEG FindIndex uses the index and count UPPER "

            //[] Verify POS FindIndex uses the index and count LOWER
            expectedItem = beforeList[1];
            index = list.FindIndex(1, count - 2, EqualsDelegate);
            Assert.Equal(1, index); //"Err_604890ahjid Verify POS FindIndex uses the index and count LOWER "

            //[] Verify POS FindIndex uses the index and count UPPER
            expectedItem = beforeList[count - 2];
            index = list.FindIndex(1, count - 2, EqualsDelegate);
            Assert.Equal(count - 2, index); //"Err_66844ahidd Verify POS FindIndex uses the index and count UPPER "
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindIndexIntInt_VerifyDuplicates(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int index;
        Predicate<T> EqualsDelegate = delegate (T item)
        { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        if (0 < count)
        {
            list.Add(beforeList[0]);

            //[] Verify first item is duplicated
            expectedItem = beforeList[0];
            index = list.FindIndex(0, list.Count, EqualsDelegate);
            Assert.Equal(0, index); //"Err_3282iahid Verify first item is duplicated"

            //[] Verify first item is duplicated and index=1
            expectedItem = beforeList[0];
            index = list.FindIndex(1, list.Count - 1, EqualsDelegate);
            Assert.Equal(count, index); //"Err_8588ahidi Verify first item is duplicated and index=1"
        }

        if (1 < count)
        {
            list.Add(beforeList[1]);

            //[] Verify second item is duplicated
            expectedItem = beforeList[1];
            index = list.FindIndex(0, list.Count, EqualsDelegate);
            Assert.Equal(1, index); //"Err_29892adewiu Verify second item is duplicated"

            //[] Verify second item is duplicated and index=2
            expectedItem = beforeList[1];
            index = list.FindIndex(2, list.Count - 2, EqualsDelegate);
            Assert.Equal(count + 1, index); //"Err_1580ahisdf Verify second item is duplicated and index=2"
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindLastIndex_VerifyVanilla(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int index;
        Predicate<T> EqualsDelegate = delegate (T item)
        { return expectedItem == null ? item == null : expectedItem.Equals(item); };


        //[] Verify FinIndex returns the correct index
        for (int i = 0; i < count; ++i)
        {
            expectedItem = beforeList[i];
            index = list.FindLastIndex(EqualsDelegate);
            Assert.Equal(i, index); //"Err_282308ahid Expected FindLastIndex to return the same."
        }

        //[] Verify FindLastIndex returns 0 if the match returns true on every item
        int expected = count == 0 ? -1 : count - 1;
        index = list.FindLastIndex(_alwaysTrueDelegate);
        Assert.Equal(expected, index); //"Err_15198ajid Verify FindLastIndex returns 0 if the match returns true on every item"

        //[] Verify FindLastIndex returns -1 if the match returns false on every item
        index = list.FindLastIndex(_alwaysFalseDelegate);
        Assert.Equal(-1, index); //"Err_305981ajodd Verify FindLastIndex returns -1 if the match returns false on every item"
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindLastIndex_VerifyDuplicates(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int index;
        Predicate<T> EqualsDelegate = delegate (T item)
        { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        if (0 < count)
        {
            list.Add(beforeList[0]);

            //[] Verify first item is duplicated
            expectedItem = beforeList[0];
            index = list.FindLastIndex(EqualsDelegate);
            Assert.Equal(count, index); //"Err_3282iahid Verify first item is duplicated"
        }

        if (1 < count)
        {
            list.Add(beforeList[1]);

            //[] Verify second item is duplicated
            expectedItem = beforeList[1];
            index = list.FindLastIndex(EqualsDelegate);
            Assert.Equal(count + 1, index); //"Err_29892adewiu Verify second item is duplicated."
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindLastIndexInt_VerifyVanilla(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int index;
        Predicate<T> EqualsDelegate = (T item) => { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        //[] Verify FinIndex returns the correct index
        for (int i = 0; i < count; ++i)
        {
            expectedItem = beforeList[i];
            index = list.FindLastIndex(count - 1, EqualsDelegate);
            Assert.Equal(i, index); //"Err_282308ahid Expected FindLastIndex to return the same."
        }

        //[] Verify FindLastIndex returns 0 if the match returns true on every item
        index = list.FindLastIndex(count - 1, _alwaysTrueDelegate);
        int expected = count == 0 ? -1 : count - 1;
        Assert.Equal(expected, index); //"Err_15198ajid Verify FindLastIndex returns 0 if the match returns true on every item"

        //[] Verify FindLastIndex returns -1 if the match returns false on every item
        index = list.FindLastIndex(count - 1, _alwaysFalseDelegate);
        Assert.Equal(-1, index); //"Err_305981ajodd Verify FindLastIndex returns -1 if the match returns false on every item"

        //[] Verify FindLastIndex returns 0 if the index == 0
        expected = 0 < count ? count - 1 : -1;
        index = list.FindLastIndex(count - 1, _alwaysTrueDelegate);
        Assert.Equal(expected, index); //"Err_4858ajodoa Verify FindLastIndex returns 0 if the index == 0 "

        if (1 < count)
        {
            //[] Verify NEG FindLastIndex uses the index
            expectedItem = beforeList[count - 1];
            index = list.FindLastIndex(count - 2, EqualsDelegate);
            Assert.Equal(-1, index); //"Err_548797ahjid Verify NEG FindLastIndex uses the index"

            //[] Verify POS FindLastIndex uses the index LOWER
            expectedItem = beforeList[0];
            index = list.FindLastIndex(count - 2, EqualsDelegate);
            Assert.Equal(0, index); //"Err_68797ahid Verify POS FindLastIndex uses the index LOWER"

            //[] Verify POS FindLastIndex uses the index UPPER
            expectedItem = beforeList[count - 2];
            expected = count - 2;
            index = list.FindLastIndex(count - 2, EqualsDelegate);
            Assert.Equal(expected, index); //"Err_51488ajod Verify POS FindLastIndex uses the index UPPER"
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindLastIndexInt_VerifyDuplicates(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int index;
        Predicate<T> EqualsDelegate = (T item) => { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        if (0 < count)
        {
            list.Add(beforeList[0]);

            //[] Verify first item is duplicated
            expectedItem = beforeList[0];
            index = list.FindLastIndex(list.Count - 1, EqualsDelegate);
            Assert.Equal(count, index); //"Err_3282iahid Verify first item is duplicated"

            //[] Verify first item is duplicated and index is on less then the index of the last duplicate
            expectedItem = beforeList[0];
            index = list.FindLastIndex(count - 1, EqualsDelegate);
            Assert.Equal(0, index); //"Err_8588ahidi Verify first item is duplicated and index is on less then the index of the last duplicate"
        }

        if (1 < count)
        {
            list.Add(beforeList[1]);

            //[] Verify second item is duplicated
            expectedItem = beforeList[1];
            index = list.FindLastIndex(list.Count - 1, EqualsDelegate);
            Assert.Equal(list.Count - 1, index); //"Err_29892adewiu Verify second item is duplicated"

            //[] Verify second item is duplicated and index is on less then the index of the last duplicate
            expectedItem = beforeList[1];
            index = list.FindLastIndex(list.Count - 3, EqualsDelegate);
            Assert.Equal(1, index); //"Err_1580ahisdf Verify second item is duplicated and index is on less then the index of the last duplicate"
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindLastIndexIntInt_VerifyVanilla(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int index;
        Predicate<T> EqualsDelegate = (T item) => { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        for (int i = 0; i < count; ++i)
            list.Add(beforeList[i]);

        //[] Verify FinIndex returns the correct index
        for (int i = 0; i < count; ++i)
        {
            expectedItem = beforeList[i];
            index = list.FindLastIndex(count - 1, count, EqualsDelegate);
            Assert.Equal(i, index); //"Err_282308ahid Expected FindLastIndex to be the same."
        }

        //[] Verify FindLastIndex returns 0 if the match returns true on every item
        int expected = count == 0 ? -1 : count - 1;
        index = list.FindLastIndex(count - 1, count, _alwaysTrueDelegate);
        Assert.Equal(expected, index); //"Err_15198ajid Verify FindLastIndex returns 0 if the match returns true on every item"

        //[] Verify FindLastIndex returns -1 if the match returns false on every item
        index = list.FindLastIndex(count - 1, count, _alwaysFalseDelegate);
        Assert.Equal(-1, index); //"Err_305981ajodd Verify FindLastIndex returns -1 if the match returns false on every item"

        if (0 < count)
        {
            //[] Verify FindLastIndex returns -1 if the index == 0
            index = list.FindLastIndex(0, 0, _alwaysTrueDelegate);
            Assert.Equal(-1, index); //"Err_298298ahdi Verify FindLastIndex returns -1 if the index=0"

            //[] Verify NEG FindLastIndex uses the count
            expectedItem = beforeList[0];
            index = list.FindLastIndex(count - 1, count - 1, EqualsDelegate);
            Assert.Equal(-1, index); //"Err_7894ahoid Verify NEG FindLastIndex uses the count"
        }

        if (1 < count)
        {
            //[] Verify NEG FindLastIndex uses the index
            expectedItem = beforeList[count - 1];
            index = list.FindLastIndex(count - 2, count - 1, EqualsDelegate);
            Assert.Equal(-1, index); //"Err_548797ahjid Verify NEG FindLastIndex uses the index"

            //[] Verify POS FindLastIndex uses the index
            expectedItem = beforeList[count - 2];
            index = list.FindLastIndex(count - 2, count - 1, EqualsDelegate);
            Assert.Equal(count - 2, index); //"Err_68797ahid Verify POS FindLastIndex uses the index"

            //[] Verify POS FindLastIndex uses the count
            expectedItem = beforeList[count - 2];
            index = list.FindLastIndex(count - 1, count - 1, EqualsDelegate);
            Assert.Equal(count - 2, index); //"Err_28278ahdii Verify POS FindLastIndex uses the count"

            //[] Verify NEG FindLastIndex uses the index and count LOWER
            expectedItem = beforeList[0];
            index = list.FindLastIndex(count - 2, count - 2, EqualsDelegate);
            Assert.Equal(-1, index); //"Err_384984ahjiod Verify NEG FindLastIndex uses the index and count LOWER"

            //[] Verify NEG FindLastIndex uses the index and count UPPER
            expectedItem = beforeList[count - 1];
            index = list.FindLastIndex(count - 2, count - 2, EqualsDelegate);
            Assert.Equal(-1, index); //"Err_1489haidid Verify NEG FindLastIndex uses the index and count UPPER"

            //[] Verify POS FindLastIndex uses the index and count LOWER
            expectedItem = beforeList[1];
            index = list.FindLastIndex(count - 2, count - 2, EqualsDelegate);
            Assert.Equal(1, index); //"Err_604890ahjid Verify POS FindLastIndex uses the index and count LOWER"

            //[] Verify POS FindLastIndex uses the index and count UPPER
            expectedItem = beforeList[count - 2];
            index = list.FindLastIndex(count - 2, count - 2, EqualsDelegate);
            Assert.Equal(count - 2, index); //"Err_66844ahidd Verify POS FindLastIndex uses the index and count UPPER"
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindLastIndexIntInt_VerifyDuplicates(int count)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        int index;
        Predicate<T> EqualsDelegate = (T item) => { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        if (0 < count)
        {
            list.Add(beforeList[0]);

            //[] Verify first item is duplicated
            expectedItem = beforeList[0];
            index = list.FindLastIndex(list.Count - 1, list.Count, EqualsDelegate);
            Assert.Equal(list.Count - 1, index); //"Err_3282iahid Verify first item is duplicated"
        }

        if (1 < count)
        {
            list.Add(beforeList[1]);

            //[] Verify second item is duplicated
            expectedItem = beforeList[1];
            index = list.FindLastIndex(list.Count - 1, list.Count, EqualsDelegate);
            Assert.Equal(list.Count - 1, index); //"Err_29892adewiu Verify second item is duplicated"
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindAll_VerifyVanilla(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> beforeList = new(list);
        T expectedItem = default(T);
        Predicate<T> EqualsDelegate = (value) => expectedItem == null ? value == null : expectedItem.Equals(value);

        //[] Verify FindAll returns the correct List with one item
        for (int i = 0; i < count; ++i)
        {
            expectedItem = beforeList[i];
            ValueListWrapper<T> results = new(list.FindAll(EqualsDelegate));
            VerifyList(results, new(beforeList.Where((value) => EqualsDelegate(value))));
        }

        //[] Verify FindAll returns an List with all of the items if the predicate always returns true
        VerifyList(new(list.FindAll(x => true)), beforeList);

        //[] Verify FindAll returns an empty List if the match returns false on every item
        VerifyList(new(list.FindAll(x => false)), new ValueListWrapper<T>());
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void FindAll_VerifyDuplicates(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        for (int i = 0; i < count / 2; i++)
            list.Add(list[i]);
        ValueListWrapper<T> beforeList = new(list);
        T expectedItem = default(T);
        Predicate<T> EqualsDelegate = (value) => expectedItem == null ? value == null : expectedItem.Equals(value);
        //[] Verify FindAll returns the correct List with one item
        for (int i = 0; i < count; ++i)
        {
            expectedItem = beforeList[i];
            ValueListWrapper<T> results = new(list.FindAll(EqualsDelegate));
            VerifyList(results, new(beforeList.Where((value) => EqualsDelegate(value))));
        }

        //[] Verify FindAll returns an List with all of the items if the predicate always returns true
        VerifyList(new(list.FindAll(x => true)), beforeList);

        //[] Verify FindAll returns an empty List if the match returns false on every item
        VerifyList(new(list.FindAll(x => false)), new ValueListWrapper<T>());
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void EnsureCapacity_RequestingLargerCapacity_DoesNotInvalidateEnumeration(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        IEnumerator<T> copiedListEnumerator = new ValueListWrapper<T>(list).GetEnumerator();
        IEnumerator<T> enumerator = list.GetEnumerator();
        var capacity = list.Capacity;

        list.EnsureCapacity(capacity + 1);

        enumerator.MoveNext();
    }

    [Fact]
    public void EnsureCapacity_NotInitialized_RequestedZero_ReturnsZero()
    {
        var list = new ValueListWrapper<T>();
        Assert.Equal(0, list.EnsureCapacity(0));
        Assert.Equal(0, list.Capacity);
    }

    [Fact]
    public void EnsureCapacity_NegativeCapacityRequested_Throws()
    {
        var list = new ValueListWrapper<T>();
        Assert.Throws<ArgumentOutOfRangeException>("capacity", () => list.EnsureCapacity(-1));
    }

    [Theory]
    [InlineData(5)]
    public void EnsureCapacity_RequestedCapacitySmallerThanOrEqualToCurrent_CapacityUnchanged(int currentCapacity)
    {
        ValueListWrapper<T> list = new(currentCapacity);
        int initialCapacity = list.Capacity;

        for (int requestCapacity = 0; requestCapacity <= currentCapacity; requestCapacity++)
        {
            Assert.Equal(initialCapacity, list.EnsureCapacity(requestCapacity));
            Assert.Equal(initialCapacity, list.Capacity);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void EnsureCapacity_RequestedCapacitySmallerThanOrEqualToCount_CapacityUnchanged(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        var currentCapacity = list.Capacity;

        for (int requestCapacity = 0; requestCapacity <= count; requestCapacity++)
        {
            Assert.Equal(currentCapacity, list.EnsureCapacity(requestCapacity));
            Assert.Equal(currentCapacity, list.Capacity);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void EnsureCapacity_CapacityIsAtLeastTheRequested(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);

        int currentCapacity = list.Capacity;
        int requestCapacity = currentCapacity + 1;
        int newCapacity = list.EnsureCapacity(requestCapacity);
        Assert.InRange(newCapacity, requestCapacity, int.MaxValue);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void EnsureCapacity_RequestingLargerCapacity_DoesNotImpactListContent(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        var copiedList = new ValueListWrapper<T>(list);

        list.EnsureCapacity(list.Capacity + 1);
        Assert.Equal(copiedList, list);
    }

    [Fact]
    public void CopyTo_InvalidArgs_Throws()
    {
        ValueListWrapper<int> list = new() { 1, 2, 3 };
        Assert.Throws<ArgumentException>(() => list.CopyTo((Span<int>)new int[2]));
    }

    [Fact]
    public void CopyTo_ItemsCopiedCorrectly()
    {
        ValueListWrapper<int> list;
        Span<int> destination;

        list = new();
        destination = Span<int>.Empty;
        list.CopyTo(destination);

        list = new() { 1, 2, 3 };
        destination = new int[3];
        list.CopyTo(destination);
        Assert.Equal([1, 2, 3], destination.ToArray());

        list = new() { 1, 2, 3 };
        destination = new int[4];
        list.CopyTo(destination);
        Assert.Equal([1, 2, 3, 0], destination.ToArray());
    }

    [Fact]
    public void ConvertAll()
    {
        ValueListWrapper<int> list = new([1, 2, 3]);
        ValueListWrapper<int> before = new(list);
        ValueListWrapper<int> after = new(list.ConvertAll(i => i * 10));

        Assert.Equal(before.Count, list.Count);
        Assert.Equal(before.Count, after.Count);

        for (int i = 0; i < list.Count; i++)
        {
            Assert.Equal(before[i], list[i]);
            Assert.Equal(before[i] * 10, after[i]);
        }
    }

    [Fact]
    public void Constructor_Default()
    {
        ValueListWrapper<T> list = new();
        Assert.Equal(0, list.Capacity); //"Expected capacity of list to be the same as given."
        Assert.Empty(list); //"Do not expect anything to be in the list."
        Assert.False(((IList<T>)list).IsReadOnly); //"List should not be readonly"
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(100)]
    public void Constructor_Capacity(int capacity)
    {
        ValueListWrapper<T> list = new(capacity);
        Assert.Empty(list); //"Do not expect anything to be in the list."
        Assert.True(list.Capacity >= capacity); //"Expected capacity of list to be the same as given."
        Assert.False(((IList<T>)list).IsReadOnly); //"List should not be readonly"
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ValueListWrapper<T>(capacity));
    }

    [Theory]
    [MemberData(nameof(EnumerableTestData))]
    public void Constructor_IEnumerable(EnumerableType enumerableType, int listLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        _ = listLength;
        _ = numberOfMatchingElements;
        IEnumerable<T> enumerable = CreateEnumerable(enumerableType, null, enumerableLength, 0, numberOfDuplicateElements);
        ValueListWrapper<T> list = new(enumerable);
        ValueListWrapper<T> expected = new(enumerable);

        Assert.Equal(enumerableLength, list.Count); //"Number of items in list do not match the number of items given."

        for (int i = 0; i < enumerableLength; i++)
            Assert.Equal(expected[i], list[i]); //"Expected object in item array to be the same as in the list"

        Assert.False(((IList<T>)list).IsReadOnly); //"List should not be readonly"
    }

    [Fact]
    public void Constructor_NullIEnumerable_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ValueListWrapper<T>(null));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void BinarySearch_ForEveryItemWithoutDuplicates(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        foreach (T item in list)
            while (list.Count((value) => value.Equals(item)) > 1)
                list.Remove(item);
        list.Sort();
        ValueListWrapper<T> beforeList = new(list);

        Assert.All(Enumerable.Range(0, list.Count), index =>
        {
            Assert.Equal(index, list.BinarySearch(beforeList[index]));
            Assert.Equal(index, list.BinarySearch(beforeList[index], GetIComparer()));
            Assert.Equal(beforeList[index], list[index]);
        });
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void BinarySearch_ForEveryItemWithDuplicates(int count)
    {
        if (count > 0)
        {
            ValueListWrapper<T> list = GenericListFactory(count);
            list.Add(list[0]);
            list.Sort();
            ValueListWrapper<T> beforeList = new(list);

            Assert.All(Enumerable.Range(0, list.Count), index =>
            {
                Assert.True(list.BinarySearch(beforeList[index]) >= 0);
                Assert.True(list.BinarySearch(beforeList[index], GetIComparer()) >= 0);
                Assert.Equal(beforeList[index], list[index]);
            });
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void BinarySearch_Validations(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        list.Sort();
        T element = CreateT(3215);
        Assert.Throws<ArgumentOutOfRangeException>(() => list.BinarySearch(0, count + 1, element, GetIComparer())); //"Finding items longer than array should throw ArgumentException"
        Assert.Throws<ArgumentOutOfRangeException>(() => list.BinarySearch(-1, count, element, GetIComparer())); //"ArgumentOutOfRangeException should be thrown on negative index."
        Assert.Throws<ArgumentOutOfRangeException>(() => list.BinarySearch(0, -1, element, GetIComparer())); //"ArgumentOutOfRangeException should be thrown on negative count."
        Assert.Throws<ArgumentOutOfRangeException>(() => list.BinarySearch(count + 1, count, element, GetIComparer())); //"ArgumentException should be thrown on index greater than length of array."
    }

    [Theory]
    [MemberData(nameof(EnumerableTestData))]
    public void AddRange(EnumerableType enumerableType, int listLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        ValueListWrapper<T> list = GenericListFactory(listLength);
        ValueListWrapper<T> listBeforeAdd = new(list);
        IEnumerable<T> enumerable = CreateEnumerable(enumerableType, list, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements);
        list.AddRange(enumerable);

        // Check that the first section of the List is unchanged
        Assert.All(Enumerable.Range(0, listLength), index =>
        {
            Assert.Equal(listBeforeAdd[index], list[index]);
        });

        // Check that the added elements are correct
        Assert.All(Enumerable.Range(0, enumerableLength), index =>
        {
            Assert.Equal(enumerable.ElementAt(index), list[index + listLength]);
        });
    }

    [Theory]
    [MemberData(nameof(ListTestData))]
    public void AddRange_Span(EnumerableType enumerableType, int listLength, int enumerableLength, int numberOfMatchingElements, int numberOfDuplicateElements)
    {
        ValueListWrapper<T> list = GenericListFactory(listLength);
        ValueListWrapper<T> listBeforeAdd = new(list);
        Span<T> span = CreateEnumerable(enumerableType, list, enumerableLength, numberOfMatchingElements, numberOfDuplicateElements).ToArray();
        list.AddRange(span);

        // Check that the first section of the List is unchanged
        Assert.All(Enumerable.Range(0, listLength), index => Assert.Equal(listBeforeAdd[index], list[index]));

        // Check that the added elements are correct
        for (int i = 0; i < enumerableLength; i++)
        {
            Assert.Equal(span[i], list[i + listLength]);
        };
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void AddRange_NullEnumerable_ThrowsArgumentNullException(int count)
    {
        ValueListWrapper<T> list = GenericListFactory(count);
        ValueListWrapper<T> listBeforeAdd = new(list);
        Assert.Throws<ArgumentNullException>(() => list.AddRange(default(IEnumerable<T>)));
        Assert.Equal(listBeforeAdd, list);
    }

    private sealed class CollectionWithLargeCount : ICollection<T>
    {
        public int Count => int.MaxValue;

        public bool IsReadOnly => throw new NotImplementedException();
        public void Add(T item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(T item) => throw new NotImplementedException();
        public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
        public bool Remove(T item) => throw new NotImplementedException();
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}

#nullable restore
