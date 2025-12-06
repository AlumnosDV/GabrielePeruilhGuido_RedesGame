using Fusion;
using UnityEngine;

namespace RedesGame.Player
{
    [RequireComponent(typeof(NetworkRigidbody2D))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float fallThroughDuration = 0.3f;

        private NetworkRigidbody2D _rb;
        private Collider2D _collider;
        private PlayerModel _playerModel;

        private bool _fallingThrough;
        private float _fallThroughTimer;
        private bool _isGrounded = true;

        public override void Spawned()
        {
            _rb = GetComponent<NetworkRigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _playerModel = GetComponent<PlayerModel>();
        }

        public override void FixedUpdateNetwork()
        {
            if (_playerModel != null)
            {
                if (!_playerModel.IsActive || _playerModel.IsDead)
                    return;
            }

            if (!GetInput(out NetworkInputData input))
                return;

            var rb = _rb.Rigidbody;
            Vector2 vel = rb.velocity;

            // --- MOVIMIENTO HORIZONTAL ---
            vel.x = input.Horizontal * moveSpeed;

            // --- SALTO ---
            if (input.Buttons.IsSet(MyButtons.Jump) && IsGrounded())
            {
                vel.y = jumpForce;
                SetGrounded(false); 
            }

            // --- FALL THROUGH ---
            if (input.Buttons.IsSet(MyButtons.FallThrough))
            {
                TryFallThrough();
            }

            UpdateFallThrough();

            rb.velocity = vel;

            // --- FIRE ---
            // PlayerModel lo captura también porque leen el mismo input.
        }


        #region  LOGICA FALL-THROUGH
        private void TryFallThrough()
        {
            if (_fallingThrough)
                return;

            _fallingThrough = true;
            _fallThroughTimer = fallThroughDuration;

            Physics2D.IgnoreLayerCollision(
                LayerMask.NameToLayer("Player"),
                LayerMask.NameToLayer("Platform"),
                true
            );
        }

        private void UpdateFallThrough()
        {
            if (!_fallingThrough)
                return;

            _fallThroughTimer -= Runner.DeltaTime;

            if (_fallThroughTimer <= 0)
            {
                _fallingThrough = false;

                Physics2D.IgnoreLayerCollision(
                    LayerMask.NameToLayer("Player"),
                    LayerMask.NameToLayer("Platform"),
                    false
                );
            }
        }
        #endregion

        #region GROUNDED
        public void SetGrounded(bool grounded)
        {
            _isGrounded = grounded;
        }

        private bool IsGrounded()
        {
            return _isGrounded;
        }
        #endregion
    }
}
