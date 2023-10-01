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

                /*
                Layout menu = new(new RectInt((renderer.Width / 2) - 15, (renderer.Height / 2) - 10, 30, 20));

                renderer.DrawBox(menu.TotalSize.X, menu.TotalSize.Y, menu.TotalSize.Width, menu.TotalSize.Height, ByteColor.Black, ByteColor.White, Ascii.BoxSides);

                renderer.DrawLabel(menu.AlignBlock("Menu".Length, 1, Layout.Align.Center).Position, "Menu");

                Layout menuContent = new(menu.TotalSize.Expand(-1));

                menuContent.ForceBreakLine();

                renderer.DrawLabel(menuContent.AlignBlock($"  Offline".Length, 1, Layout.Align.Center).Position, $"  Offline");

                menuContent.ForceBreakLine();

                renderer.DrawLabel(menuContent.AlignBlock($"  Server".Length, 1, Layout.Align.Center).Position, $"  Server");

                menuContent.ForceBreakLine();

                renderer.DrawLabel(menuContent.AlignBlock($"  Client".Length, 1, Layout.Align.Center).Position, $"  Client");
                */

                /*
                Scene = new Scene(this);
                this.networkMode = networkMode;
                
                if (networkMode == NetworkMode.Server ||
                    networkMode == NetworkMode.Offline)
                {
                    Scene.AddObject(new Player(new Vector(3, 4), Scene.GenerateNetworkId(), GameObjectPrototype.PLAYER, new NetworkPlayer(connection?.LocalEndPoint ?? throw new NullReferenceException())));
                }

                switch (networkMode)
                {
                    case NetworkMode.Server:
                        connection = new Net.UDP(false);
                        connection.Server("127.0.0.1", 7777);
                        connection.OnReceive += OnDataReceive;
                        connection.OnClientConnected += OnClientConnected;
                        connection.OnClientDisconnected += OnClientDisconnected;
                        break;
                    case NetworkMode.Client:
                        connection = new Net.UDP(false);
                        connection.Client("127.0.0.1", 7777);
                        connection.OnReceive += OnDataReceive;
                        connection.OnClientConnected += OnClientConnected;
                        connection.OnClientDisconnected += OnClientDisconnected;
                        break;
                    case NetworkMode.Offline:
                    default:
                        connection = null;
                        break;
                }
                */
            }

            renderer.DrawLabel(1, 0, $"FPS: {(int)MathF.Round(1f / deltaTime)}", ByteColor.Black, ByteColor.Gray);

            if (connection != null && Keyboard.IsKeyPressed(VirtualKeyCodes.TAB))
            {
                int width = 40, height = 10;
                int x = (renderer.Width - width) / 2;
                int y = (renderer.Height - height) / 2;

                RectInt menuBox = new(x, y, width, height);

                renderer.DrawBox(menuBox, ByteColor.Black, ByteColor.White);

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
                    renderer.DrawLabel(x + 1, y + 1, $"{server} (server)", ByteColor.Black, ByteColor.White);

                    for (int i = 0; i < clientCount; i++)
                    {
                        Socket client = Clients[i];
                        if (client == connection.LocalEndPoint)
                        {
                            renderer.DrawLabel(x + 1, y + 2 + i, $"{client} (you)", ByteColor.Black, ByteColor.White);
                        }
                        else
                        {
                            renderer.DrawLabel(x + 2, y + 2 + i, $"{client}", ByteColor.Black, ByteColor.White);
                        }
                    }
                }
                else if (networkMode == NetworkMode.Server)
                {
                    renderer.DrawLabel(x + 1, y + 1, $"{connection.LocalEndPoint} (you) (server)", ByteColor.Black, ByteColor.White);

                    int clientCount = connection.Clients.Length;
                    for (int i = 0; i < clientCount; i++)
                    {
                        Socket client = connection.Clients[i];
                        renderer.DrawLabel(x + 1, y + 2 + i, $"{client}", ByteColor.Black, ByteColor.White);
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

            renderer.Render();
        }
    }
}
