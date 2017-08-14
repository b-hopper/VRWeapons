using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRWeapons;

namespace VRWeapons
{
    public class ObjectPool : MonoBehaviour, IObjectPool
    {
        public int ObjectsPerElement;
        public GameObject[] ObjectsInPool;
        Dictionary<int, GameObject[]> ObjPool;
        int[] objIndex;
        int poolIndex;

        private void Start()
        {
            ObjPool = new Dictionary<int, GameObject[]>();
            poolIndex = 0;
            objIndex = new int[ObjectsInPool.Length];
            foreach (GameObject obj in ObjectsInPool)
            {
                GameObject newPool = new GameObject(obj.name + "_Pool");
                GameObject[] pool = new GameObject[ObjectsPerElement];

                for (int i = 0; i < ObjectsPerElement; i++)
                {
                    GameObject tmp = Instantiate(obj);
                    pool[i] = tmp;
                    tmp.SetActive(false);
                    tmp.transform.parent = newPool.transform;                    
                }
                ObjPool.Add(poolIndex, pool);
                poolIndex++;
            }
            poolIndex--;
        }

        public GameObject GetNewObj()
        {
            GameObject[] objArray;
            GameObject obj = null;
            if (ObjPool.Count > 0)
            {
                int selectedIndex = Random.Range(0, poolIndex);

                objArray = ObjPool[selectedIndex];

                obj = objArray[objIndex[selectedIndex]];

                objIndex[selectedIndex] = (objIndex[selectedIndex] + 1) % (ObjectsPerElement);

                obj.SetActive(false);
                obj.SetActive(true);
            }
            return obj;
        }
    }
}