namespace ConsoleGame
{
    public class Component
    {
        public int SystemIndex;
        public readonly Entity Entity;
        public int ComponentIndex;

        public ref bool IsDestroyed => ref Entity.IsDestroyed;

        public ref Vector Position => ref Entity.Position;

        public Component(Entity entity)
        {
            SystemIndex = -1;
            Entity = entity;
        }

        public virtual void Make()
        {
            Game.Instance.Scene.AllComponents.Register(this);
        }

        public virtual void Update() { }

        public virtual void Destroy()
        {
            Game.Instance.Scene.AllComponents.Deregister(SystemIndex);
        }
    }
}
