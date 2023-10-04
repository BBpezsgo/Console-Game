namespace ConsoleGame
{
    public class Component
    {
        public int SystemIndex;
        public readonly Entity Entity;

        public bool IsDestroyed
        {
            get => Entity.IsDestroyed;
            set => Entity.IsDestroyed = value;
        }

        public Component(Entity entity)
        {
            SystemIndex = -1;
            Entity = entity;

            Game.Instance.Scene.AllComponents.Register(this);
        }

        public virtual void Update()
        {

        }

        public virtual void Destroy()
        {
            Game.Instance.Scene.AllComponents.Deregister(SystemIndex);
        }
    }
}
