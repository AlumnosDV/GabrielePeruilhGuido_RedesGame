using UnityEngine;

namespace RedesGame.Player
{
    public class LocalPlayerInitializer : MonoBehaviour
    {
        [SerializeField] private NetworkPlayer _networkPlayer;

        private void Awake()
        {
            if (_networkPlayer == null)
                _networkPlayer = GetComponent<NetworkPlayer>();
        }

        private void Start()
        {
            if (_networkPlayer == null)
                return;

            // Solo el jugador local debe enviar su propio nick
            if (!_networkPlayer.IsLocal)
                return;

            var nick = PlayerPrefs.GetString("PlayerNickName", "Player");
            _networkPlayer.RPC_SetNickName(nick);
        }
    }
}
