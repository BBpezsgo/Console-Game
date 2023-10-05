namespace ConsoleGame
{
    public delegate Entity EntityPrototypeBuilder(int networkId, ObjectOwner owner);

    public static class EntityPrototypes
    {
        public static readonly Dictionary<int, EntityPrototypeBuilder> Builders = new()
        {
            {
                GameObjectPrototype.PLAYER, (int networkId, ObjectOwner owner) => {
                    Entity newEntity = new("Player");
                    newEntity.Tags |= Tags.Player;
                    newEntity.AddComponent(new NetworkEntityComponent(newEntity)
                    {
                        NetworkId = networkId,
                        ObjectId = GameObjectPrototype.PLAYER,
                        Owner = owner,
                    });
                    newEntity.AddComponent(new NetworkTransform(newEntity));
                    newEntity.AddComponent(new DamageableRendererComponent(newEntity)
                    {
                        Character = 'O',
                        Color = ByteColor.Magenta,
                        Priority = Depths.PLAYER,
                    });
                    newEntity.AddComponent(new PlayerBehavior(newEntity));
                    return newEntity;
                }
            },
            {
                GameObjectPrototype.ENEMY, (int networkId, ObjectOwner owner) => {
                    Entity newEntity = new("Enemy");
                    newEntity.Tags |= Tags.Enemy;
                    newEntity.AddComponent(new NetworkEntityComponent(newEntity)
                    {
                        NetworkId = networkId,
                        ObjectId = GameObjectPrototype.ENEMY,
                        Owner = owner,
                    });
                    newEntity.AddComponent(new NetworkTransform(newEntity));
                    newEntity.AddComponent(new DamageableRendererComponent(newEntity)
                    {
                        Character = '@',
                        Color = ByteColor.BrightRed,
                        Priority = Depths.OTHER_LIVING,
                    });
                    newEntity.AddComponent(new EnemyBehavior(newEntity));
                    return newEntity;
                }
            },
            {
                GameObjectPrototype.HELPER_TURRET, (int networkId, ObjectOwner owner) => {
                    Entity newEntity = new("Helper Turret");
                    newEntity.Tags |= Tags.Helper;
                    newEntity.AddComponent(new NetworkEntityComponent(newEntity)
                    {
                        NetworkId = networkId,
                        ObjectId = GameObjectPrototype.HELPER_TURRET,
                        Owner = owner,
                    });
                    newEntity.AddComponent(new NetworkTransform(newEntity));
                    newEntity.AddComponent(new DamageableRendererComponent(newEntity)
                    {
                        Character = 'X',
                        Color = ByteColor.BrightBlue,
                        Priority = Depths.OTHER_LIVING,
                    });
                    newEntity.AddComponent(new HelperTurretBehavior(newEntity));
                    newEntity.AddComponent(new EntityHoverPopup(newEntity));
                    return newEntity;
                }
            },
        };
    }
}
