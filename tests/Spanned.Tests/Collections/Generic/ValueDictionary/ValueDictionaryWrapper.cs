using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Spanned.Collections.Generic;

namespace Spanned.Tests.Collections.Generic.ValueDictionary;

public sealed class ValueDictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDictionary where TKey : notnull
{
    private KeyValuePair<TKey, TValue>[] _buffer;

    private int _count;

    private IEqualityComparer<TKey> _comparer;

    public ValueDictionaryWrapper()
    {
        ValueDictionary<TKey, TValue> dictionary = new();
        (_buffer, _count, _comparer) = (dictionary.AsCapacitySpan().ToArray(), dictionary.Count, dictionary.Comparer);
    }

    public ValueDictionaryWrapper(IDictionary<TKey, TValue> collection)
    {
        ValueDictionary<TKey, TValue> dictionary = new(collection);
        (_buffer, _count, _comparer) = (dictionary.AsCapacitySpan().ToArray(), dictionary.Count, dictionary.Comparer);
    }

    public ValueDictionaryWrapper(IEnumerable<KeyValuePair<TKey, TValue>> collection)
    {
        ValueDictionary<TKey, TValue> dictionary = new(collection);
        (_buffer, _count, _comparer) = (dictionary.AsCapacitySpan().ToArray(), dictionary.Count, dictionary.Comparer);
    }

    public ValueDictionaryWrapper(IEqualityComparer<TKey>? comparer)
    {
        ValueDictionary<TKey, TValue> dictionary = new(comparer);
        (_buffer, _count, _comparer) = (dictionary.AsCapacitySpan().ToArray(), dictionary.Count, dictionary.Comparer);
    }

    public ValueDictionaryWrapper(int capacity)
    {
        ValueDictionary<TKey, TValue> dictionary = new(capacity);
        (_buffer, _count, _comparer) = (dictionary.AsCapacitySpan().ToArray(), dictionary.Count, dictionary.Comparer);
    }

    public ValueDictionaryWrapper(IDictionary<TKey, TValue> collection, IEqualityComparer<TKey>? comparer)
    {
        ValueDictionary<TKey, TValue> dictionary = new(collection, comparer);
        (_buffer, _count, _comparer) = (dictionary.AsCapacitySpan().ToArray(), dictionary.Count, dictionary.Comparer);
    }

    public ValueDictionaryWrapper(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey>? comparer)
    {
        ValueDictionary<TKey, TValue> dictionary = new(collection, comparer);
        (_buffer, _count, _comparer) = (dictionary.AsCapacitySpan().ToArray(), dictionary.Count, dictionary.Comparer);
    }

    public ValueDictionaryWrapper(int capacity, IEqualityComparer<TKey>? comparer)
    {
        ValueDictionary<TKey, TValue> dictionary = new(capacity, comparer);
        (_buffer, _count, _comparer) = (dictionary.AsCapacitySpan().ToArray(), dictionary.Count, dictionary.Comparer);
    }

    public TValue this[TKey key]
    {
        get => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary[key]);
        set => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary[key] = value);
    }

    object? IDictionary.this[object key]
    {
        get => this[(TKey)key];
        set => this[(TKey)key] = (TValue)value!;
    }

    public Dictionary<TKey, TValue>.KeyCollection Keys => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.ToDictionary().Keys);

    ICollection IDictionary.Keys => Keys;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    public Dictionary<TKey, TValue>.ValueCollection Values => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.ToDictionary().Values);

    ICollection IDictionary.Values => Values;

    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    public int Capacity => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.Capacity);

    public int Count => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.Count);

    public IEqualityComparer<TKey> Comparer => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.Comparer);

    public void Add(TKey key, TValue value) => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.Add(key, value));

    public void Clear() => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.Clear());

    public bool ContainsKey(TKey key) => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.ContainsKey(key));

    public bool ContainsValue(TValue value) => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.ContainsValue(value));

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.CopyTo(array, arrayIndex));

    public int EnsureCapacity(int capacity) => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.EnsureCapacity(capacity));

    public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value) => Run((ref ValueDictionary<TKey, TValue> dictionary, out TValue value) => dictionary.Remove(key, out value!), out value);

    public bool Remove(TKey key) => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.Remove(key));

    public void TrimExcess() => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.TrimExcess());

    public void TrimExcess(int capacity) => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.TrimExcess(capacity));

    public bool TryAdd(TKey key, TValue value) => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.TryAdd(key, value));

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => Run((ref ValueDictionary<TKey, TValue> dictionary, out TValue value) => dictionary.TryGetValue(key, out value!), out value);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
            yield return new ValueDictionary<TKey, TValue>(_buffer.AsSpan(), _comparer) { Count = _count }.AsSpan()[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    IDictionaryEnumerator IDictionary.GetEnumerator() => Run((ref ValueDictionary<TKey, TValue> dictionary) => dictionary.ToDictionary().GetEnumerator());

    void IDictionary.Add(object key, object? value) => Add((TKey)key, (TValue?)value!);

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    bool IDictionary.Contains(object key) => ContainsKey((TKey)key);

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => TryGetValue(item.Key, out TValue? value) && Equals(item.Value, value);

    void IDictionary.Remove(object key) => Remove((TKey)key);

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)this).Contains(item) && Remove(item.Key);

    void ICollection.CopyTo(Array array, int index) => throw new NotImplementedException();

    bool IDictionary.IsReadOnly => false;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

    bool IDictionary.IsFixedSize => false;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => throw new NotImplementedException();

    private delegate void ValueDictionaryAction(ref ValueDictionary<TKey, TValue> dictionary);

    private delegate U ValueDictionaryFunc<U>(ref ValueDictionary<TKey, TValue> dictionary);

    private delegate U ValueDictionaryFunc<U, V>(ref ValueDictionary<TKey, TValue> dictionary, out V result);

    private void Run(ValueDictionaryAction action)
    {
        ValueDictionary<TKey, TValue> dictionary = new(_buffer.AsSpan(), _comparer) { Count = _count };
        action(ref dictionary);
        (_buffer, _count, _comparer) = (dictionary.AsCapacitySpan().ToArray(), dictionary.Count, dictionary.Comparer);
    }

    private U Run<U>(ValueDictionaryFunc<U> func)
    {
        ValueDictionary<TKey, TValue> dictionary = new(_buffer.AsSpan(), _comparer) { Count = _count };
        U result = func(ref dictionary);
        (_buffer, _count, _comparer) = (dictionary.AsCapacitySpan().ToArray(), dictionary.Count, dictionary.Comparer);
        return result;
    }

    private U Run<U, V>(ValueDictionaryFunc<U, V> func, out V value)
    {
        ValueDictionary<TKey, TValue> dictionary = new(_buffer.AsSpan(), _comparer) { Count = _count };
        U result = func(ref dictionary, out value);
        (_buffer, _count, _comparer) = (dictionary.AsCapacitySpan().ToArray(), dictionary.Count, dictionary.Comparer);
        return result;
    }
}
