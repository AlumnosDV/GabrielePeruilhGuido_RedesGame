using RedesGame.Bullets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using RedesGame.Player;

namespace RedesGame.Guns
{
    public class Gun : MonoBehaviour
    {
        public Bullet BulletPrefab;
        public GameObject FirePoint;
        public Sprite GunSprite;

        private Transform _targetTransform;

        public void Shoot(Bullet bullet)
        {
            bullet.transform.up = transform.right;
            bullet.Launch(transform.right, gameObject);
        }

        public Gun SetTarget(PlayerModel player)
        {
            _targetTransform = player.PlayerBody.transform;
            transform.SetParent(player.PlayerBody.transform);
            SetLayer("Gun");

            return this;
        }

        public void UpdatePosition()
        {
            transform.position = _targetTransform.position;
            transform.right = _targetTransform.right;
        }

        public void SetLayer(string layerName)
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
        }

    }
}
