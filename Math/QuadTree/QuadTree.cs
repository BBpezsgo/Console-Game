using System.Collections;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ConsoleGame
{
    public abstract class QuadTree
    {
        public const int MAX_DEPTH = 4;
    }

    public readonly struct QuadTreeLocation :
        IEquatable<QuadTreeLocation>,
        IEqualityOperators<QuadTreeLocation, QuadTreeLocation, bool>
    {
        public readonly uint Branch;
        public readonly uint Depth;

        public uint[] Branches
        {
            get
            {
                uint[] result = new uint[Depth];

                uint branch = Branch;

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = branch & 0b_11;
                    branch >>= 2;
                }
                Array.Reverse(result);

                return result;
            }
        }

        public QuadTreeLocation(uint branch, uint depth)
        {
            this.Branch = branch;
            this.Depth = depth;
        }

        public static explicit operator ulong(QuadTreeLocation location) => ((ulong)location.Branch << 32) | location.Depth;
        public static explicit operator QuadTreeLocation(ulong location) => new((uint)(location >> 32), (uint)(location & uint.MaxValue));

        public static bool operator ==(QuadTreeLocation left, QuadTreeLocation right) => left.Equals(right);
        public static bool operator !=(QuadTreeLocation left, QuadTreeLocation right) => !left.Equals(right);

        public override bool Equals(object? obj) =>
            obj is QuadTreeLocation other &&
            Equals(other);

        public bool Equals(QuadTreeLocation other) =>
            Branch == other.Branch &&
            Depth == other.Depth;

        public override int GetHashCode() => HashCode.Combine(Branch, Depth);

        public override string ToString() => $"({Branch}, {Depth})";
    }

    public class QuadTree<T> : QuadTree, IEnumerable<T>
    {
        public int Count
        {
            get
            {
                int count = _container.Count;
                for (int i = 0; i < _children.Length; i++)
                {
                    count += _children[i]?.Count ?? 0;
                }
                return count;
            }
        }

        readonly int _depth;
        Rect _rect;
        readonly Rect[] _childrenRects;
        readonly QuadTree<T>?[] _children;
        readonly List<(Rect Rect, T Item)> _container;

        QuadTree()
        {
            _childrenRects = new Rect[4];
            _children = new QuadTree<T>?[4];
            _container = new List<(Rect, T)>();
        }

        QuadTree(Rect rect, int depth) : this()
        {
            _rect = rect;
            _depth = depth;
            Resize(rect);
        }

        public QuadTree(Rect rect) : this()
        {
            _rect = rect;
            _depth = 0;
            Resize(rect);
        }

        public void Resize(Rect rect)
        {
            Clear();
            _rect = rect;
            Rect childSize = new(rect.Position, rect.Size * 0.5f);
            _childrenRects[0] = childSize;
            _childrenRects[1] = Rect.Move(childSize, childSize.Size.X, 0f);
            _childrenRects[2] = Rect.Move(childSize, 0f, childSize.Size.Y);
            _childrenRects[3] = Rect.Move(childSize, childSize.Size.X, childSize.Size.Y);
        }

        public void Clear()
        {
            _container.Clear();
            for (int i = 0; i < _children.Length; i++)
            {
                _children[i]?.Clear();
                _children[i] = null;
            }
        }

        public QuadTreeLocation Add(ValueTuple<T, Rect> item) => Add(item.Item1, item.Item2);
        public QuadTreeLocation Add(T item, Rect itemArea)
        {
            uint branchId = 0;
            uint depth = 0;
            Add(item, itemArea, ref branchId, ref depth);
            return new QuadTreeLocation(branchId, depth);
        }
        void Add(T item, Rect itemArea, ref uint branchId, ref uint depth)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_childrenRects[i].Contains(itemArea))
                {
                    if (_depth + 1 < MAX_DEPTH)
                    {
                        if (_children[i] is null)
                        {
                            _children[i] = new QuadTree<T>(_childrenRects[i], _depth + 1);
                        }

                        if (i is < 0b_00 or > 0b_11) throw new Exception("Bruh");

                        branchId <<= 2;
                        branchId |= (uint)i;
                        depth++;

                        _children[i]?.Add(item, itemArea, ref branchId, ref depth);
                        return;
                    }
                }
            }

            _container.Add((itemArea, item));
        }

        public ReadOnlySpan<T> Search(Rect area)
        {
            List<T> result = new();
            Search(area, result);
            return CollectionsMarshal.AsSpan(result);
        }

        public void Search(Rect area, List<T> result)
        {
            for (int i = 0; i < _container.Count; i++)
            {
                if (_container[i].Rect.Overlaps(area))
                {
                    result.Add(_container[i].Item);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (_children[i] is null) continue;

                if (area.Contains(_childrenRects[i]))
                {
                    _children[i]?.GetItems(result);
                }
                else
                {
                    _children[i]?.Search(area, result);
                }
            }
        }

        public void GetItems(List<T> list)
        {
            for (int i = 0; i < _container.Count; i++)
            { list.Add(_container[i].Item); }

            for (int i = 0; i < _children.Length; i++)
            {
                if (_children[i] is null) continue;
                _children[i]?.GetItems(list);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _container.Count; i++)
            {
                yield return _container[i].Item;
            }

            for (int i = 0; i < _children.Length; i++)
            {
                QuadTree<T>? child = _children[i];

                if (child is null) continue;

                foreach (T item in child)
                { yield return item; }
            }
        }

        public ReadOnlySpan<QuadTree<T>> Branches(Vector2 point)
        {
            List<QuadTree<T>> result = new();
            Branches(point, result);
            return CollectionsMarshal.AsSpan(result);
        }

        public void Branches(Vector2 point, List<QuadTree<T>> list)
        {
            list.Add(this);
            for (int i = 0; i < _childrenRects.Length; i++)
            {
                if (_childrenRects[i].Contains(point))
                {
                    _children[i]?.Branches(point, list);
                    break;
                }
            }
        }

        /*
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Container.Count; i++)
            {
                yield return Container[i].Item2;
            }

            for (int i = 0; i < Children.Length; i++)
            {
                StaticQuadTree<T>? child = Children[i];

                if (child is null) continue;

                foreach (T item in child)
                { yield return item; }
            }
        }
        */

        public QuadTree<T>? GetBranch(QuadTreeLocation branch) => GetBranch(0, branch.Branches);
        QuadTree<T>? GetBranch(int i, uint[] branches)
        {
            if (i >= branches.Length) return this;
            QuadTree<T>? child = _children[branches[i]];
            return child?.GetBranch(i + 1, branches);
        }

        public bool Remove(T? element)
        {
            if (element is null) return false;

            for (int i = _container.Count - 1; i >= 0; i--)
            {
                if (element.Equals(_container[i].Item))
                {
                    _container.RemoveAt(i);
                    return true;
                }
            }

            for (int i = 0; i < _children.Length; i++)
            {
                if (_children[i]?.Remove(element) ?? false)
                {
                    return true;
                }
            }

            return true;
        }
        public bool Remove(QuadTreeLocation branch, T? element)
        {
            if (element is null) return false;

            QuadTree<T>? _branch = GetBranch(0, branch.Branches);
            if (_branch is null) return false;

            return _branch.Remove(element);
        }
        public bool Remove<T2>(T2? element) where T2 : IEquatable<T>
        {
            if (element is null) return false;

            for (int i = _container.Count - 1; i >= 0; i--)
            {
                if (element.Equals(_container[i].Item))
                {
                    _container.RemoveAt(i);
                    return true;
                }
            }

            for (int i = 0; i < _children.Length; i++)
            {
                if (_children[i]?.Remove<T2>(element) ?? false)
                {
                    return true;
                }
            }

            return true;
        }

        public QuadTreeLocation Relocate(QuadTreeLocation branch, T element, Vector2 position)
            => Relocate(branch, element, new Rect(position, Vector2.Zero));
        public QuadTreeLocation Relocate(QuadTreeLocation branch, T element, Rect rect)
        {
            Remove(branch, element);
            return Add(element, rect);
        }

        public void Relocate(ref QuadTreeLocation branch, T element, Vector2 position)
            => Relocate(ref branch, element, new Rect(position, Vector2.Zero));
        public void Relocate(ref QuadTreeLocation branch, T element, Rect rect)
        {
            Remove(branch, element);
            branch = Add(element, rect);
        }
    }
}
