using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class SceneChanger : MonoBehaviour
{
    [Header("SceneChanger Settings")]
    public string targetSceneName;          // Ҫ��ת��Ŀ�곡����
    public bool showLoadingPage = true;     // �Ƿ�ʹ�ü���ҳ
    public string loadingPageName = "LoadingPage1";

    [Header("BGM Fade Settings")]
    public bool useBGMFadeOut = false;      // �Ƿ�ʹ�����ֵ���
    private float fadeOutDuration = 2f;   // ���ֵ���ʱ��

    public void ChangeScene()
    {
        if (useBGMFadeOut)
        {
            // ���ҳ����е� BGMController������������ + �ӳ��л�
            BGMController bgm = FindObjectOfType<BGMController>();
            if (bgm != null)
            {
                bgm.FadeOut(fadeOutDuration);
                DOVirtual.DelayedCall(fadeOutDuration, DoSceneChange);
                return;
            }
        }

        // ��δ���� fadeOut��ֱ���л�����
        DoSceneChange();
    }

    private void DoSceneChange()
    {
        LoadingManager.useLoadingPage = showLoadingPage;

        if (showLoadingPage)
        {
            PlayerPrefs.SetString("NextSceneName", targetSceneName);
            SceneManager.LoadScene(loadingPageName);
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
    public void OnConfirmClick(){
        AudioHub.Instance.PlayGlobal("click_confirm");
        ChangeScene();
    }
}
