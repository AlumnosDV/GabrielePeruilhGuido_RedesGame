using Fusion;
using RedesGame.Bullets;
using RedesGame.Damageables;
using System;
using UnityEngine;

namespace RedesGame.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerModel : NetworkBehaviour, IDamageable
    {
        [SerializeField] private NetworkMecanimAnimator _netWorkAnimator;
        [SerializeField] private NetworkRigidbody2D _networkRigidbody2D;

        [SerializeField] private BulletPool _bulletPool;
        [SerializeField] private GameObject _firePoint;
        [SerializeField] private GameObject _canvas;
        [SerializeField] private Bullet _bulletPrefab;


        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _jumpForce;
        [SerializeField] private int _maxLife = 3;
        private Vector3 _initialPosition;
        private Vector2 _initialRight;

        private NetworkInputData _inputs;

        public bool IsJumping = false;
        private float _moveHorizontal;
        private int _currentSign, _previousSign;

        public event Action<float> OnLifeUpdate = delegate { };

        [Networked(OnChanged = nameof(OnFiringChanged))]
        private bool _isFiring { get; set; }

        [Networked(OnChanged = nameof(OnLifeChanged))]
        private int Life { get; set; }

        void Start()
        {
            //_bulletPool = FindObjectOfType<BulletPool>();
            _initialPosition = transform.position;
            _initialRight = transform.right;
            Life = _maxLife;
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out _inputs))
            {
                if (_inputs.isFirePressed)
                {
                    Shoot();
                }

                if (_inputs.isJumpPressed && !IsJumping)
                {
                    Jump();
                }

                Move(_inputs.xMovement);
            }
        }

        void Move(float xAxis)
        {

            if (xAxis != 0)
            {
                _networkRigidbody2D.Rigidbody.AddForce(new Vector2(xAxis * _moveSpeed, 0f), ForceMode2D.Force);

                _currentSign = (int)Mathf.Sign(xAxis);

                if (_currentSign != _previousSign)
                {
                    _previousSign = _currentSign;

                    transform.right = Vector2.right * _currentSign;
                    _canvas.transform.right = Vector2.right;
                }

                _netWorkAnimator.Animator.SetFloat("HorizontalValue", Mathf.Abs(xAxis));
            }
            else if (_currentSign != 0)
            {
                _currentSign = 0;
                _netWorkAnimator.Animator.SetFloat("HorizontalValue", 0);
            }
        }

        void Jump()
        {
            _networkRigidbody2D.Rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }

        static void OnFiringChanged(Changed<PlayerModel> changed)
        {
            var updatedFiring = changed.Behaviour._isFiring;
            changed.LoadOld();
            var oldFiring = changed.Behaviour._isFiring;

        }

        void Shoot()
        {
            var bullet = Runner.Spawn(_bulletPrefab, _firePoint.transform.position);
            bullet.transform.up = transform.right;
            bullet.Launch(transform.right, gameObject);
            //var bulletObject = _bulletPool.GetObject();
            //if (bulletObject.TryGetComponent(out Bullet bullet))
            //{
            //    //Debug.Log(bulletObject.name);
            //    bullet.MyBulletPool = _bulletPool;
            //    bullet.transform.position = _firePoint.transform.position;
            //    bullet.transform.up = Vector2.right;
            //}
        }


        public void TakeForceDamage(float dmg, Vector2 direction)
        {
            _networkRigidbody2D.Rigidbody.AddForce(direction * dmg, ForceMode2D.Force);
        }

        public void TakeLifeDamage()
        {
            RPC_TakeLifeDamage(1);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_TakeLifeDamage(int lostLife)
        {
            Life -= lostLife;
            gameObject.transform.position = _initialPosition;
            if (Life <= 0)
            {
                Dead();
            }
        }

        static void OnLifeChanged(Changed<PlayerModel> changed)
        {
            var behaviour = changed.Behaviour;

            behaviour.OnLifeUpdate(behaviour.Life / 100);
        }

        void Dead()
        {
            Runner.Shutdown();
        }

        private void ResetPlayer()
        {
            transform.position = _initialPosition;
            transform.right = _initialRight;
            Life = _maxLife;
        }

    }
}