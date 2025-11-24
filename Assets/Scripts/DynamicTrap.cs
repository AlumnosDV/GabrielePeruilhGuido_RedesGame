using System.Collections;
using Fusion;
using RedesGame.Damageables;
using UnityEngine;

namespace RedesGame
{
    public class DynamicTrap : NetworkBehaviour
    {
        [SerializeField] private Collider2D _damageZone;
        [SerializeField] private SpriteRenderer _indicator;
        [SerializeField] private float _activeDuration = 2f;
        [SerializeField] private float _inactiveDuration = 1.5f;
        [SerializeField] private Color _activeColor = Color.red;
        [SerializeField] private Color _inactiveColor = Color.gray;

        [Networked(OnChanged = nameof(OnTrapStateChanged))]
        private bool IsActive { get; set; }

        public override void Spawned()
        {
            if (_damageZone == null)
                _damageZone = GetComponent<Collider2D>();

            UpdateVisuals();

            if (Object.HasStateAuthority)
                StartCoroutine(TrapRoutine());
        }

        private IEnumerator TrapRoutine()
        {
            while (true)
            {
                IsActive = true;
                yield return new WaitForSeconds(_activeDuration);

                IsActive = false;
                yield return new WaitForSeconds(_inactiveDuration);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!IsActive)
                return;

            if (collision.CompareTag("Player"))
            {
                collision.GetComponent<IDamageable>()?.TakeLifeDamage();
            }
        }

        static void OnTrapStateChanged(Changed<DynamicTrap> changed)
        {
            changed.Behaviour.UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_damageZone != null)
                _damageZone.enabled = IsActive;

            if (_indicator != null)
                _indicator.color = IsActive ? _activeColor : _inactiveColor;
        }
    }
}
