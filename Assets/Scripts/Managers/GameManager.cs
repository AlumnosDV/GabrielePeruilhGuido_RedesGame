using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using System;

namespace RedesGame.Managers
{
    public class GameManager : NetworkBehaviour
    {

        [SerializeField] private int _maxPlayersPerGame = 10;
        [SerializeField] private int _minPlayersPerGame = 2;


        [Networked]
        private int _playersInGame { get; set; }
        [Networked]
        private float Timer { get; set; }


        public override void Spawned()
        {
            ScreenManager.Instance.Deactivate();
            EventManager.StartListening("PlayerJoined", OnPlayerJoined);
        }

        private void OnPlayerJoined(object[] obj)
        {

            _playersInGame++;

            if (_playersInGame >= _minPlayersPerGame)
                EventManager.TriggerEvent("AllPlayersInGame");
        }


        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority)
                Timer += Runner.DeltaTime;

            EventManager.TriggerEvent("UpdateTimer", FormatDate(Timer));
        }

        private string FormatDate(float myTime)
        {
            int hours = Mathf.FloorToInt(myTime / 3600);
            int minutes = Mathf.FloorToInt((myTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(myTime % 60);

            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }
}
