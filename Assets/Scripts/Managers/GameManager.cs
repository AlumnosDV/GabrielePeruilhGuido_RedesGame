using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RedesGame.Managers
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private int _minPlayersPerGame = 2;

        [Networked(OnChanged = nameof(OnPlayersInGameChanged))]
        private int PlayersInGame { get; set; }
        [Networked]
        private float Timer { get; set; }

        [Networked(OnChanged = nameof(OnReadyPlayersChanged))]
        private int ReadyPlayers { get; set; }

        [Networked]
        private int AlivePlayers { get; set; }

        [Networked(OnChanged = nameof(OnMatchStartedChanged))]
        private bool MatchStarted { get; set; }

        [Networked(OnChanged = nameof(OnMatchEndedChanged))]
        private bool MatchEnded { get; set; }

        [Networked]
        private PlayerRef Winner { get; set; }

        [Networked(OnChanged = nameof(OnAllPlayersLeft))]
        private bool AllPlayersLeft { get; set; }

        private readonly Dictionary<PlayerRef, bool> _playerReadyState = new();
        private HashSet<PlayerRef> _alivePlayers = new();


        public override void Spawned()
        {
            ScreenManager.Instance.Deactivate();
            EventManager.StartListening("PlayerJoined", OnPlayerJoined);
            EventManager.StartListening("GoToMainMenu", DespawnPlayers);
            EventManager.StartListening("PlayerReadyChanged", OnPlayerReadyChanged);
            EventManager.StartListening("PlayerEliminated", OnPlayerEliminated);
        }
        static void OnAllPlayersLeft(Changed<GameManager> changed)
        {
            var behaviour = changed.Behaviour;
            SceneManager.LoadScene("MainMenu");
        }

        private void DespawnPlayers(object[] obj)
        {
            AllPlayersLeft = true;
        }

        private void OnPlayerJoined(object[] obj)
        {

            PlayersInGame++;

            var player = (PlayerRef)obj[0];
            if (!_playerReadyState.ContainsKey(player))
            {
                _playerReadyState.Add(player, false);
            }

            BroadcastReadyStatus();

            if (PlayersInGame >= _minPlayersPerGame)
                EventManager.TriggerEvent("AllPlayersInGame");
        }

        private void OnPlayerReadyChanged(object[] obj)
        {
            if (!Object.HasStateAuthority)
                return;

            var player = (PlayerRef)obj[0];
            var ready = (bool)obj[1];

            var hadEntry = _playerReadyState.TryGetValue(player, out var wasReady);
            if (hadEntry && wasReady == ready)
                return;

            _playerReadyState[player] = ready;

            var newStateValue = ready ? 1 : 0;
            var previousStateValue = hadEntry && wasReady ? 1 : 0;
            ReadyPlayers += newStateValue - previousStateValue;

            EvaluateMatchStart();
            BroadcastReadyStatus();
        }

        private void EvaluateMatchStart()
        {
            if (MatchStarted || !Object.HasStateAuthority)
                return;

            if (PlayersInGame < _minPlayersPerGame)
                return;

            if (ReadyPlayers < PlayersInGame)
                return;

            MatchStarted = true;
            AlivePlayers = PlayersInGame;
            _alivePlayers = new HashSet<PlayerRef>(_playerReadyState
                .Where(kv => kv.Value)
                .Select(kv => kv.Key));
        }

        private void OnPlayerEliminated(object[] obj)
        {
            if (!Object.HasStateAuthority || !MatchStarted || MatchEnded)
                return;

            var player = (PlayerRef)obj[0];
            if (_alivePlayers.Remove(player))
            {
                AlivePlayers = Mathf.Max(0, AlivePlayers - 1);
            }

            if (AlivePlayers <= 1)
            {
                Winner = _alivePlayers.Count == 1 ? _alivePlayers.First() : player;
                MatchEnded = true;
            }
        }

        static void OnMatchStartedChanged(Changed<GameManager> changed)
        {
            EventManager.TriggerEvent("MatchStarted");
            ScreenManager.Instance.Activate();
        }

        static void OnMatchEndedChanged(Changed<GameManager> changed)
        {
            var behaviour = changed.Behaviour;
            EventManager.TriggerEvent("MatchEnded", behaviour.Winner);
            ScreenManager.Instance.Deactivate();
        }

        static void OnPlayersInGameChanged(Changed<GameManager> changed)
        {
            var behaviour = changed.Behaviour;
            behaviour.BroadcastReadyStatus();
        }

        static void OnReadyPlayersChanged(Changed<GameManager> changed)
        {
            var behaviour = changed.Behaviour;
            behaviour.BroadcastReadyStatus();
        }


        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority)
                Timer += Runner.DeltaTime;

            EventManager.TriggerEvent("UpdateTimer", FormatDate(Timer));
        }

        private void BroadcastReadyStatus()
        {
            EventManager.TriggerEvent("ReadyStatusChanged", ReadyPlayers, PlayersInGame, _minPlayersPerGame);
        }

        private string FormatDate(float myTime)
        {
            int hours = Mathf.FloorToInt(myTime / 3600);
            int minutes = Mathf.FloorToInt((myTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(myTime % 60);

            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }
}
