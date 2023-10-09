using Fusion;
using RedesGame.Managers;
using TMPro;
using UnityEngine;

namespace RedesGame.UI
{
    public class GameCanvasController : MonoBehaviour
    {

        [SerializeField] private GameObject _waitingScreen;
        [SerializeField] private GameObject _winConditionScreen;
        [SerializeField] private GameObject _loseConditionScreen;
        [SerializeField] private TextMeshProUGUI _timerText;


        private void Awake()
        {
            _winConditionScreen.SetActive(false);
            _loseConditionScreen.SetActive(false);
            _waitingScreen.SetActive(true);
            ScreenManager.Instance.Deactivate();
        }


        private void OnEnable()
        {
            EventManager.StartListening("UpdateTimer", OnUpdateTimer);
            EventManager.StartListening("AllPlayersInGame", OnAllPlayersInGame);
            EventManager.StartListening("Dead", OnWiningCondition);
        }


        private void OnDisable()
        {
            EventManager.StopListening("UpdateTimer", OnUpdateTimer);
            EventManager.StopListening("AllPlayersInGame", OnAllPlayersInGame);
            EventManager.StopListening("Dead", OnWiningCondition);

        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void OnWiningCondition(object[] obj)
        {
            ScreenManager.Instance.Deactivate();

            if ((bool)obj[0])
                _loseConditionScreen.SetActive(true);
            else
                _winConditionScreen.SetActive(true);

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


        public void ReturnToMainMenu()
        {
            EventManager.TriggerEvent("GoToMainMenu");
        }
    }
}