using Spanned.Tests.TestUtilities;

namespace Spanned.Tests.Collections.Generic;

/// <summary>
/// Contains tests that ensure the correctness of any class that implements the generic
/// IDictionary interface
/// </summary>
public abstract partial class IDictionaryTests<TKey, TValue> : ICollectionTests<KeyValuePair<TKey, TValue>>
{
    /// <summary>
    /// Creates an instance of an IDictionary{TKey, TValue} that can be used for testing.
    /// </summary>
    /// <returns>An instance of an IDictionary{TKey, TValue} that can be used for testing.</returns>
    protected abstract IDictionary<TKey, TValue> GenericIDictionaryFactory();

    /// <summary>
    /// Creates an instance of an IDictionary{TKey, TValue} that can be used for testing, with a specific comparer.
    /// </summary>
    /// <param name="comparer">The comparer to use with the dictionary.</param>
    /// <returns>An instance of an IDictionary{TKey, TValue} that can be used for testing, or null if the tested type doesn't support an equality comparer.</returns>
    protected virtual IDictionary<TKey, TValue> GenericIDictionaryFactory(IEqualityComparer<TKey> comparer) => null!;

    /// <summary>
    /// Creates an instance of an IDictionary{TKey, TValue} that can be used for testing.
    /// </summary>
    /// <param name="count">The number of items that the returned IDictionary{TKey, TValue} contains.</param>
    /// <returns>An instance of an IDictionary{TKey, TValue} that can be used for testing.</returns>
    protected virtual IDictionary<TKey, TValue> GenericIDictionaryFactory(int count)
    {
        IDictionary<TKey, TValue> collection = GenericIDictionaryFactory();
        AddToCollection(collection, count);
        return collection;
    }

    /// <summary>
    /// To be implemented in the concrete Dictionary test classes. Creates an instance of TKey that
    /// is dependent only on the seed passed as input and will return the same key on repeated
    /// calls with the same seed.
    /// </summary>
    protected abstract TKey CreateTKey(int seed);

    /// <summary>
    /// To be implemented in the concrete Dictionary test classes. Creates an instance of TValue that
    /// is dependent only on the seed passed as input and will return the same value on repeated
    /// calls with the same seed.
    /// </summary>
    protected abstract TValue CreateTValue(int seed);

    /// <summary>
    /// Helper method to get a key that doesn't already exist within the dictionary given
    /// </summary>
    protected TKey GetNewKey(IDictionary<TKey, TValue> dictionary)
    {
        int seed = 840;
        TKey missingKey = CreateTKey(seed++);
        while (dictionary.ContainsKey(missingKey) || Equals(missingKey, default(TKey)))
            missingKey = CreateTKey(seed++);
        return missingKey;
    }

    /// <summary>
    /// For a Dictionary, the key comparer is primarily important rather than the KeyValuePair. For this
    /// reason, we rely only on the KeyComparer methods instead of the GetIComparer methods.
    /// </summary>
    public virtual IEqualityComparer<TKey> GetKeyIEqualityComparer()
    {
        return EqualityComparer<TKey>.Default;
    }

    /// <summary>
    /// For a Dictionary, the key comparer is primarily important rather than the KeyValuePair. For this
    /// reason, we rely only on the KeyComparer methods instead of the GetIComparer methods.
    /// </summary>
    public virtual IComparer<TKey> GetKeyIComparer()
    {
        return Comparer<TKey>.Default;
    }

    /// <summary>
    /// Class to provide an indirection around a Key comparer. Allows us to use a key comparer as a KeyValuePair comparer
    /// by only looking at the key of a KeyValuePair.
    /// </summary>
    public class KVPComparer : IEqualityComparer<KeyValuePair<TKey, TValue>>, IComparer<KeyValuePair<TKey, TValue>>
    {
        private readonly IComparer<TKey> _comparer;
        private readonly IEqualityComparer<TKey> _equalityComparer;

        public KVPComparer(IComparer<TKey> comparer, IEqualityComparer<TKey> eq)
        {
            _comparer = comparer;
            _equalityComparer = eq;
        }

        public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) => _comparer.Compare(x.Key, y.Key);

        public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) => _equalityComparer.Equals(x.Key, y.Key);

        public int GetHashCode(KeyValuePair<TKey, TValue> obj) => _equalityComparer.GetHashCode(obj.Key!);
    }

    protected override ICollection<KeyValuePair<TKey, TValue>> GenericICollectionFactory()
    {
        return GenericIDictionaryFactory();
    }

    protected override ICollection<KeyValuePair<TKey, TValue>> GenericICollectionFactory(int count)
    {
        return GenericIDictionaryFactory(count);
    }

    protected override bool DefaultValueAllowed => false;

    protected override bool DuplicateValuesAllowed => false;

    protected override void AddToCollection(ICollection<KeyValuePair<TKey, TValue>> collection, int numberOfItemsToAdd)
    {
        Assert.False(IsReadOnly);
        int seed = 12353;
        IDictionary<TKey, TValue> casted = (IDictionary<TKey, TValue>)collection;
        int initialCount = casted.Count;
        while (casted.Count - initialCount < numberOfItemsToAdd)
        {
            KeyValuePair<TKey, TValue> toAdd = CreateT(seed++);
            while (casted.ContainsKey(toAdd.Key) || Enumerable.Contains(InvalidValues, toAdd))
                toAdd = CreateT(seed++);
            collection.Add(toAdd);
        }
    }

    protected override IEqualityComparer<KeyValuePair<TKey, TValue>> GetIEqualityComparer()
    {
        return new KVPComparer(GetKeyIComparer(), GetKeyIEqualityComparer());
    }

    protected override IComparer<KeyValuePair<TKey, TValue>> GetIComparer()
    {
        return new KVPComparer(GetKeyIComparer(), GetKeyIEqualityComparer());
    }

    /// <summary>
    /// Returns a set of ModifyEnumerable delegates that modify the enumerable passed to them.
    /// </summary>
    protected override IEnumerable<ModifyEnumerable> GetModifyEnumerables(ModifyOperation operations)
    {
        if ((operations & ModifyOperation.Add) == ModifyOperation.Add)
        {
            yield return (IEnumerable<KeyValuePair<TKey, TValue>> enumerable) =>
            {
                IDictionary<TKey, TValue> casted = (IDictionary<TKey, TValue>)enumerable;
                casted.Add(CreateTKey(12), CreateTValue(5123));
                return true;
            };
        }
        if ((operations & ModifyOperation.Insert) == ModifyOperation.Insert)
        {
            yield return (IEnumerable<KeyValuePair<TKey, TValue>> enumerable) =>
            {
                IDictionary<TKey, TValue> casted = (IDictionary<TKey, TValue>)enumerable;
                casted[CreateTKey(541)] = CreateTValue(12);
                return true;
            };
        }
        if ((operations & ModifyOperation.Overwrite) == ModifyOperation.Overwrite)
        {
            yield return (IEnumerable<KeyValuePair<TKey, TValue>> enumerable) =>
            {
                IDictionary<TKey, TValue> casted = (IDictionary<TKey, TValue>)enumerable;
                if (casted.Count > 0)
                {
                    IEnumerator<TKey> keys = casted.Keys.GetEnumerator();
                    keys.MoveNext();
                    casted[keys.Current] = CreateTValue(12);
                    return true;
                }
                return false;
            };
        }
        if ((operations & ModifyOperation.Remove) == ModifyOperation.Remove)
        {
            yield return (IEnumerable<KeyValuePair<TKey, TValue>> enumerable) =>
            {
                IDictionary<TKey, TValue> casted = (IDictionary<TKey, TValue>)enumerable;
                if (casted.Count > 0)
                {
                    IEnumerator<TKey> keys = casted.Keys.GetEnumerator();
                    keys.MoveNext();
                    casted.Remove(keys.Current);
                    return true;
                }
                return false;
            };
        }
        if ((operations & ModifyOperation.Clear) == ModifyOperation.Clear)
        {
            yield return (IEnumerable<KeyValuePair<TKey, TValue>> enumerable) =>
            {
                IDictionary<TKey, TValue> casted = (IDictionary<TKey, TValue>)enumerable;
                if (casted.Count > 0)
                {
                    casted.Clear();
                    return true;
                }
                return false;
            };
        }
    }

    /// <summary>
    /// Used in IDictionary_Values_Enumeration_ParentDictionaryModifiedInvalidates and
    /// IDictionary_Keys_Enumeration_ParentDictionaryModifiedInvalidates.
    /// Some collections (e.g. ConcurrentDictionary) do not throw an InvalidOperationException
    /// when enumerating the Keys or Values property and the parent is modified.
    /// </summary>
    protected virtual bool IDictionary_Keys_Values_Enumeration_ThrowsInvalidOperation_WhenParentModified => false;

    /// <summary>
    /// Used in IDictionary_Values_ModifyingTheDictionaryUpdatesTheCollection and
    /// IDictionary_Keys_ModifyingTheDictionaryUpdatesTheCollection.
    /// Some collections (e.g ConcurrentDictionary) use iterators in the Keys and Values properties,
    /// and do not respond to updates in the base collection.
    /// </summary>
    protected virtual bool IDictionary_Keys_Values_ModifyingTheDictionaryUpdatesTheCollection => false;

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ItemGet_DefaultKey(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            if (!DefaultValueAllowed)
            {
                Assert.Throws<ArgumentNullException>(() => dictionary[default!]);
            }
            else
            {
                TValue value = CreateTValue(3452);
                dictionary[default!] = value;
                Assert.Equal(value, dictionary[default!]);
            }
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ItemGet_MissingNonDefaultKey_ThrowsKeyNotFoundException(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        TKey missingKey = GetNewKey(dictionary);
        Assert.Throws<KeyNotFoundException>(() => dictionary[missingKey]);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ItemGet_MissingDefaultKey_ThrowsKeyNotFoundException(int count)
    {
        if (DefaultValueAllowed && !IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = default!;
            while (dictionary.ContainsKey(missingKey))
                dictionary.Remove(missingKey);
            Assert.Throws<KeyNotFoundException>(() => dictionary[missingKey]);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ItemGet_PresentKeyReturnsCorrectValue(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
        {
            Assert.Equal(pair.Value, dictionary[pair.Key]);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ItemSet_DefaultKey(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            if (!DefaultValueAllowed)
            {
                Assert.Throws<ArgumentNullException>(() => dictionary[default!] = CreateTValue(3));
            }
            else
            {
                TValue value = CreateTValue(3452);
                dictionary[default!] = value;
                Assert.Equal(value, dictionary[default!]);
            }
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ItemSet_OnReadOnlyDictionary_ThrowsNotSupportedException(int count)
    {
        if (IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            Assert.Throws<NotSupportedException>(() => dictionary[missingKey] = CreateTValue(5312));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ItemSet_AddsNewValueWhenNotPresent(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            dictionary[missingKey] = CreateTValue(543);
            Assert.Equal(count + 1, dictionary.Count);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ItemSet_ReplacesExistingValueWhenPresent(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey existingKey = GetNewKey(dictionary);
            dictionary.Add(existingKey, CreateTValue(5342));
            TValue newValue = CreateTValue(1234);
            dictionary[existingKey] = newValue;
            Assert.Equal(count + 1, dictionary.Count);
            Assert.Equal(newValue, dictionary[existingKey]);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Keys_ContainsAllCorrectKeys(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        IEnumerable<TKey> expected = dictionary.Select((pair) => pair.Key);
        Assert.True(expected.SequenceEqual(dictionary.Keys));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Keys_ModifyingTheDictionaryUpdatesTheCollection(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            ICollection<TKey> keys = dictionary.Keys;
            int previousCount = keys.Count;
            if (count > 0)
                Assert.NotEmpty(keys);
            dictionary.Clear();
            if (IDictionary_Keys_Values_ModifyingTheDictionaryUpdatesTheCollection)
            {
                Assert.Empty(keys);
            }
            else
            {
                Assert.Equal(previousCount, keys.Count);
            }
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Keys_Enumeration_ParentDictionaryModifiedInvalidates(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            ICollection<TKey> keys = dictionary.Keys;
            IEnumerator<TKey> keysEnum = keys.GetEnumerator();
            dictionary.Add(GetNewKey(dictionary), CreateTValue(3432));
            if (count == 0 ? Enumerator_Empty_ModifiedDuringEnumeration_ThrowsInvalidOperationException : IDictionary_Keys_Values_Enumeration_ThrowsInvalidOperation_WhenParentModified)
            {
                Assert.Throws<InvalidOperationException>(() => keysEnum.MoveNext());
                Assert.Throws<InvalidOperationException>(() => keysEnum.Reset());
            }
            else
            {
                if (keysEnum.MoveNext())
                {
                    _ = keysEnum.Current;
                }
                keysEnum.Reset();
            }
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Keys_IsReadOnly(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        ICollection<TKey> keys = dictionary.Keys;
        Assert.True(keys.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => keys.Add(CreateTKey(11)));
        Assert.Throws<NotSupportedException>(keys.Clear);
        Assert.Throws<NotSupportedException>(() => keys.Remove(CreateTKey(11)));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Values_ContainsAllCorrectValues(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        IEnumerable<TValue> expected = dictionary.Select((pair) => pair.Value);
        Assert.True(expected.SequenceEqual(dictionary.Values));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Values_IncludeDuplicatesMultipleTimes(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            int seed = 431;
            foreach (KeyValuePair<TKey, TValue> pair in dictionary.ToList())
            {
                TKey missingKey = CreateTKey(seed++);
                while (dictionary.ContainsKey(missingKey))
                    missingKey = CreateTKey(seed++);
                dictionary.Add(missingKey, pair.Value);
            }
            Assert.Equal(count * 2, dictionary.Values.Count);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Values_ModifyingTheDictionaryUpdatesTheCollection(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        ICollection<TValue> values = dictionary.Values;
        int previousCount = values.Count;
        if (count > 0)
            Assert.NotEmpty(values);

        if (!IsReadOnly)
        {
            dictionary.Clear();
            if (IDictionary_Keys_Values_ModifyingTheDictionaryUpdatesTheCollection)
            {
                Assert.Empty(values);
            }
            else
            {
                Assert.Equal(previousCount, values.Count);
            }
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Values_Enumeration_ParentDictionaryModifiedInvalidates(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            ICollection<TValue> values = dictionary.Values;
            IEnumerator<TValue> valuesEnum = values.GetEnumerator();
            dictionary.Add(GetNewKey(dictionary), CreateTValue(3432));
            if (count == 0 ? Enumerator_Empty_ModifiedDuringEnumeration_ThrowsInvalidOperationException : IDictionary_Keys_Values_Enumeration_ThrowsInvalidOperation_WhenParentModified)
            {
                Assert.Throws<InvalidOperationException>(() => valuesEnum.MoveNext());
                Assert.Throws<InvalidOperationException>(() => valuesEnum.Reset());
            }
            else
            {
                if (valuesEnum.MoveNext())
                {
                    _ = valuesEnum.Current;
                }
                valuesEnum.Reset();
            }
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Values_IsReadOnly(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        ICollection<TValue> values = dictionary.Values;
        Assert.True(values.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => values.Add(CreateTValue(11)));
        Assert.Throws<NotSupportedException>(() => values.Clear());
        Assert.Throws<NotSupportedException>(() => values.Remove(CreateTValue(11)));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Add_OnReadOnlyDictionary_ThrowsNotSupportedException(int count)
    {
        if (IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            Assert.Throws<NotSupportedException>(() => dictionary.Add(CreateTKey(0), CreateTValue(0)));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Add_DefaultKey_DefaultValue(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        TKey missingKey = default!;
        TValue value = default!;
        if (DefaultValueAllowed && !IsReadOnly)
        {
            dictionary.Add(missingKey, value);
            Assert.Equal(count + 1, dictionary.Count);
            Assert.Equal(value, dictionary[missingKey]);
        }
        else if (!IsReadOnly)
        {
            Assert.Throws<ArgumentNullException>(() => dictionary.Add(missingKey, value));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Add_DefaultKey_NonDefaultValue(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        TKey missingKey = default!;
        TValue value = CreateTValue(1456);
        if (DefaultValueAllowed && !IsReadOnly)
        {
            dictionary.Add(missingKey, value);
            Assert.Equal(count + 1, dictionary.Count);
            Assert.Equal(value, dictionary[missingKey]);
        }
        else if (!IsReadOnly)
        {
            Assert.Throws<ArgumentNullException>(() => dictionary.Add(missingKey, value));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Add_NonDefaultKey_DefaultValue(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            TValue value = default!;
            dictionary.Add(missingKey, value);
            Assert.Equal(count + 1, dictionary.Count);
            Assert.Equal(value, dictionary[missingKey]);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Add_NonDefaultKey_NonDefaultValue(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            TValue value = CreateTValue(1342);
            dictionary.Add(missingKey, value);
            Assert.Equal(count + 1, dictionary.Count);
            Assert.Equal(value, dictionary[missingKey]);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Add_DuplicateValue(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            int seed = 321;
            TValue duplicate = CreateTValue(seed++);
            while (dictionary.Values.Contains(duplicate))
                duplicate = CreateTValue(seed++);
            dictionary.Add(GetNewKey(dictionary), duplicate);
            dictionary.Add(GetNewKey(dictionary), duplicate);
            Assert.Equal(2, dictionary.Values.Count((value) => value!.Equals(duplicate)));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Add_DuplicateKey(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            dictionary.Add(missingKey, CreateTValue(34251));
            Assert.Throws<ArgumentException>(() => dictionary.Add(missingKey, CreateTValue(134)));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Add_DistinctValuesWithHashCollisions(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(new ConstantHashCodeEqualityComparer<TKey>());
            if (dictionary != null)
            {
                AddToCollection(dictionary, count);
                Assert.Equal(count, dictionary.Count);
            }
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ContainsKey_ValidKeyNotContainedInDictionary(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            Assert.False(dictionary.ContainsKey(missingKey));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ContainsKey_ValidKeyContainedInDictionary(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            dictionary.Add(missingKey, CreateTValue(34251));
            Assert.True(dictionary.ContainsKey(missingKey));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ContainsKey_DefaultKeyNotContainedInDictionary(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        if (DefaultValueAllowed)
        {
            if (!IsReadOnly)
            {
                TKey missingKey = default!;
                while (dictionary.ContainsKey(missingKey))
                    dictionary.Remove(missingKey);
                Assert.False(dictionary.ContainsKey(missingKey));
            }
        }
        else
        {
            Assert.Throws<ArgumentNullException>(() => dictionary.ContainsKey(default!));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_ContainsKey_DefaultKeyContainedInDictionary(int count)
    {
        if (DefaultValueAllowed && !IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = default!;
            if (!dictionary.ContainsKey(missingKey))
                dictionary.Add(missingKey, CreateTValue(5341));
            Assert.True(dictionary.ContainsKey(missingKey));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_RemoveKey_OnReadOnlyDictionary_ThrowsNotSupportedException(int count)
    {
        if (IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            Assert.Throws<NotSupportedException>(() => dictionary.Remove(CreateTKey(0)));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_RemoveKey_EveryKey(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            Assert.All(dictionary.Keys.ToList(), key => Assert.True(dictionary.Remove(key)));
            Assert.Empty(dictionary);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_RemoveKey_ValidKeyNotContainedInDictionary(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            Assert.False(dictionary.Remove(missingKey));
            Assert.Equal(count, dictionary.Count);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_RemoveKey_ValidKeyContainedInDictionary(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            dictionary.Add(missingKey, CreateTValue(34251));
            Assert.True(dictionary.Remove(missingKey));
            Assert.Equal(count, dictionary.Count);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_RemoveKey_DefaultKeyNotContainedInDictionary(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            if (DefaultValueAllowed)
            {
                TKey missingKey = default!;
                while (dictionary.ContainsKey(missingKey))
                    dictionary.Remove(missingKey);
                Assert.False(dictionary.Remove(missingKey));
            }
            else
            {
                Assert.Throws<ArgumentNullException>(() => dictionary.Remove(default!));
            }
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_RemoveKey_DefaultKeyContainedInDictionary(int count)
    {
        if (DefaultValueAllowed && !IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = default!;
            dictionary.TryAdd(missingKey, CreateTValue(5341));
            Assert.True(dictionary.Remove(missingKey));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_TryGetValue_ValidKeyNotContainedInDictionary(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        TKey missingKey = GetNewKey(dictionary);
        TValue value = CreateTValue(5123);
        Assert.False(dictionary.TryGetValue(missingKey, out _));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_TryGetValue_ValidKeyContainedInDictionary(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            TValue value = CreateTValue(5123);
            dictionary.TryAdd(missingKey, value);
            Assert.True(dictionary.TryGetValue(missingKey, out TValue? outValue));
            Assert.Equal(value, outValue);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_TryGetValue_DefaultKeyNotContainedInDictionary(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        if (DefaultValueAllowed)
        {
            if (!IsReadOnly)
            {
                TKey missingKey = default!;
                while (dictionary.ContainsKey(missingKey))
                    dictionary.Remove(missingKey);
                Assert.False(dictionary.TryGetValue(missingKey, out _));
            }
        }
        else
        {
            Assert.Throws<ArgumentNullException>(() => dictionary.TryGetValue(default!, out _));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_TryGetValue_DefaultKeyContainedInDictionary(int count)
    {
        if (DefaultValueAllowed && !IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = default!;
            TValue value = CreateTValue(5123);
            dictionary.TryAdd(missingKey, value);
            Assert.True(dictionary.TryGetValue(missingKey, out TValue? outValue));
            Assert.Equal(value, outValue);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ICollection_Contains_ValidPresentKeyWithDefaultValue(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            dictionary.Add(missingKey, default!);
            Assert.True(dictionary.Contains(new KeyValuePair<TKey, TValue>(missingKey, default!)));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ICollection_Remove_ValidPresentKeyWithDifferentValue(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            TValue present = CreateTValue(324);
            TValue missing = CreateTValue(5612);
            while (present!.Equals(missing))
                missing = CreateTValue(5612);
            dictionary.Add(missingKey, present);
            Assert.False(dictionary.Remove(new KeyValuePair<TKey, TValue>(missingKey, missing)));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ICollection_Contains_ValidPresentKeyWithDifferentValue(int count)
    {
        if (!IsReadOnly)
        {
            IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
            TKey missingKey = GetNewKey(dictionary);
            TValue present = CreateTValue(324);
            TValue missing = CreateTValue(5612);
            while (present!.Equals(missing))
                missing = CreateTValue(5612);
            dictionary.Add(missingKey, present);
            Assert.False(dictionary.Contains(new KeyValuePair<TKey, TValue>(missingKey, missing)));
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IDictionary_Contains_DefaultValueWhenNotAllowed(int count)
    {
        ICollection<KeyValuePair<TKey, TValue>> collection = GenericIDictionaryFactory(count);
        if (!DefaultValueAllowed && !IsReadOnly)
        {
            if (DefaultValueWhenNotAllowed_Throws)
                Assert.Throws<ArgumentNullException>(() => collection.Contains(default));
            else
                Assert.False(collection.Remove(default));
        }
    }
}
