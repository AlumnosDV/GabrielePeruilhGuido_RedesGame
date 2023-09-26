using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace RedesGame.Bullets
{
    public class BulletPool : ObjectPool<Bullet>
    {
        [SerializeField] private Bullet _bulletPrefab;
        protected override Bullet IntantiateObject()
        {
            return Runner.Spawn(_bulletPrefab);
        }
    }
}
