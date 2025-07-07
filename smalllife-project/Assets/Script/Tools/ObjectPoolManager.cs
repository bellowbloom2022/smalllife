using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    // 每种预制体的池子，按prefab名分类
    private Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();

    private Transform uiRoot;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private Transform UIRoot{
        get{
            if (uiRoot == null){
                uiRoot = GameObject.Find("Canvas")?.transform;
                if (uiRoot == null){
                    Debug.LogError("ObjectPoolManager: Canvas not found in scene.");
                }
            }
            return uiRoot;
        }
    }

    /// <summary>
    /// 注册一个新的预制体，用于生成对象池
    /// </summary>
    public void RegisterPrefab(string key, GameObject prefab)
    {
        if (!prefabDict.ContainsKey(key)) {
            prefabDict[key] = prefab;
            poolDict[key] = new Queue<GameObject>();
        }
    }

    /// <summary>
    /// 从对象池中获取对象
    /// </summary>
    public GameObject GetFromPool(string key, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        if (!poolDict.ContainsKey(key)) {
            Debug.LogWarning($"Pool for '{key}' not found. Please register prefab first.");
            return null;
        }

        GameObject obj = poolDict[key].Count > 0 ? poolDict[key].Dequeue() : Instantiate(prefabDict[key]);

        obj.transform.SetParent(parent);
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 回收对象到池中
    /// </summary>
    public void ReturnToPool(string key, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(this.transform);
        poolDict[key].Enqueue(obj);
    }

    public GameObject GetUIPanelFromPool(string key)
    {
        GameObject obj = GetFromPool(key, Vector3.zero, Quaternion.identity, UIRoot);
        return obj;
    }

    public void ReturnUIPanelToPool(string key, GameObject obj)
    {
        ReturnToPool(key, obj);
    }
}
