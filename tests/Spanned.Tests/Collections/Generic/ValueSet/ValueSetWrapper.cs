using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Spanned.Collections.Generic;

namespace Spanned.Tests.Collections.Generic.ValueSet;

public sealed class ValueSetWrapper<T> : ISet<T>, IReadOnlySet<T>
{
    private T[] _buffer;

    private int _count;

    private IEqualityComparer<T> _comparer;

    public ValueSetWrapper()
    {
        ValueSet<T> set = new();
        (_buffer, _count, _comparer) = (set.AsCapacitySpan().ToArray(), set.Count, set.Comparer);
    }

    public ValueSetWrapper(IEnumerable<T> collection)
    {
        ValueSet<T> set = new(collection);
        (_buffer, _count, _comparer) = (set.AsCapacitySpan().ToArray(), set.Count, set.Comparer);
    }

    public ValueSetWrapper(IEqualityComparer<T>? comparer)
    {
        ValueSet<T> set = new(comparer);
        (_buffer, _count, _comparer) = (set.AsCapacitySpan().ToArray(), set.Count, set.Comparer);
    }

    public ValueSetWrapper(int capacity)
    {
        ValueSet<T> set = new(capacity);
        (_buffer, _count, _comparer) = (set.AsCapacitySpan().ToArray(), set.Count, set.Comparer);
    }

    public ValueSetWrapper(IEnumerable<T> collection, IEqualityComparer<T>? comparer)
    {
        ValueSet<T> set = new(collection, comparer);
        (_buffer, _count, _comparer) = (set.AsCapacitySpan().ToArray(), set.Count, set.Comparer);
    }

    public ValueSetWrapper(int capacity, IEqualityComparer<T>? comparer)
    {
        ValueSet<T> set = new(capacity, comparer);
        (_buffer, _count, _comparer) = (set.AsCapacitySpan().ToArray(), set.Count, set.Comparer);
    }

    public int Capacity => Run((ref ValueSet<T> set) => set.Capacity);

    public int Count => Run((ref ValueSet<T> set) => set.Count);

    bool ICollection<T>.IsReadOnly => false;

    public IEqualityComparer<T> Comparer => Run((ref ValueSet<T> set) => set.Comparer);

    public bool Add(T item) => Run((ref ValueSet<T> set) => set.Add(item));

    void ICollection<T>.Add(T item) => Add(item);

    public void Clear() => Run((ref ValueSet<T> set) => set.Clear());

    public bool Contains(T item) => Run((ref ValueSet<T> set) => set.Contains(item));

    public void CopyTo(T[] array) => Run((ref ValueSet<T> set) => set.CopyTo(array));

    public void CopyTo(T[] array, int arrayIndex) => Run((ref ValueSet<T> set) => set.CopyTo(array, arrayIndex));

    public void CopyTo(T[] array, int arrayIndex, int count) => Run((ref ValueSet<T> set) => set.CopyTo(array, arrayIndex, count));

    public int EnsureCapacity(int capacity) => Run((ref ValueSet<T> set) => set.EnsureCapacity(capacity));

    public void ExceptWith(IEnumerable<T> other) => Run((ref ValueSet<T> set) =>
    {
        if (ReferenceEquals(other, this))
        {
            set.ExceptWith(set);
        }
        else
        {
            set.ExceptWith(other);
        }
    });

    public void SymmetricExceptWith(IEnumerable<T> other) => Run((ref ValueSet<T> set) =>
    {
        if (ReferenceEquals(other, this))
        {
            set.SymmetricExceptWith(set);
        }
        else
        {
            set.SymmetricExceptWith(other);
        }
    });

    public void IntersectWith(IEnumerable<T> other) => Run((ref ValueSet<T> set) => set.IntersectWith(other));

    public bool IsProperSubsetOf(IEnumerable<T> other) => Run((ref ValueSet<T> set) => set.IsProperSubsetOf(other));

    public bool IsProperSupersetOf(IEnumerable<T> other) => Run((ref ValueSet<T> set) => set.IsProperSupersetOf(other));

    public bool IsSubsetOf(IEnumerable<T> other) => Run((ref ValueSet<T> set) => set.IsSubsetOf(other));

    public bool IsSupersetOf(IEnumerable<T> other) => Run((ref ValueSet<T> set) => set.IsSupersetOf(other));

    public bool Overlaps(IEnumerable<T> other) => Run((ref ValueSet<T> set) => set.Overlaps(other));

    public bool Remove(T item) => Run((ref ValueSet<T> set) => set.Remove(item));

    public int RemoveWhere(Predicate<T> match) => Run((ref ValueSet<T> set) => set.RemoveWhere(match));

    public bool SetEquals(IEnumerable<T> other) => Run((ref ValueSet<T> set) => set.SetEquals(other));

    public void TrimExcess() => Run((ref ValueSet<T> set) => set.TrimExcess());

    public void TrimExcess(int newCapacity) => Run((ref ValueSet<T> set) => set.TrimExcess(newCapacity));

    public bool TryGetValue(T equalValue, [MaybeNullWhen(false)] out T actualValue) => Run((ref ValueSet<T> set, out T actualValue) => set.TryGetValue(equalValue, out actualValue!), out actualValue);

    public void UnionWith(IEnumerable<T> other) => Run((ref ValueSet<T> set) => set.UnionWith(other));

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
            yield return new ValueSet<T>(_buffer.AsSpan(), _comparer) { Count = _count }[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    private delegate void ValueSetAction(ref ValueSet<T> set);

    private delegate U ValueSetFunc<U>(ref ValueSet<T> set);

    private delegate U ValueSetFunc<U, V>(ref ValueSet<T> set, out V result);

    private void Run(ValueSetAction action)
    {
        ValueSet<T> set = new(_buffer.AsSpan(), _comparer) { Count = _count };
        action(ref set);
        (_buffer, _count, _comparer) = (set.AsCapacitySpan().ToArray(), set.Count, set.Comparer);
    }

    private U Run<U>(ValueSetFunc<U> func)
    {
        ValueSet<T> set = new(_buffer.AsSpan(), _comparer) { Count = _count };
        U result = func(ref set);
        (_buffer, _count, _comparer) = (set.AsCapacitySpan().ToArray(), set.Count, set.Comparer);
        return result;
    }

    private U Run<U, V>(ValueSetFunc<U, V> func, out V value)
    {
        ValueSet<T> set = new(_buffer.AsSpan(), _comparer) { Count = _count };
        U result = func(ref set, out value);
        (_buffer, _count, _comparer) = (set.AsCapacitySpan().ToArray(), set.Count, set.Comparer);
        return result;
    }
}
