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

        private NetworkRigidbody2D _myRigidBody;
        private TrailRenderer _trailRenderer;
        private PlayerModel _shooter;
        private bool _canHandleCollisions;

        public override void Spawned()
        {
            _myRigidBody = GetComponent<NetworkRigidbody2D>();
            _trailRenderer = GetComponent<TrailRenderer>();
            _canHandleCollisions = false;

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
        }

        private void DestroyBullet()
        {
            if (Object != null && Object.IsValid)
                Runner.Despawn(Object);
        }

        /// <summary>
        /// Lanza la bala en una dirección con un dueño (player que la disparó).
        /// </summary>
        public void Launch(Vector2 direction, PlayerModel shooter)
        {
            _shooter = shooter;

            if (_myRigidBody == null)
                _myRigidBody = GetComponent<NetworkRigidbody2D>();

            _myRigidBody.Rigidbody.velocity = Vector2.zero;
            _myRigidBody.Rigidbody.AddForce(direction.normalized * _bulletData.Speed, ForceMode2D.Impulse);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_canHandleCollisions) return;
            if (!Object || !Object.HasStateAuthority) return;

            // Evitar golpear al dueño
            if (_shooter != null)
            {
                var shooterRoot = _shooter.transform;
                if (collision.transform.IsChildOf(shooterRoot))
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
