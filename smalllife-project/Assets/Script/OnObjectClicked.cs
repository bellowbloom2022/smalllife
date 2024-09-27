using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnObjectClicked : MonoBehaviour
{
    public GameObject ripplePrefab;//ˮ�ƶ�����Ԥ����
    private AnimAudioEffect audioEffect; //���ò��������Ľű�

    private void Start()
    {
        audioEffect = GetComponent<AnimAudioEffect>();
    }
    private void OnMouseDown()
    {
        //��ȡ���λ��
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //�����λ��ת��Ϊˮ̶��������ϵ�е�λ��
        Vector3 localClickPos = transform.InverseTransformPoint(clickPos);

        //��ˮ̶��������ϵ�м���ˮ��ʵ����λ��
        Vector3 ripplePos = transform.TransformPoint(localClickPos);
        ripplePos.z = -1f;

        //�ڵ��λ�ô���ˮ��ʵ��
        GameObject ripple = Instantiate(ripplePrefab, ripplePos, Quaternion.identity,transform);

        //����ˮ��ʵ����λ�úʹ�С
        ripple.transform.localScale = Vector3.one;

        //��ˮ�ƶ���������Ϻ�ɾ��ˮ��ʵ��
        Destroy(ripple, ripple.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);

        if (audioEffect != null)
        {
            audioEffect.onAnimTriggerAudioEffect();
        }
    }
}
