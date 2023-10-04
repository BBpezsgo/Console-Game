namespace ConsoleGame
{
    public delegate Entity EntityPrototypeBuilder(int networkId, ObjectOwner owner);

    public static class EntityPrototypes
    {
        public static readonly Dictionary<int, EntityPrototypeBuilder> Builders = new()
        {
            {
                GameObjectPrototype.PLAYER, (int networkId, ObjectOwner owner) => {
                    Entity newEntity = new();
                    newEntity.AddComponent(new TransformComponent(newEntity));
                    newEntity.AddComponent(new NetworkEntityComponent(newEntity)
                    {
                        NetworkId = networkId,
                        ObjectId = GameObjectPrototype.PLAYER,
                        Owner = owner,
                    });
                    newEntity.AddComponent(new NetworkTransform(newEntity));
                    newEntity.AddComponent(new RendererComponent(newEntity)
                    {
                        Character = 'O',
                        Color = ByteColor.Magenta,
                    });
                    return newEntity;
                }
            },
            {
                GameObjectPrototype.ENEMY, (int networkId, ObjectOwner owner) => {
                    Entity newEntity = new();
                    newEntity.AddComponent(new TransformComponent(newEntity));
                    newEntity.AddComponent(new NetworkEntityComponent(newEntity)
                    {
                        NetworkId = networkId,
                        ObjectId = GameObjectPrototype.ENEMY,
                        Owner = owner,
                    });
                    newEntity.AddComponent(new NetworkTransform(newEntity));
                    newEntity.AddComponent(new RendererComponent(newEntity)
                    {
                        Character = '@',
                        Color = ByteColor.BrightRed,
                    });
                    return newEntity;
                }
            },
            {
                GameObjectPrototype.HELPER_TURRET, (int networkId, ObjectOwner owner) => {
                    Entity newEntity = new();
                    newEntity.AddComponent(new TransformComponent(newEntity));
                    newEntity.AddComponent(new NetworkEntityComponent(newEntity)
                    {
                        NetworkId = networkId,
                        ObjectId = GameObjectPrototype.HELPER_TURRET,
                        Owner = owner,
                    });
                    newEntity.AddComponent(new NetworkTransform(newEntity));
                    newEntity.AddComponent(new RendererComponent(newEntity)
                    {
                        Character = '#',
                        Color = ByteColor.BrightCyan,
                    });
                    return newEntity;
                }
            },
        };

    }
}
