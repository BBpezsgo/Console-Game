using System.Runtime.CompilerServices;

namespace ConsoleGame;

[DebuggerDisplay("Count = {Count}")]
public ref struct ValueList<T>
{
    readonly Span<T> _items;
    int _count;

    public readonly int Count => _count;

    /// <exception cref="IndexOutOfRangeException"/>
    public readonly ref T this[int index] => ref _items[index];

    public ValueList(Span<T> items, int length)
    {
        _items = items;
        _count = length;
    }

    public ValueList(Span<T> items) : this(items, items.Length) { }

    /// <exception cref="OverflowException"/>
    public void Add(T item)
    {
        if (_count >= _items.Length)
        { throw new OverflowException("Tried to add a new item but the list is full"); }

        _items[_count] = item;
        _count++;
    }

    /// <exception cref="OverflowException"/>
    public void AddRange(IEnumerable<T> collection)
    {
        using IEnumerator<T> en = collection.GetEnumerator();
        while (en.MoveNext())
        { Add(en.Current); }
    }

    public void Clear()
    {
        _items.Clear();
        _count = 0;
    }

    public readonly void CopyTo(Span<T> destination) => _items.CopyTo(destination);

    public readonly Span<T>.Enumerator GetEnumerator() => _items.GetEnumerator();

    /// <exception cref="OverflowException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public void Insert(int index, T item)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _count);

        if (_count >= _items.Length)
        { throw new OverflowException("Tried to add a new item but the list is full"); }

        if (index < _count)
        { _items[index.._count].CopyTo(_items.Slice(index + 1, _count - index)); }

        _items[index] = item;
        _count++;
    }

    /// <exception cref="ArgumentOutOfRangeException"/>
    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _count);

        if (index + 1 < _count)
        { _items[(index + 1).._count].CopyTo(_items[index.._count]); }
        _count--;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        { _items[_count] = default!; }
    }

    public readonly void Reverse() => Reverse(0, _count);

    /// <exception cref="ArgumentOutOfRangeException"/>
    public readonly void Reverse(int index, int count) => _items.Slice(index, count).Reverse();

    public readonly void Sort() => Sort(0, _count, null);

    public readonly void Sort(IComparer<T>? comparer) => Sort(0, _count, comparer);

    /// <exception cref="ArgumentOutOfRangeException"/>
    public readonly void Sort(int index, int count, IComparer<T>? comparer) => _items.Slice(index, count).Sort(comparer);

    public readonly T[] ToArray() => _items[.._count].ToArray();

    public readonly Span<T> AsSpan() => _items;
}
