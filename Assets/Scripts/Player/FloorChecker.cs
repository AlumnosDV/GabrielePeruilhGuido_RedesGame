using UnityEngine;

namespace RedesGame.Player
{
    public class FloorChecker : MonoBehaviour
    {
        private PlayerController _controller;

        private void Awake()
        {
            _controller = GetComponentInParent<PlayerController>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Floor"))
                _controller.SetGrounded(true);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Floor"))
                _controller.SetGrounded(false);
        }
    }
}
