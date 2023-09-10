using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkRunner))]
[RequireComponent(typeof(NetworkSceneManagerDefault))]
public class NetworkHandler : MonoBehaviour
{
    private NetworkRunner _runner;
    
    void Start()
    {
        _runner = GetComponent<NetworkRunner>();

        var clientTask = InitializeGame(GameMode.Shared, SceneManager.GetActiveScene().buildIndex);
    }

    Task InitializeGame(GameMode gameMode, SceneRef sceneToLoad)
    {
        var sceneManager = GetComponent<NetworkSceneManagerDefault>();

        _runner.ProvideInput = true;
        
        return _runner.StartGame(new StartGameArgs()
        {
            GameMode = gameMode,
            Scene = sceneToLoad,
            SessionName = "SessionName",
            SceneManager = sceneManager
        });
    }

}
