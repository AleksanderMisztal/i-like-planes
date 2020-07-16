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

        private GameObject mainMenu;
        private GameObject ui;
        private GameObject board;
        private GameObject particles;
        private GameObject waitingText;
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
            ui = GameObject.FindWithTag("UI");
            board = GameObject.FindWithTag("Board");
            particles = GameObject.FindWithTag("Particles");
            //gameUI = GameObject.FindWithTag("Game UI");
            gameEnded = GameObject.FindWithTag("Game Ended");
            mainMenu = GameObject.FindWithTag("Main Menu");
            waitingText = GameObject.FindWithTag("Waiting");
            background = GameObject.FindWithTag("Background");
            boardCamera = GameObject.FindWithTag("Board Camera");

            ui.SetActive(false);
            board.SetActive(false);
            waitingText.SetActive(false);
            //gameUI.SetActive(false);
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

        public static void OnConnected()
        {
            instance.mainMenu.SetActive(true);
            TransitionController.EndTransition();
        }

        public static async UniTask StartGame(PlayerId side, string oponentName, BoardParams board)
        {
            await TransitionController.StartTransition();

            instance.waitingText.SetActive(false);
            instance.particles.SetActive(false);

            //instance.gameUI.SetActive(true);
            OponentName = oponentName;
            Side = side;
            UpdateScoreDisplay();


            instance.ui.SetActive(true);
            instance.background.SetActive(false);
            instance.boardCamera.SetActive(true);
            TileManager.CreateBoard(board);
            BoardCamera.Initialize(board);

            GameStarted = true;

            await TransitionController.EndTransition();
        }

        public static void UpdateScoreDisplay()
        {
            instance.participantsText.text = Side == PlayerId.Red ?
                $"{OponentName} {GameController.BlueScore} : {GameController.RedScore} {Username}" :
                $"{Username} {GameController.BlueScore} : {GameController.RedScore} {OponentName}";
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

            GameController.instance.EndGame();

            instance.background.SetActive(true);
            board.SetActive(false);
            gameEnded.SetActive(true);
            particles.SetActive(true);

            resultText.text = message;
        }
    }
}
