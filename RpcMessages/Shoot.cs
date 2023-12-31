﻿using DataUtilities.Serializer;

namespace ConsoleGame.RpcMessages
{
    public struct Shoot2 : ISerializable<Shoot2>
    {
        public Vector Origin;
        public Vector Direction;
        public float Speed;

        public Shoot2()
        {
            Origin = Vector.Zero;
            Direction = Vector.Zero;
            Speed = 0f;
        }

        public Shoot2(Vector origin, Vector direction, float speed)
        {
            Origin = origin;
            Direction = direction;
            Speed = speed;
        }

        public Shoot2(Vector origin, Vector velocity)
        {
            Origin = origin;
            Direction = velocity.Normalized;
            Speed = velocity.Magnitude;
        }

        public void Deserialize(Deserializer deserializer)
        {
            Origin = deserializer.DeserializeObject<Vector>();
            Direction = deserializer.DeserializeObject(Vector.DeserializeAsDirection);
            Speed = deserializer.DeserializeFloat();
        }

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(Origin);
            serializer.Serialize(Direction, Vector.SerializeAsDirection);
            serializer.Serialize(Speed);
        }
    }
}
