using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using RedesGame.Player;
using RedesGame.Managers;

[RequireComponent(typeof(NetworkRunner))]
[RequireComponent(typeof(NetworkSceneManagerDefault))]
public class NetworkRunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkRunnerHandler Instance { get; private set; }

    [Header("Callbacks externos")]
    [SerializeField] private SpawnNetworkPlayer _spawnNetworkPlayer;

    private NetworkRunner _runner;
    private NetworkSceneManagerDefault _sceneManager;

    private const string LOBBY_NAME = "OurLobbyId";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Estos componentes viven en el mismo GameObject
        _runner = GetComponent<NetworkRunner>();
        _sceneManager = GetComponent<NetworkSceneManagerDefault>();
    }

    private void Start()
    {
        if (_sceneManager == null)
            _sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
    }

    // ================== API PÚBLICA ==================

    // Botón "Buscar sesiones" o para asegurar lobby
    public async void OnJoinLobby()
    {
        bool ok = await EnsureRunnerAndLobby();
        if (!ok)
            Debug.LogError("[NetworkRunnerHandler] No se pudo unir al lobby.");
    }

    // Botón "Crear partida"
    public async void CreateGame(string sessionName, string sceneName)
    {
        bool ok = await EnsureRunnerAndLobby();
        if (!ok)
        {
            Debug.LogError("[NetworkRunnerHandler] No se pudo crear el Runner / Lobby.");
            return;
        }

        // En Build Settings la escena es "Scenes/Level-1"
        string scenePath = $"Scenes/{sceneName}";
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

        if (sceneIndex < 0)
        {
            Debug.LogError($"[NetworkRunnerHandler] La escena '{scenePath}' no está en Build Settings.");
            return;
        }

        await InitializeGame(GameMode.Host, sessionName, sceneIndex);
    }

    public async void JoinGame(SessionInfo sessionInfo)
    {
        bool ok = await EnsureRunnerAndLobby();
        if (!ok)
            return;

        string scenePath = $"Scenes/Level-1";
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);

        await InitializeGame(GameMode.Client, sessionInfo.Name, sceneIndex);
    }

    // ================== LÓGICA INTERNA ==================

    /// <summary>
    /// Se asegura de que exista un runner, tenga callbacks y esté unido al lobby.
    /// </summary>
    private async Task<bool> EnsureRunnerAndLobby()
    {
        // Aseguro referencia al runner del mismo GameObject
        if (_runner == null)
            _runner = GetComponent<NetworkRunner>();

        if (_runner == null)
        {
            Debug.LogError("[NetworkRunnerHandler] No hay NetworkRunner en el GameObject.");
            return false;
        }

        // Registrar callbacks una sola vez
        if (!_runner.IsRunning)
        {
            _runner.AddCallbacks(this);

            if (_spawnNetworkPlayer != null)
                _runner.AddCallbacks(_spawnNetworkPlayer);

            var result = await _runner.JoinSessionLobby(SessionLobby.Custom, LOBBY_NAME);

            if (!result.Ok)
            {
                Debug.LogError($"[NetworkRunnerHandler] Unable to join lobby {LOBBY_NAME}: {result.ShutdownReason}");
                return false;
            }

            Debug.Log($"[NetworkRunnerHandler] Joined lobby {LOBBY_NAME}");
        }

        return true;
    }

    private async Task InitializeGame(GameMode mode, string sessionName, int sceneIndex)
    {
        if (_runner == null)
        {
            Debug.LogError("[NetworkRunnerHandler] Runner es null en InitializeGame.");
            return;
        }

        _runner.ProvideInput = true;

        var startArgs = new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = sceneIndex,       // Fusion 1.x usa int
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

    // ================== CALLBACKS RUNNER ==================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[NetworkRunnerHandler] Player joined: {player}");
        EventManager.TriggerEvent("PlayerJoined", player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[NetworkRunnerHandler] Player left: {player}");
        EventManager.TriggerEvent("PlayerLeft", player);
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
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        ((INetworkRunnerCallbacks)_spawnNetworkPlayer).OnDisconnectedFromServer(runner);
    }
}
