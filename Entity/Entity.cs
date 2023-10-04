using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class Entity
    {
        public Component[] Components;
        public int Tags;
        public bool IsDestroyed;
        public Vector Position;
        public string? Name;

        public Entity()
        {
            Components = Array.Empty<Component>();
            Tags = 0;
            Position = default;
        }

        public Entity(IEnumerable<Component> components) : this(components.ToArray())
        {

        }

        public Entity(params Component[] components)
        {
            Components = components;
            Tags = 0;
            Position = default;

            for (int i = 0; i < Components.Length; i++)
            { Components[i].ComponentIndex = i; }
        }

        public void AddComponent(Component component)
        {
            Components = new List<Component>(Components) { component }.ToArray();
            component.ComponentIndex = Components.Length - 1;
        }

        public void SetComponents(params Component[] components)
        {
            Components = components;

            for (int i = 0; i < Components.Length; i++)
            { Components[i].ComponentIndex = i; }
        }

        public void SetComponents(IEnumerable<Component> components)
            => SetComponents(components.ToArray());

        public bool RemoveComponent(Component component)
        {
            int i = Array.IndexOf(Components, component);
            if (i < 0) return false;
            List<Component> temp = new(Components);
            temp.RemoveAt(i);
            Components = temp.ToArray();
            return true;
        }

        /// <exception cref="Exception"></exception>
        public T GetComponent<T>()
        {
            for (int i = 0; i < Components.Length; i++)
            {
                if (Components[i] is T result)
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
            for (int i = 0; i < Components.Length; i++)
            {
                if (Components[i] is T component)
                { return component; }
            }
            return default;
        }

        public void OnDestroy()
        {
            for (int i = 0; i < Components.Length; i++)
            {
                Components[i].Destroy();
            }
        }

        public override string ToString()
        {
            string yeah = Name ?? base.ToString() ?? "?";
            return IsDestroyed ? $"{yeah} (null)" : yeah;
        }

        string GetDebuggerDisplay() => ToString();
    }
}
