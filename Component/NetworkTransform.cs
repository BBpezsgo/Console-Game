using ConsoleGame.Net;

namespace ConsoleGame
{
    public class NetworkTransform : NetworkComponent
    {
        TransformComponent Transform;
        Vector NetPosition;

        public NetworkTransform(Entity entity) : base(entity)
        {
            Transform = entity.GetComponentOfType<TransformComponent>();
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        public override void OnRpc(MessageRpc message)
        {
            base.OnRpc(message);
        }

        public override void Synchronize(NetworkMode mode, Connection socket)
        {
            if (mode == NetworkMode.Server)
            {
                socket.Send(new ObjectPositionMessage()
                {
                    Type = MessageType.OBJ_POSITION,
                    Position = Transform.Position,
                    NetworkId = NetworkEntity.NetworkId,
                });
            }
        }

        public override void Update()
        {
            base.Update();

            if (Game.NetworkMode == NetworkMode.Client)
            { Transform.Position += Vector.MoveTowards(Transform.Position, NetPosition, Game.DeltaTime); }
        }

        public override void OnMessageReceived(ObjectMessage message)
        {
            base.OnMessageReceived(message);

            if (message is ObjectPositionMessage positionMessage)
            {
                NetPosition = positionMessage.Position;
            }
        }
    }
}
