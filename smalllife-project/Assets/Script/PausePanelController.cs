using UnityEngine;
using UnityEngine.UI;

public class PausePanelController : MonoBehaviour
{
    public GameManager gameManager;
    public Button resumeButton; // Resume按钮
    public AudioClip pauseSound; // 按键音效

    private bool isPaused = false;

    void Start()
    {
        // 给Resume按钮添加监听器
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            gameManager.PauseGame(); // 暂停游戏
            OpenPanel(); // 打开panel
            PauseAllAudio(); // 暂停所有声音
        }
        else
        {
            gameManager.ResumeGame(); // 恢复游戏
            ClosePanel(); // 关闭panel
            ResumeAllAudio(); // 恢复所有声音
        }
    }

    private void OpenPanel()
    {
        gameObject.SetActive(true);
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    private void OnResumeButtonClicked()
    {
        TogglePause();
    }


    private void PauseAllAudio()
    {
        AudioListener.pause = true;
    }

    private void ResumeAllAudio()
    {
        AudioListener.pause = false;
    }
}
