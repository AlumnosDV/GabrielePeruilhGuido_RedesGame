using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedesGame.Player
{
    public class FloorChecker : MonoBehaviour
    {
        [SerializeField] private PlayerModel _myPlayer;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Floor"))
                _myPlayer.IsJumping = false;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Floor"))
                _myPlayer.IsJumping = true;
        }
    }
}
