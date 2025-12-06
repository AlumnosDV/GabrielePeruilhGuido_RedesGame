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
        }

        private void Start()
        {
            _allGuns = new List<Gun>();
            var gunsInGame = FindObjectsOfType<Gun>().Where(gun => gun.gameObject.layer == LayerMask.NameToLayer("InGameGun"));
            _allGuns = _allGuns.Concat(gunsInGame).ToList();
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
            _allGuns.Add(gun);
            return gun;
        }

        public void ChangeGun(PlayerModel target, int oldGunIndex,int newGunIndex)
        {
            _allGuns[newGunIndex].SetTarget(target);
            Destroy(_allGuns[oldGunIndex].gameObject);
            _allGuns.RemoveAt(oldGunIndex);
        }
        
        public int GetIndexForGun(Gun gunToCheck)
        {
            return _allGuns.IndexOf(gunToCheck);
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
