using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedesGame.Bullets
{
    public class BulletPool : ObjectPool
    {
        [SerializeField] private GameObject _bulletPrefab;
        protected override GameObject IntantiateObject()
        {
            return Instantiate(_bulletPrefab);
        }
    }
}
