using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.Net;
using DataUtilities.Serializer;

namespace ConsoleGame
{
    public enum NetworkMode
    {
        Offline,
        Server,
        Client,
    }

    public partial class Game
    {
        public const float NetworkTickRate = 1f;

        NetworkMode networkMode;
        Connection? connection;
        readonly RequestManager Requests = new();

        float synchronizeCooldown = NetworkTickRate;
        Socket[] Clients = Array.Empty<Socket>();

        public static Connection? Connection => Instance.connection;
        public static NetworkMode NetworkMode => Instance.networkMode;

        void OnClientDisconnected(Socket client)
        {

        }

        void OnClientConnected(Socket client)
        {
            Scene.AddObject(new Player(new Vector(3, 6), Scene.GenerateNetworkId(), GameObjectPrototype.PLAYER, new NetworkPlayer(client)));
        }

        void OnDataReceive(Socket sender, byte[] data)
        {
            Deserializer deserializer = new(data);
            int endlessSafe = 256;
            while (deserializer.HasData)
            {
                OnDataReceive(sender, Message.DeserializeMessage(deserializer));

                if (endlessSafe-- < 0)
                { break; }
            }
        }

        void OnDataReceive(Socket sender, Message message)
        {
            /*
            if (LastMessageGUID == message.GUID)
            {
                Debug.WriteLine($"Duplicated message");
                return;
            }
            LastMessageGUID = message.GUID;
            */

            if (message is ClientListMessage clientListMessage)
            {
                if (networkMode != NetworkMode.Client) return;
                Clients = clientListMessage.Clients;
                Requests.Finished(new Request(RequestKinds.CLIENT_LIST, 1));
                return;
            }

            if (message is ClientListRequestMessage)
            {
                if (networkMode != NetworkMode.Server) return;
                connection?.Send(new ClientListMessage()
                {
                    Type = MessageType.CLIENT_LIST,
                    Clients = connection.Clients,
                });
                return;
            }

            if (message is NetControlMessage netControlMessage)
            {
                connection?.FeedControlMessage(netControlMessage);
                return;
            }

            if (message is MessageRpc rpcMessage)
            {
                if (!Scene.TryGetNetworkObject(rpcMessage, out var @object))
                {
                    if (networkMode != NetworkMode.Client) return;
                    if (Requests.Request(new Request(RequestKinds.OBJ_DETAILS_REQUEST, HashCode.Combine(rpcMessage.NetworkId))))
                    {
                        connection?.Send(new ObjectRequestMessage(rpcMessage.NetworkId));
                        Debug.WriteLine($"Network object {rpcMessage.NetworkId} not found; requesting object details ...");
                    }
                    return;
                }

                @object.OnRpc(rpcMessage);
                return;
            }

            if (message is ObjectRequestMessage objectRequestMessage)
            {
                if (!Scene.TryGetNetworkObject(objectRequestMessage.NetworkId, out var @object))
                {
                    Debug.WriteLine($"Network object {objectRequestMessage.NetworkId} not found; can not send object details");
                    return;
                }

                Debug.WriteLine($"Sending object details for object {objectRequestMessage.NetworkId} ...");
                connection?.Send(new ObjectDetailsMessage(@object));
                return;
            }

            if (message is ObjectDetailsMessage objectDetailsMessage)
            {
                if (networkMode != NetworkMode.Client) return;
                Scene.AddObject(objectDetailsMessage);
                return;
            }

            if (message is ObjectSpawnMessage objectSpawnMessage)
            {
                if (networkMode != NetworkMode.Client) return;
                Scene.AddObject(objectSpawnMessage);
                return;
            }

            if (message is ObjectMessage objectMessage)
            {
                if (!Scene.TryGetNetworkObject(objectMessage, out var @object))
                {
                    if (networkMode != NetworkMode.Client) return;
                    if (Requests.Request(new Request(RequestKinds.OBJ_DETAILS_REQUEST, HashCode.Combine(objectMessage.NetworkId))))
                    {
                        connection?.Send(new ObjectRequestMessage(objectMessage.NetworkId));
                        Debug.WriteLine($"Network object {objectMessage.NetworkId} not found; requesting object details ...");
                    }
                    return;
                }

                @object.OnMessageReceived(objectMessage);
                return;
            }

            throw new NotImplementedException();
        }

    }
}
