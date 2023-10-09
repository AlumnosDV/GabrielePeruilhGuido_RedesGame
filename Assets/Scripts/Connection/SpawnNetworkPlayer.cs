using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using RedesGame.UI.Sessions;
using RedesGame.ExtensionsClass;
using RedesGame.Managers;
using UnityEngine.SceneManagement;

namespace RedesGame.Player
{
    public class SpawnNetworkPlayer : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkPlayer _playerPrefab;
        [SerializeField] private SessionListUIHandler _sessionListUIHandler;
        private NetworkCharacterController _characterController;

        public void OnConnectedToServer(NetworkRunner runner)
        {

        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (!NetworkPlayer.Local || !_characterController) return;

            input.Set(_characterController.GetLocalInputs());
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
        {

        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnDisconnectedFromServer(NetworkRunner runner) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
        {
            Debug.Log("OnPlayerLeft");
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) 
        {
            Debug.Log($"OnSceneLoadDone {SceneManager.GetActiveScene().buildIndex != 0}");
            if (SceneManager.GetActiveScene().buildIndex != 0)
            {
                if (runner.Topology == SimulationConfig.Topologies.Shared)
                {
                
                    var localPlayer = runner.Spawn(_playerPrefab,
                        Extensions.GetRandomSpawnPoint(),
                        Quaternion.identity,
                        runner.LocalPlayer);
                    _characterController = localPlayer.GetComponent<NetworkCharacterController>();
                }
            }
        }

        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) 
        {
            if (_sessionListUIHandler == null)
                return;

            if(sessionList.Count == 0)
                _sessionListUIHandler.OnNoSessionFound();
            else
            {
                _sessionListUIHandler.ClearList();

                foreach (SessionInfo session in sessionList)
                {
                    _sessionListUIHandler.AddToList(session);
                }
            }

            _sessionListUIHandler.ActiveCreateGameOption();
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
        {
            Debug.Log("OnShotdown");
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    }

}
