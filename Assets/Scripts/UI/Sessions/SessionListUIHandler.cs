using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

namespace RedesGame.UI.Sessions
{
    public class SessionListUIHandler : MonoBehaviour
    {
        [SerializeField] private NetworkRunnerHandler _networkHandler;
        [SerializeField] private MenuPrincipalUI _menuPrincipalUI;
        [Header("Canvas Items")]
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private GameObject _sessionItemListPrefab;
        [SerializeField] private GameObject _createSessionButton;
        [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;

        private void Awake()
        {
            ClearList();
            _createSessionButton.SetActive(false);
        }

        public void ClearList()
        {
            foreach (Transform child in _verticalLayoutGroup.transform)
            {
                Destroy(child.gameObject);
            }
            if (_statusText == null) return;
                _statusText.gameObject.SetActive(false);
        }

        public void AddToList(SessionInfo sessionInfo)
        {
            SessionInfoListUIItem addedSessionInfoListUIItem = Instantiate(_sessionItemListPrefab, _verticalLayoutGroup.transform).GetComponent<SessionInfoListUIItem>();

            addedSessionInfoListUIItem.SetInfomartion(sessionInfo);

            addedSessionInfoListUIItem.OnJoinSession += AddedSessionInfoListUIItem_OnJoinSession;
        }

        private void AddedSessionInfoListUIItem_OnJoinSession(SessionInfo sessionInfo)
        {
            _networkHandler.JoinGame(sessionInfo);
            _menuPrincipalUI.GoToJoiningSessionScreen();
        }

        public void ActiveCreateGameOption()
        {
            _createSessionButton.SetActive(true);
        }

        public void OnNoSessionFound()
        {
            ClearList();
            _statusText.text = "No Games Founded";
            _statusText.gameObject.SetActive(true);
        }

        public void OnLookingForSessions()
        {
            ClearList();
            _statusText.text = "Looking for Game Sessions";
            _statusText.gameObject.SetActive(true);
        }
    }
}
