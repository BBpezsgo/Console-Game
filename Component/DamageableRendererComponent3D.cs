using Win32;

namespace ConsoleGame
{
    internal class DamageableRendererComponent3D : RendererComponent3D
    {
        float LastDamaged;

        const float BlinkPerSec = 4f * 2;
        const float BlinkingDuration = 1f;

        static readonly Material DamagedMaterial = new()
        {
            DiffuseColor = Color.White,
        };

        public DamageableRendererComponent3D(Entity entity) : base(entity) { }

        public DamageableRendererComponent3D(Entity entity, Action<Material> materializer) : base(entity, materializer) { }

        public override void Render(List<TransformedMesh> meshBuffer)
        {
            float lastDamagedInterval = Time.Now - LastDamaged;
            if (lastDamagedInterval < BlinkingDuration && (int)(lastDamagedInterval * BlinkPerSec) % 2 == 0)
            { Mesh.Materials[0] = DamagedMaterial; }
            else
            { Mesh.Materials[0] = Material; }

            base.Render(meshBuffer);
        }

        public void OnDamage() => LastDamaged = Time.Now;
    }
}
