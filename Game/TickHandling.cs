using System.Diagnostics.CodeAnalysis;
using ConsoleGame.Net;
using Win32;

namespace ConsoleGame
{
    public partial class Game
    {
        float ah = .5f;
        float LastEnemySpawn = Time.UtcNow;
        int EnemyWave = 5;

        void Tick()
        {
            Keyboard.BeginTick();

            renderer.Clear();
            depthBuffer.Clear();
            if (renderer.Resize())
            { depthBuffer.Resize(); }

            TickWrapped();

            renderer.Render();
        }

        void TickWrapped()
        {
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

            int stateBarX = 1;

            stateBarX += renderer.DrawLabel(
                stateBarX, 0,
                $"FPS: {(int)MathF.Round(1f / deltaTime),4}",
                ByteColor.Black, ByteColor.Gray);

            stateBarX += renderer.DrawLabel(stateBarX, 0, $" | ", ByteColor.Black, ByteColor.Gray);

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

                bool hasPlayer = false;
                Entity[] players = Scene.ObjectsOfTag(Tags.Player);
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].GetComponent<NetworkEntityComponent>().IsOwned)
                    {
                        hasPlayer = true;
                        break;
                    }
                }

                if (!hasPlayer && (connection == null || connection.IsDone))
                {
                    RectInt box = renderer.MakeMenu(30, 8);
                    renderer.DrawBox(box, ByteColor.Black, ByteColor.White, Ascii.BoxSides);
                    box.Expand(-1);
                    VectorInt labelPos = Layout.MakeCenteredLabel(box, "YOU DIED");
                    labelPos.Y = box.Y + 1;
                    renderer.DrawLabel(labelPos.X, labelPos.Y, "YOU DIED", ByteColor.Black, ByteColor.BrightRed);

                    box.Top += 3;

                    Menu_YouDied.Tick(box);
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

                if (shouldSync && connection != null)
                { connection.Flush(); }
                connection?.Receive();
            }
            else
            {
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
                    RectInt menu = renderer.MakeMenu(30, 3);
                    renderer.DrawBox(menu, ByteColor.Black, ByteColor.White, Ascii.BoxSides);
                    menu.Expand(-1);

                    string text = connection.StatusText;

                    VectorInt labelPos = Layout.MakeCenteredLabel(menu, text);

                    renderer.DrawLabel(labelPos.X, labelPos.Y, text, ByteColor.Black, ByteColor.White);
                }

                string statusText = connection.StatusText;
                stateBarX += renderer.DrawLabel(stateBarX, 0, statusText, ByteColor.Black, ByteColor.Gray);

                stateBarX += renderer.DrawLabel(stateBarX, 0, $" | ", ByteColor.Black, ByteColor.Gray);

                string modeText = networkMode switch
                {
                    NetworkMode.Offline => $"Offline",
                    NetworkMode.Server => $"Hosting on {connection?.LocalEndPoint.Simplify()}",
                    NetworkMode.Client => $"Client on {connection?.ServerEndPoint.Simplify()}",
                    _ => $"Unknown",
                };
                stateBarX += renderer.DrawLabel(stateBarX, 0, modeText, ByteColor.Black, ByteColor.Gray);
            }

            if (Keyboard.IsKeyPressed(VirtualKeyCodes.TAB))
            { DrawClientListMenu(); }
        }

        void DrawClientListMenu()
        {
            if (connection == null) return;

            RectInt menuBox = renderer.MakeMenu(40, 10);

            renderer.DrawBox(menuBox, ByteColor.Black, ByteColor.White);

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
                renderer.DrawLabel(menuBox.X, menuBox.Y, $"{server} (server)", ByteColor.Black, ByteColor.White);

                for (int i = 0; i < clientCount; i++)
                {
                    Socket client = Clients[i];
                    if (client == connection.LocalEndPoint)
                    {
                        renderer.DrawLabel(menuBox.X, menuBox.Y + 1 + i, $"{client} (you)", ByteColor.Black, ByteColor.White);
                    }
                    else
                    {
                        renderer.DrawLabel(menuBox.X + 1, menuBox.Y + 1 + i, $"{client}", ByteColor.Black, ByteColor.White);
                    }
                }
            }
            else if (networkMode == NetworkMode.Server)
            {
                renderer.DrawLabel(menuBox.X, menuBox.Y, $"{connection.LocalEndPoint} (you) (server)", ByteColor.Black, ByteColor.White);

                int clientCount = connection.Clients.Length;
                for (int i = 0; i < clientCount; i++)
                {
                    Socket client = connection.Clients[i];
                    renderer.DrawLabel(menuBox.X, menuBox.Y + 1 + i, $"{client}", ByteColor.Black, ByteColor.White);
                }
            }
        }

        public bool TrySpawnEnemy([NotNullWhen(true)] out Entity? enemy)
        {
            enemy = null;

            if (Scene == null) return false;

            Vector randomPoint = Random.Point(Scene.Size);
            if (Scene.FirstObjectAt(randomPoint, Tags.Player, 13f) == null)
            {
                enemy = EntityPrototypes.Builders[GameObjectPrototype.ENEMY](Scene.GenerateNetworkId(), LocalOwner);
                enemy.Position = randomPoint;
                Scene.AddEntity(enemy);
                return true;
            }

            return false;
        }
    }
}
