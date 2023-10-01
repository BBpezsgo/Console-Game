using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame
{
    public class Scene
    {
        public IReadOnlyList<GameObject> Objects => objects;

        readonly List<GameObject> objects;
        readonly Game Game;

        public Rect Size
        {
            get
            {
                Rect result = new(0f, 0f, (Game.Renderer.Width / 2f) - 1f, (Game.Renderer.Height) - 1f);

                result.Top += 4;

                return result;
            }
        }

        public Scene(Game game)
        {
            objects = new List<GameObject>();
            Game = game;
        }

        public void AddObject(ObjectSpawnMessage objectSpawnMessage)
        {
            if (TryGetNetworkObject(objectSpawnMessage.NetworkId, out _))
            {
                Debug.WriteLine($"Network object {objectSpawnMessage.NetworkId} already spawned");
                return;
            }

            switch (objectSpawnMessage.ObjectId)
            {
                case GameObjectPrototype.PLAYER:
                    AddObject(new Player(objectSpawnMessage.Position, objectSpawnMessage.NetworkId, objectSpawnMessage.ObjectId, (NetworkPlayer)objectSpawnMessage.OwnerId));
                    break;
                default:
                    Debug.WriteLine($"Unknown object prototype id {objectSpawnMessage.ObjectId}");
                    break;
            }
        }

        public void AddObject(ObjectDetailsMessage objectDetailsMessage)
        {
            if (TryGetNetworkObject(objectDetailsMessage.NetworkId, out _))
            {
                Debug.WriteLine($"Unexpected object details message for network object {objectDetailsMessage.NetworkId}");
                return;
            }

            switch (objectDetailsMessage.ObjectId)
            {
                case GameObjectPrototype.PLAYER:
                    AddObject(new Player(objectDetailsMessage.Position, objectDetailsMessage.NetworkId, objectDetailsMessage.ObjectId, (NetworkPlayer)objectDetailsMessage.OwnerId));
                    break;
                default:
                    Debug.WriteLine($"Unknown object prototype id {objectDetailsMessage.ObjectId}");
                    break;
            }
        }

        public void AddObject(GameObject obj)
        {
            objects.Add(obj);

            if (obj is NetworkedGameObject networkedGameObject &&
                Game.NetworkMode == NetworkMode.Server)
            {
                Game.Connection?.Send(new ObjectSpawnMessage(networkedGameObject));
            }
        }

        public void Tick(float deltaTime, bool shouldSync)
        {
            {
                System.Random r = new(0);
                Rect rect = Game.Instance.Scene.Size;
                for (int _y = 0; _y <= rect.Height; _y++)
                {
                    int y = (int)(_y + rect.Y);
                    for (int _x = 0; _x <= (rect.Width * 2); _x++)
                    {
                        int x = (int)(_x + rect.X);

                        int v = r.Next(0, 15);
                        if (v < 10) continue;
                        v -= 10;
                        if (v < 3)
                        { Game.Renderer[x, y].Foreground = 0b_1000; }
                        else
                        { Game.Renderer[x, y].Foreground = 0b_0111; }
                        Game.Renderer[x, y].Char = '░';
                    }
                }
            }

            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i].IsDestroyed) { objects.RemoveAt(i); continue; }

                objects[i].Tick();

                if (objects[i].IsDestroyed) { objects.RemoveAt(i); continue; }

                if (shouldSync &&
                    objects[i] is NetworkedGameObject networkedObject &&
                    Game.Connection != null)
                { networkedObject.Synchronize(Game.NetworkMode, Game.Connection); }

                objects[i].Render();
            }
        }

        public bool TryGetNetworkObject(ObjectMessage message, [NotNullWhen(true)] out NetworkedGameObject? gameObject)
            => TryGetNetworkObject(message.NetworkId, out gameObject);
        public bool TryGetNetworkObject(int networkId, [NotNullWhen(true)] out NetworkedGameObject? gameObject)
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] is NetworkedGameObject @object &&
                    @object.NetworkId == networkId)
                {
                    gameObject = @object;
                    return true;
                }
            }

            gameObject = null;
            return false;
        }

        public int GenerateNetworkId()
        {
            int result = 1;
            while (TryGetNetworkObject(result, out _))
            { result++; }
            return result;
        }
    }
}
