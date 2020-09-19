﻿using System.Collections.Generic;
using GameDataStructures;
using Planes262.GameLogic.Troops;
using UnityEngine;

namespace Planes262.UnityLayer.Managers
{
    public class ServerJudge
    {
        private readonly UIManager uiManager;
        private readonly Messenger messenger;
        private readonly GameManager gameManager;

        public ServerJudge(Messenger messenger, UIManager uiManager, GameManager gameManager)
        {
            this.messenger = messenger;
            this.uiManager = uiManager;
            this.gameManager = gameManager;
        }

        
        public void OnWelcome()
        {
            Debug.Log("Connected to server!");
            uiManager.ActivateMainMenu();
        }

        public void OnGameEnded(int redScore, int blueScore)
        {
            string message = $"Final score: red: {redScore}, blue: {blueScore}";
            uiManager.EndGame(message, 1.5f);
            gameManager.OnGameEnded();
        }

        public void OnOpponentDisconnected()
        {
            uiManager.EndGame("Opponent has disconnected :(", 0);
            gameManager.OnGameEnded();
        }

        
        public void OnGameJoined(string opponentName, PlayerSide side, Board board)
        {
            Debug.Log("Game joined received! Playing against " + opponentName);
            gameManager.StartNewGame(board, side);
            messenger.ResetMessages();
            uiManager.TransitionIntoGame(board);
        }

        public void OnTroopSpawned(List<Troop> troops)
        {
            gameManager.BeginNextRound(troops);
        }

        public void OnTroopMoved(VectorTwo position, int direction, List<BattleResult> battleResults)
        {
            gameManager.MoveTroop(position, direction, battleResults);
        }

        
        public void OnMessageSent(string message)
        {
            messenger.MessageReceived(message);
        }
    }
}