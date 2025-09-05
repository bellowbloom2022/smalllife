using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public static bool useLoadingPage = true;//��̬���������ڿ����Ƿ���Ҫ��ʾ����ҳ��
    void Start()
    {
        // �� PlayerPrefs �л�ȡĿ�곡�����Ʋ��ж��Ƿ���Ҫ����ҳ��
        StartCoroutine(LoadScene(PlayerPrefs.GetString("NextSceneName")));
    }

    IEnumerator LoadScene(string sceneName)
    {
        if (useLoadingPage)
        {
            // �����Ҫ����ҳ�棬�ȴ�3��
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            yield return new WaitForSeconds(2f); // ǿ�Ƶȴ� 2 ��

            operation.allowSceneActivation = true; // ����Ŀ�곡��
        }
        else
        {
            // �������Ҫ����ҳ�棬ֱ���л�����
            SceneManager.LoadScene(sceneName);
        }
    }
}
