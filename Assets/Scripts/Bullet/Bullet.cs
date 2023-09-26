using RedesGame.Damageables;
using RedesGame.SO;
using Fusion;
using System.Collections;
using System.Collections.Generic;
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


        public override void Spawned()
        {
            base.Spawned();
            _myRigidBody = GetComponent<NetworkRigidbody2D>();
            _trailRenderer = GetComponent<TrailRenderer>();
            ActiveTrailRenderer(false);
        }

        private void DestroyBullet()
        {
            ActiveTrailRenderer(false);
            if (MyBulletPool == null)
                gameObject.SetActive(false);
            else
                MyBulletPool.ReturnObject(this);

            //Runner.Despawn(Object);
        }

        public void Launch(Vector2 direction)
        {
            ActiveTrailRenderer(true);
            _myRigidBody.Rigidbody.velocity = direction.normalized * _bulletData.Speed;
        }

        private void ActiveTrailRenderer(bool active)
        {
            _trailRenderer.enabled = active;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(_bulletData.Damage);
            DestroyBullet();
        }
    }
}
