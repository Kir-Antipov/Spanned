using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Spanned.Collections.Generic;

namespace Spanned.Tests.Collections.Generic.ValueQueue;

public sealed class ValueQueueWrapper<T> : IReadOnlyCollection<T>, ICollection
{
    private T[] _buffer;

    private int _count;

    public ValueQueueWrapper()
    {
        ValueQueue<T> queue = new();
        (_buffer, _count) = (queue.AsCapacitySpan().ToArray(), queue.Count);
    }

    public ValueQueueWrapper(IEnumerable<T> collection)
    {
        ValueQueue<T> queue = new(collection);
        (_buffer, _count) = (queue.AsCapacitySpan().ToArray(), queue.Count);
    }

    public ValueQueueWrapper(int capacity)
    {
        ValueQueue<T> queue = new(capacity);
        (_buffer, _count) = (queue.AsCapacitySpan().ToArray(), queue.Count);
    }

    public int Count => Run((ref ValueQueue<T> x) => x.Count);

    public int Capacity => Run((ref ValueQueue<T> x) => x.Capacity);

    public void Clear() => Run((ref ValueQueue<T> x) => x.Clear());

    public bool Contains(T item) => Run((ref ValueQueue<T> x) => x.Contains(item));

    public void CopyTo(T[] array, int arrayIndex) => Run((ref ValueQueue<T> x) => x.CopyTo(array, arrayIndex));

    public int EnsureCapacity(int capacity) => Run((ref ValueQueue<T> x) => x.EnsureCapacity(capacity));

    public T Peek() => Run((ref ValueQueue<T> x) => x.Peek());

    public T Dequeue() => Run((ref ValueQueue<T> x) => x.Dequeue());

    public void Enqueue(T item) => Run((ref ValueQueue<T> x) => x.Enqueue(item));

    public T[] ToArray() => Run((ref ValueQueue<T> x) => x.AsSpan().ToArray());

    public void TrimExcess() => Run((ref ValueQueue<T> x) => x.TrimExcess());

    public void TrimExcess(int newCapacity) => Run((ref ValueQueue<T> x) => x.TrimExcess(newCapacity));

    public bool TryPeek([MaybeNullWhen(false)] out T result) => Run((ref ValueQueue<T> x, out T result) => x.TryPeek(out result!), out result);

    public bool TryDequeue([MaybeNullWhen(false)] out T result) => Run((ref ValueQueue<T> x, out T result) => x.TryDequeue(out result!), out result);

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
            yield return new ValueQueue<T>(_buffer.AsSpan()) { Count = _count }[i];
    }


    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => throw new NotImplementedException();

    void ICollection.CopyTo(Array array, int index) => throw new NotImplementedException();


    private delegate void ValueQueueAction(ref ValueQueue<T> queue);

    private delegate U ValueQueueFunc<U>(ref ValueQueue<T> queue);

    private delegate U ValueQueueFunc<U, V>(ref ValueQueue<T> queue, out V result);

    private void Run(ValueQueueAction action)
    {
        ValueQueue<T> queue = new(_buffer.AsSpan()) { Count = _count };
        action(ref queue);
        (_buffer, _count) = (queue.AsCapacitySpan().ToArray(), queue.Count);
    }

    private U Run<U>(ValueQueueFunc<U> func)
    {
        ValueQueue<T> queue = new(_buffer.AsSpan()) { Count = _count };
        U result = func(ref queue);
        (_buffer, _count) = (queue.AsCapacitySpan().ToArray(), queue.Count);
        return result;
    }

    private U Run<U, V>(ValueQueueFunc<U, V> func, out V value)
    {
        ValueQueue<T> queue = new(_buffer.AsSpan()) { Count = _count };
        U result = func(ref queue, out value);
        (_buffer, _count) = (queue.AsCapacitySpan().ToArray(), queue.Count);
        return result;
    }
}
