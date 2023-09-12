using RedesGame.Bullets;
using RedesGame.Damageables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedesGame.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerModel : MonoBehaviour, IDamageable
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody2D _myRigidBody;
        [SerializeField] private BulletPool _bulletPool;
        [SerializeField] private GameObject _firePoint;
        [SerializeField] private float _life;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _jumpForce;

        private bool _isJumping = false;
        private float _moveHorizontal;
        private int _currentSign, _previousSign;

        private void Awake()
        {
            _myRigidBody = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            transform.right = Vector2.right;
        }

        void Update()
        {
            _moveHorizontal = Input.GetAxis("Horizontal");
            if (Input.GetKeyDown(KeyCode.W) && !_isJumping)
            {
                Jump();
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Shoot();
            }
        }

        private void FixedUpdate()
        {
            Move();
        }

        void Move()
        {
            if (_moveHorizontal > 0.1f || _moveHorizontal < -0.1f)
            {
                _myRigidBody.AddForce(new Vector2(_moveHorizontal * _moveSpeed, 0f), ForceMode2D.Impulse);

                _currentSign = (int)Mathf.Sign(_moveHorizontal);
                if (_currentSign != _previousSign)
                {
                    _previousSign = _currentSign;

                    transform.right = Vector2.right * _currentSign;
                    Debug.Log($"transform.right => {transform.right}");
                }
                _animator.SetFloat("HorizontalValue", Mathf.Abs(_moveHorizontal));
            }
        }

        void Jump()
        {
            _myRigidBody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }

        void Shoot()
        {
            var bulletObject = _bulletPool.GetObject();
            if (bulletObject.TryGetComponent(out Bullet bullet))
            {
                bullet.MyBulletPool = _bulletPool;
                bullet.transform.position = _firePoint.transform.position;
                bullet.transform.up = Vector2.right;
                //bullet.transform.right = transform.right;
                bullet.Launch(transform.right);
            }
        }


        public void TakeDamage(float dmg)
        {
            _life -= dmg;
            Debug.Log($"TakeDamage: {dmg}");
            if (_life <= 0)
            {
                Debug.Log($"Player Murio. Actual Life => {_life}");
            }
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