using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedesGame.SO
{
    [CreateAssetMenu(fileName = "BulletDataSO", menuName = "SO/Bullet Data", order = 0)]
    public class BulletDataSO : ScriptableObject
    {
        [field: SerializeField, Range(0f,1000f)] public float ForceDamage { get; private set; } = 10f;
        [field: SerializeField, Range(0f, 500f)] public float Speed { get; private set; } = 10f;

    }
}
