using ConsoleGame.Net;

namespace ConsoleGame
{
    public class NetworkEntityComponent : Component
    {
        public int NetworkId;
        public int ObjectId;
        public ObjectOwner Owner;

        public bool IsOwned => Game.NetworkMode == NetworkMode.Offline || Owner == new ObjectOwner(Game.Connection?.LocalEndPoint ?? throw new NullReferenceException());

        public NetworkEntityComponent(Entity entity) : base(entity)
        {
            Game.Instance.Scene.NetworkEntityComponents.Register(this);
        }

        public override void Destroy()
        {
            base.Destroy();

            if (Game.NetworkMode != NetworkMode.Client)
            {
                Game.Connection?.Send(new ObjectDestroyMessage()
                {
                    Type = MessageType.OBJ_DESTROY,
                    NetworkId = NetworkId,
                });
            }

            Game.Instance.Scene.NetworkEntityComponents.Deregister(this);
        }

        public void HandleMessage(ObjectMessage message)
        {

        }

        public void HandleMessage(ComponentMessage message)
        {
            Component component = Entity.Components[message.ComponentIndex];
            if (component is not NetworkComponent networkComponent) return;
            networkComponent.OnMessage(message);
        }

        public void HandleRpc(MessageRpc message)
        {
            Component component = Entity.Components[message.ComponentIndex];
            if (component is not NetworkComponent networkComponent) return;
            networkComponent.OnRpc(message);
        }

        public void SynchronizeComponents(NetworkMode networkMode, Connection socket)
        {
            for (int i = 0; i < Entity.Components.Length; i++)
            {
                Component component = Entity.Components[i];
                if (component is not NetworkComponent networkComponent) continue;
                networkComponent.Synchronize(networkMode, socket);
            }
        }
    }
}
