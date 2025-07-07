using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    // ����������ڰ�ť�����ʱ����
    public void OnExitButtonClick()
    {
        AudioHub.Instance.PlayGlobal("click_confirm");
#if UNITY_EDITOR
        // ��Unity�༭���У������ť��ֹͣ���ţ������ڰ���Stop��ť
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // �ڷ�������Ϸ�У������ť���˳�����
        Application.Quit();
#endif
    }
}
