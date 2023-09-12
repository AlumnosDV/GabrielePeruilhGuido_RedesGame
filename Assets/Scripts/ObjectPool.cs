using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedesGame
{
    public abstract class ObjectPool : MonoBehaviour
    {
        [SerializeField] private int _initialPoolSize;
        [SerializeField] private Transform parentTransform;
        private List<GameObject> objects;

        private void Awake()
        {
            objects = new List<GameObject>(_initialPoolSize);
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewObject();
            }
        }

        public GameObject GetObject()
        {
            GameObject obj = null;
            for (int i = 0; i < objects.Count; i++)
            {
                if (!objects[i].activeInHierarchy)
                {
                    obj = objects[i];
                    obj.SetActive(true);
                    break;
                }
            }
            if (obj == null)
            {
                obj = CreateNewObject();
                obj.SetActive(true);
            }
            return obj;
        }

        public void ReturnObject(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(parentTransform);
        }

        private GameObject CreateNewObject()
        {
            GameObject obj = IntantiateObject();
            obj.SetActive(false);
            obj.transform.SetParent(parentTransform);
            objects.Add(obj);
            return obj;
        }

        protected abstract GameObject IntantiateObject();
    }
}
