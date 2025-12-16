using RedesGame.Managers;
using RedesGame.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedesGame.Guns
{
    public class GunHandler : Singleton<GunHandler>, IActivable
    {
        [SerializeField] private Gun _initialGunPrefab;
        private bool _isActive = false;
        private readonly Dictionary<string, Gun> _gunsById = new();
        private readonly List<string> _orderedGunIds = new();
        protected override void Awake()
        {
            itDestroyOnLoad = true;
            base.Awake();
            Debug.Log("Gun Handler Awake");
        }

        private void Start()
        {
            var gunsInGame = FindObjectsOfType<Gun>()
                .Where(gun => gun.gameObject.layer == LayerMask.NameToLayer("InGameGun"))
                .OrderBy(gun => gun.GunId);

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
            if (newGunIndex < 0 || newGunIndex >= _orderedGunIds.Count)
                return -1;
            var newGun = GetGunByIndex(newGunIndex);
            newGun.SetTarget(target);

            Gun oldGun = null;
            if (oldGunIndex >= 0 && oldGunIndex < _orderedGunIds.Count)
            {
                oldGun = GetGunByIndex(oldGunIndex);
            }

            if (oldGun != null)
            {
                if (oldGun.IsPickupGun)
                    oldGun.BeginRespawnCycle();
                else
                    Destroy(oldGun.gameObject);
            }

            return GetIndexForGun(newGun);
        }

        public Gun GetGunByIndex(int index)
        {
            if (index < 0 || index >= _orderedGunIds.Count)
                return null;

            var id = _orderedGunIds[index];
            return _gunsById.ContainsKey(id) ? _gunsById[id] : null;
        }

        public int GetIndexForGun(Gun gunToCheck)
        {
            if (gunToCheck == null || string.IsNullOrWhiteSpace(gunToCheck.GunId))
                return -1;

            return _orderedGunIds.IndexOf(gunToCheck.GunId);
        }

        public int SpawnDefaultGun(PlayerModel target)
        {
            var gun = CreateGun(target);
            return GetIndexForGun(gun);
        }

        public void RegisterGun(Gun gun)
        {
            if (gun == null)
                return;

            if (string.IsNullOrWhiteSpace(gun.GunId))
                return;

            if (!_gunsById.ContainsKey(gun.GunId))
            {
                _gunsById.Add(gun.GunId, gun);
                _orderedGunIds.Add(gun.GunId);
            }
            else
            {
                _gunsById[gun.GunId] = gun;
            }
        }

        private void LateUpdate()
        {
            if (!_isActive) return;

            var filteredGuns = _gunsById.Values.Where(gun => gun != null && gun.gameObject.layer == LayerMask.NameToLayer("Gun")).ToList();
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

        private void OnDestroy()
        {
            _orderedGunIds.Clear();
            _gunsById.Clear();
        }
    }
}
