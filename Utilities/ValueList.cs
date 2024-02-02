using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ConsoleGame
{
    [DebuggerDisplay("Count = {Count}")]
    public ref struct ValueList<T>
    {
        readonly Span<T> _items;
        int _size;

        public readonly int Count => _size;

        /// <exception cref="IndexOutOfRangeException"/>
        public readonly ref T this[int index] => ref _items[index];

        public ValueList(Span<T> items, int length)
        {
            _items = items;
            _size = length;
        }

        public ValueList(Span<T> items) : this(items, items.Length) { }
        public ValueList(T[]? array) : this(new Span<T>(array)) { }
        public unsafe ValueList(void* pointer, int length) : this(new Span<T>(pointer, length)) { }
        public ValueList(T[]? array, int start, int length) : this(new Span<T>(array, start, length)) { }

        /// <exception cref="OverflowException"/>
        public void Add(T item)
        {
            if (_size >= _items.Length)
            { throw new OverflowException("Tried to add a new item but the list is full"); }

            _items[_size] = item;
            _size++;
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
            _size = 0;
        }

        public readonly void CopyTo(Span<T> destination) => _items.CopyTo(destination);

        public readonly Span<T>.Enumerator GetEnumerator() => _items.GetEnumerator();

        /// <exception cref="OverflowException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public void Insert(int index, T item)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _size);

            if (_size >= _items.Length)
            { throw new OverflowException("Tried to add a new item but the list is full"); }

            if (index < _size)
            { _items[index.._size].CopyTo(_items.Slice(index + 1, _size - index)); }

            _items[index] = item;
            _size++;
        }

        /// <exception cref="ArgumentOutOfRangeException"/>
        public void RemoveAt(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _size);

            if (index + 1 < _size)
            { _items[(index + 1).._size].CopyTo(_items[index.._size]); }
            _size--;

            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            { _items[_size] = default!; }
        }

        public readonly void Reverse() => Reverse(0, _size);

        /// <exception cref="ArgumentOutOfRangeException"/>
        public readonly void Reverse(int index, int count) => _items.Slice(index, count).Reverse();

        public readonly void Sort() => Sort(0, _size, null);

        public readonly void Sort(IComparer<T>? comparer) => Sort(0, _size, comparer);

        /// <exception cref="ArgumentOutOfRangeException"/>
        public readonly void Sort(int index, int count, IComparer<T>? comparer) => _items.Slice(index, count).Sort(comparer);

        public readonly T[] ToArray() => _items[.._size].ToArray();

        public readonly Span<T> AsSpan() => _items;
    }
}
