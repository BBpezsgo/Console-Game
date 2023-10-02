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
                    AddObject(new Player(objectSpawnMessage.Position, objectSpawnMessage.NetworkId, objectSpawnMessage.ObjectId, (ObjectOwner)objectSpawnMessage.OwnerId));
                    break;
                case GameObjectPrototype.ENEMY:
                    AddObject(new Enemy(objectSpawnMessage.Position, objectSpawnMessage.NetworkId, objectSpawnMessage.ObjectId, (ObjectOwner)objectSpawnMessage.OwnerId));
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
                    AddObject(new Player(objectDetailsMessage.Position, objectDetailsMessage.NetworkId, objectDetailsMessage.ObjectId, (ObjectOwner)objectDetailsMessage.OwnerId));
                    break;
                case GameObjectPrototype.ENEMY:
                    AddObject(new Enemy(objectDetailsMessage.Position, objectDetailsMessage.NetworkId, objectDetailsMessage.ObjectId, (ObjectOwner)objectDetailsMessage.OwnerId));
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
                if (objects[i].IsDestroyed)
                {
                    objects[i].OnDestroy();
                    objects.RemoveAt(i);
                    continue;
                }

                objects[i].Tick();

                if (objects[i].IsDestroyed)
                {
                    objects[i].OnDestroy();
                    objects.RemoveAt(i);
                    continue;
                }

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

        public void Load()
        {
            objects.Clear();

            for (int i = 0; i < 500; i++)
            {
                Game.TrySpawnEnemy(out _);
            }
            return;

            for (int i = 0; i < 30; i++)
            {
                AddObject(new Enemy(new Vector(2 + i, 2), GenerateNetworkId(), GameObjectPrototype.ENEMY, Game.LocalOwner));
            }
        }

        public T[] ObjectsOfType<T>()
        {
            List<T> result = new();
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                if (obj is not T obj2) continue;
                result.Add(obj2);
            }
            return result.ToArray();
        }
        public GameObject[] ObjectsOfTag(int tags)
        {
            List<GameObject> result = new();
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                if (!obj.HasTag(tags)) continue;
                result.Add(obj);
            }
            return result.ToArray();
        }

        public GameObject? FirstObjectAt(Vector position, float distanceThreshold = 1f)
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                Vector diff = obj.Position - position;
                float diffSqrMag = diff.SqrMagnitude;
                if (diffSqrMag < distanceThreshold * distanceThreshold)
                {
                    return obj;
                }
            }
            return null;
        }

        public GameObject? ClosestObject(Vector position, float distanceThreshold)
        {
            GameObject? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                Vector diff = obj.Position - position;
                float sqrDistance = diff.SqrMagnitude;
                if (sqrDistance >= distanceThreshold * distanceThreshold) continue;

                if (sqrDistance < closestSqrDistance)
                {
                    result = obj;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }

        public GameObject? ClosestObject(Vector position)
        {
            GameObject? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                float sqrDistance = (obj.Position - position).SqrMagnitude;

                if (sqrDistance < closestSqrDistance)
                {
                    result = obj;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }

        public T? FirstObjectAt<T>(Vector position, float distanceThreshold = 1f) where T : GameObject
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                if (obj is not T obj2) continue;
                Vector diff = obj.Position - position;
                float diffSqrMag = diff.SqrMagnitude;
                if (diffSqrMag < distanceThreshold * distanceThreshold)
                {
                    return obj2;
                }
            }
            return null;
        }
        public GameObject? FirstObjectAt(Vector position, int tags, float distanceThreshold = 1f)
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                if (!obj.HasTag(tags)) continue;
                Vector diff = obj.Position - position;
                float diffSqrMag = diff.SqrMagnitude;
                if (diffSqrMag < distanceThreshold * distanceThreshold)
                {
                    return obj;
                }
            }
            return null;
        }

        public T? ClosestObject<T>(Vector position, float distanceThreshold) where T : GameObject
        {
            T? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                if (obj is not T obj2) continue;
                Vector diff = obj.Position - position;
                float sqrDistance = diff.SqrMagnitude;
                if (sqrDistance >= distanceThreshold * distanceThreshold) continue;

                if (sqrDistance < closestSqrDistance)
                {
                    result = obj2;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }
        public GameObject? ClosestObject(Vector position, int tags, float distanceThreshold)
        {
            GameObject? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                if (!obj.HasTag(tags)) continue;
                Vector diff = obj.Position - position;
                float sqrDistance = diff.SqrMagnitude;
                if (sqrDistance >= distanceThreshold * distanceThreshold) continue;

                if (sqrDistance < closestSqrDistance)
                {
                    result = obj;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }

        public T? ClosestObject<T>(Vector position) where T : GameObject
        {
            T? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                if (obj is not T obj2) continue;
                float sqrDistance = (obj.Position - position).SqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    result = obj2;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }
        public GameObject? ClosestObject(Vector position, int tags)
        {
            GameObject? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                if (!obj.HasTag(tags)) continue;
                float sqrDistance = (obj.Position - position).SqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    result = obj;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }

        public GameObject[] ObjectsAt(Vector position, float distanceThreshold = 1f)
        {
            List<GameObject> result = new();
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                Vector diff = obj.Position - position;
                float diffSqrMag = diff.SqrMagnitude;
                if (diffSqrMag < distanceThreshold * distanceThreshold)
                {
                    result.Add(obj);
                }
            }
            return result.ToArray();
        }
        public T[] ObjectsAt<T>(Vector position, float distanceThreshold = 1f) where T : GameObject
        {
            List<T> result = new();
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                if (obj is not T obj2) continue;
                Vector diff = obj.Position - position;
                float diffSqrMag = diff.SqrMagnitude;
                if (diffSqrMag < distanceThreshold * distanceThreshold)
                {
                    result.Add(obj2);
                }
            }
            return result.ToArray();
        }
        public GameObject[] ObjectsAt(Vector position, int tags, float distanceThreshold = 1f)
        {
            List<GameObject> result = new();
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                GameObject obj = objects[i];
                if (obj.IsDestroyed) continue;
                if (!obj.HasTag(tags)) continue;
                Vector diff = obj.Position - position;
                float diffSqrMag = diff.SqrMagnitude;
                if (diffSqrMag < distanceThreshold * distanceThreshold)
                {
                    result.Add(obj);
                }
            }
            return result.ToArray();
        }
    }
}
