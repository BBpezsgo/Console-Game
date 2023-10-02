namespace ConsoleGame
{
    public class GameObject
    {
        public Vector Position;
        public bool IsDestroyed;
        public int Tag;

        public GameObject()
        {
            Position = Vector.Zero;
            IsDestroyed = false;
            Tag |= Tags.Effect;
        }

        public virtual void Render()
        {
            if (!Game.Instance.Scene.Size.Contains(Position)) return;
            Game.Renderer[Game.WorldToConsole(Position)].Foreground = 0b_0100;
            Game.Renderer[Game.WorldToConsole(Position)].Char = 'O';
        }

        public virtual void Tick() { }
        public virtual void OnDestroy() { }

        public bool HasTag(int tag) => (Tag & tag) != 0;
    }
}
