using RedesGame.Bullets;
using UnityEngine;
using RedesGame.Player;
using System.Collections;

namespace RedesGame.Guns
{
    public class Gun : MonoBehaviour
    {
        public Bullet BulletPrefab;
        public GameObject FirePoint;
        public Sprite GunSprite;

        [Header("Ammo")]
        [SerializeField, Tooltip("-1 for infinite ammo")] private int _ammoCapacity = -1;
        [SerializeField, Tooltip("Time to respawn this gun when it's a pickup")] private float _respawnTime = 5f;

        private Transform _targetTransform;
        private PlayerModel _owner;
        private int _currentAmmo;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        private Vector3 _spawnScale;
        private bool _isRespawning;

        public bool IsPickupGun { get; private set; }
        public bool HasLimitedAmmo => _ammoCapacity >= 0;
        public bool HasAmmo => !HasLimitedAmmo || _currentAmmo > 0;
        public bool IsOutOfAmmo => HasLimitedAmmo && _currentAmmo <= 0;

        private void Awake()
        {
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
            _spawnScale = transform.localScale;

            IsPickupGun = gameObject.layer == LayerMask.NameToLayer("InGameGun");
            ResetAmmo();
        }

        public Vector2 GetDirection()
        {
            bool facingRight = _owner != null ? _owner.IsFacingRight : transform.lossyScale.x >= 0f;
            return facingRight ? Vector2.right : Vector2.left;
        }
        public void Shoot(Bullet bullet)
        {
            if (bullet == null || FirePoint == null)
                return;

            Vector2 dir = GetDirection();
            bullet.transform.position = FirePoint.transform.position;
            bullet.transform.up = dir;
            bullet.Launch(dir, _owner);
        }

        public Gun SetTarget(PlayerModel player)
        {
            _owner = player;
            _targetTransform = player.PlayerBody.transform;
            transform.SetParent(player.PlayerBody.transform);
            SetLayer("Gun");

            if (IsPickupGun)
            {
                ResetAmmo();
                _isRespawning = false;
            }

            return this;
        }

        public void UpdatePosition()
        {
            if (_targetTransform == null)
                return;

            transform.position = _targetTransform.position;
            bool facingRight = _owner != null ? _owner.IsFacingRight : _targetTransform.lossyScale.x >= 0f;
            transform.right = facingRight ? Vector2.right : Vector2.left;
            
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
            transform.localScale = scale;
        }

        public void SetLayer(string layerName)
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
        }

        public void ConsumeAmmo()
        {
            if (!HasLimitedAmmo)
                return;

            _currentAmmo = Mathf.Max(0, _currentAmmo - 1);
        }

        public void ResetAmmo()
        {
            _currentAmmo = _ammoCapacity < 0 ? int.MaxValue : _ammoCapacity;
        }

        public void BeginRespawnCycle()
        {
            if (!IsPickupGun)
            {
                Destroy(gameObject);
                return;
            }

            if (!_isRespawning)
            {
                StartCoroutine(RespawnRoutine());
            }
        }

        private IEnumerator RespawnRoutine()
        {
            _isRespawning = true;
            _owner = null;
            _targetTransform = null;
            transform.SetParent(null);

            ToggleRenderersAndColliders(false);
            SetLayer("Default");

            yield return new WaitForSeconds(_respawnTime);

            transform.position = _spawnPosition;
            transform.rotation = _spawnRotation;
            transform.localScale = _spawnScale;
            ResetAmmo();

            ToggleRenderersAndColliders(true);
            SetLayer("InGameGun");
            _isRespawning = false;

            GunHandler.Instance.RegisterGun(this);
        }

        private void ToggleRenderersAndColliders(bool enabled)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = enabled;
            }

            foreach (var collider in GetComponentsInChildren<Collider2D>(true))
            {
                collider.enabled = enabled;
            }

            foreach (var collider in GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = enabled;
            }
        }
    }
}
