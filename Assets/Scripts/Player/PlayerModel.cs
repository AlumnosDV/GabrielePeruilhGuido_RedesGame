using Fusion;
using RedesGame.Bullets;
using RedesGame.Damageables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedesGame.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerModel : NetworkBehaviour, IDamageable
    {
        [SerializeField] private NetworkMecanimAnimator _netWorkAnimator;
        [SerializeField] private NetworkRigidbody2D _networkRigidbody;

        [SerializeField] private BulletPool _bulletPool;
        [SerializeField] private GameObject _firePoint;

        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _jumpForce;

        [Networked(OnChanged = nameof(OnFiringChanged))]
        private bool _isFiring { get; set; }

        private NetworkInputData _inputs;

        private bool _isJumping = false;
        private float _moveHorizontal;
        private int _currentSign, _previousSign;


        void Start()
        {
            transform.right = Vector2.right;
            _bulletPool = FindObjectOfType<BulletPool>();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out _inputs))
            {
                if (_inputs.isFirePressed)
                {
                    Shoot();
                }

                if (_inputs.isJumpPressed && !_isJumping)
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
                _networkRigidbody.Rigidbody.AddForce(new Vector2(xAxis * _moveSpeed, 0f), ForceMode2D.Impulse);

                _currentSign = (int)Mathf.Sign(xAxis);

                if (_currentSign != _previousSign)
                {
                    _previousSign = _currentSign;

                    transform.right = Vector2.right * _currentSign;
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
            _networkRigidbody.Rigidbody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }

        static void OnFiringChanged(Changed<PlayerModel> changed)
        {
            var updatedFiring = changed.Behaviour._isFiring;
            changed.LoadOld();
            var oldFiring = changed.Behaviour._isFiring;

        }

        void Shoot()
        {
            var bulletObject = _bulletPool.GetObject();
            if (bulletObject.TryGetComponent(out Bullet bullet))
            {
                //Debug.Log(bulletObject.name);
                bullet.MyBulletPool = _bulletPool;
                bullet.transform.position = _firePoint.transform.position;
                bullet.transform.up = Vector2.right;
                //Runner.Spawn(bulletObject, bullet.transform.position, bullet.transform.rotation);
                bullet.Launch(transform.right);
            }
        }


        public void TakeDamage(float dmg)
        {
            Debug.Log("Agregar el impulso hacia atras");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Floor"))
                _isJumping = false;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Floor"))
                _isJumping = true;
        }
    }
}