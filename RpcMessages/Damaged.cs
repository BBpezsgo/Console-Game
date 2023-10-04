using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataUtilities.Serializer;

namespace ConsoleGame.RpcMessages
{

    public struct Damaged : ISerializable<Damaged>
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

        public Damaged()
        {
            Amount = 0f;
            ByNetworkId = 0;
        }

        public Damaged(float amount)
        {
            Amount = amount;
            ByNetworkId = -1;
        }

        public Damaged(float amount, int by)
        {
            Amount = amount;
            ByNetworkId = by;
        }

        public Damaged(float amount, Component? by)
        {
            Amount = amount;
            ByNetworkId = -1;
            if (by == null) return;
            if (by.Entity.IsDestroyed) return;
            if (!by.Entity.TryGetComponent(out NetworkEntityComponent? networkEntity)) return;
            ByNetworkId = networkEntity.NetworkId;
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
