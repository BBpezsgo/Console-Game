namespace ConsoleGame
{
    public abstract class QuadTree
    {
        public const int MAX_DEPTH = 4;
    }

    public readonly struct QuadTreeLocation
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
    }

    public class QuadTree<T> : QuadTree
    {
        public int Depth;
        public Rect Rect;
        public Rect[] ChildrenRects;
        public QuadTree<T>?[] Children;

        public List<(Rect, T)> Container;

        QuadTree()
        {
            ChildrenRects = new Rect[4];
            Children = new QuadTree<T>?[4];
            Container = new List<(Rect, T)>();
        }

        QuadTree(Rect rect, int depth) : this()
        {
            Rect = rect;
            Depth = depth;
            Resize(rect);
        }

        public QuadTree(Rect rect) : this()
        {
            Rect = rect;
            Depth = 0;
            Resize(rect);
        }

        public void Resize(Rect rect)
        {
            Clear();
            Rect = rect;
            Rect childSize = new(rect.Position, rect.Size * 0.5f);
            ChildrenRects[0] = childSize;
            ChildrenRects[1] = Rect.Move(childSize, childSize.Size.X, 0f);
            ChildrenRects[2] = Rect.Move(childSize, 0f, childSize.Size.Y);
            ChildrenRects[3] = Rect.Move(childSize, childSize.Size.X, childSize.Size.Y);
        }

        public void Clear()
        {
            Container.Clear();
            for (int i = 0; i < Children.Length; i++)
            {
                Children[i]?.Clear();
                Children[i] = null;
            }
        }

        public int Count
        {
            get
            {
                int count = Container.Count;
                for (int i = 0; i < Children.Length; i++)
                {
                    count += Children[i]?.Count ?? 0;
                }
                return count;
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
                if (ChildrenRects[i].Contains(itemArea))
                {
                    if (Depth + 1 < MAX_DEPTH)
                    {
                        if (Children[i] == null)
                        {
                            Children[i] = new QuadTree<T>(ChildrenRects[i], Depth + 1);
                        }

                        if (i is < 0b_00 or > 0b_11) throw new Exception("Bruh");

                        branchId <<= 2;
                        branchId |= (uint)i;
                        depth++;

                        Children[i]?.Add(item, itemArea, ref branchId, ref depth);
                        return;
                    }
                }
            }

            Container.Add((itemArea, item));
        }

        public T[] Search(Rect area)
        {
            List<T> result = new();
            Search(area, result);
            return result.ToArray();
        }

        public void Search(Rect area, List<T> result)
        {
            for (int i = 0; i < Container.Count; i++)
            {
                if (Container[i].Item1.Overlaps(area))
                {
                    result.Add(Container[i].Item2);
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (Children[i] == null) continue;

                if (area.Contains(ChildrenRects[i]))
                {
                    Children[i]?.GetItems(result);
                }
                else
                {
                    Children[i]?.Search(area, result);
                }
            }
        }

        public void GetItems(List<T> list)
        {
            for (int i = 0; i < Container.Count; i++)
            { list.Add(Container[i].Item2); }

            for (int i = 0; i < Children.Length; i++)
            {
                if (Children[i] == null) continue;
                Children[i]?.GetItems(list);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Container.Count; i++)
            {
                yield return Container[i].Item2;
            }

            for (int i = 0; i < Children.Length; i++)
            {
                QuadTree<T>? child = Children[i];

                if (child == null) continue;

                foreach (T item in child)
                { yield return item; }
            }
        }

        public QuadTree<T>[] Branches(Vector point)
        {
            List<QuadTree<T>> result = new();
            Branches(point, result);
            return result.ToArray();
        }

        public void Branches(Vector point, List<QuadTree<T>> list)
        {
            list.Add(this);
            for (int i = 0; i < ChildrenRects.Length; i++)
            {
                if (ChildrenRects[i].Contains(point))
                {
                    Children[i]?.Branches(point, list);
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

                if (child == null) continue;

                foreach (T item in child)
                { yield return item; }
            }
        }
        */

        public QuadTree<T>? GetBranch(QuadTreeLocation branch) => GetBranch(0, branch.Branches);
        QuadTree<T>? GetBranch(int i, uint[] branches)
        {
            if (i >= branches.Length) return this;
            QuadTree<T>? child = Children[branches[i]];
            return child?.GetBranch(i + 1, branches);
        }

        public bool Remove(T element)
        {
            for (int i = Container.Count - 1; i >= 0; i--)
            {
                if (object.Equals(Container[i].Item2, element))
                {
                    Container.RemoveAt(i);
                    return true;
                }
            }

            for (int i = 0; i < Children.Length; i++)
            {
                if (Children[i]?.Remove(element) ?? false)
                {
                    return true;
                }
            }

            return true;
        }
        public bool Remove(QuadTreeLocation branch, T element)
        {
            QuadTree<T>? _branch = GetBranch(0, branch.Branches);
            if (_branch == null) return false;
            return _branch.Remove(element);
        }
        public bool Remove<T2>(T2 element) where T2 : IEquatable<T>
        {
            for (int i = Container.Count - 1; i >= 0; i--)
            {
                if (((IEquatable<T>)element).Equals(Container[i].Item2))
                {
                    Container.RemoveAt(i);
                    return true;
                }
            }

            for (int i = 0; i < Children.Length; i++)
            {
                if (Children[i]?.Remove<T2>(element) ?? false)
                {
                    return true;
                }
            }

            return true;
        }

        public QuadTreeLocation Relocate(QuadTreeLocation branch, T element, Vector position)
            => Relocate(branch, element, new Rect(position, Vector.Zero));
        public QuadTreeLocation Relocate(QuadTreeLocation branch, T element, Rect rect)
        {
            Remove(branch, element);
            return Add(element, rect);
        }

        public void Relocate(ref QuadTreeLocation branch, T element, Vector position)
            => Relocate(ref branch, element, new Rect(position, Vector.Zero));
        public void Relocate(ref QuadTreeLocation branch, T element, Rect rect)
        {
            Remove(branch, element);
            branch = Add(element, rect);
        }
    }
}
