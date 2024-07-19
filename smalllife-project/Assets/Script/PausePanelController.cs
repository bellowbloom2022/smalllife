using UnityEngine;
using UnityEngine.UI;

public class PausePanelController : MonoBehaviour
{
    public GameManager gameManager;
    public Button resumeButton; // Resume��ť
    public AudioClip pauseSound; // ������Ч

    private bool isPaused = false;

    void Start()
    {
        // ��Resume��ť��Ӽ�����
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
            gameManager.PauseGame(); // ��ͣ��Ϸ
            OpenPanel(); // ��panel
            PauseAllAudio(); // ��ͣ��������
        }
        else
        {
            gameManager.ResumeGame(); // �ָ���Ϸ
            ClosePanel(); // �ر�panel
            ResumeAllAudio(); // �ָ���������
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
