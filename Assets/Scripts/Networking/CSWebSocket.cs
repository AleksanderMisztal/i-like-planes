﻿#if UNITY_EDITOR || !UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Scripts.Utils;

namespace Scripts.Networking
{
    public class WebSocket
    {
        public static WebSocket instance;

        //private readonly static string host = "wwsserver.azurewebsites.net";
        private readonly static string host = "localhost";
        private readonly static int port = 5001;
        public int myId;
        public WsClient wsClient;

        private delegate void PacketHandler(Packet _packet);
        private static Dictionary<int, PacketHandler> packetHandlers;

        private void InitializePacketHandlers()
        {
            packetHandlers = new Dictionary<int, PacketHandler>
            {
                {(int)ServerPackets.Welcome, ClientHandle.Welcome },
                {(int)ServerPackets.GameJoined, ClientHandle.GameJoined },
                {(int)ServerPackets.TroopSpawned, ClientHandle.TroopSpawned },
                {(int)ServerPackets.TroopMoved, ClientHandle.TroopMoved },
                {(int)ServerPackets.GameEnded, ClientHandle.GameEnded },
            };
        }

        public async Task InitializeConnection()
        {
            InitializePacketHandlers();
            wsClient = new WsClient();
            await wsClient.Connect();
        }

        public void SendData(Packet packet)
        {
            Packet perm = new Packet(packet.ToArray());
            wsClient.AddToQueue(perm);
        }

        public class WsClient
        {
            public Queue<Packet> sendQueue = new Queue<Packet>();

            public void AddToQueue(Packet packet)
            {
                sendQueue.Enqueue(packet);
            }

            public async Task SendPackets()
            {
                while (sendQueue.Count != 0)
                {
                    Packet packet = sendQueue.Dequeue();
                    await SendData(packet);
                }
            }


            public ClientWebSocket socket;

            public async Task Connect()
            {
                Uri serverUri = new Uri($"wss://{host}:{port}");
                socket = new ClientWebSocket();
                Debug.Log("Attempting to connect to " + serverUri.ToString());
                await socket.ConnectAsync(serverUri, CancellationToken.None);
                await BeginListen();
            }

            private async Task<byte[]> Receive()
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[4 * 1024]);
                var memoryStream = new MemoryStream();
                WebSocketReceiveResult result;

                do {
                    result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                    memoryStream.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                memoryStream.Seek(0, SeekOrigin.Begin);

                if (result.MessageType != WebSocketMessageType.Close)
                {
                    using (var reader = new StreamReader(memoryStream, Encoding.UTF8))
                    {
                        string bytes = reader.ReadToEnd();
                        try
                        {
                            return Serializer.Deserialize(bytes);
                        }
                        catch
                        {
                            Debug.Log("Couldn't convert to bytes");
                        }
                    }
                }
                throw new Exception("Something went wrong while reading the message.");
            }

            private async Task BeginListen()
            {
                byte[] data = await Receive();

                using (Packet packet = new Packet(data))
                {
                    int packetType = packet.ReadInt();
                    packetHandlers[packetType](packet);
                }

                await BeginListen();
            }

            private async Task SendData(Packet packet)
            {
                Debug.Log("Sending a packet: " + packet);
                var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(Serializer.Serialize(packet.ToArray())));
                await socket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
                Debug.Log("Sent!");
            }
        }
    }
}
#endif