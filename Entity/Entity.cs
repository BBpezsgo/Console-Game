using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class Entity
    {
        public IReadOnlyList<Component> Components => components;
        readonly List<Component> components;
        public int Tags;
        public bool IsDestroyed;
        public Vector Position;
        public string? Name;

        public Entity()
        {
            components = new List<Component>();
            Tags = 0;
            Position = default;
            Name = null;
        }

        public Entity(IEnumerable<Component> components) : this(components.ToArray()) { }
        public Entity(params Component[] components)
        {
            this.components = new List<Component>(components);
            Tags = 0;
            Position = default;

            for (int i = 0; i < this.components.Count; i++)
            { this.components[i].ComponentIndex = i; }

            for (int i = 0; i < this.components.Count; i++)
            { this.components[i].Make(); }
        }

        public Entity(string name)
        {
            this.components = new List<Component>();
            Tags = 0;
            Position = default;
            Name = name;
        }

        public Entity(string name, IEnumerable<Component> components) : this(name, components.ToArray()) { }
        public Entity(string name, params Component[] components)
        {
            this.components = new List<Component>(components);
            Tags = 0;
            Position = default;
            Name = name;

            for (int i = 0; i < this.components.Count; i++)
            { this.components[i].ComponentIndex = i; }

            for (int i = 0; i < this.components.Count; i++)
            { this.components[i].Make(); }
        }

        public void AddComponent(Component component)
        {
            this.components.Add(component);

            component.ComponentIndex = this.components.Count - 1;
            component.Make();
        }

        public void SetComponents(params Component[] components)
        {
            this.components.Clear();
            this.components.AddRange(components);

            for (int i = 0; i < this.components.Count; i++)
            { this.components[i].ComponentIndex = i; }

            for (int i = 0; i < this.components.Count; i++)
            { this.components[i].Make(); }
        }

        public void SetComponents(IEnumerable<Component> components)
            => SetComponents(components.ToArray());

        public bool RemoveComponent(Component component)
        {
            return this.components.Remove(component);
        }

        /// <exception cref="Exception"></exception>
        public T GetComponent<T>()
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T result)
                {
                    return result;
                }
            }
            throw new Exception($"Component {typeof(T).Name} not found in entity {this}");
        }

        public bool TryGetComponent<T>([NotNullWhen(true)] out T? component)
        {
            component = TryGetComponent<T>();
            return component != null;
        }

        public T? TryGetComponent<T>()
        {
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] is T component)
                { return component; }
            }
            return default;
        }

        public void OnDestroy()
        {
            for (int i = 0; i < components.Count; i++)
            { components[i].Destroy(); }
        }

        public override string ToString()
        {
            string yeah = Name ?? base.ToString() ?? "?";
            return IsDestroyed ? $"{yeah} (null)" : yeah;
        }

        string GetDebuggerDisplay() => ToString();
    }
}
