using RedesGame.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedesGame.Guns
{
    public class GunHandler : Singleton<GunHandler>
    {
        [SerializeField] private Gun _initialGunPrefab;
        private List<Gun> _allGuns;
        protected override void Awake()
        {
            base.Awake();
            _allGuns = new List<Gun>();
            
        }

        public Gun CreateGun(PlayerModel target)
        {
            var gun = Instantiate(_initialGunPrefab, transform).SetTarget(target);
            _allGuns.Add(gun);
            return gun;
        }

        public void ChangeGun(PlayerModel target, Gun oldGun, Gun newGun)
        {
            _allGuns.Remove(oldGun);
            _allGuns.Add(newGun);
            newGun.SetTarget(target);
        }

        private void LateUpdate()
        {
            foreach (var gun in _allGuns)
            {
                gun.UpdatePosition();
            }
        }
    }
}
