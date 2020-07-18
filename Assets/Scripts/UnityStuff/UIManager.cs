﻿using Cysharp.Threading.Tasks;
using Scripts.GameLogic;
using Scripts.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UnityStuff
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager instance;

        [SerializeField] private InputField username;
        [SerializeField] private Text resultText;
        [SerializeField] private Text participantsText;
        [SerializeField] private GameObject waitingText;
        [SerializeField] private GameObject particles;
        [SerializeField] private GameObject gameUi;
        [SerializeField] private GameObject board;

        private GameObject mainMenu;
        private GameObject gameEnded;
        private GameObject background;
        private GameObject boardCamera;

        public static bool GameStarted { get;  private set; }

        public static string Username => instance.username.text;
        public static string OponentName { get; private set; }

        public static PlayerId Side { get; private set; }

        private int oponentId = -1;


        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("Instance already exists, destroying this...");
                Destroy(this);
            }
        }

        private void Start()
        {
            gameEnded = GameObject.FindWithTag("Game Ended");
            mainMenu = GameObject.FindWithTag("Main Menu");
            background = GameObject.FindWithTag("Background");
            boardCamera = GameObject.FindWithTag("Board Camera");

            gameUi.SetActive(false);
            board.SetActive(false);
            waitingText.SetActive(false);
            gameEnded.SetActive(false);
            mainMenu.SetActive(false);
            boardCamera.SetActive(false);

            Camera.main.rect = new Rect(0, 0, 1, 1);
        }

        public void JoinLobby()
        {
            ClientSend.JoinLobby(username.text);
            ClientSend.JoinGame(oponentId);

            mainMenu.SetActive(false);
            board.SetActive(true);
            waitingText.SetActive(true);
        }

        public async void BackToMainMenu()
        {
            await TransitionController.StartTransition();

            gameEnded.SetActive(false);
            //gameUI.SetActive(false);
            mainMenu.SetActive(true);

            await TransitionController.EndTransition();
        }

        public static UniTask OnConnected()
        {
            instance.mainMenu.SetActive(true);
            return TransitionController.EndTransition();
        }

        public static async UniTask StartTransitionIntoGame(PlayerId side, string oponentName, BoardParams board)
        {
            await TransitionController.StartTransition();

            OponentName = oponentName;
            Side = side;

            instance.waitingText.SetActive(false);
            instance.particles.SetActive(false);
            instance.gameUi.SetActive(true);
            instance.background.SetActive(false);
            instance.boardCamera.SetActive(true);

            UpdateScoreDisplay(0, 0);

            TileManager.CreateBoard(board);
            BoardCamera.Initialize(board);
        }

        public static UniTask EndTransitionIntoGame()
        {
            return TransitionController.EndTransition();
        }

        public static void UpdateScoreDisplay(int redScore, int blueScore)
        {
            instance.participantsText.text = Side == PlayerId.Red ?
                $"{OponentName} {blueScore} : {redScore} {Username}" :
                $"{Username} {blueScore} : {redScore} {OponentName}";
        }

        public static void OpponentDisconnected()
        { 
            string message = "Opponent has disconnected :(";
            instance.EndGame(message);
        }

        public static async UniTask EndGame(int blueScore, int redScore)
        {
            await UniTask.Delay(1500);

            string message = $"Final score: red: {redScore}, blue: {blueScore}";
            instance.EndGame(message);
        }

        private void EndGame(string message)
        {
            Debug.Log("UI manager ending the game");
            TileManager.DeactivateTiles();

            instance.background.SetActive(true);
            board.SetActive(false);
            gameEnded.SetActive(true);
            particles.SetActive(true);

            resultText.text = message;
        }
    }
}
