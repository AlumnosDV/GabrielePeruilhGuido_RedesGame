using UnityEngine;

namespace RedesGame.Damageables
{
    public interface IDamageable
    {
        public void TakeForceDamage(float forceRecived, Vector2 direction);

        public void TakeLifeDamage();
    }
}