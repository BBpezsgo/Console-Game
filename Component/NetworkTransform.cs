using System.Numerics;
using ConsoleGame.Net;

namespace ConsoleGame
{
    public class NetworkTransform : NetworkComponent
    {
        Vector2 NetPosition;
        const float Speed = 1.5f;
        const float MinTeleportDistance = 5f;

        public NetworkTransform(Entity entity) : base(entity)
        {

        }

        public override void Synchronize(NetworkMode mode, Connection socket)
        {
            if (mode == NetworkMode.Server)
            {
                socket.Send(new ObjectPositionMessage()
                {
                    Type = MessageType.OBJ_POSITION,
                    Position = Position,
                    NetworkId = NetworkEntity.NetworkId,
                    ComponentIndex = ComponentIndex,
                });
            }
        }

        public override void Update()
        {
            base.Update();

            if (Game.NetworkMode == NetworkMode.Client)
            {
                if ((Position - NetPosition).LengthSquared() > MinTeleportDistance * MinTeleportDistance)
                { Position = NetPosition; }
                else
                { Position += Vector.MoveTowards(Position, NetPosition, Speed * Time.DeltaTime); }
            }
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
}
