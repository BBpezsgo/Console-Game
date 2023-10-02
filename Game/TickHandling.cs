using ConsoleGame.Net;
using Win32;

namespace ConsoleGame
{
    public partial class Game
    {
        float ah = .5f;

        void Tick()
        {
            renderer.Clear();
            renderer.Resize();

            TickWrapped();

            renderer.Render();
        }

        void TickWrapped()
        {
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

                Scene.Tick(deltaTime, shouldSync);

                if (shouldSync && connection != null)
                { connection.Flush(); }
                connection?.Receive();
            }
            else
            {
                MainMenu.Tick();
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

            if (connection != null && Keyboard.IsKeyPressed(VirtualKeyCodes.TAB))
            {
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

            /*
            for (ushort i = 0; i < 255; i++)
            {
                Keyboard.State key = Keyboard.Keys[i];
                renderer[i].Background = key switch
                {
                    Keyboard.State.None => ByteColor.Black,
                    Keyboard.State.Pressing => ByteColor.Blue,
                    Keyboard.State.Pressed => ByteColor.Red,
                    Keyboard.State.Releasing => ByteColor.Green,
                    _ => ByteColor.Black,
                };
            }
            */
        }
    }
}
