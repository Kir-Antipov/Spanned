using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Spanned.Collections.Generic;

namespace Spanned.Tests.Collections.Generic.ValueStack;

public sealed class ValueStackWrapper<T> : IReadOnlyCollection<T>, ICollection
{
    private T[] _buffer;

    private int _count;

    public ValueStackWrapper()
    {
        ValueStack<T> stack = new();
        (_buffer, _count) = (stack.AsCapacitySpan().ToArray(), stack.Count);
    }

    public ValueStackWrapper(IEnumerable<T> collection)
    {
        ValueStack<T> stack = new(collection);
        (_buffer, _count) = (stack.AsCapacitySpan().ToArray(), stack.Count);
    }

    public ValueStackWrapper(int capacity)
    {
        ValueStack<T> stack = new(capacity);
        (_buffer, _count) = (stack.AsCapacitySpan().ToArray(), stack.Count);
    }

    public int Count => Run((ref ValueStack<T> x) => x.Count);

    public int Capacity => Run((ref ValueStack<T> x) => x.Capacity);

    public void Clear() => Run((ref ValueStack<T> x) => x.Clear());

    public bool Contains(T item) => Run((ref ValueStack<T> x) => x.Contains(item));

    public void CopyTo(T[] array, int arrayIndex) => Run((ref ValueStack<T> x) => x.CopyTo(array, arrayIndex));

    public int EnsureCapacity(int capacity) => Run((ref ValueStack<T> x) => x.EnsureCapacity(capacity));

    public T Peek() => Run((ref ValueStack<T> x) => x.Peek());

    public T Pop() => Run((ref ValueStack<T> x) => x.Pop());

    public void Push(T item) => Run((ref ValueStack<T> x) => x.Push(item));

    public T[] ToArray() => Run((ref ValueStack<T> x) => x.AsSpan().ToArray());

    public void TrimExcess() => Run((ref ValueStack<T> x) => x.TrimExcess());

    public void TrimExcess(int newCapacity) => Run((ref ValueStack<T> x) => x.TrimExcess(newCapacity));

    public bool TryPeek([MaybeNullWhen(false)] out T result) => Run((ref ValueStack<T> x, out T result) => x.TryPeek(out result!), out result);

    public bool TryPop([MaybeNullWhen(false)] out T result) => Run((ref ValueStack<T> x, out T result) => x.TryPop(out result!), out result);

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
            yield return new ValueStack<T>(_buffer.AsSpan()) { Count = _count }[i];
    }


    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => throw new NotImplementedException();

    void ICollection.CopyTo(Array array, int index) => throw new NotImplementedException();


    private delegate void ValueStackAction(ref ValueStack<T> stack);

    private delegate U ValueStackFunc<U>(ref ValueStack<T> stack);

    private delegate U ValueStackFunc<U, V>(ref ValueStack<T> stack, out V result);

    private void Run(ValueStackAction action)
    {
        ValueStack<T> stack = new(_buffer.AsSpan()) { Count = _count };
        action(ref stack);
        (_buffer, _count) = (stack.AsCapacitySpan().ToArray(), stack.Count);
    }

    private U Run<U>(ValueStackFunc<U> func)
    {
        ValueStack<T> stack = new(_buffer.AsSpan()) { Count = _count };
        U result = func(ref stack);
        (_buffer, _count) = (stack.AsCapacitySpan().ToArray(), stack.Count);
        return result;
    }

    private U Run<U, V>(ValueStackFunc<U, V> func, out V value)
    {
        ValueStack<T> stack = new(_buffer.AsSpan()) { Count = _count };
        U result = func(ref stack, out value);
        (_buffer, _count) = (stack.AsCapacitySpan().ToArray(), stack.Count);
        return result;
    }
}
