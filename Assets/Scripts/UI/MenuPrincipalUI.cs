using RedesGame.UI.Commands;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using RedesGame.UI.Sessions;

namespace RedesGame.UI
{
    public class MenuPrincipalUI : MonoBehaviour
    {
        [SerializeField] private NetworkRunnerHandler _networkHandler;
        [SerializeField] private SessionListUIHandler _sessionListUIHandler;

        [Header("Player Settings")]
        [SerializeField] private TMP_InputField _nickNameInput;

        [Header("Session Settings")]
        [SerializeField] private TMP_InputField _sessionName;

        [Header("Menus")]
        [SerializeField] private GameObject _nickNameScreen;
        [SerializeField] private GameObject _creditsScreen;
        [SerializeField] private GameObject _controlsScreen;
        [SerializeField] private GameObject _sessionsScreen;
        [SerializeField] private GameObject _createSessionScreen;
        [SerializeField] private GameObject _mainMenuButtons;
        [SerializeField] private GameObject _backButton;
        [SerializeField] private GameObject _onJoiningSessionScreen;

        private Stack<ICommand> commandStack = new Stack<ICommand>();

        [ContextMenu("Default Awake")]
        private void Awake()
        {
            Debug.Log("Awake Menu Principal UI");
            bool hasStoredNick = PlayerPrefs.HasKey("PlayerNickName");

            _mainMenuButtons.SetActive(hasStoredNick);
            _creditsScreen.SetActive(false);
            _controlsScreen.SetActive(false);
            _backButton.SetActive(false);
            _nickNameScreen.SetActive(!hasStoredNick);
            _sessionsScreen.SetActive(false);
            _createSessionScreen.SetActive(false);
            _onJoiningSessionScreen.SetActive(false);
        }

        private void Start()
        {
            if (PlayerPrefs.HasKey("PlayerNickName"))
                _nickNameInput.text = PlayerPrefs.GetString("PlayerNickName");
            else
                _nickNameInput.text = $"Player_{Random.Range(100000000, 999999999)}";
        }

        public void Play(string sceneName)
        {
            if (_networkHandler == null)
            {
                Debug.LogError("[MenuPrincipalUI] No hay NetworkRunnerHandler para crear la partida.");
                return;
            }

            var sessionName = string.IsNullOrWhiteSpace(_sessionName.text)
                ? "Session_" + Random.Range(1000, 9999)
                : _sessionName.text;

            _networkHandler.CreateGame(sessionName, sceneName);
        }

        public void GoToControls()
        {
            ExecuteCommand(new ChangeMenuCommand(
                new[] { _controlsScreen, _backButton }, new[] { _mainMenuButtons }));
        }

        public void GoToCredits()
        {
            ExecuteCommand(new ChangeMenuCommand(
                new[] { _creditsScreen, _backButton }, new[] { _mainMenuButtons }));
        }

        public void GoToNickNameScreen()
        {
            ExecuteCommand(new ChangeMenuCommand(
                new[] { _nickNameScreen, _backButton }, new[] { _mainMenuButtons }));
        }

        public void GoToSessionsList()
        {
            ExecuteCommand(new ChangeMenuCommand(
                new[] { _sessionsScreen, _backButton }, new[] { _mainMenuButtons }));

            RefreshSessionsList();
        }

        public void RefreshSessionsList()
        {
            _sessionListUIHandler.OnLookingForSessions();

            if (_networkHandler != null)
                _networkHandler.OnJoinLobby();
            else
                Debug.LogError("[MenuPrincipalUI] No hay NetworkRunnerHandler para buscar sesiones.");
        }

        public void GoToCreateSession()
        {
            ExecuteCommand(new ChangeMenuCommand(
                new[] { _createSessionScreen }, new[] { _sessionsScreen }));
        }

        public void GoToJoiningSessionScreen()
        {
            ExecuteCommand(new ChangeMenuCommand(
                new[] { _onJoiningSessionScreen }, new[] { _sessionsScreen, _backButton }));
        }

        public void GoBack()
        {
            UndoLastCommand();
        }

        public void SaveNickName()
        {
            PlayerPrefs.SetString("PlayerNickName", _nickNameInput.text);
            PlayerPrefs.Save();
            ExecuteCommand(new ChangeMenuCommand(
                new[] { _mainMenuButtons }, new[] { _nickNameScreen }));
        }

        public void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }

        private void ExecuteCommand(ICommand command)
        {
            command.Execute();
            commandStack.Push(command);
        }

        private void UndoLastCommand()
        {
            if (commandStack.Count > 0)
            {
                ICommand lastCommand = commandStack.Pop();
                lastCommand.Undo();
            }
        }
    }
}
