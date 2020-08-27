﻿using Planes262.GameLogic;
using Planes262.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Planes262.UnityLayer
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        private ClientSend sender;

        [SerializeField] private GameObject particles;
        
        [SerializeField] private InputField username;
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private Text participantsText;
        [SerializeField] private GameObject waitingText;
        [SerializeField] private GameObject mainBackground;
        
        [SerializeField] private GameObject boardCamera;
        [SerializeField] private GameObject board;
        [SerializeField] private GameObject gameUi;
        
        [SerializeField] private GameObject gameEnded;
        [SerializeField] private Text resultText;

        private string Username => username.text;
        private string OpponentsName { get; set; }
        private static PlayerSide Side { get; set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);
        }

        private void Start()
        {
            gameUi.SetActive(false);
            board.SetActive(false);
            waitingText.SetActive(false);
            gameEnded.SetActive(false);
            mainMenu.SetActive(false);
            boardCamera.SetActive(false);

            // ReSharper disable once PossibleNullReferenceException
            Camera.main.rect = new Rect(0, 0, 1, 1);
        }

        public void OnConnected()
        {
            mainMenu.SetActive(true);
        }

        public void JoinGame()
        {
            Messenger.SetUsername(username.text);
            sender.JoinGame(username.text);

            mainMenu.SetActive(false);
            board.SetActive(true);
            waitingText.SetActive(true);

        }

        public void StartTransitionIntoGame(PlayerSide side, string opponentsName, Board boardDims)
        {
            OpponentsName = opponentsName;
            Side = side;

            waitingText.SetActive(false);
            particles.SetActive(false);
            gameUi.SetActive(true);
            mainBackground.SetActive(false);
            boardCamera.SetActive(true);

            UpdateScoreDisplay(0, 0);

            TileManager.CreateBoard(boardDims);
            BoardCamera.Initialize(boardDims);
        }

        private void UpdateScoreDisplay(int redScore, int blueScore)
        {
            participantsText.text = Side == PlayerSide.Red ?
                $"{OpponentsName} {blueScore} : {redScore} {Username}" :
                $"{Username} {blueScore} : {redScore} {OpponentsName}";
        }

        public void OpponentDisconnected()
        {
            const string message = "Opponent has disconnected :(";
            EndGame(message);
        }

        public void EndGame(int blueScore, int redScore)
        {
            string message = $"Final score: red: {redScore}, blue: {blueScore}";
            EndGame(message);
        }

        private void EndGame(string message)
        {
            Debug.Log("UI manager ending the game");
            TileManager.DeactivateTiles();
            TroopController.ResetForNewGame();

            Instance.mainBackground.SetActive(true);
            board.SetActive(false);
            gameEnded.SetActive(true);
            particles.SetActive(true);

            resultText.text = message;
        }

        public void BackToMainMenu()
        {
            gameEnded.SetActive(false);
            mainMenu.SetActive(true);
        }

        public void SetSender(ClientSend sender)
        {
            this.sender = sender;
        }
    }
}
