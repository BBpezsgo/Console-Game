using Win32;

namespace ConsoleGame
{
    public class EntityHoverPopup : RendererComponent
    {
        public bool IsShown;
        public const float DistanceToShow = 2f;
        
        public static bool ShouldNotShow;
        public static EntityHoverPopup? AlreadyShown;

        public VectorInt Size;

        readonly ICanDrawEntityHoverPopup[] DrawerComponents;

        public EntityHoverPopup(Entity entity) : base(entity)
        {
            Size = new VectorInt(20, 5);
            DrawerComponents = Entity.GetComponents<ICanDrawEntityHoverPopup>();
        }

        public override void Render()
        {
            if (!IsShown) return;

            VectorInt conPos = Game.WorldToConsole(Position);

            RectInt box = new(conPos.X + 1, conPos.Y - Size.Y, Size.X, Size.Y);

            if (box.Y < 4) box.Y = 4;
            if (box.X < 0) box.X = 0;
            if (box.Right >= Game.Renderer.Width) box.X += Game.Renderer.Width - 1 - box.Right;
            if (box.Bottom >= Game.Renderer.Height) box.X += Game.Renderer.Height - 1 - box.Bottom;

            Game.DepthBuffer.SetRect(box, Depths.GUI);

            GUI.Box(box, ByteColor.Black, ByteColor.White, Ascii.BoxSides);
            box.Expand(-1);

            for (int i = 0; i < DrawerComponents.Length; i++)
            {
                DrawerComponents[i].RenderHoverPopup(box);
            }
        }

        public override void Update()
        {
            if (ShouldNotShow)
            {
                IsShown = false;
                return;
            }

            float dist = (Game.ConsoleToWorld(Mouse.RecordedPosition) - Position).SqrMagnitude;
            bool shouldShown = dist <= (DistanceToShow * DistanceToShow);

            if (!shouldShown)
            {
                IsShown = false;
                return;
            }

            if (AlreadyShown != null)
            {
                float dist2 = (Game.ConsoleToWorld(Mouse.RecordedPosition) - AlreadyShown.Position).SqrMagnitude;
                if (dist2 < dist)
                {
                    IsShown = false;
                    return;
                }
                AlreadyShown.IsShown = false;
            }

            AlreadyShown = this;
            IsShown = true;
        }
    }

    public interface ICanDrawEntityHoverPopup
    {
        public void RenderHoverPopup(RectInt content);
    }
}
