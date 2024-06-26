﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ConsoleGame.Behavior;

namespace ConsoleGame;

public partial class Scene
{
    public readonly List<Entity> Entities;

    public readonly BaseSystem<Component> AllComponents = new(true);
    public readonly BaseSystem<RendererComponent> RendererComponents = new(false);
    public readonly BaseSystem<RendererComponent3D> RendererComponent3Ds = new(false);
    public readonly BaseSystem<PostRendererComponent3D> PostRendererComponent3Ds = new(false);
    public readonly BaseSystem<NetworkEntityComponent> NetworkEntityComponents = new(false);

    public Vector2Int Size = new(50, 50);

    bool DrawGround;

    public RectInt SizeR => new(Vector2Int.Zero, Size);

    public readonly ConsoleChar[,] BackgroundTexture;

    public readonly QuadTree<Entity?> QuadTree;

    public readonly Camera Camera = new();
    Vector2Int LastMousePosition;
    readonly List<TransformedMesh> MeshBuffer = new();

    float Zoom;
    const float MinZoom = .3f;
    const float MaxZoom = 3f;
    float NormalizedZoom => (Zoom - MinZoom) / (MaxZoom - MinZoom);

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

                float v = Noise.Simplex(new Vector2(x, y) * 0.25f);
                if (v < .5f) continue;

                pixel.Foreground = v < .75f ? CharColor.Gray : CharColor.Silver;
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

        entity.QuadTreeLocation = QuadTree.Add(entity, new Maths.RectF(entity.Position, Vector2.Zero));

        if (Game.NetworkMode == NetworkMode.Server && Game.Connection != null)
        {
            if (entity.TryGetComponent(out NetworkEntityComponent? networkEntity))
            { Game.Connection.Send(new ObjectSpawnMessage(networkEntity)); }
        }
    }

    [SupportedOSPlatform("windows")]
    public void Update(bool shouldSync)
    {
        /*
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

                    Vector2Int conPos = Game.WorldToConsole(x, y);

                    if (!Game.Renderer.IsVisible(conPos) || Game.IsOnGui(conPos)) continue;

                    Game.Renderer[conPos] = BackgroundTexture[_x / 2 + rect2.X, _y + rect2.Y];
                }
            }
        }
        */

        /*
        {
            Vector2Int a = Game.WorldToConsole(Vector2Int.Zero);
            Vector2Int b = Game.WorldToConsole(Size);

            int top = a.Y;
            int left = a.X;
            int bottom = b.Y;
            int right = b.X;

            Renderer<ConsoleChar> r = Game.Renderer;

            const byte c = CharColor.Silver;

            for (int y = top + 1; y <= bottom - 1; y++)
            {
                Vector2Int p1 = new(left, y);
                Vector2Int p2 = new(right, y);

                if (r.IsVisible(p1) && !Game.IsOnGui(p1))
                {
                    r[p1] = new ConsoleChar('|', c);
                }

                if (r.IsVisible(p2) && !Game.IsOnGui(p2))
                {
                    r[p2] = new ConsoleChar('|', c);
                }
            }

            for (int x = left + 1; x <= right - 1; x++)
            {
                Vector2Int p1 = new(x, top);
                Vector2Int p2 = new(x, bottom);

                if (r.IsVisible(p1) && !Game.IsOnGui(p1))
                {
                    r[p1] = new ConsoleChar('-', c);
                }

                if (r.IsVisible(p2) && !Game.IsOnGui(p2))
                {
                    r[p2] = new ConsoleChar('-', c);
                }
            }

            Vector2Int p3 = new(left, top);
            Vector2Int p4 = new(left, bottom);
            Vector2Int p5 = new(right, top);
            Vector2Int p6 = new(right, bottom);

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
        */

        Debug.WriteLine(Camera.Bruh);

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

        PlayerBehavior? player = null;
        for (int i = 0; i < AllComponents.Components.Count; i++)
        {
            if (AllComponents.Components[i] is PlayerBehavior _player &&
                _player.IsOwned)
            { player = _player; }
            AllComponents.Components[i].Update();
        }

        if (shouldSync && Game.Connection != null)
        {
            for (int i = 0; i < NetworkEntityComponents.Components.Count; i++)
            {
                NetworkEntityComponents.Components[i].SynchronizeComponents(Game.NetworkMode, Game.Connection);
            }
        }

        if (Game.Instance.HandleInput)
        {
            Vector2Int mouseDelta = (Vector2Int)Mouse.ScreenPosition - LastMousePosition;
            LastMousePosition = Mouse.ScreenPosition;

            const float MouseIntensity = 0.001f;
            Camera.Yaw -= mouseDelta.X * MouseIntensity;
            Camera.Bruh -= mouseDelta.Y * MouseIntensity;

            Vector2Int center = DisplayMetrics.Size / 2;
            Mouse.ScreenPosition = center;
            LastMousePosition = center;

            Zoom += -Math.Sign(ConsoleMouse.ScrollDelta) * .3f;

            if (player is not null)
            {
                Zoom = Math.Clamp(Zoom, MinZoom, MaxZoom);
                float normalizedZoom = NormalizedZoom;
                Camera.Position =
                    new Vector3(player.Position.X + .25f, .6f + ((1f - NormalizedZoom) * .25f), player.Position.Y + .25f) -
                    (Camera.LookDirection * Zoom);
                // Camera.Bruh = (normalizedZoom * .6f) + .3f;
            }
        }

        Camera.DoMath(new Size(Game.Renderer.Width / 2, Game.Renderer.Height));

        MeshBuffer.Clear();

        for (int i = 0; i < RendererComponent3Ds.Components.Count; i++)
        {
            RendererComponent3Ds.Components[i].Render(MeshBuffer);
        }

        // for (int i = 0; i < RendererComponents.Components.Count; i++)
        // {
        //     RendererComponents.Components[i].Render();
        // }

        if (Game.Renderer is IOnlySetterRenderer<GdiColor> ansiRendererTrueColor)
        { Renderer3D.Render(ansiRendererTrueColor, Game.DepthBuffer, CollectionsMarshal.AsSpan(MeshBuffer), Camera, null, static v => (GdiColor)v); }
        else if (Game.Renderer is IOnlySetterRenderer<AnsiChar> ansiRenderer)
        { Renderer3D.Render(ansiRenderer, Game.DepthBuffer, CollectionsMarshal.AsSpan(MeshBuffer), Camera, null, static v => new AnsiChar(' ', 0, Ansi.ToAnsi256((GdiColor)v))); }
        else
        { Renderer3D.Render(Game.Renderer, Game.DepthBuffer, CollectionsMarshal.AsSpan(MeshBuffer), Camera, null, static v => CharColor.ToCharacterShaded((GdiColor)v)); }

        for (int i = 0; i < PostRendererComponent3Ds.Components.Count; i++)
        {
            PostRendererComponent3Ds.Components[i].Render();
        }

        // for (int i = 0; i < RendererComponents.Components.Count; i++)
        // {
        //     RendererComponents.Components[i].Render();
        // }

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
        RendererComponent3Ds.Components.Clear();
        PostRendererComponent3Ds.Components.Clear();
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
            newEntity.Position = new Vector2(2 + i, 20);
            AddEntity(newEntity);
        }

        Entity newEntity2 = EntityPrototypes.Builders[GameObjectPrototype.ENEMY_FACTORY](GenerateNetworkId(), Game.LocalOwner);
        newEntity2.Position = new Vector2(30, 30);
        AddEntity(newEntity2);
    }
}
