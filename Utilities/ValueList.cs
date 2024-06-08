using System.Runtime.CompilerServices;

namespace ConsoleGame;

[DebuggerDisplay("Count = {Count}")]
public ref struct ValueList<T>
{
    readonly Span<T> _items;

    public int Count { get; private set; }

    /// <exception cref="IndexOutOfRangeException"/>
    public readonly ref T this[int index] => ref _items[index];

    public ValueList(Span<T> items, int length)
    {
        _items = items;
        Count = length;
    }

    public ValueList(Span<T> items) : this(items, items.Length) { }

    /// <exception cref="OverflowException"/>
    public void Add(T item)
    {
        if (Count >= _items.Length)
        { throw new OverflowException("Tried to add a new item but the list is full"); }

        _items[Count] = item;
        Count++;
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
        Count = 0;
    }

    public readonly void CopyTo(Span<T> destination) => _items.CopyTo(destination);

    public readonly Span<T>.Enumerator GetEnumerator() => _items.GetEnumerator();

    /// <exception cref="OverflowException"/>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public void Insert(int index, T item)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

        if (Count >= _items.Length)
        { throw new OverflowException("Tried to add a new item but the list is full"); }

        if (index < Count)
        { _items[index..Count].CopyTo(_items.Slice(index + 1, Count - index)); }

        _items[index] = item;
        Count++;
    }

    /// <exception cref="ArgumentOutOfRangeException"/>
    public void RemoveAt(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);

        if (index + 1 < Count)
        { _items[(index + 1)..Count].CopyTo(_items[index..Count]); }
        Count--;

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        { _items[Count] = default!; }
    }

    public readonly void Reverse() => Reverse(0, Count);

    /// <exception cref="ArgumentOutOfRangeException"/>
    public readonly void Reverse(int index, int count) => _items.Slice(index, count).Reverse();

    public readonly void Sort() => Sort(0, Count, null);

    public readonly void Sort(IComparer<T>? comparer) => Sort(0, Count, comparer);

    /// <exception cref="ArgumentOutOfRangeException"/>
    public readonly void Sort(int index, int count, IComparer<T>? comparer) => _items.Slice(index, count).Sort(comparer);

    public readonly T[] ToArray() => _items[..Count].ToArray();

    public readonly Span<T> AsSpan() => _items[..Count];
}
