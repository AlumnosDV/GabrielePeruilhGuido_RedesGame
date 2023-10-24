using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

namespace RedesGame.Managers
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private int _minPlayersPerGame = 2;

        [Networked]
        private int PlayersInGame { get; set; }
        [Networked]
        private float Timer { get; set; }

        [Networked(OnChanged = nameof(OnAllPlayersLeft))]
        private bool AllPlayersLeft { get; set; }


        public override void Spawned()
        {
            ScreenManager.Instance.Deactivate();
            EventManager.StartListening("PlayerJoined", OnPlayerJoined);
            EventManager.StartListening("GoToMainMenu", DespawnPlayers);
        }
        static void OnAllPlayersLeft(Changed<GameManager> changed)
        {
            var behaviour = changed.Behaviour;
            SceneManager.LoadScene("MainMenu");
        }

        private void DespawnPlayers(object[] obj)
        {
            AllPlayersLeft = true;
        }

        private void OnPlayerJoined(object[] obj)
        {

            PlayersInGame++;

            if (PlayersInGame >= _minPlayersPerGame)
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
