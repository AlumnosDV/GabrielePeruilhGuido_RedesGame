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
        private Transform _playerBody;
        private Collider2D _currentPlatformCollider;

        private bool _fallingThrough;
        private float _fallThroughTimer;
        private bool _isGrounded = true;

        [Networked(OnChanged = nameof(OnFacingChanged))]
        private NetworkBool FacingRight { get; set; }

        public override void Spawned()
        {
            _rb = GetComponent<NetworkRigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _playerModel = GetComponent<PlayerModel>();
            _playerBody = _playerModel != null ? _playerModel.PlayerBody.transform : null;

            if (Object.HasStateAuthority && !FacingRight)
            {
                FacingRight = true;
            }

            ApplyFacing(FacingRight);
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
            UpdateFacingDirection(input.Horizontal);

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

            if (!_isGrounded)
                return;

            if (!TryGetPlatformBelow(out var platformCollider))
                return;

            _fallingThrough = true;
            _fallThroughTimer = fallThroughDuration;

            _currentPlatformCollider = platformCollider;
            Physics2D.IgnoreCollision(_collider, _currentPlatformCollider, true);
        }

        private void UpdateFallThrough()
        {
            if (!_fallingThrough)
                return;

            _fallThroughTimer -= Runner.DeltaTime;

            if (_fallThroughTimer <= 0)
            {
                _fallingThrough = false;

                RestorePlatformCollision();
            }
        }

        private bool TryGetPlatformBelow(out Collider2D platformCollider)
        {
            Vector2 origin = _collider.bounds.center;
            float rayDistance = _collider.bounds.extents.y + 0.1f;
            int platformLayerMask = LayerMask.GetMask("Floor");

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayDistance, platformLayerMask);
            if (hit.collider != null)
            {
                platformCollider = hit.collider;
                return true;
            }

            platformCollider = null;
            return false;
        }

        private void RestorePlatformCollision()
        {
            if (_collider != null && _currentPlatformCollider != null)
            {
                Physics2D.IgnoreCollision(_collider, _currentPlatformCollider, false);
            }

            _currentPlatformCollider = null;
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

        private void OnDisable()
        {
            RestorePlatformCollision();
            _fallingThrough = false;
        }

        private void OnDestroy()
        {
            RestorePlatformCollision();
            _fallingThrough = false;
        }

        private void UpdateFacingDirection(float horizontalInput)
        {
            if (Mathf.Approximately(horizontalInput, 0f))
                return;

            bool newFacingRight = horizontalInput > 0f;

            if (FacingRight != newFacingRight)
            {
                FacingRight = newFacingRight;
            }

            ApplyFacing(newFacingRight);
        }

        private void ApplyFacing(bool facingRight)
        {
            if (_playerBody == null)
                return;

            var scale = _playerBody.localScale;
            scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
            _playerBody.localScale = scale;
        }

        static void OnFacingChanged(Changed<PlayerController> changed)
        {
            changed.Behaviour.ApplyFacing(changed.Behaviour.FacingRight);
        }
    }
}
