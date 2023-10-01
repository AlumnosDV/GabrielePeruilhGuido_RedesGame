using UnityEngine;

namespace RedesGame.UI.Commands
{
    public class ChangeMenuCommand : ICommand
    {
        private readonly GameObject[] _toShow;
        private readonly GameObject[] _toHide;

        public ChangeMenuCommand(GameObject[] toShow, GameObject[] toHide = null)
        {
            _toShow = toShow;
            _toHide = toHide;
        }

        public virtual void Execute()
        {
            foreach (var gameObject in _toHide)
            {
                gameObject.SetActive(false);
            }

            foreach (var gameObject in _toShow)
            {
                gameObject.SetActive(true);
            }
        }

        public virtual void Undo()
        {
            foreach (var gameObject in _toShow)
            {
                gameObject.SetActive(false);
            }

            foreach (var gameObject in _toHide)
            {
                gameObject.SetActive(true);
            }
        }
    }
}