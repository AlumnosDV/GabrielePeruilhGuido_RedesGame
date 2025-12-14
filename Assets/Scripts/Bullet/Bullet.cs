using RedesGame.Damageables;
using RedesGame.SO;
using Fusion;
using UnityEngine;
using RedesGame.Player;

namespace RedesGame.Bullets
{
    [RequireComponent(typeof(NetworkRigidbody2D))]
    public class Bullet : NetworkBehaviour
    {
        [SerializeField] private BulletDataSO _bulletData;
        [SerializeField, Tooltip("Tiempo antes de destruirse si no impacta")] private float _lifeTime = 5f;
        [Networked] private Vector2 LaunchDirection { get; set; }
        [Networked] private PlayerRef ShooterRef { get; set; }
        [Networked] private float ElapsedLife { get; set; }

        private NetworkRigidbody2D _myRigidBody;
        private TrailRenderer _trailRenderer;
        private PlayerModel _shooter;
        private Transform _shooterRoot;
        private bool _canHandleCollisions;
        private bool _hasAppliedLaunch;

        public override void Spawned()
        {
            _myRigidBody = GetComponent<NetworkRigidbody2D>();
            _trailRenderer = GetComponent<TrailRenderer>();
            _canHandleCollisions = false;
            _hasAppliedLaunch = false;
            ElapsedLife = 0f;

            if (ShooterRef.IsValid && Runner.TryGetPlayerObject(ShooterRef, out var shooterObj))
            {
                _shooterRoot = shooterObj.transform;
            }

            if (_trailRenderer != null)
                _trailRenderer.enabled = false;
        }

        public override void FixedUpdateNetwork()
        {
            // Esperamos 1 tick de simulación antes de procesar colisiones
            if (!_canHandleCollisions)
            {
                _canHandleCollisions = true;
                if (_trailRenderer != null)
                    _trailRenderer.enabled = true;
            }

            if (!_hasAppliedLaunch && LaunchDirection != Vector2.zero)
            {
                ApplyLaunch(LaunchDirection);
            }

            if (Object.HasStateAuthority && _lifeTime > 0f)
            {
                ElapsedLife += Runner.DeltaTime;
                if (ElapsedLife >= _lifeTime)
                {
                    DestroyBullet();
                }
            }
        }

        private void DestroyBullet()
        {
            if (Object != null && Object.IsValid)
                Runner.Despawn(Object);
        }

        public void Launch(Vector2 direction, PlayerModel shooter)
        {
            _shooterRoot = shooter != null ? shooter.transform : null;

            if (Object != null && Object.HasStateAuthority)
            {
                LaunchDirection = direction;
                ShooterRef = shooter != null && shooter.Object != null ? shooter.Object.InputAuthority : PlayerRef.None;
            }

            if (_myRigidBody == null)
                _myRigidBody = GetComponent<NetworkRigidbody2D>();

            ApplyLaunch(direction);
        }

        private void ApplyLaunch(Vector2 direction)
        {
            if (_myRigidBody == null)
                _myRigidBody = GetComponent<NetworkRigidbody2D>();

            _hasAppliedLaunch = true;
            _myRigidBody.Rigidbody.velocity = Vector2.zero;
            _myRigidBody.Rigidbody.AddForce(direction.normalized * _bulletData.Speed, ForceMode2D.Impulse);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_canHandleCollisions) return;
            if (!Object || !Object.HasStateAuthority) return;

            // Evitar golpear al dueño
            if (_shooterRoot != null)
            {
                if (collision.transform.IsChildOf(_shooterRoot))
                    return;
            }

            var damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 dir = _myRigidBody.Rigidbody.velocity.normalized;
                damageable.TakeForceDamage(_bulletData.ForceDamage, dir);
                // Si en algún caso querés que también quite vida:
                // damageable.TakeLifeDamage();
            }

            DestroyBullet();
        }
    }
}
