namespace ConsoleGame
{
    public class RendererComponent : Component
    {
        public byte Color;
        public char Character;
        public byte Priority;

        public RendererComponent(Entity entity) : base(entity) { }

        public override void Make()
        {
            base.Make();
            Game.Instance.Scene.RendererComponents.Register(this);
        }

        public override void Destroy()
        {
            base.Destroy();
            Game.Instance.Scene.RendererComponents.Deregister(this);
        }

        public virtual void Render()
        {
            if (!Game.IsVisible(Position)) return;

            VectorInt p = Game.WorldToConsole(Position);

            ref byte depth = ref Game.DepthBuffer[p];

            if (depth > Priority) return;

            depth = Priority;

            ref Win32.CharInfo pixel = ref Game.Renderer[p];

            pixel.Foreground = Color;
            pixel.Char = Character;
        }
    }
}
