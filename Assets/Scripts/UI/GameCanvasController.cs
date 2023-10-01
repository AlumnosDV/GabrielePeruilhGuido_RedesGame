using RedesGame.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RedesGame.UI
{
    public class GameCanvasController : MonoBehaviour
    {
        [SerializeField] private GameObject _pauseButton;
        [SerializeField] private GameObject _pauseMenu;
        [SerializeField] private GameObject _winConditionMenu;
        [SerializeField] private GameObject _wonMenu;
        [SerializeField] private GameObject _loseMenu;
        [SerializeField] private GameObject _clonableObjectsPlane;
        [SerializeField] private GameObject _starGameButton;

        private void Awake()
        {
            _pauseButton.SetActive(true);
            _clonableObjectsPlane.SetActive(true);
            _starGameButton.SetActive(true);
            _pauseMenu.SetActive(false);
            _winConditionMenu.SetActive(false);
            _wonMenu.SetActive(false);
            _loseMenu.SetActive(false);
        }

        public void SetPauseMenu(bool pause)
        {
            _pauseMenu.SetActive(pause);
            _pauseButton.SetActive(!pause);

            if (pause)
                ScreenManager.Instance.Deactivate();
            else
                ScreenManager.Instance.Activate();
        }

        public void SetWinningConditionScreen(bool playerWins)
        {
            _winConditionMenu.SetActive(true);
            _pauseButton.SetActive(false);
            _pauseButton.SetActive(false);
            _wonMenu.SetActive(playerWins);
            _loseMenu.SetActive(!playerWins);
            ScreenManager.Instance.Deactivate();
        }

        public void StartGame()
        {
            _clonableObjectsPlane.SetActive(false);
            _starGameButton.SetActive(false);
            EventManager.TriggerEvent("StartLevel");
        }


        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}