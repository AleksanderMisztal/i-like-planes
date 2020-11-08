﻿using GameDataStructures.Messages.Client;
using GameDataStructures.Positioning;
using Planes262.UnityLayer;
using UnityEngine;

namespace Planes262.Networking
{
    public class Client : MonoBehaviour
    {
        public static Client instance;

        private void Awake()
        {
            if (instance == this) return;
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Debug.Log("Destroying the client!");
                Destroy(this);
            }
        }

        private IPacketSender messageSender;
        public readonly ServerEvents serverEvents = new ServerEvents();

        private void Start()
        {
#if UNITY_EDITOR || !UNITY_WEBGL
            ws = new CsWebSocket(serverEvents);
            ws.InitializeConnection();
#else
            JsWebSocket ws = Instantiate(new GameObject().AddComponent<JsWebSocket>());
            ws.gameObject.name = "JsWebSocket";
            ws.SetEvents(serverEvents);
            ws.InitializeConnection();
#endif
            messageSender = ws;
        }
#if UNITY_EDITOR || !UNITY_WEBGL
        private CsWebSocket ws;
        private void OnApplicationQuit()
        {
            ws.Close();
        }
#endif

        public void JoinGame(string gameType)
        {
            JoinGameMessage message = new JoinGameMessage
            {
                gameType = gameType,
                username = PlayerMeta.name,
            };
            messageSender.SendData(message);
        }

        public void MoveTroop(VectorTwo position, int direction)
        {
            MoveTroopMessage message = new MoveTroopMessage
            {
                direction = direction,
                position = position,
            };
            messageSender.SendData(message);
        }

        public void SendAMessage(string m)
        {
            SendChatMessage message = new SendChatMessage{message = m};
            messageSender.SendData(message);
        }
    }
}
