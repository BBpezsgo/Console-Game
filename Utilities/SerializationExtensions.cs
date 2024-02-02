using System.Numerics;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    public static class SerializationExtensions
    {
        public static Vector2 DeserializeVector2(this Deserializer deserializer)
        {
            float x = deserializer.DeserializeFloat();
            float y = deserializer.DeserializeFloat();
            return new Vector2(x, y);
        }

        public static void Serialize(this Serializer serializer, Vector2 vector)
        {
            serializer.Serialize(vector.X);
            serializer.Serialize(vector.Y);
        }
    }
}
