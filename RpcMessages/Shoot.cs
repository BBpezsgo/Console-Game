using System.Numerics;
using DataUtilities.Serializer;

namespace ConsoleGame.RpcMessages
{
    public struct Shoot2 : ISerializable<Shoot2>
    {
        public Vector2 Origin;
        public Vector2 Direction;
        public float Speed;

        public Shoot2()
        {
            Origin = Vector2.Zero;
            Direction = Vector2.Zero;
            Speed = 0f;
        }

        public Shoot2(Vector2 origin, Vector2 direction, float speed)
        {
            Origin = origin;
            Direction = direction;
            Speed = speed;
        }

        public Shoot2(Vector2 origin, Vector2 velocity)
        {
            Origin = origin;
            Direction = Vector2.Normalize(velocity);
            Speed = velocity.Length();
        }

        public void Deserialize(Deserializer deserializer)
        {
            Origin.X = deserializer.DeserializeFloat();
            Origin.Y = deserializer.DeserializeFloat();
            Direction = deserializer.DeserializeDirection();
            Speed = deserializer.DeserializeFloat();
        }

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(Origin.X);
            serializer.Serialize(Origin.Y);
            serializer.SerializeDirection(Direction);
            serializer.Serialize(Speed);
        }
    }
}
