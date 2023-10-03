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
        [SerializeField] private NetworkHandler _networkHandler;
        [SerializeField] private SessionListUIHandler _sessionListUIHandler;

        [Header("Player Settings")]
        [SerializeField] private TMP_InputField _nickNameInput;

        [Header("Player Settings")]
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
            _mainMenuButtons.SetActive(false);
            _creditsScreen.SetActive(false);
            _controlsScreen.SetActive(false);
            _backButton.SetActive(false);
            _nickNameScreen.SetActive(true);
            _sessionsScreen.SetActive(false);
            _createSessionScreen.SetActive(false);
            _onJoiningSessionScreen.SetActive(false);
            _nickNameInput.gameObject.SetActive(true);
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
            _networkHandler.CreateGame(_sessionName.text, sceneName);
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
                new[] { _nickNameScreen }, new[] { _mainMenuButtons }));
        }

        public void GoToSessionsList()
        {
            ExecuteCommand(new ChangeMenuCommand(
                new[] { _sessionsScreen, _backButton }, new[] { _mainMenuButtons }));
            _sessionListUIHandler.OnLookingForSessions();
            _networkHandler.OnJoinLobby();
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