using DataUtilities.Serializer;

namespace ConsoleGame.RpcMessages
{
    public struct Shoot : ISerializable<Shoot>
    {
        public Vector Origin;
        public Vector Direction;

        public Shoot()
        {
            Origin = Vector.Zero;
            Direction = Vector.Zero;
        }

        public Shoot(Vector origin, Vector direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public void Deserialize(Deserializer deserializer)
        {
            Origin = deserializer.DeserializeObject<Vector>();
            Direction = deserializer.DeserializeObject(Vector.DeserializeAsDirection);
        }

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(Origin);
            serializer.Serialize(Direction, Vector.SerializeAsDirection);
        }
    }
}
