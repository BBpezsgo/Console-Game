using DataUtilities.Serializer;

namespace ConsoleGame
{
    public interface IDamageable
    {
        public void Damage(float amount, GameObject? by);
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

        public RpcData_Damage(float amount, GameObject? by)
        {
            Amount = amount;

            if (by != null && by is NetworkedGameObject networkedBy)
            { ByNetworkId = networkedBy.NetworkId; }
            else
            { ByNetworkId = -1; }
        }

        public RpcData_Damage(float amount, NetworkedGameObject? by)
        {
            Amount = amount;
            ByNetworkId = by?.NetworkId ?? -1;
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
