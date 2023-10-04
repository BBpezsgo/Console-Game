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

        public override void Update()
        {
            base.Update();
        }
        
        public void OnMessageReceived(ObjectMessage message) { }
        public void OnRpc(MessageRpc message) { }
    }
}
