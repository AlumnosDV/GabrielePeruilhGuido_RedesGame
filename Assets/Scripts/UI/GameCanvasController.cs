using Fusion;
using RedesGame.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RedesGame.Player;
using NetworkPlayer = RedesGame.Player.NetworkPlayer;

namespace RedesGame.UI
{
    public class GameCanvasController : MonoBehaviour
    {

        [SerializeField] private GameObject _waitingScreen;
        [SerializeField] private GameObject _winConditionScreen;
        [SerializeField] private GameObject _loseConditionScreen;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Button _readyButton;
        [SerializeField] private TextMeshProUGUI _readyStatus;
        [SerializeField] private TextMeshProUGUI _matchResult;

        private bool _localReady;


        private void Awake()
        {
            _winConditionScreen.SetActive(false);
            _loseConditionScreen.SetActive(false);
            _waitingScreen.SetActive(true);
            ScreenManager.Instance.Deactivate();
            UpdateReadyStatus(0, 0, 0);
        }


        private void OnEnable()
        {
            EventManager.StartListening("UpdateTimer", OnUpdateTimer);
            EventManager.StartListening("AllPlayersInGame", OnAllPlayersInGame);
            EventManager.StartListening("Dead", OnWiningCondition);
            EventManager.StartListening("MatchStarted", OnMatchStarted);
            EventManager.StartListening("ReadyStatusChanged", OnReadyStatusChanged);
            EventManager.StartListening("MatchEnded", OnMatchEnded);
        }


        private void OnDisable()
        {
            EventManager.StopListening("UpdateTimer", OnUpdateTimer);
            EventManager.StopListening("AllPlayersInGame", OnAllPlayersInGame);
            EventManager.StopListening("Dead", OnWiningCondition);
            EventManager.StopListening("MatchStarted", OnMatchStarted);
            EventManager.StopListening("ReadyStatusChanged", OnReadyStatusChanged);
            EventManager.StopListening("MatchEnded", OnMatchEnded);

        }

        private void OnWiningCondition(object[] obj)
        {
            ScreenManager.Instance.Deactivate();
            if ((bool)obj[0])
            {
                _loseConditionScreen.SetActive(true);
            }
        }

        private void OnAllPlayersInGame(object[] obj)
        {
            _waitingScreen.SetActive(true);
            if (_readyButton != null)
                _readyButton.gameObject.SetActive(true);
            ScreenManager.Instance.Deactivate();
        }

        private void OnUpdateTimer(object[] obj)
        {
            _timerText.text = $"Waiting For Other Player...\n{obj[0]}";
        }

        private void OnReadyStatusChanged(object[] obj)
        {
            var ready = (int)obj[0];
            var total = (int)obj[1];
            var minRequired = (int)obj[2];
            UpdateReadyStatus(ready, total, minRequired);
        }

        private void UpdateReadyStatus(int ready, int total, int minRequired)
        {
            if (_readyStatus != null)
            {
                _readyStatus.text = $"Ready: {ready}/{total} (min {minRequired})";
            }
        }

        private void OnMatchStarted(object[] obj)
        {
            _waitingScreen.SetActive(false);
            ScreenManager.Instance.Activate();
        }

        private void OnMatchEnded(object[] obj)
        {
            var winner = (PlayerRef)obj[0];
            var isWinner = NetworkPlayer.Local != null && NetworkPlayer.Local.Object.InputAuthority == winner;

            _waitingScreen.SetActive(false);
            _matchResult?.gameObject.SetActive(true);
            if (_matchResult != null)
            {
                _matchResult.text = isWinner ? "You Won!" : "You Lost";
            }

            if (isWinner)
            {
                _winConditionScreen.SetActive(true);
            }
            else
            {
                _loseConditionScreen.SetActive(true);
            }
        }

        public void ToggleReady()
        {
            if (NetworkPlayer.Local == null)
                return;

            var model = NetworkPlayer.Local.GetComponent<PlayerModel>();
            if (model == null)
                return;

            _localReady = !_localReady;
            model.ToggleReadyState();

            if (_readyButton != null)
            {
                var label = _readyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = _localReady ? "Unready" : "Ready";
            }
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