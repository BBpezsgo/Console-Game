namespace ConsoleGame;

public readonly struct BaseSystem<T> where T : Component
{
    public readonly List<T> Components;
    public readonly bool IsBase;

    public BaseSystem(bool isBase)
    {
        Components = new List<T>();
        IsBase = isBase;
    }

    public void Register(T component)
    {
        Components.Add(component);
        if (!IsBase) return;
        component.SystemIndex = Components.Count - 1;
    }

    public bool Deregister(T component)
    {
        int index = Components.IndexOf(component);
        if (index < 0) return false;
        Deregister(index);
        return true;
    }

    public void Deregister(int index)
    {
        Components.RemoveAt(index);
        if (!IsBase) return;
        for (int i = index; i < Components.Count; i++)
        { Components[i].SystemIndex = i; }
    }

    public void Update()
    {
        for (int i = 0; i < Components.Count; i++)
        {
            Components[i].Update();
        }
    }
}
