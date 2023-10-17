﻿using System.Diagnostics.CodeAnalysis;
using ConsoleGame.Behavior;
using ConsoleGame.Net;
using Win32;

namespace ConsoleGame
{
    public partial class Game
    {
        float ah = .5f;

        float LastEnemySpawn = Time.UtcNow;
        float LastItemSpawn = Time.UtcNow;

        int EnemyWave = 5;

        readonly Mesh MeshCube = new()
        {
            Triangles = new List<Triangle>()
            {
                new Triangle((0, 0, 0), (0, 1, 0), (1, 1, 0)),
                new Triangle((0, 0, 0), (1, 1, 0), (1, 0, 0)),

                new Triangle((1, 0, 0), (1, 1, 0), (1, 1, 1)),
                new Triangle((1, 0, 0), (1, 1, 1), (1, 0, 1)),

                new Triangle((1, 0, 1), (1, 1, 1), (0, 1, 1)),
                new Triangle((1, 0, 1), (0, 1, 1), (0, 0, 1)),

                new Triangle((0, 0, 1), (0, 1, 1), (0, 1, 0)),
                new Triangle((0, 0, 1), (0, 1, 0), (0, 0, 0)),

                new Triangle((0, 1, 0), (0, 1, 1), (1, 1, 1)),
                new Triangle((0, 1, 0), (1, 1, 1), (1, 1, 0)),

                new Triangle((1, 0, 1), (0, 0, 1), (0, 0, 0)),
                new Triangle((1, 0, 1), (0, 0, 0), (1, 0, 0)),
            },
        };
        readonly Mesh MeshSpaceship = Obj.LoadFile($"C:\\users\\{Environment.UserName}\\Desktop\\VideoShip.obj");
        readonly Mesh MeshTeapot = Obj.LoadFile($"C:\\users\\{Environment.UserName}\\Desktop\\teapot.obj");
        readonly Mesh MeshAxis = Obj.LoadFile($"C:\\users\\{Environment.UserName}\\Desktop\\axis.obj");
        readonly Mesh MeshMountains = Obj.LoadFile($"C:\\users\\{Environment.UserName}\\Desktop\\mountains.obj");

        const float fNear = 0.1f;
        const float fFar = 1000.0f;
        const float fFov = 90.0f;
        readonly float fFovRad = 1.0f / MathF.Tan(fFov * 0.5f / 180f * MathF.PI);

        Vector3 CameraPosition;
        Vector3 CameraLookDirection = new(0f, 0f, 1f);

        readonly Matrix4x4 projectionMatrix = Matrix4x4.Zero;
        readonly Matrix4x4 matRotZ = Matrix4x4.Zero;
        readonly Matrix4x4 matRotX = Matrix4x4.Zero;

        void Tick()
        {
            Keyboard.BeginTick();

            renderer.ClearBuffer();
            depthBuffer.Clear();
            if (renderer.Resize())
            { depthBuffer.Resize(); }

            FpsCounter.Sample((int)MathF.Round(1f / deltaTime));

            // TickWrapped();

            const float CameraSpeed = 8f;

            if (Keyboard.IsKeyPressed('W'))
            {
                CameraPosition.Z += Time.DeltaTime * CameraSpeed;
            }
            if (Keyboard.IsKeyPressed('A'))
            {
                CameraPosition.X -= Time.DeltaTime * CameraSpeed;
            }
            if (Keyboard.IsKeyPressed('S'))
            {
                CameraPosition.Z -= Time.DeltaTime * CameraSpeed;
            }
            if (Keyboard.IsKeyPressed('D'))
            {
                CameraPosition.X += Time.DeltaTime * CameraSpeed;
            }
            if (Keyboard.IsKeyPressed(VirtualKeyCodes.SHIFT))
            {
                CameraPosition.Y -= Time.DeltaTime * CameraSpeed;
            }
            if (Keyboard.IsKeyPressed(VirtualKeyCodes.SPACE))
            {
                CameraPosition.Y += Time.DeltaTime * CameraSpeed;
            }

            float aspectRatio = (float)renderer.Height / (float)renderer.Width;

            Matrix4x4.MakeProjection(projectionMatrix, aspectRatio, fFovRad, fFar, fNear);

            float theta = 0f;

            Matrix4x4.MakeRotationZ(matRotZ, theta);
            Matrix4x4.MakeRotationX(matRotX, theta * 0.5f);

            Matrix4x4 worldMatrix = Matrix4x4.MakeRotationZ(theta * 0.5f) * Matrix4x4.MakeRotationX(theta);
            worldMatrix *= Matrix4x4.MakeTransition(0f, 0f, 8f);

            Vector3 up = new(0f, 1f, 0f);
            Vector3 target = CameraPosition + CameraLookDirection;

            Matrix4x4 cameraMatrix = Matrix4x4.MakePointAt(CameraPosition, target, up);

            Matrix4x4 viewMatrix = Matrix4x4.QuickInverse(cameraMatrix);

            VectorInt screenSize = new(renderer.Width, renderer.Height);

            List<(Triangle, Color)> trianglesToDraw = new();

            for (int i = 0; i < MeshAxis.Triangles.Count; i++)
            {
                Triangle tri = MeshAxis.Triangles[i];

                tri.A *= worldMatrix;
                tri.B *= worldMatrix;
                tri.C *= worldMatrix;

                tri.A -= CameraPosition;
                tri.B -= CameraPosition;
                tri.C -= CameraPosition;

                Vector3 normal, line1, line2;

                line1 = tri.B - tri.A;
                line2 = tri.C - tri.A;

                normal = Vector3.Cross(line1, line2);

                normal.Normalize();

                Vector3 cameraRay = tri.A - CameraPosition;

                if (Vector3.Dot(normal, cameraRay) >= float.Epsilon) continue;

                Vector3 sunDirection = (0f, 0f, -1f);
                Color color = Color.White;

                float dp = Math.Max(0.1f, Vector3.Dot(sunDirection, normal));
                color *= Math.Clamp(dp, 0f, 1f);

                tri.A *= viewMatrix;
                tri.B *= viewMatrix;
                tri.C *= viewMatrix;

                tri.A *= projectionMatrix;
                tri.B *= projectionMatrix;
                tri.C *= projectionMatrix;

                tri.A += Vector3.One;
                tri.B += Vector3.One;
                tri.C += Vector3.One;

                tri.A *= 0.5f;
                tri.B *= 0.5f;
                tri.C *= 0.5f;

                trianglesToDraw.Add((tri, color));
            }

            trianglesToDraw.Sort(new Comparison<(Triangle, Color)>((a, b) =>
            {
                float midA = (a.Item1.A.Z + a.Item1.B.Z + a.Item1.C.Z) / 3;
                float midB = (b.Item1.A.Z + b.Item1.B.Z + b.Item1.C.Z) / 3;
                return -midA.CompareTo(midB);
            }));

            for (int i = 0; i < trianglesToDraw.Count; i++)
            {
                (Triangle tri, Color color) = trianglesToDraw[i];

                renderer.FillTriangle(
                    ((Vector)tri.A * screenSize).Round(),
                    ((Vector)tri.B * screenSize).Round(),
                    ((Vector)tri.C * screenSize).Round(),
                    Color.ToCharacterShaded(color));
                /*
                renderer.DrawLines(new VectorInt[]
                {
                    ((Vector)tri.A * screenSize).Round(),
                    ((Vector)tri.B * screenSize).Round(),
                    ((Vector)tri.C * screenSize).Round(),
                }, ByteColor.White << 4, ' ', true);
                */
            }

            renderer.Render();
        }

        void TickWrapped()
        {
            EntityHoverPopup.ShouldNotShow = false;
            EntityHoverPopup.AlreadyShown = null;

            for (int i = Timers.Count - 1; i >= 0; i--)
            {
                if (Timers[i].Elapsed >= Timers[i].Duration)
                {
                    Timers[i].Do();
                    Timers.RemoveAt(i);
                }
            }

            {
                QuadTree<Entity?>[] branches = Scene.QuadTree.Branches(Mouse.WorldPosition);

                for (int i = 0; i < branches.Length; i++)
                {
                    QuadTree<Entity?> branch = branches[i];
                    RectInt conRect = Game.WorldToConsole(branch.Rect).Expand(0);
                    conRect.Position += VectorInt.One;
                    GUI.Box(conRect, ByteColor.White, Ascii.BoxSides);

                    for (int j = 0; j < branch.Container.Count; j++)
                    {
                        Entity? entity = branch.Container[j].Item2;
                        if (entity == null) continue;

                        VectorInt conPos = Game.WorldToConsole(entity.Position);
                        if (!renderer.IsVisible(conPos)) continue;
                        renderer[conPos].Background = ByteColor.Magenta;
                    }
                }
            }

            /*
            BitUtils.RenderBits(renderer, new VectorInt(0, 0), Keyboard.Accumulated);
            BitUtils.RenderBits(renderer, new VectorInt(0, 7), Keyboard.Stage1);
            BitUtils.RenderBits(renderer, new VectorInt(0, 14), Keyboard.Stage2);
            BitUtils.RenderBits(renderer, new VectorInt(0, 21), Keyboard.Stage3);
            Thread.Sleep(100);
            return;
            */

            /*
            for (ushort i = 0; i < 255; i++)
            {
                bool isPressed = Keyboard.IsKeyPressed(i);
                bool isDown = Keyboard.IsKeyDown(i);
                bool isHold = Keyboard.IsKeyHold(i);
                bool isUp = Keyboard.IsKeyUp(i);

                byte bg = ByteColor.Black;

                if (isDown)
                {
                    bg = ByteColor.Red;
                }

                if (isHold)
                {
                    bg = ByteColor.Green;
                }

                if (isUp)
                {
                    bg = ByteColor.Blue;
                }

                if (isPressed)
                {
                    renderer.DrawLabel(0, 0, i.ToString(), ByteColor.Black, ByteColor.White);
                }

                renderer[i].Background = bg;
            }

            Thread.Sleep(100);
            return;
            */

            for (int x = 0; x < renderer.Width; x++)
            {
                if (3 >= renderer.Height) break;
                ref CharInfo pixel = ref renderer[x, 3];
                pixel.Attributes = ByteColor.Silver;
                pixel.Char = Ascii.BoxSides[0b_0010];
            }

            int stateBarX = 1;

            stateBarX += GUI.Label(
                stateBarX, 0,
                $"FPS: {FpsCounter.Value,4}",
                ByteColor.Black, ByteColor.Gray);

            stateBarX += GUI.Label(stateBarX, 0, $" | ", ByteColor.Black, ByteColor.Gray);

            /*
            int width = renderer.Width;
            int height = renderer.Height;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    renderer[x, y].Background = (byte)Color.FromHSL((float)x / width, (1f - ((float)y / height)), ah / 255f);
                }
            }

            if (Keyboard.IsKeyDown(VirtualKeyCodes.ADD))
            {
                ah += .1f;
            }
            if (Keyboard.IsKeyDown(VirtualKeyCodes.SUBTRACT))
            {
                ah -= .1f;
            }
            ah = Math.Clamp(ah, 0, 255);
            */

            if (Keyboard.IsKeyPressed(VirtualKeyCodes.TAB))
            { EntityHoverPopup.ShouldNotShow = true; }

            if (Scene != null)
            {
                bool shouldSync = false;
                synchronizeCooldown -= deltaTime;
                if (synchronizeCooldown <= 0f)
                {
                    synchronizeCooldown = NetworkTickRate;
                    shouldSync = true;
                }

                Scene.Update(shouldSync);

                Entity[] players = Scene.ObjectsOfTag(Tags.Player);
                Entity? player = null;
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].IsDestroyed) continue;
                    if (players[i].GetComponent<NetworkEntityComponent>().IsOwned)
                    {
                        player = players[i];
                        break;
                    }
                }

                if (player == null && (connection == null || connection.IsDone))
                {
                    RectInt box = GUI.GetCenteredBox(30, 8);
                    GUI.Box(box, ByteColor.Black, ByteColor.White, Ascii.BoxSides);
                    box.Expand(-1);
                    VectorInt labelPos = Layout.MakeCenteredLabel(box, "YOU DIED");
                    labelPos.Y = box.Y + 1;
                    GUI.Label(labelPos, "YOU DIED", ByteColor.Black, ByteColor.BrightRed);

                    box.Top += 3;

                    Menu_YouDied.Tick(box);
                }

                if (player != null)
                {
                    ViewportWorldPosition.X = Math.Clamp(ViewportWorldPosition.X,
                        (int)player.Position.X - (renderer.Width / 4) - (renderer.Width / 8),
                        (int)player.Position.X - (renderer.Width / 4) + (renderer.Width / 8));

                    ViewportWorldPosition.Y = Math.Clamp(ViewportWorldPosition.Y,
                        (int)player.Position.Y - (renderer.Height / 2) - (renderer.Height / 4),
                        (int)player.Position.Y - (renderer.Height / 2) + (renderer.Height / 4));

                    // Vector optimalViewportPosition = player.Position - new Vector(renderer.Width / 4f, renderer.Height / 2f);
                    // ViewportWorldPosition += (optimalViewportPosition - ViewportWorldPosition) * Math.Clamp(Time.DeltaTime, 0f, 1f);

                    PlayerBehavior playerBehavior = player.GetComponent<PlayerBehavior>();

                    renderer[1, 1].Attributes = ByteColor.Silver;
                    renderer[1, 1].Char = '♥';
                    renderer[2, 1].Attributes = ByteColor.Silver;
                    renderer[2, 1].Char = ':';

                    const int HealthBarWidth = 10;

                    float health = (playerBehavior.Health / PlayerBehavior.MaxHealth) * HealthBarWidth;

                    for (int x = 0; x < HealthBarWidth; x++)
                    {
                        ref CharInfo pixel = ref renderer[x + 4, 1];
                        pixel.Background = ByteColor.Gray;
                        pixel.Foreground = ByteColor.BrightRed;
                        if (health > x)
                        {
                            pixel.Char = Ascii.Blocks.Full;
                        }
                        else if (health <= x)
                        {
                            pixel.Char = ' ';
                        }
                    }

                    renderer[15, 1].Attributes = ByteColor.Gray;
                    renderer[15, 1].Char = '|';

                    renderer[17, 1].Attributes = ByteColor.Silver;
                    renderer[17, 1].Char = 'C';

                    renderer[18, 1].Attributes = ByteColor.Silver;
                    renderer[18, 1].Char = ':';

                    GUI.Label(20, 1, PlayerData.Coins.ToString(), ByteColor.BrightYellow);

                }

                if (players.Length > 0 &&
                    networkMode != NetworkMode.Client &&
                    Time.UtcNow - LastEnemySpawn > 10f)
                {
                    for (int i = 0; i < EnemyWave; i++)
                    {
                        for (int @try = 0; @try < 5; @try++)
                        {
                            if (TrySpawnEnemy(out _)) break;
                        }
                    }
                    EnemyWave++;
                    LastEnemySpawn = Time.UtcNow;
                }

                if (networkMode != NetworkMode.Client &&
                    Time.UtcNow - LastItemSpawn > 11f)
                {
                    for (int i = 0; i < 2; i++)
                    { TrySpawnItem(ItemBehavior.ItemKind.Health, 5f, out _); }
                    LastItemSpawn = Time.UtcNow;
                }

                if (shouldSync && connection != null)
                { connection.Flush(); }
                connection?.Receive();
            }
            else
            {
                EntityHoverPopup.ShouldNotShow = true;

                switch (CurrentMenu)
                {
                    case 1:
                        {
                            MainMenu.Tick(40);
                            break;
                        }

                    case 2:
                        {
                            InputBox_ConnectAddress.Tick(30);
                            break;
                        }

                    case 3:
                        {
                            InputBox_HostAddress.Tick(30);
                            break;
                        }

                    default: break;
                }
            }

            if (connection != null)
            {
                if (!connection.IsDone)
                {
                    RectInt menu = GUI.GetCenteredBox(30, 3);
                    GUI.Box(menu, ByteColor.Black, ByteColor.White, Ascii.BoxSides);
                    menu.Expand(-1);

                    string text = connection.StatusText;

                    VectorInt labelPos = Layout.MakeCenteredLabel(menu, text);

                    GUI.Label(labelPos, text, ByteColor.Black, ByteColor.White);
                }

                string statusText = connection.StatusText;
                stateBarX += GUI.Label(stateBarX, 0, statusText, ByteColor.Black, ByteColor.Gray);

                stateBarX += GUI.Label(stateBarX, 0, $" | ", ByteColor.Black, ByteColor.Gray);

                string modeText = networkMode switch
                {
                    NetworkMode.Offline => $"Offline",
                    NetworkMode.Server => $"Hosting on {connection?.LocalEndPoint.Simplify()}",
                    NetworkMode.Client => $"Client on {connection?.ServerEndPoint.Simplify()}",
                    _ => $"Unknown",
                };
                stateBarX += GUI.Label(stateBarX, 0, modeText, ByteColor.Black, ByteColor.Gray);
            }

            if (Keyboard.IsKeyPressed(VirtualKeyCodes.TAB))
            { DrawClientListMenu(); }
        }

        void DrawClientListMenu()
        {
            if (connection == null) return;

            RectInt menuBox = GUI.GetCenteredBox(40, 10);

            GUI.Box(menuBox, ByteColor.Black, ByteColor.White);

            menuBox.Expand(-1);

            if (networkMode == NetworkMode.Client)
            {
                if (Requests.Request(new Request(RequestKinds.CLIENT_LIST, 1)))
                {
                    connection.Send(new ClientListRequestMessage()
                    {
                        Type = MessageType.CLIENT_LIST_REQUEST,
                    });
                }

                int clientCount = Clients.Length;
                Socket server = connection.ServerEndPoint;
                GUI.Label(menuBox.X, menuBox.Y, $"{server} (server)", ByteColor.Black, ByteColor.White);

                for (int i = 0; i < clientCount; i++)
                {
                    Socket client = Clients[i];
                    if (client == connection.LocalEndPoint)
                    {
                        GUI.Label(menuBox.X, menuBox.Y + 1 + i, $"{client} (you)", ByteColor.Black, ByteColor.White);
                    }
                    else
                    {
                        GUI.Label(menuBox.X + 1, menuBox.Y + 1 + i, $"{client}", ByteColor.Black, ByteColor.White);
                    }
                }
            }
            else if (networkMode == NetworkMode.Server)
            {
                GUI.Label(menuBox.X, menuBox.Y, $"{connection.LocalEndPoint} (you) (server)", ByteColor.Black, ByteColor.White);

                int clientCount = connection.Clients.Length;
                for (int i = 0; i < clientCount; i++)
                {
                    Socket client = connection.Clients[i];
                    GUI.Label(menuBox.X, menuBox.Y + 1 + i, $"{client}", ByteColor.Black, ByteColor.White);
                }
            }
        }

        public bool TrySpawnEnemy([NotNullWhen(true)] out Entity? enemy)
        {
            enemy = null;

            if (Scene == null) return false;

            Vector randomPoint = Random.Point(Scene.SizeR);
            if (Scene.FirstObjectAt(randomPoint, Tags.Player, 13f) == null)
            {
                enemy = EntityPrototypes.Builders[GameObjectPrototype.ENEMY](Scene.GenerateNetworkId(), LocalOwner);
                enemy.Position = randomPoint;
                Scene.AddEntity(enemy);
                return true;
            }

            return false;
        }

        public bool TrySpawnItem(ItemBehavior.ItemKind kind, float amount, [NotNullWhen(true)] out ItemBehavior? item)
        {
            item = null;
            if (Scene == null) return false;
            Entity entity = new("Item");
            entity.AddComponent(new RendererComponent(entity)
            {
                Character = 'P',
                Color = ByteColor.BrightGreen,
            });
            entity.AddComponent(item = new ItemBehavior(entity)
            {
                Kind = kind,
                Amount = amount,
            });
            entity.Position = Random.Point(Scene.SizeR);
            Scene.AddEntity(entity);
            return true;
        }

        public CoinItemBehavior SpawnCoin(Vector position, int amount)
        {
            CoinItemBehavior item;

            Entity entity = new("Coin Item");
            entity.AddComponent(item = new CoinItemBehavior(entity)
            {
                Amount = amount,
            });
            entity.AddComponent(new CoinItemRendererComponent(entity)
            {
                Character = Ascii.CircleNumbersOutline[0],
                Color = ByteColor.BrightYellow,
            });

            entity.Position = position;
            Scene.AddEntity(entity);
            return item;
        }
    }
}
