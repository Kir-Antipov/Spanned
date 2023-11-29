namespace Spanned.Tests.Collections.Generic.ValueList;

#nullable disable

internal sealed class Driver<T>
{
    public Func<T[], IEnumerable<T>>[] CollectionGenerators { get; }

    public Driver()
    {
        CollectionGenerators = new Func<T[], IEnumerable<T>>[]
        {
            ConstructTestList,
            ConstructTestEnumerable,
            ConstructLazyTestEnumerable,
        };
    }

    public void BasicInsert(T[] items, T item, int index, int repeat)
    {
        ValueListWrapper<T> list = new(items);

        for (int i = 0; i < repeat; i++)
        {
            list.Insert(index, item);
        }

        Assert.Contains(item, list); //"Expect it to contain the item."
        Assert.Equal(list.Count, items.Length + repeat); //"Expect to be the same."

        for (int i = 0; i < index; i++)
        {
            Assert.Equal(list[i], items[i]); //"Expect to be the same."
        }

        for (int i = index; i < index + repeat; i++)
        {
            Assert.Equal(list[i], item); //"Expect to be the same."
        }


        for (int i = index + repeat; i < list.Count; i++)
        {
            Assert.Equal(list[i], items[i - repeat]); //"Expect to be the same."
        }
    }

    public void InsertValidations(T[] items)
    {
        ValueListWrapper<T> list = new(items);
        int[] bad = new int[] { items.Length + 1, items.Length + 2, int.MaxValue, -1, -2, int.MinValue };
        for (int i = 0; i < bad.Length; i++)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(bad[i], items[0])); //"ArgumentOutOfRangeException expected."
        }
    }

    public void InsertRangeIEnumerable(T[] itemsX, T[] itemsY, int index, int repeat, Func<T[], IEnumerable<T>> constructIEnumerable)
    {
        ValueListWrapper<T> list = new(constructIEnumerable(itemsX));

        for (int i = 0; i < repeat; i++)
        {
            list.InsertRange(index, constructIEnumerable(itemsY));
        }

        foreach (T item in itemsY)
        {
            Assert.Contains(item, list); //"Should contain the item."
        }
        Assert.Equal(list.Count, itemsX.Length + (itemsY.Length * repeat)); //"Should have the same result."

        for (int i = 0; i < index; i++)
        {
            Assert.Equal(list[i], itemsX[i]); //"Should have the same result."
        }

        for (int i = index; i < index + (itemsY.Length * repeat); i++)
        {
            Assert.Equal(list[i], itemsY[(i - index) % itemsY.Length]); //"Should have the same result."
        }

        for (int i = index + (itemsY.Length * repeat); i < list.Count; i++)
        {
            Assert.Equal(list[i], itemsX[i - (itemsY.Length * repeat)]); //"Should have the same result."
        }

        //InsertRange into itself
        list = new ValueListWrapper<T>(constructIEnumerable(itemsX));
        list.InsertRange(index, list);

        foreach (T item in itemsX)
        {
            Assert.Contains(item, list); //"Should contain the item."
        }
        Assert.Equal(list.Count, itemsX.Length + (itemsX.Length)); //"Should have the same result."

        for (int i = 0; i < index; i++)
        {
            Assert.Equal(list[i], itemsX[i]); //"Should have the same result."
        }

        for (int i = index; i < index + (itemsX.Length); i++)
        {
            Assert.Equal(list[i], itemsX[(i - index) % itemsX.Length]); //"Should have the same result."
        }

        for (int i = index + (itemsX.Length); i < list.Count; i++)
        {
            Assert.Equal(list[i], itemsX[i - (itemsX.Length)]); //"Should have the same result."
        }
    }

    public void InsertRangeValidations(T[] items, Func<T[], IEnumerable<T>> constructIEnumerable)
    {
        ValueListWrapper<T> list = new(constructIEnumerable(items));
        int[] bad = new int[] { items.Length + 1, items.Length + 2, int.MaxValue, -1, -2, int.MinValue };
        for (int i = 0; i < bad.Length; i++)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => list.InsertRange(bad[i], constructIEnumerable(items))); //"ArgumentOutOfRangeException expected"
        }

        Assert.Throws<ArgumentNullException>(() => list.InsertRange(0, default(IEnumerable<T>))); //"ArgumentNullException expected."
    }

    public IEnumerable<T> ConstructTestEnumerable(T[] items)
    {
        return items;
    }

    public IEnumerable<T> ConstructLazyTestEnumerable(T[] items)
    {
        return ConstructTestEnumerable(items)
            .Select(item => item);
    }

    public IEnumerable<T> ConstructTestList(T[] items)
    {
        return items.ToList();
    }

    public void BasicGetRange(T[] items, int index, int count, bool useSlice)
    {
        ValueListWrapper<T> list = new(items);
        ValueListWrapper<T> range = new(useSlice ? list.Slice(index, count) : list.GetRange(index, count));

        //ensure range is good
        for (int i = 0; i < count; i++)
        {
            Assert.Equal(range[i], items[i + index]); //String.Format("Err_170178aqhbpa Expected item: {0} at: {1} actual: {2}", items[i + index], i, range[i])
        }

        //ensure no side effects
        for (int i = 0; i < items.Length; i++)
        {
            Assert.Equal(list[i], items[i]); //String.Format("Err_00125698ahpap Expected item: {0} at: {1} actual: {2}", items[i], i, list[i])
        }
    }

    public void BasicSliceSyntax(T[] items, int index, int count)
    {
        ValueListWrapper<T> list = new(items);
        ValueListWrapper<T> range = new(list[index..(index + count)]);

        //ensure range is good
        for (int i = 0; i < count; i++)
        {
            Assert.Equal(range[i], items[i + index]); //String.Format("Err_170178aqhbpa Expected item: {0} at: {1} actual: {2}", items[i + index], i, range[i])
        }

        //ensure no side effects
        for (int i = 0; i < items.Length; i++)
        {
            Assert.Equal(list[i], items[i]); //String.Format("Err_00125698ahpap Expected item: {0} at: {1} actual: {2}", items[i], i, list[i])
        }
    }

    public void EnsureRangeIsReference(T[] items, T item, int index, int count, bool useSlice)
    {
        ValueListWrapper<T> list = new(items);
        ValueListWrapper<T> range = new(useSlice ? list[index..(index + count)] : list.GetRange(index, count));
        T tempItem = list[index];
        range[0] = item;
        Assert.Equal(list[index], tempItem); //String.Format("Err_707811hapba Expected item: {0} at: {1} actual: {2}", tempItem, index, list[index])
    }

    public void EnsureThrowsAfterModification(T[] items, T item, int index, int count, bool useSlice)
    {
        ValueListWrapper<T> list = new(items);
        ValueListWrapper<T> range = new(useSlice ? list[index..(index + count)] : list.GetRange(index, count));
        T tempItem = list[index];
        list[index] = item;

        Assert.Equal(range[0], tempItem); //String.Format("Err_1221589ajpa Expected item: {0} at: {1} actual: {2}", tempItem, 0, range[0])
    }

    public void GetRangeValidations(T[] items, bool useSlice)
    {
        //
        //Always send items.Length is even
        //
        ValueListWrapper<T> list = new(items);
        int[] bad = new int[] {  /**/items.Length,1,
                    /**/
                                    items.Length+1,0,
                    /**/
                                    items.Length+1,1,
                    /**/
                                    items.Length,2,
                    /**/
                                    items.Length/2,items.Length/2+1,
                    /**/
                                    items.Length-1,2,
                    /**/
                                    items.Length-2,3,
                    /**/
                                    1,items.Length,
                    /**/
                                    0,items.Length+1,
                    /**/
                                    1,items.Length+1,
                    /**/
                                    2,items.Length,
                    /**/
                                    items.Length/2+1,items.Length/2,
                    /**/
                                    2,items.Length-1,
                    /**/
                                    3,items.Length-2
                                };

        for (int i = 0; i < bad.Length; i++)
        {
            Assert.Throws<ArgumentException>(null, () =>
            {
                int index = bad[i];
                int count = bad[++i];
                return useSlice ? list.Slice(index, count) : list.GetRange(index, count);
            }); //"ArgumentException expected."
        }

        bad = new int[] {
                    /**/
                                    -1,-1,
                    /**/
                                    -1,0,
                    /**/
                                    -1,1,
                    /**/
                                    -1,2,
                    /**/
                                    0,-1,
                    /**/
                                    1,-1,
                    /**/
                                    2,-1
                                };

        for (int i = 0; i < bad.Length; i++)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                int index = bad[i];
                int count = bad[++i];
                return useSlice ? list.Slice(index, count) : list.GetRange(index, count);
            }); //"ArgumentOutOfRangeException expected."
        }
    }

    public void Exists_Verify(T[] items)
    {
        Exists_VerifyVanilla(items);
        Exists_VerifyDuplicates(items);
    }

    public void Exists_VerifyExceptions(T[] items)
    {
        ValueListWrapper<T> list = new();
        Predicate<T> predicate = (T item) => { return true; };

        for (int i = 0; i < items.Length; ++i)
            list.Add(items[i]);

        //[] Verify Null match
        Assert.Throws<ArgumentNullException>(() => list.Exists(null)); //"Err_858ahia Expected null match to throw ArgumentNullException"
    }

    private void Exists_VerifyVanilla(T[] items)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = new();
        Predicate<T> expectedItemDelegate = (T item) => { return expectedItem == null ? item == null : expectedItem.Equals(item); };
        bool typeNullable = default(T) == null;

        for (int i = 0; i < items.Length; ++i)
            list.Add(items[i]);

        //[] Verify Exists returns the correct index
        for (int i = 0; i < items.Length; ++i)
        {
            expectedItem = items[i];

            Assert.True(list.Exists(expectedItemDelegate),
                "Err_282308ahid Verifying Nullable returned FAILED\n");
        }

        //[] Verify Exists returns true if the match returns true on every item
        Assert.True((0 < items.Length) == list.Exists((T item) => { return true; }),
                "Err_548ahid Verify Exists returns 0 if the match returns true on every item FAILED\n");

        //[] Verify Exists returns false if the match returns false on every item
        Assert.True(!list.Exists((T item) => { return false; }),
                "Err_30848ahidi Verify Exists returns -1 if the match returns false on every item FAILED\n");

        //[] Verify with default(T)
        list.Add(default(T));
        Assert.True(list.Exists((T item) => { return item == null ? default(T) == null : item.Equals(default(T)); }),
                "Err_541848ajodi Verify with default(T) FAILED\n");
        list.RemoveAt(list.Count - 1);
    }

    private void Exists_VerifyDuplicates(T[] items)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = new();
        Predicate<T> expectedItemDelegate = (T item) => { return expectedItem == null ? item == null : expectedItem.Equals(item); };

        if (0 < items.Length)
        {
            for (int i = 0; i < items.Length; ++i)
                list.Add(items[i]);

            for (int i = 0; i < items.Length && i < 2; ++i)
                list.Add(items[i]);

            //[] Verify first item is duplicated
            expectedItem = items[0];
            Assert.True(list.Exists(expectedItemDelegate),
                    "Err_2879072qaiadf  Verify first item is duplicated FAILED\n");
        }

        if (1 < items.Length)
        {
            //[] Verify second item is duplicated
            expectedItem = items[1];
            Assert.True(list.Exists(expectedItemDelegate),
                    "Err_4588ajdia Verify second item is duplicated FAILED\n");

            //[] Verify with match that matches more then one item
            Assert.True(list.Exists((T item) => { return item != null && (item.Equals(items[0]) || item.Equals(items[1])); }),
                    "Err_4489ajodoi Verify with match that matches more then one item FAILED\n");
        }
    }

    public void BasicContains(T[] items)
    {
        ValueListWrapper<T> list = new(items);

        for (int i = 0; i < items.Length; i++)
        {
            Assert.Contains(items[i], list); //"Should contain item."
        }
    }

    public void NonExistingValues(T[] itemsX, T[] itemsY)
    {
        ValueListWrapper<T> list = new(itemsX);

        for (int i = 0; i < itemsY.Length; i++)
        {
            Assert.DoesNotContain(itemsY[i], list); //"Should not contain item"
        }
    }

    public void RemovedValues(T[] items)
    {
        ValueListWrapper<T> list = new(items);
        for (int i = 0; i < items.Length; i++)
        {
            list.Remove(items[i]);
            Assert.DoesNotContain(items[i], list); //"Should not contain item"
        }
    }

    public void AddRemoveValues(T[] items)
    {
        ValueListWrapper<T> list = new(items);
        for (int i = 0; i < items.Length; i++)
        {
            list.Add(items[i]);
            list.Remove(items[i]);
            list.Add(items[i]);
            Assert.Contains(items[i], list); //"Should contain item."
        }
    }

    public void MultipleValues(T[] items, int times)
    {
        ValueListWrapper<T> list = new(items);

        for (int i = 0; i < times; i++)
        {
            list.Add(items[items.Length / 2]);
        }

        for (int i = 0; i < times + 1; i++)
        {
            Assert.Contains(items[items.Length / 2], list); //"Should contain item."
            list.Remove(items[items.Length / 2]);
        }
        Assert.DoesNotContain(items[items.Length / 2], list); //"Should not contain item"
    }

    public void ContainsNullWhenReference(T[] items, T value)
    {
        if ((object)value != null)
        {
            throw new ArgumentException("invalid argument passed to testcase");
        }

        ValueListWrapper<T> list = new(items);
        list.Add(value);
        Assert.Contains(value, list); //"Should contain item."
    }

    public void ClearEmptyList()
    {
        ValueListWrapper<T> list = new();
        Assert.Empty(list); //"Should be equal to 0"
        list.Clear();
        Assert.Empty(list); //"Should be equal to 0."
    }

    public void ClearMultipleTimesEmptyList(int times)
    {
        ValueListWrapper<T> list = new();
        Assert.Empty(list); //"Should be equal to 0."
        for (int i = 0; i < times; i++)
        {
            list.Clear();
            Assert.Empty(list); //"Should be equal to 0."
        }
    }

    public void ClearNonEmptyList(T[] items)
    {
        ValueListWrapper<T> list = new(items);
        list.Clear();
        Assert.Empty(list); //"Should be equal to 0."
    }

    public void ClearMultipleTimesNonEmptyList(T[] items, int times)
    {
        ValueListWrapper<T> list = new(items);
        for (int i = 0; i < times; i++)
        {
            list.Clear();
            Assert.Empty(list); //"Should be equal to 0."
        }
    }

    public void TrueForAll_VerifyVanilla(T[] items)
    {
        T expectedItem = default(T);
        ValueListWrapper<T> list = new();
        Predicate<T> expectedItemDelegate = delegate (T item)
        { return expectedItem == null ? item != null : !expectedItem.Equals(item); };
        bool typeNullable = default(T) == null;

        for (int i = 0; i < items.Length; ++i)
            list.Add(items[i]);

        //[] Verify TrueForAll looks at every item
        for (int i = 0; i < items.Length; ++i)
        {
            expectedItem = items[i];
            Assert.False(list.TrueForAll(expectedItemDelegate)); //"Err_282308ahid Verify TrueForAll looks at every item FAILED\n"
        }

        //[] Verify TrueForAll returns true if the match returns true on every item
        Assert.True(list.TrueForAll(delegate (T item)
        { return true; }),
                "Err_548ahid Verify TrueForAll returns true if the match returns true on every item FAILED\n");

        //[] Verify TrueForAll returns false if the match returns false on every item
        Assert.True((0 == items.Length) == list.TrueForAll(delegate (T item)
        { return false; }),
                "Err_30848ahidi Verify TrueForAll returns " + (0 == items.Length) + " if the match returns false on every item FAILED\n");
    }

    public void TrueForAll_VerifyExceptions(T[] items)
    {
        var list = new ValueListWrapper<T>(items);
        Assert.True(list.TrueForAll(item => true));
        Assert.Throws<ArgumentNullException>(() => list.TrueForAll(null)); //"Err_858ahia Expected null match to throw ArgumentNullException"
    }

    public void BasicToArray(T[] items)
    {
        ValueListWrapper<T> list = new(items);

        T[] arr = list.ToArray();

        for (int i = 0; i < items.Length; i++)
        {
            Assert.Equal(((object)arr[i]), items[i]); //"Should be equal."
        }
    }

    public void EnsureNotUnderlyingToArray(T[] items, T item)
    {
        ValueListWrapper<T> list = new(items);
        T[] arr = list.ToArray();
        list[0] = item;
        if (((object)arr[0]) == null)
            Assert.NotNull(list[0]); //"Should NOT be null"
        else
            Assert.NotEqual(((object)arr[0]), list[0]); //"Should NOT be equal."
    }
}

#nullable restore
