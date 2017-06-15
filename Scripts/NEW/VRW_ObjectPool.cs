using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRW_ObjectPool : MonoBehaviour, IVRW_ObjectPool
{
    public int ObjectsPerElement;
    public GameObject[] ObjectsInPool;
    Dictionary<int, GameObject[]> ObjPool;
    int[] objIndex;
    int pools;

    private void Start()
    {
        ObjPool = new Dictionary<int, GameObject[]>();
        pools = 0;
        foreach (GameObject obj in ObjectsInPool)
        {
            GameObject newPool = new GameObject(obj.name + "Pool");
            for (int i = 0; i < ObjectsPerElement; i++)
            {
                ObjPool[j] = new GameObject[ObjectsPerElement];
                
                ObjPool[j][i] = Instantiate(obj);
                ObjPool[j][i].SetActive(false);
                ObjPool[j][i].transform.parent = newPool.transform;
                Debug.Log("Added object " + obj + " to Object Pool in position: " + j + ", " + i);
            }
            pools++;
        }
        objIndex = new int[pools];
    }

    public GameObject GetNewObj()
    {
        GameObject obj;
        int selectedIndex = Random.Range(0, pools);
        obj = ObjPool[selectedIndex][objIndex[selectedIndex]];
        objIndex[selectedIndex] = (objIndex[selectedIndex] + 1) % ObjectsPerElement;

        obj.SetActive(true);
        return obj;
    }
}
