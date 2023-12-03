using Spanned.Tests.TestUtilities;

namespace Spanned.Tests.Collections.Generic.ValueDictionary;

#nullable disable

public partial class ValueDictionaryTests_String_String : ValueDictionaryTests<string, string>
{
    protected override KeyValuePair<string, string> CreateT(int seed) => new(CreateTKey(seed), CreateTKey(seed + 500));

    protected override string CreateTKey(int seed)
    {
        int stringLength = seed % 10 + 5;
        Random rand = new(seed);
        byte[] bytes1 = new byte[stringLength];
        rand.NextBytes(bytes1);
        return Convert.ToBase64String(bytes1);
    }

    protected override string CreateTValue(int seed) => CreateTKey(seed);
}

public class ValueDictionaryTests_Int_Int : ValueDictionaryTests<int, int>
{
    protected override bool DefaultValueAllowed => true;

    protected override KeyValuePair<int, int> CreateT(int seed)
    {
        Random rand = new(seed);
        return new KeyValuePair<int, int>(rand.Next(), rand.Next());
    }

    protected override int CreateTKey(int seed) => new Random(seed).Next();

    protected override int CreateTValue(int seed) => CreateTKey(seed);
}

public class ValueDictionaryTests_SimpleInt_Int_With_StructuralComparerWrapper_SimpleInt : ValueDictionaryTests<SimpleInt, int>
{
    protected override bool DefaultValueAllowed { get { return true; } }

    public override IEqualityComparer<SimpleInt> GetKeyIEqualityComparer() => new StructuralComparerWrapper_SimpleInt();

    public override IComparer<SimpleInt> GetKeyIComparer() => new StructuralComparerWrapper_SimpleInt();

    protected override SimpleInt CreateTKey(int seed) => new(new Random(seed).Next());

    protected override int CreateTValue(int seed) => new Random(seed).Next();

    protected override KeyValuePair<SimpleInt, int> CreateT(int seed) => new(CreateTKey(seed), CreateTValue(seed));
}

/// <summary>
/// Contains tests that ensure the correctness of the ValueDictionary class.
/// </summary>
public abstract class ValueDictionaryTests<TKey, TValue> : IDictionaryTests<TKey, TValue>
{
    protected override IDictionary<TKey, TValue> GenericIDictionaryFactory() => new ValueDictionaryWrapper<TKey, TValue>();

    protected override IDictionary<TKey, TValue> GenericIDictionaryFactory(IEqualityComparer<TKey> comparer) => new ValueDictionaryWrapper<TKey, TValue>(comparer);

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_Constructor_IDictionary(int count)
    {
        IDictionary<TKey, TValue> source = GenericIDictionaryFactory(count);
        IDictionary<TKey, TValue> copied = new ValueDictionaryWrapper<TKey, TValue>(source);

        Assert.Equal(source, copied);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_Constructor_IValueDictionary_IEqualityComparer(int count)
    {
        IEqualityComparer<TKey> comparer = GetKeyIEqualityComparer();
        IDictionary<TKey, TValue> source = GenericIDictionaryFactory(count);
        ValueDictionaryWrapper<TKey, TValue> copied = new(source, comparer);

        Assert.Equal(source, copied);
        Assert.Equal(comparer, copied.Comparer);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_Constructor_IEqualityComparer(int count)
    {
        IEqualityComparer<TKey> comparer = GetKeyIEqualityComparer();
        IDictionary<TKey, TValue> source = GenericIDictionaryFactory(count);
        ValueDictionaryWrapper<TKey, TValue> copied = new(source, comparer);

        Assert.Equal(source, copied);
        Assert.Equal(comparer, copied.Comparer);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_Constructor_Int(int capacity)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = new(capacity);

        Assert.Empty(dictionary);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_Constructor_Int_IEqualityComparer(int count)
    {
        IEqualityComparer<TKey> comparer = GetKeyIEqualityComparer();
        ValueDictionaryWrapper<TKey, TValue> dictionary = new(count, comparer);

        Assert.Empty(dictionary);
        Assert.Equal(comparer, dictionary.Comparer);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void ValueDictionary_CreateWithCapacity_CapacityAtLeastPassedValue(int capacity)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = new(capacity);

        Assert.True(dictionary.Capacity >= capacity);
    }

    public void DictResized_CapacityChanged()
    {
        ValueDictionaryWrapper<TKey, TValue> dict = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(1);
        int initialCapacity = dict.Capacity;

        int seed = 85877;
        for (int i = 0; i < dict.Capacity; i++)
        {
            dict.Add(CreateTKey(seed++), CreateTValue(seed++));
        }

        int afterCapacity = dict.Capacity;

        Assert.True(afterCapacity > initialCapacity);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_ContainsValue_NotPresent(int count)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);
        int seed = 4315;
        TValue notPresent = CreateTValue(seed++);
        while (((IDictionary<TKey, TValue>)dictionary).Values.Contains(notPresent))
            notPresent = CreateTValue(seed++);

        Assert.False(dictionary.ContainsValue(notPresent));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_ContainsValue_Present(int count)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);
        int seed = 4315;
        KeyValuePair<TKey, TValue> notPresent = CreateT(seed++);
        while (dictionary.Contains(notPresent))
            notPresent = CreateT(seed++);
        dictionary.Add(notPresent.Key, notPresent.Value);

        Assert.True(dictionary.ContainsValue(notPresent.Value));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_ContainsValue_DefaultValueNotPresent(int count)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);

        Assert.False(dictionary.ContainsValue(default));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_ContainsValue_DefaultValuePresent(int count)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);
        int seed = 4315;
        TKey notPresent = CreateTKey(seed++);
        while (dictionary.ContainsKey(notPresent))
            notPresent = CreateTKey(seed++);
        dictionary.Add(notPresent, default);

        Assert.True(dictionary.ContainsValue(default));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IReadOnlyValueDictionary_Keys_ContainsAllCorrectKeys(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        IEnumerable<TKey> expected = dictionary.Select((pair) => pair.Key);
        IEnumerable<TKey> keys = ((IReadOnlyDictionary<TKey, TValue>)dictionary).Keys;

        Assert.True(expected.SequenceEqual(keys));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void IReadOnlyValueDictionary_Values_ContainsAllCorrectValues(int count)
    {
        IDictionary<TKey, TValue> dictionary = GenericIDictionaryFactory(count);
        IEnumerable<TValue> expected = dictionary.Select((pair) => pair.Value);
        IEnumerable<TValue> values = ((IReadOnlyDictionary<TKey, TValue>)dictionary).Values;
        Assert.True(expected.SequenceEqual(values));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_RemoveKey_ValidKeyNotContainedInDictionary(int count)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);
        TKey missingKey = GetNewKey(dictionary);

        Assert.False(dictionary.Remove(missingKey, out TValue value));
        Assert.Equal(count, dictionary.Count);
        Assert.Equal(default, value);
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_RemoveKey_ValidKeyContainedInDictionary(int count)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);
        TKey missingKey = GetNewKey(dictionary);
        TValue inValue = CreateTValue(count);
        dictionary.Add(missingKey, inValue);

        Assert.True(dictionary.Remove(missingKey, out TValue outValue));
        Assert.Equal(count, dictionary.Count);
        Assert.Equal(inValue, outValue);
        Assert.False(dictionary.TryGetValue(missingKey, out outValue));
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_RemoveKey_DefaultKeyNotContainedInDictionary(int count)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);

        if (DefaultValueAllowed)
        {
            TKey missingKey = default;
            while (dictionary.ContainsKey(missingKey))
                dictionary.Remove(missingKey);

            Assert.False(dictionary.Remove(missingKey, out TValue outValue));
            Assert.Equal(default, outValue);
        }
        else
        {
            TValue initValue = CreateTValue(count);
            TValue outValue = initValue;

            Assert.Throws<ArgumentNullException>(() => dictionary.Remove(default, out _));
            Assert.Equal(initValue, outValue);
        }
    }

    [Theory]
    [MemberData(nameof(ValidCollectionSizes))]
    public void ValueDictionary_RemoveKey_DefaultKeyContainedInDictionary(int count)
    {
        if (DefaultValueAllowed)
        {
            ValueDictionaryWrapper<TKey, TValue> dictionary = (ValueDictionaryWrapper<TKey, TValue>)(GenericIDictionaryFactory(count));
            TKey missingKey = default;
            dictionary.TryAdd(missingKey, default);

            Assert.True(dictionary.Remove(missingKey, out _));
        }
    }

    [Fact]
    public void ValueDictionary_Remove_RemoveLastEnumerationFinishes()
    {
        ValueDictionaryWrapper<TKey, TValue> dict = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(3);
        TKey key = default;

        using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = dict.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                key = enumerator.Current.Key;
            }
        }

        using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = dict.GetEnumerator())
        {
            enumerator.MoveNext();
            enumerator.MoveNext();
            dict.Remove(key);

            Assert.False(enumerator.MoveNext());
        }
    }

    [Fact]
    public void EnsureCapacity_NegativeCapacityRequested_Throws()
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = new();

        Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.EnsureCapacity(-1));
    }

    [Fact]
    public void EnsureCapacity_DictionaryNotInitialized_RequestedZero_ReturnsZero()
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = new();

        Assert.Equal(0, dictionary.EnsureCapacity(0));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void EnsureCapacity_DictionaryNotInitialized_RequestedNonZero_CapacityIsSetToAtLeastTheRequested(int requestedCapacity)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = new();

        Assert.InRange(dictionary.EnsureCapacity(requestedCapacity), requestedCapacity, int.MaxValue);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(7)]
    public void EnsureCapacity_RequestedCapacitySmallerThanCurrent_CapacityUnchanged(int currentCapacity)
    {
        // assert capacity remains the same when ensuring a capacity smaller or equal than existing
        for (int i = 0; i <= currentCapacity; i++)
        {
            ValueDictionaryWrapper<TKey, TValue> dictionary = new(currentCapacity);
            int capacity = dictionary.Capacity;

            Assert.Equal(capacity, dictionary.EnsureCapacity(i));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void EnsureCapacity_EnsureCapacityCalledTwice_ReturnsSameValue(int count)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);
        int capacity = dictionary.EnsureCapacity(0);
        Assert.Equal(capacity, dictionary.EnsureCapacity(0));

        dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);
        capacity = dictionary.EnsureCapacity(count);
        Assert.Equal(capacity, dictionary.EnsureCapacity(count));

        dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);
        capacity = dictionary.EnsureCapacity(count + 1);
        Assert.Equal(capacity, dictionary.EnsureCapacity(count + 1));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(7)]
    public void EnsureCapacity_DictionaryNotEmpty_RequestedSmallerThanCount_ReturnsAtLeastSizeOfCount(int count)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);

        Assert.InRange(dictionary.EnsureCapacity(count - 1), count, int.MaxValue);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(20)]
    public void EnsureCapacity_DictionaryNotEmpty_SetsToAtLeastTheRequested(int count)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = (ValueDictionaryWrapper<TKey, TValue>)GenericIDictionaryFactory(count);

        // get current capacity
        int currentCapacity = dictionary.EnsureCapacity(0);

        // assert we can update to a larger capacity
        int newCapacity = dictionary.EnsureCapacity(currentCapacity * 2);
        Assert.InRange(newCapacity, currentCapacity * 2, int.MaxValue);
    }

    [Fact]
    public void TrimExcess_NegativeCapacity_Throw()
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = new();

        Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.TrimExcess(-1));
    }

    [Theory]
    [InlineData(20)]
    [InlineData(23)]
    public void TrimExcess_CapacitySmallerThanCount_Throws(int suggestedCapacity)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = new();
        dictionary.Add(GetNewKey(dictionary), CreateTValue(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.TrimExcess(0));

        dictionary = new ValueDictionaryWrapper<TKey, TValue>(suggestedCapacity);
        dictionary.Add(GetNewKey(dictionary), CreateTValue(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => dictionary.TrimExcess(0));
    }

    [Fact]
    public void TrimExcess_LargeInitialCapacity_TrimReducesSize()
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = new(100);
        int initialCapacity = dictionary.Capacity;
        dictionary.TrimExcess(7);

        Assert.True(dictionary.Capacity < initialCapacity);
    }

    [Theory]
    [InlineData(20)]
    [InlineData(23)]
    public void TrimExcess_TrimToLargerThanExistingCapacity_DoesNothing(int suggestedCapacity)
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = new(suggestedCapacity / 2);
        int initialCapacity = dictionary.Capacity;
        dictionary.TrimExcess(suggestedCapacity);

        Assert.Equal(initialCapacity, dictionary.EnsureCapacity(0));
    }

    [Fact]
    public void TrimExcess_DictionaryNotInitialized_CapacityRemainsAsMinPossible()
    {
        ValueDictionaryWrapper<TKey, TValue> dictionary = new();
        Assert.Equal(0, dictionary.EnsureCapacity(0));
        dictionary.TrimExcess();
        Assert.Equal(0, dictionary.EnsureCapacity(0));
    }

    [Theory]
    [InlineData(85)]
    [InlineData(89)]
    public void TrimExcess_NoArguments_TrimsToAtLeastCount(int count)
    {
        ValueDictionaryWrapper<int, int> dictionary = new ValueDictionaryWrapper<int, int>(20);
        for (int i = 0; i < count; i++)
        {
            dictionary.Add(i, 0);
        }
        dictionary.TrimExcess();
        Assert.InRange(dictionary.EnsureCapacity(0), count, int.MaxValue);
    }

    [Theory]
    [InlineData(85)]
    [InlineData(89)]
    public void TrimExcess_WithArguments_OnDictionaryWithManyElementsRemoved_TrimsToAtLeastRequested(int finalCount)
    {
        const int InitToFinalRatio = 10;
        int initialCount = InitToFinalRatio * finalCount;
        ValueDictionaryWrapper<int, int> dictionary = new(initialCount);
        Assert.InRange(dictionary.EnsureCapacity(0), initialCount, int.MaxValue);
        for (int i = 0; i < initialCount; i++)
        {
            dictionary.Add(i, 0);
        }
        for (int i = 0; i < initialCount - finalCount; i++)
        {
            dictionary.Remove(i);
        }
        for (int i = InitToFinalRatio; i > 0; i--)
        {
            dictionary.TrimExcess(i * finalCount);
            Assert.InRange(dictionary.EnsureCapacity(0), i * finalCount, int.MaxValue);
        }
    }

    [Fact]
    public void TrimExcess_DictionaryHasElementsChainedWithSameHashCode_Success()
    {
        ValueDictionaryWrapper<string, int> dictionary = new(7);
        for (int i = 0; i < 4; i++)
        {
            dictionary.Add(i.ToString(), 0);
        }
        string[] s_64bit = ["95e85f8e-67a3-4367-974f-dd24d8bb2ca2", "eb3d6fe9-de64-43a9-8f58-bddea727b1ca"];
        string[] s_32bit = ["25b1f130-7517-48e3-96b0-9da44e8bfe0e", "ba5a3625-bc38-4bf1-a707-a3cfe2158bae"];
        string[] chained = (Environment.Is64BitProcess ? s_64bit : s_32bit).ToArray();
        dictionary.Add(chained[0], 0);
        dictionary.Add(chained[1], 0);
        for (int i = 0; i < 4; i++)
        {
            dictionary.Remove(i.ToString());
        }
        dictionary.TrimExcess(3);

        Assert.Equal(2, dictionary.Count);
        Assert.True(dictionary.TryGetValue(chained[0], out int val));
        Assert.True(dictionary.TryGetValue(chained[1], out val));
    }
}

#nullable restore
