using Fusion;
using RedesGame.Guns;
using RedesGame.Damageables;
using System;
using UnityEngine;
using RedesGame.ExtensionsClass;
using RedesGame.Managers;
using System.Collections;
using System.Linq;

namespace RedesGame.Player
{
    public class PlayerModel : NetworkBehaviour, IDamageable, IActivable
    {
        [SerializeField] private NetworkMecanimAnimator _netWorkAnimator;
        [SerializeField] private NetworkRigidbody2D _networkRigidbody2D;
        [SerializeField] private NetworkPlayer _networkPlayer;

        [SerializeField] private GameObject _canvas;
        [SerializeField] private GameObject _playerBody;
        [SerializeField] private LayerMask _gunsLayerMask;
        [SerializeField] private Gun _myGun;
        private Gun _myNewGun;

        [SerializeField] private float _checkGunsRadious;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _jumpForce;
        [SerializeField] private int _maxLife = 3;

        public bool IsJumping = false;
        private bool _isActive = false;
        private float _moveHorizontal;
        private int _currentSign, _previousSign;
        private bool _playerDead = false;
        private bool _isCloseFromGun = false;

        private NetworkInputData _inputs;

        //[Networked(OnChanged = nameof(OnFiringChanged))]
        private bool _isFiring { get; set; }

        private int Life { get; set; }

        [Networked(OnChanged = nameof(OnDeadChanged))]
        private bool PlayerDead { get; set; }

        void Start()
        {
            Life = _maxLife;
            _myGun.gameObject.layer = LayerMask.NameToLayer("Gun");
        }

        private void OnEnable()
        {
            ScreenManager.Instance.Subscribe(this);
            EventManager.StartListening("AllPlayersInGame", OnAllPlayersInGame);
        }


        private void OnDisable()
        {
            ScreenManager.Instance.Unsubscribe(this);
            EventManager.StopListening("AllPlayersInGame", OnAllPlayersInGame);
        }

        private void OnAllPlayersInGame(object[] obj)
        {
            transform.position = Extensions.GetRandomSpawnPoint();
        }

        public override void FixedUpdateNetwork()
        {
            if (!_isActive) return;

            if (GetInput(out _inputs))
            {
                if (_inputs.isFirePressed)
                {
                    _myGun.Shoot();
                }

                if (_inputs.isJumpPressed && !IsJumping)
                {
                    Jump();
                }

                if (IsCloseFromGun())
                {
                    ChangeGun();
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

        //static void OnFiringChanged(Changed<PlayerModel> changed)
        //{
        //    var updatedFiring = changed.Behaviour._isFiring;
        //    changed.LoadOld();
        //    var oldFiring = changed.Behaviour._isFiring;

        //}

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
            Debug.Log($"Player ID {Runner.LocalPlayer.PlayerId}: Life {Life}");
            transform.position = Extensions.GetRandomSpawnPoint();
            if (Life <= 0)
            {
                PlayerDead = true;
                _playerDead = true;
            }
        }


        private void ChangeGun()
        {
            RPC_ChangeGun();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ChangeGun()
        {
            Debug.Log("Change Gun");
            Runner.Despawn(_myGun.GetComponent<NetworkObject>());
            _myNewGun.gameObject.transform.SetParent(_playerBody.transform);
            _myNewGun.transform.position = _playerBody.transform.position;
            _myNewGun.gameObject.layer = LayerMask.NameToLayer("Gun");
            _myGun = _myNewGun;
        }

        static void OnDeadChanged(Changed<PlayerModel> changed)
        {
            var behaviour = changed.Behaviour;
            EventManager.TriggerEvent("Dead", behaviour._playerDead);
        }

        private bool IsCloseFromGun()
        {
            var guns = FindObjectsOfType<Gun>()
                .Where(gun => gun.gameObject.layer == LayerMask.NameToLayer("InGameGun") && Vector2.Distance(transform.position, gun.gameObject.transform.position) <= _checkGunsRadious)
                .ToArray();

            if (guns.Length > 0)
            {
                _isCloseFromGun = true;
                _myNewGun = guns[0];
            }
            else
            {
                _isCloseFromGun = false;
                _myNewGun = null;
            }

            return _isCloseFromGun;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _checkGunsRadious);
        }

        public void Activate()
        {
            _isActive = true;
        }

        public void Deactivate()
        {
            _isActive = false;
        }
    }
}