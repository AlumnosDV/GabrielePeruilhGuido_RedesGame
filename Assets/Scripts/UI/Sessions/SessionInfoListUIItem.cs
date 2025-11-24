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
            _playerCountText.text = $"{sessionInfo.PlayerCount}/{sessionInfo.MaxPlayers}";

            _joinButton.gameObject.SetActive(sessionInfo.PlayerCount >= sessionInfo.MaxPlayers);
        }

        public void OnClick()
        {
            OnJoinSession?.Invoke(sessionInfo);
        }
    }
}
