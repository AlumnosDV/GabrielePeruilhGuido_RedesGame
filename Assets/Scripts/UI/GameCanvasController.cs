using RedesGame.Managers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RedesGame.UI
{
    public class GameCanvasController : MonoBehaviour
    {
        [SerializeField] private GameObject _wonMenu;
        [SerializeField] private GameObject _loseMenu;
        [SerializeField] private GameObject _waitingScreen;
        [SerializeField] private TextMeshProUGUI _timerText;


        private void Awake()
        {
            _wonMenu.SetActive(false);
            _loseMenu.SetActive(false);
            _waitingScreen.SetActive(true);
            ScreenManager.Instance.Deactivate();
        }

        private void OnEnable()
        {
            EventManager.StartListening("UpdateTimer", OnUpdateTimer);
            EventManager.StartListening("AllPlayersInGame", OnAllPlayersInGame);
        }



        private void OnDisable()
        {
            EventManager.StopListening("UpdateTimer", OnUpdateTimer);
            EventManager.StopListening("AllPlayersInGame", OnAllPlayersInGame);
        }

        private void OnAllPlayersInGame(object[] obj)
        {
            _waitingScreen.SetActive(false);
            ScreenManager.Instance.Activate();
        }

        private void OnUpdateTimer(object[] obj)
        {
            _timerText.text = $"Waiting For Other Player...\n{obj[0]}";
        }

        public void SetPauseMenu(bool pause)
        {

            if (pause)
                ScreenManager.Instance.Deactivate();
            else
                ScreenManager.Instance.Activate();
        }

        public void SetWinningConditionScreen(bool playerWins)
        {

            _wonMenu.SetActive(playerWins);
            _loseMenu.SetActive(!playerWins);
            ScreenManager.Instance.Deactivate();
        }


        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}