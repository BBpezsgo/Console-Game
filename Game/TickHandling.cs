using System.Diagnostics.CodeAnalysis;
using ConsoleGame.Behavior;
using ConsoleGame.Net;
using Win32;
using Win32.LowLevel;

namespace ConsoleGame
{
    public partial class Game
    {
        float ah = .5f;

        float LastEnemySpawn = Time.UtcNow;
        float LastItemSpawn = Time.UtcNow;

        int EnemyWave = 5;

        static readonly string AssetsProject = @$"C:\Users\{Environment.UserName}\source\repos\ConsoleGame\Assets\";
        static readonly string AssetsRuntime = @$"{Path.Combine(Directory.GetCurrentDirectory(), "Assets").TrimEnd('\\')}\";

        readonly Mesh MeshCube = Mesh.MakeCube();
        readonly Mesh MeshSpaceship = Obj.LoadFile($"{AssetsRuntime}VideoShip.obj");
        readonly Mesh MeshTeapot = Obj.LoadFile($"{AssetsRuntime}teapot.obj");
        readonly Mesh MeshAxis = Obj.LoadFile($"{AssetsRuntime}axis.obj");
        readonly Mesh MeshMountains = Obj.LoadFile($"{AssetsRuntime}mountains.obj");
        readonly Mesh MeshTerrain = Obj.LoadFile($"{AssetsRuntime}uploads_files_3707747_landscape.obj").Scale(10f);

        readonly Image ImgUv = Ppm.LoadFile($"{AssetsRuntime}bruh.ppm");

        readonly Camera camera = new();

        VectorInt LastMousePosition;

        readonly Window ConsoleWindow = new(Kernel32.GetConsoleWindow());

        public static readonly int width = User32.GetSystemMetrics(SystemMetricsFlags.CXSCREEN);
        public static readonly int height = User32.GetSystemMetrics(SystemMetricsFlags.CYSCREEN);

        bool LockCursor;
        const float MouseIntensity = 0.001f;

        const bool Test3D = true;

        unsafe void Tick()
        {
            Keyboard.Tick();

            renderer.ClearBuffer();
            depthBuffer.Clear();
            if (renderer is ConsoleRenderer consoleRenderer && consoleRenderer.Resize())
            { depthBuffer.Resize(); }

            FpsCounter.Sample((int)MathF.Round(1f / deltaTime));

            if (Test3D)
            {
                if (true)
                {
                    if (Keyboard.IsKeyDown('C'))
                    { LockCursor = !LockCursor; }

                    camera.HandleInput(LockCursor, ref LastMousePosition);
                    camera.DoMath(renderer.Rect, out _, out _);

                    AnsiRenderer ansi = new();

                    ansi.ClearBuffer();

                    Renderer3D.Render(ansi, MeshMountains, camera, null, (v) => v);

                    ansi.Render();

                    /*
                    for (int y = 0; y < renderer.BloomBlur.Height; y++)
                    {
                        for (int x = 0; x < renderer.BloomBlur.Width; x++)
                        {
                            renderer.BloomBlur[x, y] -= Color.White;
                        }
                    }
                    Renderer3D.FastBlur((Color[])renderer.BloomBlur, renderer.BloomBlur.Width, renderer.BloomBlur.Height, 3);
                    for (int y = 0; y < renderer.BloomBlur.Height; y++)
                    {
                        for (int x = 0; x < renderer.BloomBlur.Width; x++)
                        {
                            renderer.BloomBlur[x, y] += Color.FromCharacter(renderer[x, y]);
                            renderer.BloomBlur[x, y].Clamp();
                        }
                    }
                    renderer.BloomBlur.Copy(renderer, Color.ToCharacterShaded);
                    */
                }
                else
                {
                    for (int y = 0; y < renderer.Height; y++)
                    {
                        for (int x = 0; x < renderer.Width; x++)
                        {
                            float vx = (float)x / (float)renderer.Width;
                            float vy = (float)y / (float)renderer.Height;
                            Color c = Color.FromHSL(vx, vy, ah2);
                            renderer[x, y] = Color.ToCharacterColored(c);
                        }
                    }

                    if (Keyboard.IsKeyDown('W'))
                    { ah2 = Math.Clamp(ah2 + .1f, 0f, 1f); }
                    if (Keyboard.IsKeyDown('S'))
                    { ah2 = Math.Clamp(ah2 - .1f, 0f, 1f); }

                    GUI.Label(0, 0, $"FPS: {FpsCounter.Value}");

                    renderer.Render();
                }
            }
            else
            {
                TickWrapped();

                renderer.Render();
            }
        }
        float ah2 = .5f;

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

            if (Scene != null)
            {
                QuadTree<Entity?>[] branches = Scene.QuadTree.Branches(Game.ConsoleToWorld(Mouse.RecordedPosition));

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
                ref ConsoleChar pixel = ref renderer[x, 3];
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

            if (Keyboard.IsKeyPressed(VirtualKeyCode.TAB))
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
                        ref ConsoleChar pixel = ref renderer[x + 4, 1];
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

            if (Keyboard.IsKeyPressed(VirtualKeyCode.TAB))
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
