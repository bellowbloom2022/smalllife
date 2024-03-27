using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public GameObject panel;
    public GameManager gameManager;

    private bool isPaused = false;

    void Update()
    {
        //������ESC��
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                gameManager.PauseGame();//��ͣ��Ϸ
                OpenPanel();//��panel
            }
            else
            {
                gameManager.ResumeGame();
                ClosePanel();
            }
        }
    }
    public void OpenPanel()
    {
        if (panel != null)
        {
           panel.SetActive(true);
        }
    }

    public void ClosePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void OnResetButtonClicked()
    {
        gameManager.ResetGame();//����gamemanager�е�resetGame����
    }
}



