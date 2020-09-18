﻿using System.Collections.Generic;
using GameDataStructures;
using Planes262.GameLogic.Troops;
using Planes262.Networking.Packets;
using Planes262.UnityLayer;

namespace Planes262.Networking
{
    public class ClientHandle
    {
        private delegate void PacketHandler(Packet packet);
        private readonly Dictionary<int, PacketHandler> packetHandlers;
        private readonly ServerJudge serverJudge;

        public ClientHandle(ServerJudge serverJudge)
        {
            this.serverJudge = serverJudge;
            packetHandlers = new Dictionary<int, PacketHandler>
            {
                {(int) ServerPackets.Welcome, Welcome },
                {(int) ServerPackets.GameJoined, GameJoined },
                {(int) ServerPackets.TroopSpawned, TroopSpawned },
                {(int) ServerPackets.TroopMoved, TroopMoved },
                {(int) ServerPackets.GameEnded, GameEnded },
                {(int) ServerPackets.OpponentDisconnected, OpponentDisconnected },
                {(int) ServerPackets.MessageSent, MessageSent },
            };
        }

        
        public void HandlePacket(string byteArray)
        {
            byte[] bytes = Serializer.Deserialize(byteArray);
            using (Packet packet = new Packet(bytes))
            {
                int packetType = packet.ReadInt();
                packetHandlers[packetType](packet);
            }
        }


        private void Welcome(Packet packet)
        {
            serverJudge.OnWelcome();
        }

        private void GameJoined(Packet packet)
        {
            string opponentName = packet.ReadString();
            PlayerSide side = (PlayerSide)packet.ReadInt();
            Board board = packet.ReadBoard();

            serverJudge.OnGameJoined(opponentName, side, board);
        }

        private void TroopSpawned(Packet packet)
        {
            List<Troop> troops = packet.ReadTroops();

            serverJudge.OnTroopSpawned(troops);
        }

        private void TroopMoved(Packet packet)
        {
            VectorTwo position = packet.ReadVector2Int();
            int direction = packet.ReadInt();
            List<BattleResult> battleResults = packet.ReadBattleResults();

            serverJudge.OnTroopMoved(position, direction, battleResults);
        }

        private void GameEnded(Packet packet)
        {
            int redScore = packet.ReadInt();
            int blueScore = packet.ReadInt();

            serverJudge.OnGameEnded(redScore, blueScore);
        }

        private void MessageSent(Packet packet)
        {
            string message = packet.ReadString();

            serverJudge.OnMessageSent(message);
        }

        private void OpponentDisconnected(Packet packet)
        {
            serverJudge.OnOpponentDisconnected();
        }
    }
}
