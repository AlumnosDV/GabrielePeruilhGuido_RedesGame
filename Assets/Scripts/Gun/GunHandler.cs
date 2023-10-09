using RedesGame.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RedesGame.Guns
{
    public class GunHandler : Singleton<GunHandler>
    {
        [SerializeField] private Gun _initialGunPrefab;
        private List<Gun> _allGuns;
        protected override void Awake()
        {
            itDestroyOnLoad = true;
        }

        private void Start()
        {
            _allGuns = new List<Gun>();
            var gunsInGame = FindObjectsOfType<Gun>().Where(gun => gun.gameObject.layer == LayerMask.NameToLayer("InGameGun"));
            _allGuns = _allGuns.Concat(gunsInGame).ToList();
        }

        public Gun CreateGun(PlayerModel target)
        {
            var gun = Instantiate(_initialGunPrefab, transform).SetTarget(target);
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
            var filteredGuns = _allGuns.Where(gun => gun.gameObject.layer == LayerMask.NameToLayer("Gun")).ToList();
            foreach (var gun in filteredGuns)
            {
                gun.UpdatePosition();
            }
        }
    }
}
