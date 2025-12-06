using System;
using Fusion;
using RedesGame.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                if (Object.HasInputAuthority)
                {
                    Local = this;
                    RPC_SetNickName(PlayerPrefs.GetString("PlayerNickName"));
                }

                // Nombre provisorio SIN usar NickName
                transform.name = $"Player_ID_{Object.Id}";
                EventManager.TriggerEvent("PlayerJoined", Object.InputAuthority);
            }
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
