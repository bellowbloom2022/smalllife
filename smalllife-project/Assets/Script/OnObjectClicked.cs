using UnityEngine;
using System.Collections;

public class OnObjectClicked : MonoBehaviour
{
    [Header("Clicked Animation")]
    public GameObject ripplePrefab;//��ǰ�������ж���prefab
    public string poolKey = "Ripple";   // ����� key��Ψһ��ʶ prefab��

    private void OnMouseDown()
    {
        //��ȡ���λ��
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //�����λ��ת��Ϊˮ̶��������ϵ�е�λ��
        Vector3 localClickPos = transform.InverseTransformPoint(clickPos);
        //��ˮ̶��������ϵ�м���ˮ��ʵ����λ��
        Vector3 ripplePos = transform.TransformPoint(localClickPos);
        ripplePos.z = -1f;

        //�Ӷ�����л�ȡ��Ӧ����ʵ��
        GameObject ripple = ObjectPoolManager.Instance.GetFromPool(poolKey, ripplePos, Quaternion.identity, transform);
        //����ˮ��ʵ����λ�úʹ�С
        ripple.transform.localScale = Vector3.one;
        
        // �ӳٻ��գ��������滻Ϊ�����¼��黹��
        StartCoroutine(ReturnToPoolAfter(ripple, poolKey, 1f));
    }

    private IEnumerator ReturnToPoolAfter(GameObject obj, string key, float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolManager.Instance.ReturnToPool(key, obj);
    }
}
