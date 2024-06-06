using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using ConsoleGame.Behavior;
using ConsoleGame.Net;

namespace ConsoleGame;

public partial class Game
{
    float ah = .5f;

    float LastEnemySpawn = Time.Now;
    float LastItemSpawn = Time.Now;

    int EnemyWave = 5;

    [SupportedOSPlatform("windows")]
    void Tick()
    {
        ConsoleKeyboard.Tick();
        ConsoleMouse.Tick();

        switch (renderer)
        {
            case IBufferedRenderer<ConsoleChar> buffered: buffered.Clear(); break;
            case IBufferedRenderer<AnsiChar> buffered: buffered.Clear(); break;
            case IBufferedRenderer<GdiColor> buffered: buffered.Clear(); break;
            case IBufferedRenderer<ColoredChar> buffered: buffered.Clear(); break;
        }

        depthBuffer.Clear();
        renderer.RefreshBufferSize();
        depthBuffer.Resize();

        FpsCounter.Sample((int)MathF.Round(1f / deltaTime));

        TickWrapped();

        renderer.Render();
    }

    float ah2 = .5f;

    [SupportedOSPlatform("windows")]
    void TickWrapped()
    {
        if (ConsoleKeyboard.IsKeyDown(VirtualKeyCode.Control))
        {
            Alted = !Alted;
        }

        EntityHoverPopup.ShouldNotShow = false;
        EntityHoverPopup.AlreadyShown = null;

        for (int i = Timers.Count - 1; i >= 0; i--)
        {
            if (Timers[i].Elapsed >= Timers[i].Duration)
            {
                Timers[i].Invoke();
                Timers.RemoveAt(i);
            }
        }

        /*
        if (Scene != null)
        {
            QuadTree<Entity?>[] branches = Scene.QuadTree.Branches(Game.ConsoleToWorld(Mouse.RecordedConsolePosition));

            for (int i = 0; i < branches.Length; i++)
            {
                QuadTree<Entity?> branch = branches[i];
                RectInt conRect = Game.WorldToConsole(branch.Rect).Expand(0);
                conRect.Position += Vector2Int.One;
                GUI.Box(conRect, CharColor.White, SideCharacters.BoxSides);

                for (int j = 0; j < branch.Container.Count; j++)
                {
                    Entity? entity = branch.Container[j].Item2;
                    if (entity == null) continue;

                    Vector2Int conPos = Game.WorldToConsole(entity.Position);
                    if (!renderer.IsVisible(conPos)) continue;
                    renderer[conPos].Background = CharColor.Magenta;
                }
            }
        }
        */

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

            byte bg = CharColor.Black;

            if (isDown)
            {
                bg = CharColor.Red;
            }

            if (isHold)
            {
                bg = CharColor.Green;
            }

            if (isUp)
            {
                bg = CharColor.Blue;
            }

            if (isPressed)
            {
                renderer.DrawLabel(0, 0, i.ToString(), CharColor.Black, CharColor.White);
            }

            renderer[i].Background = bg;
        }

        Thread.Sleep(100);
        return;
        */

        for (int x = 0; x < renderer.Width; x++)
        {
            if (3 >= renderer.Height) break;
            renderer.Set(x, 3, new ConsoleChar(Ascii.BoxSides[0b_0010], CharColor.Silver));
        }

        int stateBarX = 1;

        stateBarX += renderer.Text(stateBarX, 0, $"FPS: {FpsCounter.Value,4}", CharColor.Gray);

        stateBarX += renderer.Text(stateBarX, 0, " | ", CharColor.Gray);

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

        if (ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Tab))
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

            ReadOnlySpan<Entity> players = Scene.ObjectsOfTag(Tags.Player);
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
                renderer.Box(box, CharColor.Black, CharColor.White, SideCharacters.BoxSides);
                box.Expand(-1);
                Vector2Int labelPos = Layout.MakeCenteredLabel(box, "YOU DIED");
                labelPos.Y = box.Y + 1;
                renderer.Text(labelPos, "YOU DIED", CharColor.BrightRed);

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

                renderer.Set(1, 1, new ConsoleChar('♥', CharColor.Silver));
                renderer.Set(2, 1, new ConsoleChar(':', CharColor.Silver));

                const int HealthBarWidth = 10;

                float health = playerBehavior.Health / PlayerBehavior.MaxHealth * HealthBarWidth;

                for (int x = 0; x < HealthBarWidth; x++)
                {
                    if (health > x)
                    {
                        renderer.Set(x + 4, 1, new ConsoleChar(Ascii.Blocks.Full, CharColor.BrightRed, CharColor.Gray));
                    }
                    else if (health <= x)
                    {
                        renderer.Set(x + 4, 1, new ConsoleChar(' ', CharColor.BrightRed, CharColor.Gray));
                    }
                }

                renderer.Set(15, 1, new ConsoleChar('|', CharColor.Gray));
                renderer.Set(17, 1, new ConsoleChar('C', CharColor.Silver));
                renderer.Set(18, 1, new ConsoleChar(':', CharColor.Silver));

                renderer.Text(20, 1, PlayerData.Coins.ToString(), CharColor.BrightYellow);
            }

            if (players.Length > 0 &&
                networkMode != NetworkMode.Client &&
                Time.Now - LastEnemySpawn > 10f)
            {
                for (int i = 0; i < EnemyWave; i++)
                {
                    for (int @try = 0; @try < 5; @try++)
                    {
                        if (TrySpawnEnemy(out _)) break;
                    }
                }
                EnemyWave++;
                LastEnemySpawn = Time.Now;
            }

            if (networkMode != NetworkMode.Client &&
                Time.Now - LastItemSpawn > 11f)
            {
                for (int i = 0; i < 2; i++)
                { TrySpawnItem(ItemBehavior.ItemKind.Health, 5f, out _); }
                LastItemSpawn = Time.Now;
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
                renderer.Box(menu, CharColor.Black, CharColor.White, SideCharacters.BoxSides);
                menu.Expand(-1);

                string text = connection.StatusText;

                Vector2Int labelPos = Layout.MakeCenteredLabel(menu, text);

                renderer.Text(labelPos, text, CharColor.White);
            }

            string statusText = connection.StatusText;
            stateBarX += renderer.Text(stateBarX, 0, statusText, CharColor.Gray);

            stateBarX += renderer.Text(stateBarX, 0, " | ", CharColor.Gray);

            string modeText = networkMode switch
            {
                NetworkMode.Offline => "Offline",
                NetworkMode.Server => $"Hosting on {connection?.LocalEndPoint.Simplify()}",
                NetworkMode.Client => $"Client on {connection?.RemoteEndPoint.Simplify()}",
                _ => "Unknown",
            };
            stateBarX += renderer.Text(stateBarX, 0, modeText, CharColor.Gray);
        }

        if (ConsoleKeyboard.IsKeyPressed(VirtualKeyCode.Tab))
        { DrawClientListMenu(); }
    }

    void DrawClientListMenu()
    {
        if (connection == null) return;

        RectInt menuBox = GUI.GetCenteredBox(40, 10);

        renderer.Fill((SmallRect)menuBox, ConsoleChar.Empty);
        renderer.Box(menuBox, CharColor.Black, CharColor.White);

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
            Socket server = connection.RemoteEndPoint;
            renderer.Text(menuBox.X, menuBox.Y, $"{server} (server)", CharColor.White);

            for (int i = 0; i < clientCount; i++)
            {
                Socket client = Clients[i];
                if (client == connection.LocalEndPoint)
                {
                    renderer.Text(menuBox.X, menuBox.Y + 1 + i, $"{client} (you)", CharColor.White);
                }
                else
                {
                    renderer.Text(menuBox.X + 1, menuBox.Y + 1 + i, $"{client}", CharColor.White);
                }
            }
        }
        else if (networkMode == NetworkMode.Server)
        {
            renderer.Text(menuBox.X, menuBox.Y, $"{connection.LocalEndPoint} (you) (server)", CharColor.White);

            int clientCount = connection.Clients.Length;
            for (int i = 0; i < clientCount; i++)
            {
                Socket client = connection.Clients[i];
                renderer.Text(menuBox.X, menuBox.Y + 1 + i, $"{client}", CharColor.White);
            }
        }
    }

    public bool TrySpawnEnemy([NotNullWhen(true)] out Entity? enemy)
    {
        enemy = null;

        if (Scene == null) return false;

        Vector2 randomPoint = Random.Point(Scene.SizeR);
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
        entity.AddComponent(item = new ItemBehavior(entity)
        {
            Kind = kind,
            Amount = amount,
        });
        entity.AddComponent(new ItemRendererComponent3D(entity)
        {
            // Character = 'P',
            Color = CharColor.To24bitColor(CharColor.BrightGreen),
        });
        entity.Position = Random.Point(Scene.SizeR);
        Scene.AddEntity(entity);
        return true;
    }

    public CoinItemBehavior SpawnCoin(Vector2 position, int amount)
    {
        CoinItemBehavior item;

        Entity entity = new("Coin Item");
        entity.AddComponent(item = new CoinItemBehavior(entity)
        {
            Amount = amount,
        });
        entity.AddComponent(new CoinItemRendererComponent3D(entity)
        {
            // Character = Ascii.CircleNumbersOutline[0],
            // Color = CharColor.BrightYellow,
        });

        entity.Position = position;
        Scene.AddEntity(entity);
        return item;
    }
}
