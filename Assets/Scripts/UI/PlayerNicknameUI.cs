using TMPro;
using UnityEngine;

namespace RedesGame.Player
{
    public class PlayerNicknameUI : MonoBehaviour
    {
        [SerializeField] private NetworkPlayer _networkPlayer;
        [SerializeField] private TextMeshProUGUI _nicknameLabel;

        private void Awake()
        {
            if (_networkPlayer == null)
                _networkPlayer = GetComponentInParent<NetworkPlayer>();
        }

        private void OnEnable()
        {
            if (_networkPlayer != null)
            {
                _networkPlayer.NickNameChanged += OnNickNameChanged;
                // Refrescar por si ya tenía un valor
                OnNickNameChanged(_networkPlayer);
            }
        }

        private void OnDisable()
        {
            if (_networkPlayer != null)
            {
                _networkPlayer.NickNameChanged -= OnNickNameChanged;
            }
        }

        private void OnNickNameChanged(NetworkPlayer player)
        {
            if (_nicknameLabel != null)
            {
                _nicknameLabel.text = player.NickName.ToString();
            }
        }
    }
}
