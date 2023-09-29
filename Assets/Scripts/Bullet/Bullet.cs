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


        public override void Spawned()
        {
            base.Spawned();
            _myRigidBody = GetComponent<NetworkRigidbody2D>();
            _trailRenderer = GetComponent<TrailRenderer>();
            //ActiveTrailRenderer(false);
        }

        private void DestroyBullet()
        {
            //ActiveTrailRenderer(false);
            //if (MyBulletPool == null)
            //    gameObject.SetActive(false);
            //else
            //    MyBulletPool.ReturnObject(this);

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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_shooter == collision.gameObject) return;
            if (!Object && !Object.HasStateAuthority) return;
            collision.gameObject.GetComponent<IDamageable>()?.TakeForceDamage(_bulletData.ForceDamage, _myRigidBody.Rigidbody.velocity.normalized);

            Runner.Despawn(Object);
        }
    }
}
