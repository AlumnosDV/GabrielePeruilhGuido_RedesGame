using RedesGame.Managers;
using RedesGame.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedesGame.Guns
{
    public class GunHandler : Singleton<GunHandler>, IActivable
    {
        [SerializeField] private Gun _initialGunPrefab;
        private bool _isActive = false;
        private List<Gun> _allGuns;
        protected override void Awake()
        {
            itDestroyOnLoad = true;
            base.Awake();
            Debug.Log("Gun Handler Awake");
            _allGuns = new List<Gun>();
        }

        private void Start()
        {
            var gunsInGame = FindObjectsOfType<Gun>().Where(gun => gun.gameObject.layer == LayerMask.NameToLayer("InGameGun"));
            foreach (var gun in gunsInGame)
            {
                RegisterGun(gun);
            }
        }

        private void OnEnable()
        {
            ScreenManager.Instance.Subscribe(this);
        }

        private void OnDisable()
        {
            ScreenManager.Instance.Unsubscribe(this);
        }

        public Gun CreateGun(PlayerModel target)
        {
            if (_initialGunPrefab == null) return default;

            var gun = Instantiate(_initialGunPrefab, target.PlayerBody.transform);
            gun.SetTarget(target);
            RegisterGun(gun);
            return gun;
        }

        public int ChangeGun(PlayerModel target, int oldGunIndex, int newGunIndex)
        {
            if (_allGuns == null || newGunIndex < 0 || newGunIndex >= _allGuns.Count)
                return -1;
            var newGun = _allGuns[newGunIndex];
            newGun.SetTarget(target);

            Gun oldGun = null;
            if (oldGunIndex >= 0 && oldGunIndex < _allGuns.Count)
            {
                oldGun = _allGuns[oldGunIndex];
                _allGuns.Remove(oldGun);
            }

            if (oldGun != null)
            {
                if (oldGun.IsPickupGun)
                    oldGun.BeginRespawnCycle();
                else
                    Destroy(oldGun.gameObject);
            }

            return _allGuns.IndexOf(newGun);
        }

        public Gun GetGunByIndex(int index)
        {
            if (_allGuns == null || index < 0 || index >= _allGuns.Count)
                return null;

            return _allGuns[index];
        }

        public int GetIndexForGun(Gun gunToCheck)
        {
            return _allGuns.IndexOf(gunToCheck);
        }

        public int SpawnDefaultGun(PlayerModel target)
        {
            var gun = CreateGun(target);
            return GetIndexForGun(gun);
        }

        public void RegisterGun(Gun gun)
        {
            if (gun == null || _allGuns == null)
                return;

            if (!_allGuns.Contains(gun))
                _allGuns.Add(gun);
        }

        private void LateUpdate()
        {
            if (!_isActive) return;

            var filteredGuns = _allGuns.Where(gun => gun.gameObject.layer == LayerMask.NameToLayer("Gun")).ToList();
            foreach (var gun in filteredGuns)
            {
                gun.UpdatePosition();
            }
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
