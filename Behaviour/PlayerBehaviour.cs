using ConsoleGame.Net;

namespace ConsoleGame
{
    public class PlayerBehaviour : NetworkComponent
    {
        public PlayerBehaviour(Entity entity) : base(entity)
        {
     
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Synchronize(NetworkMode mode, Connection socket)
        {
            base.Synchronize(mode, socket);
        }

        public override void OnRpc(MessageRpc message)
        {
            base.OnRpc(message);
        }

        public override void OnMessageReceived(ObjectMessage message)
        {
            base.OnMessageReceived(message);
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}
