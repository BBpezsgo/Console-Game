﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Win32;

namespace ConsoleGame
{
    public partial class Scene
    {
        public readonly List<Entity> Entities;

        public readonly BaseSystem<Component> AllComponents = new(true);
        public readonly BaseSystem<RendererComponent> RendererComponents = new(false);
        public readonly BaseSystem<NetworkEntityComponent> NetworkEntityComponents = new(false);

        public VectorInt Size = new(50, 50);

        bool DrawGround;

        public RectInt SizeR => new(VectorInt.Zero, Size);

        public readonly ConsoleChar[,] BackgroundTexture;

        public readonly QuadTree<Entity?> QuadTree;

        public Scene()
        {
            Entities = new List<Entity>();
            QuadTree = new QuadTree<Entity?>(SizeR);

            BackgroundTexture = new ConsoleChar[Size.X, Size.Y];

            for (int y = 0; y < Size.Y; y++)
            {
                for (int x = 0; x < Size.X; x++)
                {
                    ref ConsoleChar pixel = ref BackgroundTexture[x, y];

                    float v = Noise.Simplex(new Vector(x, y) * 0.25f);
                    if (v < .5f) continue;

                    pixel.Foreground = v < .75f ? ByteColor.Gray : ByteColor.Silver;
                    pixel.Char = '░';
                }
            }

            DrawGround = true;
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

            QuadTreeLocation location = QuadTree.Add(entity, new Rect(entity.Position, Vector.Zero));
            entity.QuadTreeLocation = location;

            if (Game.NetworkMode == NetworkMode.Server && Game.Connection != null)
            {
                if (entity.TryGetComponent(out NetworkEntityComponent? networkEntity))
                { Game.Connection.Send(new ObjectSpawnMessage(networkEntity)); }
            }
        }

        public void Update(bool shouldSync)
        {
            if (DrawGround)
            {
                Rect rect = Game.VisibleWorldRect();
                RectInt rect2 = RectInt.Intersection(rect, SizeR);
                for (int _y = 0; _y < rect2.Height; _y++)
                {
                    float y = _y + rect2.Y;

                    for (int _x = 0; _x < rect2.Width * 2; _x++)
                    {
                        float x = (_x / 2f) + rect2.X;
                        x = MathF.Round(x * 2) / 2;

                        VectorInt conPos = Game.WorldToConsole(x, y);

                        if (!Game.Renderer.IsVisible(conPos) || Game.IsOnGui(conPos)) continue;

                        Game.Renderer[conPos] = BackgroundTexture[_x / 2 + rect2.X, _y + rect2.Y];
                    }
                }
            }

            {
                VectorInt a = Game.WorldToConsole(0f, 0f);
                VectorInt b = Game.WorldToConsole(Size);

                int top = a.Y;
                int left = a.X;
                int bottom = b.Y;
                int right = b.X;

                IRenderer<ConsoleChar> r = Game.Renderer;

                const byte c = ByteColor.Silver;

                for (int y = top + 1; y <= bottom - 1; y++)
                {
                    VectorInt p1 = new(left, y);
                    VectorInt p2 = new(right, y);

                    if (r.IsVisible(p1) && !Game.IsOnGui(p1))
                    {
                        ref ConsoleChar pixel = ref r[p1];
                        pixel.Attributes = c;
                        pixel.Char = '|';
                    }

                    if (r.IsVisible(p2) && !Game.IsOnGui(p2))
                    {
                        ref ConsoleChar pixel = ref r[p2];
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
                        ref ConsoleChar pixel = ref r[p1];
                        pixel.Attributes = c;
                        pixel.Char = '-';
                    }

                    if (r.IsVisible(p2) && !Game.IsOnGui(p2))
                    {
                        ref ConsoleChar pixel = ref r[p2];
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
                    ref ConsoleChar pixel = ref r[p3];
                    pixel.Attributes = c;
                    pixel.Char = '+';
                }
                if (r.IsVisible(p4) && !Game.IsOnGui(p4))
                {
                    ref ConsoleChar pixel = ref r[p4];
                    pixel.Attributes = c;
                    pixel.Char = '+';
                }
                if (r.IsVisible(p5) && !Game.IsOnGui(p5))
                {
                    ref ConsoleChar pixel = ref r[p5];
                    pixel.Attributes = c;
                    pixel.Char = '+';
                }
                if (r.IsVisible(p6) && !Game.IsOnGui(p6))
                {
                    ref ConsoleChar pixel = ref r[p6];
                    pixel.Attributes = c;
                    pixel.Char = '+';
                }
            }

            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                if (Entities[i].IsDestroyed)
                {
                    if (!QuadTree.Remove(Entities[i].QuadTreeLocation, Entities[i]))
                    { Debug.WriteLine($"Could not remove {Entities[i]} from the quadtree"); }
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

            for (int i = 0; i < Entities.Count; i++)
            {
                QuadTree.Relocate(ref Entities[i].QuadTreeLocation, Entities[i], Entities[i].Position);
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
            QuadTree.Clear();

            for (int i = 0; i < 10; i++)
            {
                /*
                for (int @try = 0; @try < 5; @try++)
                {
                    if (Game.Instance.TrySpawnEnemy(out Entity? newEntity))
                    {
                        break;
                    }
                }
                */

                Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.ENEMY](GenerateNetworkId(), Game.LocalOwner);
                newEntity.Position = new Vector(2 + i, 20);
                AddEntity(newEntity);
            }

            Entity newEntity2 = EntityPrototypes.Builders[GameObjectPrototype.ENEMY_FACTORY](GenerateNetworkId(), Game.LocalOwner);
            newEntity2.Position = new Vector(30, 30);
            AddEntity(newEntity2);
        }
    }
}
