using RedesGame.Damageables;
using RedesGame.SO;
using Fusion;
using UnityEngine;

namespace RedesGame.Bullets
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : NetworkBehaviour
    {
        [SerializeField] private BulletDataSO _bulletData;
        public BulletPool MyBulletPool { set; protected get; }
        private NetworkRigidbody2D _myRigidBody;
        private TrailRenderer _trailRenderer;
        private GameObject _shooter;
        private int _spawnTick;


        public override void Spawned()
        {
            base.Spawned();
            _myRigidBody = GetComponent<NetworkRigidbody2D>();
            _trailRenderer = GetComponent<TrailRenderer>();
            _spawnTick = Runner.Simulation.Tick;
        }

        private void DestroyBullet()
        {
            Runner.Despawn(Object);
        }

        public void Launch(Vector2 direction, GameObject shooter)
        {
            ActiveTrailRenderer(true);
            _myRigidBody.Rigidbody.AddForce(direction.normalized * _bulletData.Speed, ForceMode2D.Impulse);
            _shooter = shooter;
        }

        private void ActiveTrailRenderer(bool active)
        {
            _trailRenderer.enabled = active;
        }

        private bool CanHandleCollision()
        {
            if (!Runner || !Runner.IsRunning) return false;
            if (Runner.Simulation.Tick <= _spawnTick) return false;
            return Object && Object.HasStateAuthority;
        }

        private void HandleCollision(Collider2D collision)
        {
            if (_shooter == collision.gameObject) return;
            if (!CanHandleCollision()) return;

            collision.gameObject.GetComponent<IDamageable>()?.TakeForceDamage(_bulletData.ForceDamage, _myRigidBody.Rigidbody.velocity.normalized);

            DestroyBullet();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandleCollision(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            HandleCollision(collision);
        }
    }
}
