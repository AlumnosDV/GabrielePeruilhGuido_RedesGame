using System;
using UnityEngine.SceneManagement;

namespace RedesGame.Managers
{
    public class ScreenManager : Singleton<ScreenManager>
    {
        private event Action Activated;
        private event Action Deactivated;

        public void Subscribe(IActivable activatable)
        {
            Activated += activatable.Activate;
            Deactivated += activatable.Deactivate;
        }

        public void Unsubscribe(IActivable activatable)
        {
            Activated -= activatable.Activate;
            Deactivated -= activatable.Deactivate;
        }

        public int GetActiveBuildIndexScene()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }

        public void Deactivate()
        {
            Deactivated?.Invoke();
        }

        public void Activate()
        {
            Activated?.Invoke();
        }
    }
}