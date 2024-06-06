namespace ConsoleGame.RpcMessages;

public struct Shoot2 : ISerializable
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

    public readonly void Serialize(BinaryWriter writer)
    {
        writer.Write(Origin.X);
        writer.Write(Origin.Y);
        writer.WriteDirection(Direction);
        writer.Write(Speed);
    }

    public void Deserialize(BinaryReader reader)
    {
        Origin.X = reader.ReadSingle();
        Origin.Y = reader.ReadSingle();
        Direction = reader.ReadDirection();
        Speed = reader.ReadSingle();
    }
}
