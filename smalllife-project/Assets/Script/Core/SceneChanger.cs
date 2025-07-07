using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class SceneChanger : MonoBehaviour
{
    [Header("SceneChanger Settings")]
    public string targetSceneName;          // 要跳转的目标场景名
    public bool showLoadingPage = true;     // 是否使用加载页
    public string loadingPageName = "LoadingPage1";

    [Header("BGM Fade Settings")]
    public bool useBGMFadeOut = false;      // 是否使用音乐淡出
    private float fadeOutDuration = 2f;   // 音乐淡出时间

    public void ChangeScene()
    {
        if (useBGMFadeOut)
        {
            // 查找场景中的 BGMController，并触发淡出 + 延迟切换
            BGMController bgm = FindObjectOfType<BGMController>();
            if (bgm != null)
            {
                bgm.FadeOut(fadeOutDuration);
                DOVirtual.DelayedCall(fadeOutDuration, DoSceneChange);
                return;
            }
        }

        // 若未启用 fadeOut，直接切换场景
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
