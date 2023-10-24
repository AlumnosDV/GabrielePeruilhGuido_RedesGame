using RedesGame.Damageables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedesGame
{
    public class KillingFloor : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {

            if (collision.CompareTag("Player"))
                collision.gameObject.GetComponent<IDamageable>()?.TakeLifeDamage();
        }

    }
}
