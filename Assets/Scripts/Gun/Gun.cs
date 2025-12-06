using RedesGame.Bullets;
using UnityEngine;
using RedesGame.Player;

namespace RedesGame.Guns
{
    public class Gun : MonoBehaviour
    {
        public Bullet BulletPrefab;
        public GameObject FirePoint;
        public Sprite GunSprite;

        private Transform _targetTransform;
        private PlayerModel _owner;

        public void Shoot(Bullet bullet)
        {
            // Dirección hacia donde mira el arma
            Vector2 dir = transform.right;
            bullet.transform.position = FirePoint.transform.position;
            bullet.transform.up = dir;
            bullet.Launch(dir, _owner);
        }

        public Gun SetTarget(PlayerModel player)
        {
            _owner = player;
            _targetTransform = player.PlayerBody.transform;
            transform.SetParent(player.PlayerBody.transform);
            SetLayer("Gun");

            return this;
        }

        public void UpdatePosition()
        {
            if (_targetTransform == null)
                return;

            transform.position = _targetTransform.position;
            transform.right = _targetTransform.right;
        }

        public void SetLayer(string layerName)
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }
}
