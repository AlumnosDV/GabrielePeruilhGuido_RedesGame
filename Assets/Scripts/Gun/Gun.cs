using RedesGame.Bullets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace RedesGame.Guns
{
    public class Gun : NetworkBehaviour
    {
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private GameObject _firePoint;


        public void Shoot()
        {
            var bullet = Runner.Spawn(_bulletPrefab, _firePoint.transform.position);
            bullet.transform.up = transform.right;
            bullet.Launch(transform.right, gameObject);
        }

    }
}
