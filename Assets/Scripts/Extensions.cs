using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedesGame.ExtensionsClass
{
    public static class Extensions
    {
        public static Vector2 GetRandomSpawnPoint()
        {
            return new Vector3(Random.Range(-5f, 5f), 5f, 0);
        }

    }
}
