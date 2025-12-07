using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using RedesGame.UI.Sessions;
using RedesGame.ExtensionsClass;
using UnityEngine.SceneManagement;
using RedesGame.Managers;

namespace RedesGame.Player
{
    public class SpawnNetworkPlayer : MonoBehaviour, INetworkRunnerCallbacks
    {
        [Header("Prefabs")]
        [SerializeField] private NetworkPlayer _playerPrefab;

        [Header("UI")]
        [SerializeField] private SessionListUIHandler _sessionListUIHandler;

        private CharacterInputHandler _inputHandler;

        // ---------- INPUT ----------

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (!NetworkPlayer.Local)
                return;

            // Lazy init: busco el CharacterInputHandler en el player local
            if (_inputHandler == null)
            {
                _inputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();
                if (_inputHandler == null)
                {
                    Debug.LogWarning("[SpawnNetworkPlayer] No CharacterInputHandler found on local player");
                    return;
                }
            }

            input.Set(_inputHandler.GetLocalInputs());
        }

        // ---------- SPAWN DEL PLAYER ----------

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            HandleSceneLoaded(runner);
        }

        public void OnSceneLoadDone(NetworkRunner runner, SceneRef scene, SceneRef? prevScene)
        {
            HandleSceneLoaded(runner);
        }

        private void HandleSceneLoaded(NetworkRunner runner)
        {
            Debug.Log($"[SpawnNetworkPlayer] OnSceneLoadDone, scene index: {SceneManager.GetActiveScene().buildIndex}");

            // asumo que scene 0 es MainMenu y el resto son escenas de juego
            if (SceneManager.GetActiveScene().buildIndex == 0)
                return;

            if (!runner.IsServer)
                return;

            foreach (var player in runner.ActivePlayers)
            {
                TrySpawnPlayer(runner, player);
            }

        }

        private void TrySpawnPlayer(NetworkRunner runner, PlayerRef player)
        {
            if (runner.TryGetPlayerObject(player, out _))
            {
                Debug.Log($"[SpawnNetworkPlayer] Player {player} already has an object, skipping spawn");
                return;
            }

            var spawnPos = Extensions.GetRandomSpawnPoint();

            var spawnedPlayer = runner.Spawn(
                _playerPrefab,
                spawnPos,
                Quaternion.identity,
                player
            );

            runner.SetPlayerObject(player, spawnedPlayer.Object);

            Debug.Log($"[SpawnNetworkPlayer] Spawned player object for {player}");
        }

        // ---------- SESIONES / UI ----------

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            if (_sessionListUIHandler == null)
                return;

            if (sessionList.Count == 0)
            {
                _sessionListUIHandler.OnNoSessionFound();
            }
            else
            {
                _sessionListUIHandler.ClearList();

                foreach (var session in sessionList)
                {
                    _sessionListUIHandler.AddToList(session);
                }
            }

            _sessionListUIHandler.ActiveCreateGameOption();
        }

        // ---------- El resto de callbacks (vacíos por ahora) ----------

        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnDisconnectedFromServer(NetworkRunner runner) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (!runner.IsServer)
                return;

            Debug.Log($"[SpawnNetworkPlayer] OnPlayerJoined {player}");

            // Avoid spawning in MainMenu; HandleSceneLoaded will spawn after the game scene loads.
            if (SceneManager.GetActiveScene().buildIndex == 0)
                return;

            TrySpawnPlayer(runner, player);
        }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("[SpawnNetworkPlayer] OnPlayerLeft " + player);
        }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log("[SpawnNetworkPlayer] OnShutdown: " + shutdownReason);
        }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    }
}
