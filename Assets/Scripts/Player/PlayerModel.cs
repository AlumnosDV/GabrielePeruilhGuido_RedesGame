using Fusion;
using RedesGame.Guns;
using RedesGame.Damageables;
using UnityEngine;
using RedesGame.ExtensionsClass;
using RedesGame.Managers;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

namespace RedesGame.Player
{
    public class PlayerModel : NetworkBehaviour, IDamageable, IActivable
    {
        [Header("Components")]
        [SerializeField] private NetworkMecanimAnimator _networkAnimator;
        [SerializeField] private NetworkRigidbody2D _networkRigidbody2D;
        [SerializeField] private NetworkPlayer _networkPlayer;

        [SerializeField] private GameObject _canvas;
        [SerializeField] public GameObject PlayerBody;
        [SerializeField] private LayerMask _gunsLayerMask;

        [Header("Combat / Guns")]
        [SerializeField] private float _fireCooldown = 0.15f;
        [SerializeField] private float _checkGunsRadius = 3f;

        [Header("Life")]
        [SerializeField] private int _maxLife = 3;

        private Gun _currentGun;
        private int _currentWeaponIndex;
        private int _currentLife;
        private bool _isActive;
        private bool _isReady;
        private bool _isFiring;
        private bool _playerDead;
        private double _lastFiringTime;

        [Networked(OnChanged = nameof(OnDeadChanged))]
        private bool PlayerDead { get; set; }

        [Networked(OnChanged = nameof(OnChangeGun))]
        private int IndexOfNewWeapon { get; set; } = -1;

        // Expuestos para que otros componentes consulten
        public bool IsActive => _isActive;
        public bool IsDead => PlayerDead;
        public bool IsFacingRight => PlayerBody != null ? PlayerBody.transform.lossyScale.x >= 0f : transform.lossyScale.x >= 0f;

        // ----------------- LIFECYCLE -----------------

        public override void Spawned()
        {
            Debug.Log($"Player Spawned {Runner.LocalPlayer.PlayerId}");

            _currentLife = _maxLife;

            _currentGun = GunHandler.Instance.CreateGun(this);
            if (_currentGun != null)
            {
                _currentWeaponIndex = GunHandler.Instance.GetIndexForGun(_currentGun);
            }

            _lastFiringTime = Runner.SimulationTime;
        }

        private void OnEnable()
        {
            ScreenManager.Instance.Subscribe(this);
            EventManager.StartListening("AllPlayersInGame", OnAllPlayersInGame);
            EventManager.StartListening("MatchStarted", OnMatchStarted);
        }

        private void OnDisable()
        {
            ScreenManager.Instance.Unsubscribe(this);
            EventManager.StopListening("AllPlayersInGame", OnAllPlayersInGame);
            EventManager.StopListening("MatchStarted", OnMatchStarted);
        }

        private void OnAllPlayersInGame(object[] obj)
        {
            _isReady = false;
        }

        private void OnMatchStarted(object[] obj)
        {
            transform.position = Extensions.GetRandomSpawnPoint();
        }

        // ----------------- FUSION TICK -----------------

        public override void FixedUpdateNetwork()
        {
            if (!_isActive)
                return;

            if (PlayerDead)
                return;

            if (!GetInput(out NetworkInputData input))
                return;

            // ----- DISPARO -----
            if (input.Buttons.IsSet(MyButtons.Fire))
            {
                TryFire();
            }

            // ----- ARMAS EN RADIO -----
            CheckNearbyGuns();
        }

        public override void Render()
        {
            base.Render();

            if (!_isActive || PlayerDead)
                return;

            _currentGun?.UpdatePosition();
        }

        #region DISPARO 

        private void TryFire()
        {
            if (_currentGun == null)
                return;

            double now = Runner.SimulationTime;
            if (now - _lastFiringTime < _fireCooldown)
                return;

            if (Object.HasStateAuthority)
            {
                FireBullet();
            }
            else
            {
                RPC_RequestFire();
            }

            if (!_isFiring)
                StartCoroutine(FiringCooldown());
        }

        private void FireBullet()
        {
            if (_currentGun == null)
                return;

            if (!_currentGun.HasAmmo)
            {
                HandleGunDepleted();
                return;
            }

            _lastFiringTime = Runner.SimulationTime;
            Vector2 direction = _currentGun != null ? _currentGun.GetDirection() : Vector2.right;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

            // Spawn network de la bala
            var bullet = Runner.Spawn(
                _currentGun.BulletPrefab,
                _currentGun.FirePoint.transform.position,
                rotation
            );

            if (bullet == null)
                return;

            _currentGun.Shoot(bullet);
            _currentGun.ConsumeAmmo();

            if (_currentGun.IsOutOfAmmo)
            {
                HandleGunDepleted();
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestFire()
        {
            if (_currentGun == null)
                return;

            double now = Runner.SimulationTime;
            if (now - _lastFiringTime < _fireCooldown)
                return;

            FireBullet();
        }

        private IEnumerator FiringCooldown()
        {
            _isFiring = true;
            yield return new WaitForSeconds(_fireCooldown);
            _isFiring = false;
        }
        #endregion

        // ----------------- DAÑO / VIDA -----------------

        public void TakeForceDamage(float dmg, Vector2 direction)
        {
            if (_networkRigidbody2D == null)
                return;

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

            if (_currentLife > 0)
            {
                transform.position = Extensions.GetRandomSpawnPoint();
                return;
            }

            HandleElimination();
        }

        private void HandleElimination()
        {
            if (PlayerDead)
                return;

            PlayerDead = true;
            _playerDead = true;

            DisableRenderersAndColliders();

            EventManager.TriggerEvent("PlayerEliminated", Object.InputAuthority);
        }

        private void DisableRenderersAndColliders()
        {
            var renderers = GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.enabled = false;
            }

            var colliders2D = GetComponentsInChildren<Collider2D>(true);
            foreach (var collider2D in colliders2D)
            {
                collider2D.enabled = false;
            }

            var colliders = GetComponentsInChildren<Collider>(true);
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            if (_networkRigidbody2D != null)
            {
                _networkRigidbody2D.Rigidbody.velocity = Vector2.zero;
                _networkRigidbody2D.Rigidbody.simulated = false;
            }
        }

        static void OnDeadChanged(Changed<PlayerModel> changed)
        {
            var behaviour = changed.Behaviour;
            var isLocal = behaviour.Object.HasInputAuthority;
            EventManager.TriggerEvent("Dead", isLocal);
        }

        // ----------------- ARMAS -----------------

        private void CheckNearbyGuns()
        {
            var guns = FindObjectsOfType<Gun>()
                .Where(gun =>
                    gun.gameObject.layer == LayerMask.NameToLayer("InGameGun") &&
                    Vector2.Distance(transform.position, gun.transform.position) <= _checkGunsRadius)
                .ToArray();

            if (guns.Length == 0)
                return;

            var newGun = guns[0];
            if (newGun == _currentGun)
                return;

            _currentGun = newGun;
            RPC_ChangeGun(GunHandler.Instance.GetIndexForGun(newGun));
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void RPC_ChangeGun(int newGunIndex)
        {
            IndexOfNewWeapon = newGunIndex;
        }

        private void HandleGunDepleted()
        {
            if (_currentGun == null)
                return;

            if (Object.HasStateAuthority)
            {
                SwitchToDefaultGun();
            }
            else
            {
                RPC_RequestDefaultGun();
            }
        }

        private void SwitchToDefaultGun()
        {
            var defaultGunIndex = GunHandler.Instance.SpawnDefaultGun(this);
            if (defaultGunIndex >= 0)
            {
                RPC_ChangeGun(defaultGunIndex);
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestDefaultGun()
        {
            SwitchToDefaultGun();
        }

        static void OnChangeGun(Changed<PlayerModel> changed)
        {
            var behaviour = changed.Behaviour;
            if (behaviour.IndexOfNewWeapon >= 0)
            {
                var newWeaponIndex = GunHandler.Instance.ChangeGun(
                    behaviour,
                    behaviour._currentWeaponIndex,
                    behaviour.IndexOfNewWeapon
                );

                if (newWeaponIndex >= 0)
                {
                    behaviour._currentWeaponIndex = newWeaponIndex;
                    behaviour._currentGun = GunHandler.Instance.GetGunByIndex(newWeaponIndex);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, _checkGunsRadius);
        }

        // ----------------- IActivable -----------------

        public void Activate()
        {
            _isActive = true;
        }

        public void Deactivate()
        {
            _isActive = false;
        }

        // ----------------- READY STATE / LOBBY -----------------

        public void ToggleReadyState()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
                return;

            var newReadyState = !_isReady;
            _isReady = newReadyState;
            
            RPC_SetReadyState(newReadyState);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_SetReadyState(bool newReadyState)
        {
            _isReady = newReadyState;
            EventManager.TriggerEvent("PlayerReadyChanged", Object.InputAuthority, newReadyState);
        }
    }
}
