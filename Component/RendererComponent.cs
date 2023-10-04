namespace ConsoleGame
{
    public class RendererComponent : Component
    {
        public byte Color;
        public char Character;

        readonly TransformComponent Transform;

        public RendererComponent(Entity entity) : base(entity)
        {
            Game.Instance.Scene.RendererComponents.Register(this);
            Transform = entity.GetComponentOfType<TransformComponent>() ?? throw new NullReferenceException();
        }

        public override void Destroy()
        {
            base.Destroy();
            Game.Instance.Scene.RendererComponents.Deregister(this);
        }

        public override void Update()
        {
            base.Update();

            Vector position = Transform.Position;
            if (!Game.Instance.Scene.Size.Contains(position)) return;
            ref Win32.CharInfo pixel = ref Game.Renderer[Game.WorldToConsole(position)];
            pixel.Attributes = Color;
            pixel.Char = Character;
        }
    }
}
