using System.Collections;
using System.Collections.ObjectModel;
using Spanned.Collections.Generic;

namespace Spanned.Tests.Collections.Generic.ValueList;

public sealed class ValueListWrapper<T> : IList<T>, IReadOnlyList<T>, IList
{
    private T[] _buffer;

    private int _count;

    public ValueListWrapper()
    {
        ValueList<T> list = new();
        (_buffer, _count) = (list.AsCapacitySpan().ToArray(), list.Count);
    }

    public ValueListWrapper(IEnumerable<T> collection)
    {
        ValueList<T> list = new(collection);
        (_buffer, _count) = (list.AsCapacitySpan().ToArray(), list.Count);
    }

    public ValueListWrapper(int capacity)
    {
        ValueList<T> list = new(capacity);
        (_buffer, _count) = (list.AsCapacitySpan().ToArray(), list.Count);
    }

    public int Count => Run((ref ValueList<T> x) => x.Count);

    public int Capacity => Run((ref ValueList<T> x) => x.Capacity);

    public T this[int index]
    {
        get => Run((ref ValueList<T> x) => x[index]);
        set => Run((ref ValueList<T> x) => x[index] = value);
    }

    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = (T)value!;
    }

    public void Add(T item) => Run((ref ValueList<T> x) => x.Add(item));

    public void AddRange(IEnumerable<T> collection) => Run((ref ValueList<T> x) => x.AddRange(collection));

    public void AddRange(scoped ReadOnlySpan<T> span)
    {
        T[] spanSource = span.ToArray();
        Run((ref ValueList<T> x) => x.AddRange(spanSource.AsSpan()));
    }

    public ReadOnlyCollection<T> AsReadOnly() => Run((ref ValueList<T> x) => x.AsReadOnly());

    public int BinarySearch(int index, int count, T item, IComparer<T>? comparer) => Run((ref ValueList<T> x) => x.BinarySearch(index, count, item, comparer));

    public int BinarySearch(T item) => Run((ref ValueList<T> x) => x.BinarySearch(item));

    public int BinarySearch(T item, IComparer<T>? comparer) => Run((ref ValueList<T> x) => x.BinarySearch(item, comparer));

    public void Clear() => Run((ref ValueList<T> x) => x.Clear());

    public bool Contains(T item) => Run((ref ValueList<T> x) => x.Contains(item));

    public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) => Run((ref ValueList<T> x) => x.ConvertAll(converter).ToList());

    public void CopyTo(T[] array, int arrayIndex) => Run((ref ValueList<T> x) => x.CopyTo(array, arrayIndex));

    public void CopyTo(T[] array) => Run((ref ValueList<T> x) => x.CopyTo(array));

    public void CopyTo(scoped Span<T> span)
    {
        ValueList<T> list = new(_buffer.AsSpan()) { Count = _count };
        list.CopyTo(span);
    }

    public void CopyTo(int index, T[] array, int arrayIndex, int count) => Run((ref ValueList<T> x) => x.CopyTo(index, array, arrayIndex, count));

    public int EnsureCapacity(int capacity) => Run((ref ValueList<T> x) => x.EnsureCapacity(capacity));

    public bool Exists(Predicate<T> match) => Run((ref ValueList<T> x) => x.Exists(match));

    public T? Find(Predicate<T> match) => Run((ref ValueList<T> x) => x.Find(match));

    public List<T> FindAll(Predicate<T> match) => Run((ref ValueList<T> x) => x.FindAll(match).ToList());

    public int FindIndex(int startIndex, int count, Predicate<T> match) => Run((ref ValueList<T> x) => x.FindIndex(startIndex, count, match));

    public int FindIndex(int startIndex, Predicate<T> match) => Run((ref ValueList<T> x) => x.FindIndex(startIndex, match));

    public int FindIndex(Predicate<T> match) => Run((ref ValueList<T> x) => x.FindIndex(match));

    public T? FindLast(Predicate<T> match) => Run((ref ValueList<T> x) => x.FindLast(match));

    public int FindLastIndex(int startIndex, int count, Predicate<T> match) => Run((ref ValueList<T> x) => x.FindLastIndex(startIndex, count, match));

    public int FindLastIndex(int startIndex, Predicate<T> match) => Run((ref ValueList<T> x) => x.FindLastIndex(startIndex, match));

    public int FindLastIndex(Predicate<T> match) => Run((ref ValueList<T> x) => x.FindLastIndex(match));

    public void ForEach(Action<T> action) => Run((ref ValueList<T> x) => x.ForEach(action));

    public List<T> GetRange(int index, int count) => Run((ref ValueList<T> x) => x.GetRange(index, count).ToList());

    public int IndexOf(T item, int index, int count) => Run((ref ValueList<T> x) => x.IndexOf(item, index, count));

    public int IndexOf(T item, int index) => Run((ref ValueList<T> x) => x.IndexOf(item, index));

    public int IndexOf(T item) => Run((ref ValueList<T> x) => x.IndexOf(item));

    public void Insert(int index, T item) => Run((ref ValueList<T> x) => x.Insert(index, item));

    public void InsertRange(int index, IEnumerable<T> collection) => Run((ref ValueList<T> x) => x.InsertRange(index, collection));

    public void InsertRange(int index, scoped ReadOnlySpan<T> collection)
    {
        ValueList<T> list = new(_buffer.AsSpan()) { Count = _count };
        list.InsertRange(index, collection);
        (_buffer, _count) = (list.AsCapacitySpan().ToArray(), list.Count);
    }

    public int LastIndexOf(T item) => Run((ref ValueList<T> x) => x.LastIndexOf(item));

    public int LastIndexOf(T item, int index) => Run((ref ValueList<T> x) => x.LastIndexOf(item, index));

    public int LastIndexOf(T item, int index, int count) => Run((ref ValueList<T> x) => x.LastIndexOf(item, index, count));

    public bool Remove(T item) => Run((ref ValueList<T> x) => x.Remove(item));

    public int RemoveAll(Predicate<T> match) => Run((ref ValueList<T> x) => x.RemoveAll(match));

    public void RemoveAt(int index) => Run((ref ValueList<T> x) => x.RemoveAt(index));

    public void RemoveRange(int index, int count) => Run((ref ValueList<T> x) => x.RemoveRange(index, count));

    public void Reverse(int index, int count) => Run((ref ValueList<T> x) => x.Reverse(index, count));

    public void Reverse() => Run((ref ValueList<T> x) => x.Reverse());

    public List<T> Slice(int start, int length) => Run((ref ValueList<T> x) => x.Slice(start, length).ToList());

    public void Sort(IComparer<T>? comparer) => Run((ref ValueList<T> x) => x.Sort(comparer));

    public void Sort(Comparison<T> comparison) => Run((ref ValueList<T> x) => x.Sort(comparison));

    public void Sort(int index, int count, IComparer<T>? comparer) => Run((ref ValueList<T> x) => x.Sort(index, count, comparer));

    public void Sort() => Run((ref ValueList<T> x) => x.Sort());

    public T[] ToArray() => Run((ref ValueList<T> x) => x.ToArray());

    public void TrimExcess() => Run((ref ValueList<T> x) => x.TrimExcess());

    public bool TrueForAll(Predicate<T> match) => Run((ref ValueList<T> x) => x.TrueForAll(match));

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
            yield return new ValueList<T>(_buffer.AsSpan()) { Count = _count }[i];
    }


    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    bool ICollection.IsSynchronized => false;

    bool ICollection<T>.IsReadOnly => false;

    object ICollection.SyncRoot => throw new NotImplementedException();

    bool IList.IsFixedSize => throw new NotImplementedException();

    bool IList.IsReadOnly => throw new NotImplementedException();

    void ICollection.CopyTo(Array array, int index) => throw new NotImplementedException();

    bool IList.Contains(object? value) => Contains((T)value!);

    int IList.IndexOf(object? value) => IndexOf((T)value!);

    void IList.Insert(int index, object? value) => Insert(index, (T)value!);

    void IList.Remove(object? value) => Remove((T)value!);

    int IList.Add(object? value)
    {
        Add((T)value!);
        return _count - 1;
    }


    private delegate void ValueListAction(ref ValueList<T> list);

    private delegate U ValueListFunc<U>(ref ValueList<T> list);

    private delegate U ValueListFunc<U, V>(ref ValueList<T> list, out V result);

    private void Run(ValueListAction action)
    {
        ValueList<T> list = new(_buffer.AsSpan()) { Count = _count };
        action(ref list);
        (_buffer, _count) = (list.AsCapacitySpan().ToArray(), list.Count);
    }

    private U Run<U>(ValueListFunc<U> func)
    {
        ValueList<T> list = new(_buffer.AsSpan()) { Count = _count };
        U result = func(ref list);
        (_buffer, _count) = (list.AsCapacitySpan().ToArray(), list.Count);
        return result;
    }
}
