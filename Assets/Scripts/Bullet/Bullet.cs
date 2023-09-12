using RedesGame.Damageables;
using RedesGame.SO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedesGame.Bullets
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private BulletDataSO _bulletData;
        public BulletPool MyBulletPool { set; protected get; }
        private Rigidbody2D _myRigidBody;
        private TrailRenderer _trailRenderer;


        private void Awake()
        {
            _myRigidBody = GetComponent<Rigidbody2D>();
            _trailRenderer = GetComponent<TrailRenderer>();
            ActiveTrailRenderer(false);
        }

        private void DestroyBullet()
        {
            ActiveTrailRenderer(false);
            if (MyBulletPool == null)
                gameObject.SetActive(false);
            else
                MyBulletPool.ReturnObject(gameObject);
        }

        public void Launch(Vector2 direction)
        {
            ActiveTrailRenderer(true);
            _myRigidBody.velocity = direction.normalized * _bulletData.Speed;
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
