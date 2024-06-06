using ConsoleGame.Net;

namespace ConsoleGame;

public class NetworkTransform : NetworkComponent
{
    const float Speed = 1.5f;
    const float MinTeleportDistance = 0f;

    Vector2 NetPosition;

    public NetworkTransform(Entity entity) : base(entity) { }

    public override void Synchronize(NetworkMode mode, Connection socket)
    {
        if (IsOwned)
        {
            socket.Send(new ObjectPositionMessage()
            {
                Type = MessageType.OBJ_POSITION,
                Position = Position,
                NetworkId = NetworkEntity.NetworkId,
                ComponentIndex = ComponentIndex,
            });
            NetPosition = Position;
        }
    }

    public override void Update()
    {
        base.Update();

        if (IsOwned)
        { return; }

        if ((Position - NetPosition).LengthSquared() > MinTeleportDistance * MinTeleportDistance)
        { Position = NetPosition; }
        else
        { Position += Vector.MoveTowards(Position, NetPosition, Speed * Time.DeltaTime); }
    }

    public override void OnMessage(ObjectMessage message)
    {
        base.OnMessage(message);

        if (message is ObjectPositionMessage positionMessage)
        {
            NetPosition = positionMessage.Position;
        }
    }
}
