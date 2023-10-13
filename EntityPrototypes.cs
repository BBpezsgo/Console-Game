using ConsoleGame.Behavior;

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
                    newEntity.IsSolid = true;
                    newEntity.IsStatic = true;
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
                    newEntity.IsSolid = true;
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
                GameObjectPrototype.ENEMY_FACTORY, (int networkId, ObjectOwner owner) => {
                    Entity newEntity = new("Enemy Factory");
                    newEntity.Tags |= Tags.Enemy;
                    newEntity.IsSolid = true;
                    newEntity.IsStatic = true;
                    newEntity.AddComponent(new NetworkEntityComponent(newEntity)
                    {
                        NetworkId = networkId,
                        ObjectId = GameObjectPrototype.ENEMY_FACTORY,
                        Owner = owner,
                    });
                    newEntity.AddComponent(new DamageableRendererComponent(newEntity)
                    {
                        Character = 'A',
                        Color = ByteColor.BrightRed,
                        Priority = Depths.OTHER_LIVING,
                    });
                    newEntity.AddComponent(new SimpleDestroyableThing(newEntity, 25f));
                    newEntity.AddComponent(new EnemyFactoryBehavior(newEntity));
                    return newEntity;
                }
            },
            {
                GameObjectPrototype.HELPER_TURRET, (int networkId, ObjectOwner owner) => {
                    Entity newEntity = new("Helper Turret");
                    newEntity.Tags |= Tags.Helper;
                    newEntity.IsSolid = true;
                    newEntity.IsStatic = true;
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
            {
                GameObjectPrototype.HELPER_TURRET2, (int networkId, ObjectOwner owner) => {
                    Entity newEntity = new("Helper Turret2");
                    newEntity.Tags |= Tags.Helper;
                    newEntity.IsSolid = true;
                    newEntity.IsStatic = true;
                    newEntity.AddComponent(new NetworkEntityComponent(newEntity)
                    {
                        NetworkId = networkId,
                        ObjectId = GameObjectPrototype.HELPER_TURRET2,
                        Owner = owner,
                    });
                    newEntity.AddComponent(new NetworkTransform(newEntity));
                    newEntity.AddComponent(new DamageableRendererComponent(newEntity)
                    {
                        Character = 'V',
                        Color = ByteColor.BrightBlue,
                        Priority = Depths.OTHER_LIVING,
                    });
                    newEntity.AddComponent(new HelperTurret2Behavior(newEntity));
                    newEntity.AddComponent(new EntityHoverPopup(newEntity));
                    return newEntity;
                }
            },
            {
                GameObjectPrototype.HELPER_THINGY, (int networkId, ObjectOwner owner) => {
                    Entity newEntity = new("Helper Thingy");
                    newEntity.Tags |= Tags.Helper;
                    newEntity.IsSolid = true;
                    newEntity.AddComponent(new NetworkEntityComponent(newEntity)
                    {
                        NetworkId = networkId,
                        ObjectId = GameObjectPrototype.HELPER_THINGY,
                        Owner = owner,
                    });
                    newEntity.AddComponent(new NetworkTransform(newEntity));
                    newEntity.AddComponent(new DamageableRendererComponent(newEntity)
                    {
                        Character = 'O',
                        Color = ByteColor.BrightBlue,
                        Priority = Depths.OTHER_LIVING,
                    });
                    newEntity.AddComponent(new HelperThingyBehavior(newEntity));
                    newEntity.AddComponent(new EntityHoverPopup(newEntity));
                    return newEntity;
                }
            },
        };
    }
}
