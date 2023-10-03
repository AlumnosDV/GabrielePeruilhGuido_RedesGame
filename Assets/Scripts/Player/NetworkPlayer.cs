using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using System;
using RedesGame.Managers;

namespace RedesGame.Player
{
    public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
    {
        public TextMeshProUGUI PlayerNickNameTM;
        public static NetworkPlayer Local { get; private set; }

        [Networked(OnChanged = nameof(OnNickNameChanged))]
        public NetworkString<_16> NickName { get; set; }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                Local = this;
                RPC_SetNickName(PlayerPrefs.GetString("PlayerNickName"));
            }

            transform.name = $"{NickName}_ID_{Object.Id}";
            EventManager.TriggerEvent("PlayerJoined");
        }

        static void OnNickNameChanged(Changed<NetworkPlayer> changed)
        {
            changed.Behaviour.OnNickNameChanged();
        }

        private void OnNickNameChanged()
        {
            PlayerNickNameTM.text = NickName.ToString();
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_SetNickName(string nickName, RpcInfo info = default)
        {
            this.NickName = nickName;
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (player == Object.InputAuthority)
                Runner.Despawn(Object);
        }
    }
}
