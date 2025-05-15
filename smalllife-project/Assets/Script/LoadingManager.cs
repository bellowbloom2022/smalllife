using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static bool useLoadingPage = true;//静态变量，用于控制是否需要显示加载页面
    void Start()
    {
        // 从 PlayerPrefs 中获取目标场景名称并判断是否需要加载页面
        StartCoroutine(LoadScene(PlayerPrefs.GetString("NextSceneName")));
    }

    IEnumerator LoadScene(string sceneName)
    {
        if (useLoadingPage)
        {
            // 如果需要加载页面，等待3秒
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            yield return new WaitForSeconds(2f); // 强制等待 2 秒

            operation.allowSceneActivation = true; // 激活目标场景
        }
        else
        {
            // 如果不需要加载页面，直接切换场景
            SceneManager.LoadScene(sceneName);
        }
    }
}
