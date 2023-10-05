using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Win32;

namespace ConsoleGame
{
    public class Scene
    {
        public readonly List<Entity> Entities;

        public readonly BaseSystem<Component> AllComponents = new(true);
        public readonly BaseSystem<RendererComponent> RendererComponents = new(false);
        public readonly BaseSystem<NetworkEntityComponent> NetworkEntityComponents = new(false);

        public VectorInt Size => new(50, 50);
        public RectInt SizeR => new(0, 0, 50, 50);

        public Scene()
        {
            Entities = new List<Entity>();
        }

        public void AddEntity(ObjectSpawnMessage objectSpawnMessage)
        {
            if (TryGetNetworkEntity(objectSpawnMessage.NetworkId, out NetworkEntityComponent? alreadyCreatedEntity))
            {
                Debug.WriteLine($"Network entity {alreadyCreatedEntity} ({objectSpawnMessage.NetworkId}) already spawned");
                return;
            }

            Entity newEntity = EntityPrototypes.Builders[objectSpawnMessage.ObjectId].Invoke(objectSpawnMessage.NetworkId, (ObjectOwner)objectSpawnMessage.OwnerId);
            newEntity.Position = objectSpawnMessage.Position;
            AddEntity(newEntity);
        }

        public void AddEntity(ObjectDetailsMessage objectDetailsMessage)
        {
            if (TryGetNetworkEntity(objectDetailsMessage.NetworkId, out NetworkEntityComponent? alreadyCreatedEntity))
            {
                Debug.WriteLine($"Unexpected entity details message for already spawned network entity {alreadyCreatedEntity.Entity} ({objectDetailsMessage.NetworkId})");
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
                Rect rect = Game.VisibleWorldRect();
                rect = Rect.Intersection(rect, SizeR);
                for (int _y = 0; _y < rect.Height; _y++)
                {
                    float y = _y + rect.Y;
                    y = MathF.Round(y);

                    for (int _x = 0; _x < rect.Width * 2; _x++)
                    {
                        float x = (_x / 2f) + rect.X;
                        x = MathF.Round(x * 2) / 2;

                        float v = SimplexNoise.Noise.Generate(x, y) * .5f + .5f;
                        if (v < .5f) continue;
                        VectorInt conPos = Game.WorldToConsole(new Vector(x, y));

                        if (!Game.Renderer.IsVisible(conPos) || Game.IsOnGui(conPos)) continue;

                        ref CharInfo pixel = ref Game.Renderer[conPos];

                        pixel.Foreground = v < .75f ? ByteColor.Gray : ByteColor.Silver;
                        pixel.Char = '░';
                    }
                }
            }

            {
                VectorInt a = Game.WorldToConsole(Vector.Zero);
                VectorInt b = Game.WorldToConsole(Size);

                int top = a.Y;
                int left = a.X;
                int bottom = b.Y;
                int right = b.X;

                ConsoleRenderer r = Game.Renderer;

                const byte c = ByteColor.Silver;

                for (int y = top + 1; y <= bottom - 1; y++)
                {
                    VectorInt p1 = new(left, y);
                    VectorInt p2 = new(right, y);

                    if (r.IsVisible(p1) && !Game.IsOnGui(p1))
                    {
                        ref CharInfo pixel = ref r[p1];
                        pixel.Attributes = c;
                        pixel.Char = '|';
                    }

                    if (r.IsVisible(p2) && !Game.IsOnGui(p2))
                    {
                        ref CharInfo pixel = ref r[p2];
                        pixel.Attributes = c;
                        pixel.Char = '|';
                    }
                }

                for (int x = left + 1; x <= right - 1; x++)
                {
                    VectorInt p1 = new(x, top);
                    VectorInt p2 = new(x, bottom);

                    if (r.IsVisible(p1) && !Game.IsOnGui(p1))
                    {
                        ref CharInfo pixel = ref r[p1];
                        pixel.Attributes = c;
                        pixel.Char = '-';
                    }

                    if (r.IsVisible(p2) && !Game.IsOnGui(p2))
                    {
                        ref CharInfo pixel = ref r[p2];
                        pixel.Attributes = c;
                        pixel.Char = '-';
                    }
                }

                VectorInt p3 = new(left, top);
                VectorInt p4 = new(left, bottom);
                VectorInt p5 = new(right, top);
                VectorInt p6 = new(right, bottom);

                if (r.IsVisible(p3) && !Game.IsOnGui(p3))
                {
                    ref CharInfo pixel = ref r[p3];
                    pixel.Attributes = c;
                    pixel.Char = '+';
                }
                if (r.IsVisible(p4) && !Game.IsOnGui(p4))
                {
                    ref CharInfo pixel = ref r[p4];
                    pixel.Attributes = c;
                    pixel.Char = '+';
                }
                if (r.IsVisible(p5) && !Game.IsOnGui(p5))
                {
                    ref CharInfo pixel = ref r[p5];
                    pixel.Attributes = c;
                    pixel.Char = '+';
                }
                if (r.IsVisible(p6) && !Game.IsOnGui(p6))
                {
                    ref CharInfo pixel = ref r[p6];
                    pixel.Attributes = c;
                    pixel.Char = '+';
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

            return;

            for (int i = 0; i < 30; i++)
            {
                Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.ENEMY](GenerateNetworkId(), Game.LocalOwner);
                newEntity.Position = new Vector(2 + i, 20);
                AddEntity(newEntity);
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
