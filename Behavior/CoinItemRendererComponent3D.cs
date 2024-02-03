using System.Numerics;

namespace ConsoleGame.Behavior
{
    public class CoinItemRendererComponent3D : RendererComponent3D
    {
        readonly CoinItemBehavior Item;
        float SpawnedTime;

        public CoinItemRendererComponent3D(Entity entity) : base(entity)
        {
            Entity.Tags |= Tags.Item;
            Item = Entity.GetComponent<CoinItemBehavior>();

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
            Color colorA = new(.7f, .7f, .2f);
            Color colorB = new(1f, 1f, .95f);
            float t = (MathF.Sin(lifetime * 1.5f) + 1f) * .5f;
            Material.AmbientColor = (colorA * t) + (colorB * (1f - t));

            base.Render(meshBuffer);
        }
    }
}
