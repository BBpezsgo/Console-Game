namespace ConsoleGame
{
    public class GameObject
    {
        public Vector Position;
        public Vector Speed;
        public bool IsDestroyed;

        public GameObject(Vector position)
        {
            Position = position;
            Speed = Vector.Zero;
            IsDestroyed = false;
        }

        public GameObject(Vector position, Vector speed)
        {
            Position = position;
            Speed = speed;
            IsDestroyed = false;
        }

        public virtual void Render()
        {
            if (!Game.Instance.Scene.Size.Contains(Position)) return;
            Game.Renderer[Game.WorldToConsole(Position)].Foreground = 0b_0100;
            Game.Renderer[Game.WorldToConsole(Position)].Char = 'O';
        }

        public virtual void Tick() { }
    }
}
