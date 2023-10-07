using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedesGame.Player
{
    public class NetworkCharacterController : MonoBehaviour
    {
        private NetworkInputData _networkInput;

        private bool _isFirePressed;
        private bool _isJumpPressed;
        private bool _isChangeWeapon;

        private void Start()
        {
            _networkInput = new NetworkInputData();
        }

        void Update()
        {
            _networkInput.xMovement = Input.GetAxis("Horizontal");
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _isFirePressed = true;
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                _isJumpPressed = true;
            }
        }

        public NetworkInputData GetLocalInputs()
        {
            _networkInput.isFirePressed = _isFirePressed;
            _isFirePressed = false;

            _networkInput.isJumpPressed = _isJumpPressed;
            _isJumpPressed = false;

            return _networkInput;
        }
    }
}
