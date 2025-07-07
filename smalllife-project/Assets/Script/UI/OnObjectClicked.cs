using UnityEngine;
using System.Collections;

public class OnObjectClicked : MonoBehaviour
{
    [Header("Clicked Animation")]
    public GameObject ripplePrefab;//当前对象所有动画prefab
    public string poolKey = "Ripple";   // 对象池 key（唯一标识 prefab）

    private void OnMouseDown()
    {
        //获取点击位置
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //将点击位置转换为水潭本地坐标系中的位置
        Vector3 localClickPos = transform.InverseTransformPoint(clickPos);
        //在水潭本地坐标系中计算水纹实例的位置
        Vector3 ripplePos = transform.TransformPoint(localClickPos);
        ripplePos.z = -1f;

        //从对象池中获取对应动画实例
        GameObject ripple = ObjectPoolManager.Instance.GetFromPool(poolKey, ripplePos, Quaternion.identity, transform);
        //调整水纹实例的位置和大小
        ripple.transform.localScale = Vector3.one;
        
        // 延迟回收（后续可替换为动画事件归还）
        StartCoroutine(ReturnToPoolAfter(ripple, poolKey, 1f));
    }

    private IEnumerator ReturnToPoolAfter(GameObject obj, string key, float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolManager.Instance.ReturnToPool(key, obj);
    }
}
