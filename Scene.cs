using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleGame
{
    public class Scene
    {
        public readonly List<Entity> Entities;

        public readonly BaseSystem<Component> AllComponents = new(true);
        public readonly BaseSystem<RendererComponent> RendererComponents = new(false);
        public readonly BaseSystem<NetworkEntityComponent> NetworkEntityComponents = new(false);

        readonly Game Game;

        public Rect Size
        {
            get
            {
                Rect result = new(0f, 0f, (Game.Renderer.Width / 2f) - 1f, Game.Renderer.Height - 1f);

                result.Top += 4;

                return result;
            }
        }

        public Scene(Game game)
        {
            Entities = new List<Entity>();
            Game = game;
        }

        public void AddEntity(ObjectSpawnMessage objectSpawnMessage)
        {
            if (TryGetNetworkEntity(objectSpawnMessage.NetworkId, out _))
            {
                Debug.WriteLine($"Network object {objectSpawnMessage.NetworkId} already spawned");
                return;
            }

            Entity newEntity = EntityPrototypes.Builders[objectSpawnMessage.ObjectId].Invoke(objectSpawnMessage.NetworkId, (ObjectOwner)objectSpawnMessage.OwnerId);
            newEntity.Position = objectSpawnMessage.Position;
            AddEntity(newEntity);
        }

        public void AddEntity(ObjectDetailsMessage objectDetailsMessage)
        {
            if (TryGetNetworkEntity(objectDetailsMessage.NetworkId, out _))
            {
                Debug.WriteLine($"Unexpected object details message for network object {objectDetailsMessage.NetworkId}");
                return;
            }

            Entity newEntity = EntityPrototypes.Builders[objectDetailsMessage.ObjectId].Invoke(objectDetailsMessage.NetworkId, (ObjectOwner)objectDetailsMessage.OwnerId);
            newEntity.Position = objectDetailsMessage.Position;
            AddEntity(newEntity);
        }

        public void AddEntity(Entity entity)
        {
            Entities.Add(entity);

            if (Game.NetworkMode == NetworkMode.Server && Game.Connection != null)
            {
                if (entity.TryGetComponent(out NetworkEntityComponent? networkEntity))
                { Game.Connection.Send(new ObjectSpawnMessage(networkEntity)); }
            }
        }

        public void Update(bool shouldSync)
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
                        Game.Renderer[x, y].Foreground = v < 3 ? ByteColor.Gray : ByteColor.Silver;
                        Game.Renderer[x, y].Char = '░';
                    }
                }
            }

            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                if (Entities[i].IsDestroyed)
                {
                    Entities[i].OnDestroy();
                    Entities.RemoveAt(i);
                    continue;
                }
            }

            for (int i = 0; i < AllComponents.Components.Count; i++)
            {
                AllComponents.Components[i].Update();
            }

            if (shouldSync && Game.Connection != null)
            {
                for (int i = 0; i < NetworkEntityComponents.Components.Count; i++)
                {
                    NetworkEntityComponents.Components[i].SynchronizeComponents(Game.NetworkMode, Game.Connection);
                }
            }

            for (int i = 0; i < RendererComponents.Components.Count; i++)
            {
                RendererComponents.Components[i].Render();
            }
        }

        public bool TryGetNetworkEntity(ObjectMessage message, [NotNullWhen(true)] out NetworkEntityComponent? gameObject)
            => TryGetNetworkEntity(message.NetworkId, out gameObject);
        public bool TryGetNetworkEntity(int networkId, [NotNullWhen(true)] out NetworkEntityComponent? gameObject)
        {
            for (int i = 0; i < NetworkEntityComponents.Components.Count; i++)
            {
                if (NetworkEntityComponents.Components[i].NetworkId == networkId)
                {
                    gameObject = NetworkEntityComponents.Components[i];
                    return true;
                }
            }
            gameObject = null;
            return false;
        }

        public int GenerateNetworkId()
        {
            int result = 1;
            while (TryGetNetworkEntity(result, out _))
            { result++; }
            return result;
        }

        public void Load()
        {
            AllComponents.Components.Clear();
            RendererComponents.Components.Clear();
            NetworkEntityComponents.Components.Clear();
            Entities.Clear();

            for (int i = 0; i < 30; i++)
            {
                Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.ENEMY](GenerateNetworkId(), Game.LocalOwner);
                newEntity.Position = new Vector(2 + i, 20);
                AddEntity(newEntity);
            }
            return;

            for (int i = 0; i < 500; i++)
            {
                Game.TrySpawnEnemy(out _);
            }
            return;

        }


        public Entity[] ObjectsOfTag(int tags)
        {
            List<Entity> result = new();
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                if ((obj.Tags & tags) == 0) continue;
                result.Add(obj);
            }
            return result.ToArray();
        }

        public Entity? FirstObjectAt(Vector position, float distanceThreshold = 1f)
        {
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                Vector diff = Entities[i].Position - position;
                float diffSqrMag = diff.SqrMagnitude;
                if (diffSqrMag < distanceThreshold * distanceThreshold)
                {
                    return obj;
                }
            }
            return null;
        }

        public Entity? ClosestObject(Vector position, float distanceThreshold)
        {
            Entity? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                Vector diff = Entities[i].Position - position;
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

        public Entity? ClosestObject(Vector position)
        {
            Entity? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                float sqrDistance = (Entities[i].Position - position).SqrMagnitude;

                if (sqrDistance < closestSqrDistance)
                {
                    result = obj;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }

        public Entity? FirstObjectAt(Vector position, int tags, float distanceThreshold = 1f)
        {
            float sqrDistanceThreshold = distanceThreshold * distanceThreshold;

            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                if ((obj.Tags & tags) == 0) continue;

                float diffSqrMag = (Entities[i].Position - position).SqrMagnitude;

                if (diffSqrMag < sqrDistanceThreshold)
                { return obj; }
            }
            return null;
        }

        public Entity? ClosestObject(Vector position, int tags, float radius)
        {
            float sqrRadius = radius * radius;

            Entity? result = null;
            float closestSqrDistance = float.PositiveInfinity;

            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                if ((obj.Tags & tags) == 0) continue;

                float sqrDistance = (Entities[i].Position - position).SqrMagnitude;
                if (sqrDistance >= sqrRadius) continue;

                if (sqrDistance < closestSqrDistance)
                {
                    result = obj;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }

        public Entity? ClosestObject(Vector position, int tags)
        {
            Entity? result = null;
            float closestSqrDistance = float.PositiveInfinity;
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                if ((obj.Tags & tags) == 0) continue;

                float sqrDistance = (Entities[i].Position - position).SqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    result = obj;
                    closestSqrDistance = sqrDistance;
                }
            }
            return result;
        }

        public Entity[] ObjectsAt(Vector position, float distanceThreshold = 1f)
        {
            List<Entity> result = new();
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                Vector diff = Entities[i].Position - position;
                float diffSqrMag = diff.SqrMagnitude;
                if (diffSqrMag < distanceThreshold * distanceThreshold)
                {
                    result.Add(obj);
                }
            }
            return result.ToArray();
        }
        public Entity[] ObjectsAt(Vector position, int tags, float distanceThreshold = 1f)
        {
            List<Entity> result = new();
            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity obj = Entities[i];
                if (obj.IsDestroyed) continue;
                if ((obj.Tags & tags) == 0) continue;
                Vector diff = Entities[i].Position - position;
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
