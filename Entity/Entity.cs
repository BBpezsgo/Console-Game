using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame
{
    public class Entity
    {
        public Component[] Components;
        public int Tags;
        public bool IsDestroyed;

        public Entity()
        {
            Components = Array.Empty<Component>();
            Tags = 0;
        }

        public Entity(IEnumerable<Component> components)
        {
            Components = components.ToArray();
            Tags = 0;
        }

        public Entity(params Component[] components)
        {
            Components = components;
            Tags = 0;
        }

        public void AddComponent(Component component)
        {
            Components = new List<Component>(Components) { component }.ToArray();
        }

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
        public T GetComponentOfType<T>()
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

        public bool TryGetComponentOfType<T>([NotNullWhen(true)] out T? component)
        {
            component = GetComponentOfType<T>();
            return component != null;
        }
    }
}
