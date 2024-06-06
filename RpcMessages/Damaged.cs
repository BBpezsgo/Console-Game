namespace ConsoleGame.RpcMessages;

public struct Damaged : ISerializable
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

    public readonly void Serialize(BinaryWriter writer)
    {
        writer.Write(Amount);
        writer.Write(ByNetworkId);
    }

    public void Deserialize(BinaryReader reader)
    {
        Amount = reader.ReadSingle();
        ByNetworkId = reader.ReadInt32();
    }
}
