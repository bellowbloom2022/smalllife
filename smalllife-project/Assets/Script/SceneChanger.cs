using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("SceneChanger")]
    public string targetSceneName; // ��һ������������
    public bool showLoadingPage = true; // �Ƿ���Ҫ����ҳ��

    public void ChangeScene()
    {
        // �������þ����Ƿ���� LoadingPage
        LoadingManager.useLoadingPage = showLoadingPage;

        if (showLoadingPage)
        {
            // �����Ҫ����ҳ�棬�л��� LoadingScene
            PlayerPrefs.SetString("NextSceneName", targetSceneName);
            SceneManager.LoadScene("LoadingPage1");
        }
        else
        {
            // �������Ҫ����ҳ�棬ֱ���л���Ŀ�곡��
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
