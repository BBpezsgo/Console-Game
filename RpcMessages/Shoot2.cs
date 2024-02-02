using System.Numerics;
using DataUtilities.Serializer;

namespace ConsoleGame.RpcMessages
{
    public struct Shoot : ISerializable<Shoot>
    {
        public Vector2 Origin;
        public Vector2 Direction;

        public Shoot()
        {
            Origin = Vector2.Zero;
            Direction = Vector2.Zero;
        }

        public Shoot(Vector2 origin, Vector2 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public void Deserialize(Deserializer deserializer)
        {
            Origin = deserializer.DeserializeVector2();
            Direction = deserializer.DeserializeDirection();
        }

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(Origin);
            serializer.SerializeDirection(Direction);
        }
    }
}
