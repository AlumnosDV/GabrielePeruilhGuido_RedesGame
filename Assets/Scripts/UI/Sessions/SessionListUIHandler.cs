using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

namespace RedesGame.UI.Sessions
{
    public class SessionListUIHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private GameObject _sessionItemListPrefab;
        [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;

        public void ClearList()
        {
            foreach (Transform child in _verticalLayoutGroup.transform)
            {
                Destroy(child.gameObject);
            }
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

        }

        public void OnNoSessionFound()
        {
            _statusText.text = "No Games Founded";
            _statusText.gameObject.SetActive(true);
        }

        public void OnLookingForSessions()
        {
            _statusText.text = "Looking for Game Sessions";
            _statusText.gameObject.SetActive(true);
        }
    }
}
