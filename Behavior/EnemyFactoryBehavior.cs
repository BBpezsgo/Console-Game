namespace ConsoleGame
{
    internal class EnemyFactoryBehavior : FactoryComponent<int>
    {
        public EnemyFactoryBehavior(Entity entity) : base(entity) { }

        public override void Destroy()
        {
            base.Destroy();

            Entity newEntity = new("Explosion Particles")
            { Position = Position };
            newEntity.SetComponents(new ParticlesRendererComponent(newEntity, PredefinedEffects.LargeExplosion) { Priority = Depths.EFFECT });
            Game.Instance.Scene.AddEntity(newEntity);
        }

        public override void Update()
        {
            base.Update();

            if (Queue.Count == 0)
            {
                base.Enqueue(GameObjectPrototype.ENEMY, 10f);
            }
        }

        protected override void OnProductDone(int product)
        {
            Entity newEntity = EntityPrototypes.Builders[product].Invoke(Game.Instance.Scene.GenerateNetworkId(), Owner);
            newEntity.Position = Position + new Vector(1f, 2f) + Random.Point(-.01f, .01f);
            Game.Instance.Scene.AddEntity(newEntity);
        }
    }
}
