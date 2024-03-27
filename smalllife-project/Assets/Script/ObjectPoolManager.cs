using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string poolName;
        public GameObject prefab;
        public int poolSize;
    }

    public static ObjectPoolManager Instance { get; private set; }
    public List<Pool> pools = new List<Pool>();
    private Dictionary<string, Queue<GameObject>> objectPools = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        //预先生成对象池中的对象
        InitializeObjectPools();
    }

    private void InitializeObjectPools()
    {
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for(int i=0; i< pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            objectPools.Add(pool.poolName, objectPool);
        }
    }

    public GameObject GetPooledObject(string poolName)
    {
        if (objectPools.ContainsKey(poolName))
        {
            if(objectPools[poolName].Count > 0)
            {
                GameObject obj = objectPools[poolName].Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                Debug.LogWarning("No available objects in the object pool:" + poolName);
            }
        }
        else
        {
            Debug.LogError("Object pool does not exist:" + poolName);
        }
        return null;
    }

    public void ReturnPooledObject(string poolName,GameObject obj)
    {
        if (objectPools.ContainsKey(poolName))
        {
            obj.SetActive(false);
            objectPools[poolName].Enqueue(obj);
        }
        else
        {
            Debug.LogError("Object pool does not exist:" + poolName);
        }
    }
}
