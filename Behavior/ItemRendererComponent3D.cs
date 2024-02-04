using System.Numerics;
using Win32.Gdi32;

namespace ConsoleGame.Behavior
{
    public class ItemRendererComponent3D : RendererComponent3D
    {
        public GdiColor Color;

        readonly ItemBehavior Item;
        float SpawnedTime;

        public ItemRendererComponent3D(Entity entity) : base(entity)
        {
            Color = GdiColor.Magenta;
            Entity.Tags |= Tags.Item;
            Item = Entity.GetComponent<ItemBehavior>();

            Mesh.Scale(new Vector3(.7f, .7f, .2f));
        }

        public override void Make()
        {
            base.Make();
            SpawnedTime = Time.Now;
        }

        public override void Render(List<TransformedMesh> meshBuffer)
        {
            float lifetime = Time.Now - SpawnedTime;

            Matrix4x4.MakeRotationY(ref Rotation, lifetime * 1.1f);
            GdiColor colorA = Color;
            GdiColor colorB = GdiColor.White;
            float t = (MathF.Sin(lifetime * 1.5f) + 1f) * .5f;
            Material.AmbientColor = (colorA * t) + (colorB * (1f - t));

            base.Render(meshBuffer);
        }
    }
}
