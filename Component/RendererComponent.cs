namespace ConsoleGame
{
    public class RendererComponent : Component
    {
        public byte Color;
        public char Character;

        public RendererComponent(Entity entity) : base(entity)
        {
            Game.Instance.Scene.RendererComponents.Register(this);
        }

        public override void Destroy()
        {
            base.Destroy();
            Game.Instance.Scene.RendererComponents.Deregister(this);
        }

        public virtual void Render()
        {
            Vector position = Position;
            if (!Game.Instance.Scene.Size.Contains(position)) return;
            ref Win32.CharInfo pixel = ref Game.Renderer[Game.WorldToConsole(position)];
            pixel.Attributes = Color;
            pixel.Char = Character;
        }
    }
}
