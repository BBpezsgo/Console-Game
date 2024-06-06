namespace ConsoleGame.RpcMessages;

public struct Shoot : ISerializable
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

    public readonly void Serialize(BinaryWriter writer)
    {
        writer.Write(Origin);
        writer.WriteDirection(Direction);
    }

    public void Deserialize(BinaryReader reader)
    {
        Origin = reader.ReadVector2();
        Direction = reader.ReadDirection();
    }
}
