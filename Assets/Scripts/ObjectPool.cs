using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace RedesGame
{
    public abstract class ObjectPool<T> : NetworkBehaviour where T : MonoBehaviour
    {
        [SerializeField] private int _initialPoolSize;
        [SerializeField] private Transform parentTransform;
        private List<T> objects;

        public override void Spawned()
        {
            base.Spawned();
            objects = new List<T>(_initialPoolSize);
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewObject();
            }
        }

        public T GetObject()
        {
            T obj = null;
            for (int i = 0; i < objects.Count; i++)
            {
                if (!objects[i].gameObject.activeInHierarchy)
                {
                    obj = objects[i];
                    obj.gameObject.SetActive(true);
                    break;
                }
            }
            if (obj == null)
            {
                obj = CreateNewObject();
                obj.gameObject.SetActive(true);
            }
            return obj;
        }

        public void ReturnObject(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(parentTransform);
        }

        private T CreateNewObject()
        {
            T obj = IntantiateObject();
            obj.transform.SetParent(parentTransform);
            objects.Add(obj);
            return obj;
        }

        protected abstract T IntantiateObject();
    }
}
