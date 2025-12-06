using System;
using Fusion;
using UnityEngine;

namespace RedesGame.Player
{
    public class NetworkPlayer : NetworkBehaviour
    {
        // Referencia al jugador local
        public static NetworkPlayer Local { get; private set; }

        // Evento local (no se sincroniza en red) para que otros scripts reaccionen
        public event Action<NetworkPlayer> NickNameChanged;

        [Networked(OnChanged = nameof(OnNickNameChanged))]
        public NetworkString<_16> NickName { get; private set; }

        public bool IsLocal => Object != null && Object.HasInputAuthority;
        public bool IsStateAuthority => Object != null && Object.HasStateAuthority;

        public override void Spawned()
        {
            if (IsLocal)
            {
                Local = this;

                var nick = PlayerPrefs.GetString(
                    "PlayerNickName",
                    $"Player_{UnityEngine.Random.Range(100000000, 999999999)}"
                );

                if (string.IsNullOrWhiteSpace(nick))
                    nick = $"Player_{UnityEngine.Random.Range(100000000, 999999999)}";

                if (nick.Length > 16)
                    nick = nick.Substring(0, 16);

                RPC_SetNickName(nick);
            }

            // El nombre del GameObject es solo una ayuda de debug
            UpdateGameObjectName();
        }

        // ------- Nickname -------

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SetNickName(string nickName, RpcInfo info = default)
        {
            NickName = nickName;
        }

        private static void OnNickNameChanged(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.HandleNickNameChanged();
        }

        private void HandleNickNameChanged()
        {
            UpdateGameObjectName();
            NickNameChanged?.Invoke(this);
        }

        private void UpdateGameObjectName()
        {
            transform.name = $"{NickName}_ID_{Object.Id}";
        }

        // ------- Ciclo de vida -------

        public void Despawn()
        {
            if (IsStateAuthority && Runner != null && Object != null)
            {
                Runner.Despawn(Object);
            }
            else
            {
                // Opcional: warning de debug
                Debug.LogWarning($"[NetworkPlayer] Intento de Despawn sin StateAuthority en {name}");
            }
        }
    }
}
