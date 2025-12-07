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
            if (bullet == null || FirePoint == null)
                return;

            bool facingRight = _owner != null ? _owner.IsFacingRight : transform.lossyScale.x >= 0f;
            Vector2 dir = facingRight ? Vector2.right : Vector2.left;
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
            bool facingRight = _owner != null ? _owner.IsFacingRight : _targetTransform.lossyScale.x >= 0f;
            transform.right = facingRight ? Vector2.right : Vector2.left;
            
            var scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
            transform.localScale = scale;
        }

        public void SetLayer(string layerName)
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }
}
