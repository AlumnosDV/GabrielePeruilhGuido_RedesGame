using UnityEngine;

namespace RedesGame.Player
{
    public class CharacterInputHandler : MonoBehaviour
    {
        private NetworkInputData _inputData;

        private bool _jumpPressed;
        private bool _firePressed;
        private bool _fallThroughPressed;

        private void Awake()
        {
            _inputData = new NetworkInputData();
        }

        private void Update()
        {
            // Movimiento
            _inputData.Horizontal = Input.GetAxisRaw("Horizontal");

            // Jump (W)
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                _jumpPressed = true;
            }

            // Fire (Space)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _firePressed = true;
            }

            // FallThrough (S)
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                _fallThroughPressed = true;
            }
        }

        public NetworkInputData GetLocalInputs()
        {
            // Dump de flags a NetworkButtons
            _inputData.Buttons.Set(MyButtons.Jump, _jumpPressed);
            _inputData.Buttons.Set(MyButtons.Fire, _firePressed);
            _inputData.Buttons.Set(MyButtons.FallThrough, _fallThroughPressed);

            // Reset
            _jumpPressed = false;
            _firePressed = false;
            _fallThroughPressed = false;

            return _inputData;
        }
    }
}
