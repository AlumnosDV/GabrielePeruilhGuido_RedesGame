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
        [SerializeField] public GameObject PlayerBody;
        [SerializeField] private LayerMask _gunsLayerMask;

        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _jumpForce;
        [SerializeField] private int _maxLife = 3;

        private Gun _myGun;
        public float _checkGunsRadious = 3;
        public bool IsJumping = false;
        private bool _isActive = false;
        private float _moveHorizontal;
        private int _currentSign, _previousSign;
        private bool _playerDead = false;
        private bool _isFiring;
        private int _currentLife;

        private NetworkInputData _inputs;

        [Networked(OnChanged = nameof(OnDeadChanged))]
        private bool PlayerDead { get; set; }

        [Networked(OnChanged = nameof(OnChangeGun))]
        private int IndexOfNewWeapon { get; set; } = -1;

        private int _currentIndexOfWeapon;

        void Start()
        {
            _currentLife = _maxLife;
        }

        public override void Spawned()
        {
            _myGun = GunHandler.Instance.CreateGun(this);
            _currentIndexOfWeapon = GunHandler.Instance.GetIndexForGun(_myGun);
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
                    var bullet = Runner.Spawn(_myGun.BulletPrefab, _myGun.FirePoint.transform.position);
                    _myGun.Shoot(bullet);
                }

                if (_inputs.isJumpPressed && !IsJumping)
                {
                    Jump();
                }

                IsCloseFromGun();


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
            _currentLife -= lostLife;
            Debug.Log($"Player ID {Runner.LocalPlayer.PlayerId}: Life {_currentLife}");
            transform.position = Extensions.GetRandomSpawnPoint();
            if (_currentLife <= 0)
            {
                PlayerDead = true;
                _playerDead = true;
            }
        }

        static void OnChangeGun(Changed<PlayerModel> changed)
        {
            var behaviour = changed.Behaviour;
            Debug.Log($"Changed Weapon Index OfNewWeapon{behaviour.IndexOfNewWeapon}");
            if (behaviour.IndexOfNewWeapon >= 0)
            {
                GunHandler.Instance.ChangeGun(behaviour,behaviour._currentIndexOfWeapon, behaviour.IndexOfNewWeapon);
            }
        }

        static void OnDeadChanged(Changed<PlayerModel> changed)
        {
            var behaviour = changed.Behaviour;
            EventManager.TriggerEvent("Dead", behaviour._playerDead);
        }

        private void IsCloseFromGun()
        {
            var guns = FindObjectsOfType<Gun>()
                .Where(gun => gun.gameObject.layer == LayerMask.NameToLayer("InGameGun") && Vector2.Distance(transform.position, gun.gameObject.transform.position) <= _checkGunsRadious)
                .ToArray();
            if (guns.Length == 0) return;
            _myGun = guns[0];
            RPC_ChangeGun(GunHandler.Instance.GetIndexForGun(guns[0]));
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ChangeGun(int newGunIndex)
        {
            IndexOfNewWeapon = newGunIndex;          
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