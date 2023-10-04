using DataUtilities.Serializer;

namespace ConsoleGame
{
    public interface IDamageable
    {
        public void Damage(float amount, Component? by);
    }

    public struct RpcData_Damage : ISerializable<RpcData_Damage>
    {
        public float Amount;
        public int ByNetworkId;
        public readonly NetworkEntityComponent? By
        {
            get
            {
                if (Game.Instance.Scene == null)
                { return null; }
                if (!Game.Instance.Scene.TryGetNetworkEntity(ByNetworkId, out NetworkEntityComponent? @object))
                { return null; }
                return @object;
            }
        }

        public RpcData_Damage()
        {
            Amount = 0f;
            ByNetworkId = 0;
        }

        public RpcData_Damage(float amount)
        {
            Amount = amount;
            ByNetworkId = -1;
        }

        public RpcData_Damage(float amount, int by)
        {
            Amount = amount;
            ByNetworkId = by;
        }

        public void Deserialize(Deserializer deserializer)
        {
            Amount = deserializer.DeserializeFloat();
            ByNetworkId = deserializer.DeserializeInt32();
        }

        public readonly void Serialize(Serializer serializer)
        {
            serializer.Serialize(Amount);
            serializer.Serialize(ByNetworkId);
        }
    }
}
