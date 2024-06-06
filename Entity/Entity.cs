using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace ConsoleGame;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class Entity
{
    public IReadOnlyList<Component> Components => components;
    readonly List<Component> components;
    public int Tags;
    public bool IsDestroyed;
    public Vector2 Position;
    public string? Name;

    public bool IsSolid;
    public bool IsStatic;

    public QuadTreeBranch QuadTreeLocation;

    public Entity()
    {
        components = new List<Component>();
        Tags = 0;
        Position = default;
        Name = null;
    }

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

    public bool RemoveComponent(Component component)
        => components.Remove(component);

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

    public ReadOnlySpan<T> GetComponents<T>()
    {
        List<T> result = new(components.Count);
        for (int i = 0; i < components.Count; i++)
        {
            if (components[i] is T _result)
            { result.Add(_result); }
        }
        return CollectionsMarshal.AsSpan(result);
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
        components.Clear();
    }

    public override string ToString()
    {
        string name = Name ?? base.ToString() ?? "?";
        return IsDestroyed ? $"{name} (destroyed)" : name;
    }

    string GetDebuggerDisplay() => ToString();

    public void DoCollisions()
    {
        if (!IsSolid) return;

        ReadOnlySpan<Entity> collided = Game.Instance.Scene.ObjectsAt(Position, .5f);
        for (int i = 0; i < collided.Length; i++)
        {
            Entity other = collided[i];
            if (other == this) continue;
            if (!other.IsSolid) continue;

            DoCollision(other);
        }
    }
    public void DoCollision(Entity other)
    {
        const float ErrorMultiplier = .4f;

        if (IsStatic && other.IsStatic) return;

        if (!IsStatic && other.IsStatic)
        {
            Vector2 error = (Position - other.Position) * ErrorMultiplier;
            Position += error;
        }
        else if (IsStatic && !other.IsStatic)
        {
            Vector2 error = (Position - other.Position) * ErrorMultiplier;
            other.Position -= error;
        }
        else
        {
            Vector2 error = (Position - other.Position) * (ErrorMultiplier * .5f);
            Position += error;
            other.Position -= error;
        }
    }

    public void DoBounceOff(ref Vector2 velocity)
    {
        // if (!IsSolid) return false;

        ReadOnlySpan<Entity> collided = Game.Instance.Scene.ObjectsAt(Position, 1f);
        for (int i = 0; i < collided.Length; i++)
        {
            Entity other = collided[i];
            if (other == this) continue;
            if (!other.IsSolid) continue;

            DoBounceOff(other, ref velocity);
        }
    }
    public void DoBounceOff(Entity other, ref Vector2 velocity)
    {
        float speed = velocity.Length();
        velocity = Vector2.Normalize(Position - other.Position) * speed;
    }

    public bool ClampIntoWord() => WorldBorders.Clamp(Game.Instance.Scene.SizeR, ref Position);
}
