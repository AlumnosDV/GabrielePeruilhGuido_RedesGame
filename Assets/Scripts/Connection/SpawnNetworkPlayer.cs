using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

namespace RedesGame.Player
{
    public class SpawnNetworkPlayer : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private int _maxPlayersPerGame = 2;
        [SerializeField] private NetworkPlayer _playerPrefab;
        private NetworkCharacterController _characterController;

        [Networked]
        private int PlayersInGame { get; set; }

        [SerializeField] public Transform[] InitialPositionOfPlayers = new Transform[2];  

        public void OnConnectedToServer(NetworkRunner runner)
        {
            if (PlayersInGame >= _maxPlayersPerGame) return;
            if (runner.Topology == SimulationConfig.Topologies.Shared)
            {
                var localPlayer = runner.Spawn(_playerPrefab, 
                    InitialPositionOfPlayers[runner.LocalPlayer.PlayerId].position, 
                    Quaternion.identity, 
                    runner.LocalPlayer);
                localPlayer.transform.right = InitialPositionOfPlayers[runner.LocalPlayer.PlayerId].right;
                _characterController = localPlayer.GetComponent<NetworkCharacterController>();
                PlayersInGame++;
            }
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

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

        public void OnSceneLoadDone(NetworkRunner runner) { }

        public void OnSceneLoadStart(NetworkRunner runner) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    }

}
