using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RedesGame.Managers
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private int _minPlayersPerGame = 1;

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

        // === NUEVO FLAG ===
        private bool _isSpawned;

        // Getters “seguros”
        public int CurrentPlayersInGame => _isSpawned ? PlayersInGame : 0;
        public int CurrentReadyPlayers => _isSpawned ? ReadyPlayers : 0;
        public int MinPlayersPerGame => _minPlayersPerGame;
        public override void Spawned()
        {
            _isSpawned = true;

            ScreenManager.Instance.Deactivate();
            EventManager.StartListening("PlayerJoined", OnPlayerJoined);
            EventManager.StartListening("GoToMainMenu", DespawnPlayers);
            EventManager.StartListening("ReplayMatch", OnReplayMatchRequested);
            EventManager.StartListening("PlayerReadyChanged", OnPlayerReadyChanged);
            EventManager.StartListening("PlayerEliminated", OnPlayerEliminated);
            EventManager.StartListening("PlayerLeft", OnPlayerLeft);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _isSpawned = false;

            EventManager.StopListening("PlayerJoined", OnPlayerJoined);
            EventManager.StopListening("GoToMainMenu", DespawnPlayers);
            EventManager.StopListening("ReplayMatch", OnReplayMatchRequested);
            EventManager.StopListening("PlayerReadyChanged", OnPlayerReadyChanged);
            EventManager.StopListening("PlayerEliminated", OnPlayerEliminated);
            EventManager.StopListening("PlayerLeft", OnPlayerLeft);
        }

        static void OnAllPlayersLeft(Changed<GameManager> changed)
        {
            var behaviour = changed.Behaviour;

            if (!behaviour.AllPlayersLeft)
                return;

            var runner = behaviour.Runner;
            if (runner != null && runner.IsRunning)
            {
                runner.Shutdown();
            }

            SceneManager.LoadScene("MainMenu");
        }

        private void DespawnPlayers(object[] obj)
        {
            if (Object.HasStateAuthority)
            {
                AllPlayersLeft = true;
            }
            else
            {
                RPC_RequestReturnToMenu();
            }
        }

        private void OnReplayMatchRequested(object[] obj)
        {
            if (Object.HasStateAuthority)
            {
                RestartMatch();
            }
            else
            {
                RPC_RequestReplay();
            }
        }

        private void OnPlayerJoined(object[] obj)
        {
            if (!Object.HasStateAuthority)
                return;

            var player = (PlayerRef)obj[0];
            if (!_playerReadyState.ContainsKey(player))
            {
                _playerReadyState.Add(player, false);
            }

            RecalculateCounts();

            if (PlayersInGame >= _minPlayersPerGame)
                EventManager.TriggerEvent("AllPlayersInGame");
        }

        private void OnPlayerReadyChanged(object[] obj)
        {
            Debug.Log($"OnPlayerReadyChanged {(bool)obj[1]} {(PlayerRef)obj[0]}");
            if (!Object.HasStateAuthority)
                return;

            var player = (PlayerRef)obj[0];
            var ready = (bool)obj[1];

            var hadEntry = _playerReadyState.TryGetValue(player, out var wasReady);
            if (hadEntry && wasReady == ready)
                return;

            _playerReadyState[player] = ready;

            RecalculateCounts();

            EvaluateMatchStart();
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
            _alivePlayers = new HashSet<PlayerRef>(_playerReadyState
                .Where(kv => kv.Value)
                .Select(kv => kv.Key));

            AlivePlayers = _alivePlayers.Count;
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

        private void OnPlayerLeft(object[] obj)
        {
            if (!Object.HasStateAuthority)
                return;

            var player = (PlayerRef)obj[0];

            _playerReadyState.Remove(player);
            _alivePlayers.Remove(player);

            RecalculateCounts();

            if (MatchStarted && !MatchEnded)
            {
                // Si se va alguien en medio de la partida, lo tratás como eliminado
                if (AlivePlayers > 0 && _alivePlayers.Count == 1)
                {
                    Winner = _alivePlayers.First();
                    MatchEnded = true;
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority)
            {
                Timer += Runner.DeltaTime;
                EventManager.TriggerEvent("UpdateTimer", FormatDate(Timer));
            }
        }

        private void BroadcastReadyStatus()
        {
            EventManager.TriggerEvent("ReadyStatusChanged", ReadyPlayers, PlayersInGame, _minPlayersPerGame);
        }

        private void RecalculateCounts()
        {
            PlayersInGame = Runner.ActivePlayers.Count();
            ReadyPlayers = _playerReadyState.Count(kv => kv.Value);

            BroadcastReadyStatus();
        }

        private string FormatDate(float myTime)
        {
            int hours = Mathf.FloorToInt(myTime / 3600);
            int minutes = Mathf.FloorToInt((myTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(myTime % 60);

            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestReturnToMenu()
        {
            AllPlayersLeft = true;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_RequestReplay()
        {
            RestartMatch();
        }

        private void RestartMatch()
        {
            MatchStarted = false;
            MatchEnded = false;
            AllPlayersLeft = false;
            Timer = 0f;
            ReadyPlayers = 0;
            PlayersInGame = 0;
            AlivePlayers = 0;
            Winner = PlayerRef.None;

            _playerReadyState.Clear();
            _alivePlayers.Clear();

            foreach (var player in Runner.ActivePlayers)
            {
                if (Runner.TryGetPlayerObject(player, out var playerObject))
                {
                    Runner.Despawn(playerObject);
                    Runner.SetPlayerObject(player, null);
                }

                _playerReadyState[player] = false;
            }

            RecalculateCounts();

            if (PlayersInGame >= _minPlayersPerGame)
            {
                EventManager.TriggerEvent("AllPlayersInGame");
            }

            // Use the runner scene loader so every client reloads the lobby together
            // instead of only the host reloading locally. Fusion 1.x uses SetActiveScene
            // on the runner instead of LoadScene.
            if (Runner != null && Runner.IsRunning)
            {
                Runner.SetActiveScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
