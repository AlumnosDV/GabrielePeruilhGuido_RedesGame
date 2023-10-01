using Fusion;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RedesGame.UI.Sessions
{
    public class SessionInfoListUIItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _sessionNameText;
        [SerializeField] private TextMeshProUGUI _playerCountText;
        [SerializeField] private Button _joinButton;

        private SessionInfo sessionInfo;

        public event Action<SessionInfo> OnJoinSession;

        public void SetInfomartion(SessionInfo sessionInfo)
        {
            this.sessionInfo = sessionInfo;

            _sessionNameText.text = sessionInfo.Name;
            _playerCountText.text = $"{sessionInfo.PlayerCount.ToString()}/{sessionInfo.MaxPlayers.ToString()}";

            bool isJoinButtonActivated = true;

            if (sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
                isJoinButtonActivated = false;

            _joinButton.gameObject.SetActive(isJoinButtonActivated);
        }

        public void OnClick()
        {
            OnJoinSession?.Invoke(sessionInfo);
        }
    }
}
