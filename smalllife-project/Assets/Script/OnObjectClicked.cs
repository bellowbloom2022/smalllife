using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnObjectClicked : MonoBehaviour
{
    public GameObject ripplePrefab;//水纹动画的预制体
    private AnimAudioEffect audioEffect; //引用播放声音的脚本

    private void Start()
    {
        audioEffect = GetComponent<AnimAudioEffect>();
    }
    private void OnMouseDown()
    {
        //获取点击位置
        Vector3 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //将点击位置转换为水潭本地坐标系中的位置
        Vector3 localClickPos = transform.InverseTransformPoint(clickPos);

        //在水潭本地坐标系中计算水纹实例的位置
        Vector3 ripplePos = transform.TransformPoint(localClickPos);
        ripplePos.z = -1f;

        //在点击位置创建水纹实例
        GameObject ripple = Instantiate(ripplePrefab, ripplePos, Quaternion.identity,transform);

        //调整水纹实例的位置和大小
        ripple.transform.localScale = Vector3.one;

        //在水纹动画播放完毕后删除水纹实例
        Destroy(ripple, ripple.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);

        if (audioEffect != null)
        {
            audioEffect.onAnimTriggerAudioEffect();
        }
    }
}
