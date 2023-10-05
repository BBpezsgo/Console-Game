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
            Entity newEntity = EntityPrototypes.Builders[GameObjectPrototype.PLAYER](Scene.GenerateNetworkId(), new ObjectOwner(client));
            newEntity.Position = new Vector(3, 4);
            Scene.AddEntity(newEntity);
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

            if (message is RespawnRequestMessage)
            {
                if (networkMode == NetworkMode.Client) return;
                OnRespawnRequest(sender);
                return;
            }

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
                connection?.FeedControlMessage(sender, netControlMessage);
                return;
            }

            if (message is MessageRpc rpcMessage)
            {
                if (!Scene.TryGetNetworkEntity(rpcMessage, out NetworkEntityComponent? @object))
                {
                    if (networkMode != NetworkMode.Client) return;
                    if (Requests.Request(new Request(RequestKinds.OBJ_DETAILS_REQUEST, HashCode.Combine(rpcMessage.NetworkId))))
                    {
                        connection?.Send(new ObjectRequestMessage(rpcMessage.NetworkId));
                        Debug.WriteLine($"Network object {rpcMessage.NetworkId} not found; requesting object details ...");
                    }
                    return;
                }

                @object.HandleRpc(rpcMessage);
                return;
            }

            if (message is ObjectRequestMessage objectRequestMessage)
            {
                if (!Scene.TryGetNetworkEntity(objectRequestMessage.NetworkId, out NetworkEntityComponent? @object))
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
                Scene.AddEntity(objectDetailsMessage);
                return;
            }

            if (message is ObjectSpawnMessage objectSpawnMessage)
            {
                if (networkMode != NetworkMode.Client) return;
                Scene.AddEntity(objectSpawnMessage);
                return;
            }

            if (message is ObjectDestroyMessage objectDestroyMessage)
            {
                if (networkMode != NetworkMode.Client) return;
                if (!Scene.TryGetNetworkEntity(objectDestroyMessage.NetworkId, out NetworkEntityComponent? @object))
                {
                    Debug.WriteLine($"Network object {objectDestroyMessage.NetworkId} not found; can not destroy");
                    return;
                }
                @object.IsDestroyed = true;
                return;
            }

            if (message is ComponentMessage componentMessage)
            {
                if (!Scene.TryGetNetworkEntity(componentMessage, out NetworkEntityComponent? @object))
                {
                    if (networkMode != NetworkMode.Client) return;
                    if (Requests.Request(new Request(RequestKinds.OBJ_DETAILS_REQUEST, HashCode.Combine(componentMessage.NetworkId))))
                    {
                        connection?.Send(new ObjectRequestMessage(componentMessage.NetworkId));
                        Debug.WriteLine($"Network object {componentMessage.NetworkId} not found; requesting object details ...");
                    }
                    return;
                }

                @object.HandleMessage(componentMessage);
                return;
            }

            if (message is ObjectMessage objectMessage)
            {
                if (!Scene.TryGetNetworkEntity(objectMessage, out NetworkEntityComponent? @object))
                {
                    if (networkMode != NetworkMode.Client) return;
                    if (Requests.Request(new Request(RequestKinds.OBJ_DETAILS_REQUEST, HashCode.Combine(objectMessage.NetworkId))))
                    {
                        connection?.Send(new ObjectRequestMessage(objectMessage.NetworkId));
                        Debug.WriteLine($"Network object {objectMessage.NetworkId} not found; requesting object details ...");
                    }
                    return;
                }

                @object.HandleMessage(objectMessage);
                return;
            }

            throw new NotImplementedException();
        }

    }
}
