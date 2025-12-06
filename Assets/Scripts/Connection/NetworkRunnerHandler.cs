using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using RedesGame.Player; // para SpawnNetworkPlayer si está en ese namespace
using System;

public class NetworkRunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Runner")]
    [SerializeField] private NetworkRunner _runnerPrefab;

    [Header("Callbacks externos")]
    [SerializeField] private SpawnNetworkPlayer _spawnNetworkPlayer;

    private NetworkRunner _runner;
    private NetworkSceneManagerDefault _sceneManager;

    private const string LOBBY_NAME = "OurLobbyId";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _sceneManager = GetComponent<NetworkSceneManagerDefault>();
        if (_sceneManager == null)
        {
            _sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
    }

    public void OnJoinLobby()
    {
        StartJoinLobbyFlow();
    }

    public void CreateGame(string sessionName, string sceneName)
    {

        int sceneIndex = SceneUtility.GetBuildIndexByScenePath($"Scenes/{sceneName}");
        InitializeGame(GameMode.Shared, sessionName, sceneIndex);
    }

    public void JoinGame(SessionInfo sessionInfo)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        InitializeGame(GameMode.Shared, sessionInfo.Name, currentSceneIndex);
    }


    private async void StartJoinLobbyFlow()
    {
        if (_runner != null)
        {
            Destroy(_runner.gameObject);
            _runner = null;
        }

        _runner = Instantiate(_runnerPrefab);
        DontDestroyOnLoad(_runner.gameObject);

        _runner.AddCallbacks(this);

        if (_spawnNetworkPlayer != null)
        {
            _runner.AddCallbacks(_spawnNetworkPlayer);
        }

        var result = await _runner.JoinSessionLobby(SessionLobby.Custom, LOBBY_NAME);

        if (!result.Ok)
        {
            Debug.LogError($"[NetworkRunnerHandler] Unable to join lobby {LOBBY_NAME}: {result.ShutdownReason}");
        }
        else
        {
            Debug.Log($"[NetworkRunnerHandler] Joined lobby {LOBBY_NAME}");
        }
    }

    private async void InitializeGame(GameMode mode, string sessionName, int sceneIndex)
    {
        if (_runner == null)
        {
            Debug.LogError("[NetworkRunnerHandler] Runner is null. Did you call OnJoinLobby() first?");
            return;
        }

        _runner.ProvideInput = true;

        var startArgs = new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = sceneIndex,     
            CustomLobbyName = LOBBY_NAME,
            SceneManager = _sceneManager
        };

        var result = await _runner.StartGame(startArgs);

        if (!result.Ok)
        {
            Debug.LogError($"[NetworkRunnerHandler] Unable to start game: {result.ShutdownReason}");
        }
        else
        {
            Debug.Log("[NetworkRunnerHandler] Game started");
        }
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnDisconnectedFromServer(NetworkRunner runner){}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) {}
}
