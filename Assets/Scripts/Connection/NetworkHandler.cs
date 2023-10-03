using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkRunner))]
[RequireComponent(typeof(NetworkSceneManagerDefault))]
public class NetworkHandler : MonoBehaviour
{
    [SerializeField] private NetworkRunner _runner;

    private void Awake()
    {
        NetworkRunner networkRunnerInScene = FindObjectOfType<NetworkRunner>();

        if (networkRunnerInScene != null)
            _runner = networkRunnerInScene;
    }

    /*
    void Start()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            var clientTask = InitializeGame(GameMode.Shared, SceneManager.GetActiveScene().buildIndex, "TestSession");
        }
    }
    */

    Task InitializeGame(GameMode gameMode, SceneRef sceneToLoad, string sessionName)
    {
        var sceneManager = GetComponent<NetworkSceneManagerDefault>();

        _runner.ProvideInput = true;
        
        return _runner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            Scene = sceneToLoad,
            SessionName = sessionName,
            CustomLobbyName = "OurLobbyId",
            SceneManager = sceneManager
        });
    }

    public void CreateGame(string sessionName, string sceneName)
    {
        //GameMode.Shared
        var clientTask = InitializeGame(GameMode.Shared, SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}"), sessionName);
    }

    public void JoinGame(SessionInfo sessionInfo)
    {
        var clientTask = InitializeGame(GameMode.Shared, SceneManager.GetActiveScene().buildIndex, sessionInfo.Name);
    }

    public void OnJoinLobby()
    {
        var clienteTask = JoinLobby();
    }

    async Task JoinLobby()
    {
        string lobbyId = "OurLobbyId";
        //GameMode.Shared
        var result = await _runner.JoinSessionLobby(SessionLobby.Shared, lobbyId);
    }


}
