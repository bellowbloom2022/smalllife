using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("SceneChanger")]
    public string targetSceneName; // 下一个场景的名称
    public bool showLoadingPage = true; // 是否需要加载页面

    public void ChangeScene()
    {
        // 根据配置决定是否加载 LoadingPage
        LoadingManager.useLoadingPage = showLoadingPage;

        if (showLoadingPage)
        {
            // 如果需要加载页面，切换到 LoadingScene
            PlayerPrefs.SetString("NextSceneName", targetSceneName);
            SceneManager.LoadScene("LoadingPage1");
        }
        else
        {
            // 如果不需要加载页面，直接切换到目标场景
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
