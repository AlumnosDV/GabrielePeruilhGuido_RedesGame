using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace RedesGame
{
    public abstract class ObjectPool<T> : MonoBehaviour where T: NetworkBehaviour
    {
        [SerializeField] private int _initialPoolSize;
        [SerializeField] private Transform parentTransform;
        private List<T> objects;

        private void Awake()
        {
            objects = new List<T>(_initialPoolSize);
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewObject();
            }
        }

        public T GetObject()
        {
            T obj = default(T);
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
            T obj = InstantiateObject();
            obj.transform.SetParent(parentTransform);
            objects.Add(obj);
            return obj;
        }

        protected abstract T InstantiateObject();
    }
}
